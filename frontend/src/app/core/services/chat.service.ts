import { Injectable, inject, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { ApiService } from '../http/api.service';
import { environment } from '../../environments/environment';
import { ChatMessage, ChatMode, ChatSession } from '../../models/app.models';

export interface SessionModeChangedEvent {
  sessionId: number;
  mode: ChatMode;
  doctorName: string | null;
}

@Injectable({ providedIn: 'root' })
export class ChatService {
  private api = inject(ApiService);
  private hub: signalR.HubConnection | null = null;
  private connectPromise: Promise<void> | null = null;

  connectionState = signal<'disconnected' | 'connecting' | 'connected'>('disconnected');

  private messageReceived = new Subject<ChatMessage>();
  messageReceived$ = this.messageReceived.asObservable();

  private sessionModeChanged = new Subject<SessionModeChangedEvent>();
  sessionModeChanged$ = this.sessionModeChanged.asObservable();

  getOrCreateSession() {
    return this.api.post<ChatSession>('/chat/sessions', {});
  }

  getMessages(sessionId: number) {
    return this.api.get<ChatSession>(`/chat/sessions/${sessionId}/messages`);
  }

  getMySessions() {
    return this.api.get<ChatSession[]>('/chat/sessions');
  }

  connect(): Promise<void> {
    if (this.connectPromise) return this.connectPromise;

    this.hub = new signalR.HubConnectionBuilder()
      .withUrl(environment.hubUrl, { withCredentials: true })
      .withAutomaticReconnect()
      .build();

    this.hub.on('ReceiveMessage', message => this.messageReceived.next(message));
    this.hub.on('SessionModeChanged', (sessionId: number, mode: ChatMode, doctorName: string | null) =>
      this.sessionModeChanged.next({ sessionId, mode, doctorName }),
    );
    this.hub.onclose(() => this.connectionState.set('disconnected'));
    this.hub.onreconnected(() => this.connectionState.set('connected'));

    this.connectionState.set('connecting');
    this.connectPromise = this.hub
      .start()
      .then(() => this.connectionState.set('connected'))
      .catch(err => {
        this.connectionState.set('disconnected');
        this.connectPromise = null;
        throw err;
      });

    return this.connectPromise;
  }

  async disconnect() {
    await this.hub?.stop();
    this.hub = null;
    this.connectPromise = null;
    this.connectionState.set('disconnected');
  }

  joinSession(sessionId: number) {
    return this.hub!.invoke('JoinSession', sessionId);
  }

  sendMessage(sessionId: number, content: string) {
    return this.hub!.invoke('SendMessage', sessionId, content);
  }

  requestDoctor(sessionId: number) {
    return this.hub!.invoke('RequestDoctor', sessionId);
  }

  endLiveChat(sessionId: number) {
    return this.hub!.invoke('EndLiveChat', sessionId);
  }

  setAvailability(isAvailable: boolean) {
    return this.hub!.invoke('SetAvailability', isAvailable);
  }
}
