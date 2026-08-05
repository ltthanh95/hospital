import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NavigationEnd, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { ChatService } from '../../core/services/chat.service';
import { ChatSession } from '../../models/app.models';

@Component({
  selector: 'app-chat-bubble',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './chat-bubble.html',
})
export class ChatBubbleComponent {
  private auth = inject(AuthService);
  private chat = inject(ChatService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  open = signal(false);
  private currentUrl = signal(this.router.url);

  role = computed(() => this.auth.user()?.role);

  hidden = computed(() => {
    const role = this.role();
    if (!role || role === 'ADMIN') return true;
    const url = this.currentUrl();
    if (role === 'PATIENT' && url.startsWith('/chat')) return true;
    if (role === 'DOCTOR' && url.startsWith('/doctor/chat')) return true;
    return false;
  });

  // Patient state
  session = signal<ChatSession | null>(null);
  patientLoading = signal(false);
  patientError = signal<string | null>(null);
  draft = signal('');
  requestingDoctor = signal(false);
  private patientInitialized = false;

  // Doctor state
  sessions = signal<ChatSession[]>([]);
  doctorLoading = signal(false);
  doctorError = signal<string | null>(null);
  private doctorInitialized = false;
  private pollHandle: ReturnType<typeof setInterval> | null = null;

  waitingCount = computed(() => this.sessions().filter(s => s.mode === 'WAITING_FOR_DOCTOR').length);
  liveCount = computed(() => this.sessions().filter(s => s.mode === 'LIVE').length);
  badgeCount = computed(() => this.waitingCount() + this.liveCount());

  constructor() {
    this.router.events.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(event => {
      if (event instanceof NavigationEnd) this.currentUrl.set(event.urlAfterRedirects);
    });

    this.destroyRef.onDestroy(() => {
      if (this.pollHandle) clearInterval(this.pollHandle);
    });
  }

  toggle() {
    const next = !this.open();
    this.open.set(next);
    if (!next) return;

    if (this.role() === 'PATIENT') this.initPatient();
    else if (this.role() === 'DOCTOR') this.initDoctor();
  }

  private initPatient() {
    if (this.patientInitialized) return;
    this.patientInitialized = true;

    this.patientLoading.set(true);
    this.chat.getOrCreateSession().subscribe({
      next: async session => {
        this.session.set(session);
        this.patientLoading.set(false);
        try {
          await this.chat.connect();
          await this.chat.joinSession(session.id);
        } catch {
          this.patientError.set('Could not connect to live chat.');
        }
      },
      error: () => {
        this.patientError.set('Failed to start chat.');
        this.patientLoading.set(false);
      },
    });

    this.chat.messageReceived$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(message => {
      const current = this.session();
      if (!current) return;
      this.session.set({ ...current, messages: [...current.messages, message] });
    });

    this.chat.sessionModeChanged$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(event => {
      const current = this.session();
      if (!current || current.id !== event.sessionId) return;
      this.session.set({ ...current, mode: event.mode, doctorName: event.doctorName });
    });
  }

  async sendMessage() {
    const content = this.draft().trim();
    const session = this.session();
    if (!content || !session) return;

    this.draft.set('');
    try {
      await this.chat.sendMessage(session.id, content);
    } catch {
      this.patientError.set('Failed to send message.');
    }
  }

  async requestDoctor() {
    const session = this.session();
    if (!session) return;

    this.requestingDoctor.set(true);
    try {
      await this.chat.requestDoctor(session.id);
    } catch {
      this.patientError.set('Failed to request a doctor.');
    } finally {
      this.requestingDoctor.set(false);
    }
  }

  private initDoctor() {
    this.refreshDoctorSessions();
    if (this.doctorInitialized) return;
    this.doctorInitialized = true;
    this.pollHandle = setInterval(() => this.refreshDoctorSessions(), 7000);
  }

  refreshDoctorSessions() {
    this.doctorLoading.set(true);
    this.chat.getMySessions().subscribe({
      next: sessions => {
        this.sessions.set(sessions);
        this.doctorLoading.set(false);
      },
      error: () => {
        this.doctorError.set('Failed to load sessions.');
        this.doctorLoading.set(false);
      },
    });
  }
}
