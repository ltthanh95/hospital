import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppointmentService } from '../../core/services/appointment.service';
import { Appointment } from '../../models/app.models';

@Component({
  selector: 'app-appointments',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './appointments.html',
})
export class AppointmentsComponent {
  private appointmentService = inject(AppointmentService);

  appointments = signal<Appointment[]>([]);
  loading = signal(false);
  errorMessage = signal<string | null>(null);

  constructor() {
    this.refresh();
  }

  refresh() {
    this.loading.set(true);
    this.errorMessage.set(null);
    this.appointmentService.getAll().subscribe({
      next: appointments => {
        this.appointments.set(appointments);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load appointments.');
        this.loading.set(false);
      },
    });
  }

  statusClass(status: Appointment['status']) {
    switch (status) {
      case 'CONFIRMED':
        return 'bg-emerald-50 text-emerald-700';
      case 'CANCELLED':
        return 'bg-red-50 text-red-700';
      default:
        return 'bg-amber-50 text-amber-700';
    }
  }
}
