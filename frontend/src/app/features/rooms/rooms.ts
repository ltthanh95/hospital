import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RoomService } from '../../core/services/room.service';
import { Room } from '../../models/app.models';

@Component({
  selector: 'app-rooms',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './rooms.html',
})
export class RoomsComponent {
  private roomService = inject(RoomService);

  rooms = signal<Room[]>([]);
  loading = signal(false);
  errorMessage = signal<string | null>(null);

  constructor() {
    this.refresh();
  }

  refresh() {
    this.loading.set(true);
    this.errorMessage.set(null);
    this.roomService.getAll().subscribe({
      next: rooms => {
        this.rooms.set(rooms);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load rooms.');
        this.loading.set(false);
      },
    });
  }
}
