import { Component, DestroyRef, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ChatService } from '../../core/services/chat.service';
import { ChatSession } from '../../models/app.models';

@Component({
  selector: 'app-doctor-chat',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './doctor-chat.html',
})
export class DoctorChatComponent {
  private chat = inject(ChatService);
  private destroyRef = inject(DestroyRef);
  private pollHandle: ReturnType<typeof setInterval> | null = null;

  sessions = signal<ChatSession[]>([]);
  activeSession = signal<ChatSession | null>(null);
  available = signal(false);
  loading = signal(true);
  errorMessage = signal<string | null>(null);
  draft = signal('');
  connecting = signal(true);

  constructor() {
    this.init();

    this.pollHandle = setInterval(() => this.refreshSessions(), 5000);
    this.destroyRef.onDestroy(() => {
      if (this.pollHandle) clearInterval(this.pollHandle);
      this.chat.disconnect();
    });
  }

  private async init() {
    this.refreshSessions();

    try {
      await this.chat.connect();
    } catch {
      this.errorMessage.set('Could not connect to live chat.');
    } finally {
      this.connecting.set(false);
    }

    this.chat.messageReceived$.subscribe(message => {
      const current = this.activeSession();
      if (!current) return;
      const updated = { ...current, messages: [...current.messages, message] };
      this.activeSession.set(updated);
      this.patchSessionInList(updated);
    });

    this.chat.sessionModeChanged$.subscribe(event => {
      const current = this.activeSession();
      if (current && current.id === event.sessionId) {
        this.activeSession.set({ ...current, mode: event.mode, doctorName: event.doctorName });
      }
      this.refreshSessions();
    });
  }

  private patchSessionInList(session: ChatSession) {
    this.sessions.set(this.sessions().map(s => (s.id === session.id ? session : s)));
  }

  refreshSessions() {
    this.chat.getMySessions().subscribe({
      next: sessions => {
        this.sessions.set(sessions);
        this.loading.set(false);

        const active = this.activeSession();
        if (active) {
          const refreshedActive = sessions.find(s => s.id === active.id);
          if (refreshedActive) this.activeSession.set(refreshedActive);
        }
      },
      error: () => {
        this.errorMessage.set('Failed to load chat sessions.');
        this.loading.set(false);
      },
    });
  }

  async toggleAvailability() {
    const next = !this.available();
    try {
      await this.chat.setAvailability(next);
      this.available.set(next);
      this.refreshSessions();
    } catch {
      this.errorMessage.set('Failed to update availability.');
    }
  }

  async openSession(session: ChatSession) {
    if (session.mode !== 'LIVE') return;

    this.chat.getMessages(session.id).subscribe({
      next: async full => {
        this.activeSession.set(full);
        try {
          await this.chat.joinSession(full.id);
        } catch {
          this.errorMessage.set('Failed to join chat session.');
        }
      },
      error: () => this.errorMessage.set('Failed to load messages.'),
    });
  }

  async send() {
    const content = this.draft().trim();
    const session = this.activeSession();
    if (!content || !session) return;

    this.draft.set('');
    try {
      await this.chat.sendMessage(session.id, content);
    } catch {
      this.errorMessage.set('Failed to send message.');
    }
  }

  async endChat() {
    const session = this.activeSession();
    if (!session) return;

    try {
      await this.chat.endLiveChat(session.id);
      this.activeSession.set(null);
      this.refreshSessions();
    } catch {
      this.errorMessage.set('Failed to end the chat.');
    }
  }
}
