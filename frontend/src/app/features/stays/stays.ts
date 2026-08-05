import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { StayService } from '../../core/services/stay.service';
import { RoomService } from '../../core/services/room.service';
import { PatientService } from '../../core/services/patient.service';
import { AuthService } from '../../core/auth/auth.service';
import { PatientSummary, Room, Stay } from '../../models/app.models';

@Component({
  selector: 'app-stays',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './stays.html',
})
export class StaysComponent {
  private fb = inject(FormBuilder).nonNullable;
  private stayService = inject(StayService);
  private roomService = inject(RoomService);
  private patientService = inject(PatientService);
  auth = inject(AuthService);

  stays = signal<Stay[]>([]);
  patients = signal<PatientSummary[]>([]);
  loading = signal(false);
  errorMessage = signal<string | null>(null);
  showForm = signal(false);

  form = this.fb.group({
    patientId: [0, Validators.required],
    roomId: [0, Validators.required],
    checkIn: ['', Validators.required],
  });

  availableRooms = signal<Room[]>([]);

  constructor() {
    this.refresh();
  }

  refresh() {
    this.loading.set(true);
    this.errorMessage.set(null);
    forkJoin({
      stays: this.stayService.getAll(),
      rooms: this.roomService.getAll(),
      patients: this.patientService.getAll(),
    }).subscribe({
      next: ({ stays, rooms, patients }) => {
        this.stays.set(stays);
        this.availableRooms.set(rooms.filter(r => r.currentOccupancy < r.capacity));
        this.patients.set(patients);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load stays.');
        this.loading.set(false);
      },
    });
  }

  toggleForm() {
    this.showForm.update(v => !v);
    this.form.reset({ patientId: 0, roomId: 0, checkIn: '' });
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { patientId, roomId, checkIn } = this.form.getRawValue();
    this.stayService.create({ patientId, roomId, checkIn }).subscribe({
      next: () => {
        this.showForm.set(false);
        this.refresh();
      },
      error: err => this.errorMessage.set(err.error?.message ?? 'Failed to assign room.'),
    });
  }

  checkout(stay: Stay) {
    this.stayService.checkout(stay.id).subscribe({
      next: () => this.refresh(),
      error: err => this.errorMessage.set(err.error?.message ?? 'Checkout failed.'),
    });
  }
}
