import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PatientService } from '../../core/services/patient.service';
import { AppointmentService } from '../../core/services/appointment.service';
import { PatientAppointmentSummary } from '../../models/app.models';

@Component({
  selector: 'app-my-appointments',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './my-appointments.html',
})
export class MyAppointmentsComponent {
  private patientService = inject(PatientService);
  private appointmentService = inject(AppointmentService);

  appointments = signal<PatientAppointmentSummary[]>([]);
  loading = signal(true);
  errorMessage = signal<string | null>(null);
  reschedulingId = signal<number | null>(null);
  newSchedule = signal('');

  constructor() {
    this.refresh();
  }

  refresh() {
    this.loading.set(true);
    this.errorMessage.set(null);
    this.patientService.getMe().subscribe({
      next: patient => {
        this.appointments.set(patient.appointments);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load your appointments.');
        this.loading.set(false);
      },
    });
  }

  statusClass(status: PatientAppointmentSummary['status']) {
    switch (status) {
      case 'CONFIRMED':
        return 'bg-emerald-50 text-emerald-700';
      case 'CANCELLED':
        return 'bg-red-50 text-red-700';
      default:
        return 'bg-amber-50 text-amber-700';
    }
  }

  cancel(appointment: PatientAppointmentSummary) {
    if (!confirm(`Cancel your appointment with Dr. ${appointment.doctorName}?`)) return;

    this.appointmentService.cancel(appointment.id).subscribe({
      next: () => this.refresh(),
      error: err => this.errorMessage.set(err.error?.message ?? 'Failed to cancel appointment.'),
    });
  }

  startReschedule(appointment: PatientAppointmentSummary) {
    this.reschedulingId.set(appointment.id);
    this.newSchedule.set(appointment.schedule.slice(0, 16));
  }

  cancelReschedule() {
    this.reschedulingId.set(null);
    this.newSchedule.set('');
  }

  submitReschedule(appointment: PatientAppointmentSummary) {
    const schedule = this.newSchedule();
    if (!schedule) return;

    this.appointmentService.reschedule(appointment.id, { schedule }).subscribe({
      next: () => {
        this.cancelReschedule();
        this.refresh();
      },
      error: err => this.errorMessage.set(err.error?.message ?? 'Failed to reschedule appointment.'),
    });
  }
}
