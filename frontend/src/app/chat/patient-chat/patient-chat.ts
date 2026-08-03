import { Component, DestroyRef, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ChatService } from '../../core/services/chat.service';
import { ChatSession } from '../../models/app.models';

@Component({
  selector: 'app-patient-chat',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './patient-chat.html',
})
export class PatientChatComponent {
  private chat = inject(ChatService);
  private destroyRef = inject(DestroyRef);

  session = signal<ChatSession | null>(null);
  loading = signal(true);
  errorMessage = signal<string | null>(null);
  draft = signal('');
  requestingDoctor = signal(false);

  constructor() {
    this.init();

    this.destroyRef.onDestroy(() => {
      this.chat.disconnect();
    });
  }

  private async init() {
    this.chat.getOrCreateSession().subscribe({
      next: async session => {
        this.session.set(session);
        this.loading.set(false);

        try {
          await this.chat.connect();
          await this.chat.joinSession(session.id);
        } catch {
          this.errorMessage.set('Could not connect to live chat.');
        }
      },
      error: () => {
        this.errorMessage.set('Failed to start chat.');
        this.loading.set(false);
      },
    });

    this.chat.messageReceived$.pipe().subscribe(message => {
      const current = this.session();
      if (!current) return;
      this.session.set({ ...current, messages: [...current.messages, message] });
    });

    this.chat.sessionModeChanged$.subscribe(event => {
      const current = this.session();
      if (!current || current.id !== event.sessionId) return;
      this.session.set({ ...current, mode: event.mode, doctorName: event.doctorName });
    });
  }

  async send() {
    const content = this.draft().trim();
    const session = this.session();
    if (!content || !session) return;

    this.draft.set('');
    try {
      await this.chat.sendMessage(session.id, content);
    } catch {
      this.errorMessage.set('Failed to send message.');
    }
  }

  async requestDoctor() {
    const session = this.session();
    if (!session) return;

    this.requestingDoctor.set(true);
    try {
      await this.chat.requestDoctor(session.id);
    } catch {
      this.errorMessage.set('Failed to request a doctor.');
    } finally {
      this.requestingDoctor.set(false);
    }
  }

  async endChat() {
    const session = this.session();
    if (!session) return;

    try {
      await this.chat.endLiveChat(session.id);
    } catch {
      this.errorMessage.set('Failed to end the chat.');
    }
  }
}
