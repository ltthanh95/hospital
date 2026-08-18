import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RoomService } from '../../core/services/room.service';
import { AuthService } from '../../core/auth/auth.service';
import { Room } from '../../models/app.models';

@Component({
  selector: 'app-rooms',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './rooms.html',
})
export class RoomsComponent {
  private roomService = inject(RoomService);
  private fb = inject(FormBuilder).nonNullable;
  auth = inject(AuthService);

  rooms = signal<Room[]>([]);
  loading = signal(false);
  errorMessage = signal<string | null>(null);
  showForm = signal(false);
  submitting = signal(false);

  form = this.fb.group({
    roomNumber: ['', Validators.required],
    type: ['', Validators.required],
    capacity: [1, [Validators.required, Validators.min(1)]],
  });

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

  toggleForm() {
    this.showForm.update(v => !v);
    this.form.reset({ roomNumber: '', type: '', capacity: 1 });
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { roomNumber, type, capacity } = this.form.getRawValue();
    this.submitting.set(true);
    this.errorMessage.set(null);
    this.roomService.create({ roomNumber, type, capacity }).subscribe({
      next: () => {
        this.submitting.set(false);
        this.showForm.set(false);
        this.refresh();
      },
      error: err => {
        this.errorMessage.set(err.error?.message ?? 'Failed to create room.');
        this.submitting.set(false);
      },
    });
  }
}
