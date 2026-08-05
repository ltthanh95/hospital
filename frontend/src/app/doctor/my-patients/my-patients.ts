import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DoctorService } from '../../core/services/doctor.service';
import { MedicalRecordService } from '../../core/services/medical-record.service';
import { Doctor, DoctorAppointmentSummary } from '../../models/app.models';

@Component({
  selector: 'app-my-patients',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './my-patients.html',
})
export class MyPatientsComponent {
  private doctorService = inject(DoctorService);
  private medicalRecordService = inject(MedicalRecordService);

  doctor = signal<Doctor | null>(null);
  loading = signal(true);
  errorMessage = signal<string | null>(null);

  recordingAppointmentId = signal<number | null>(null);
  visitDate = signal('');
  diagnosis = signal('');
  notes = signal('');
  submitting = signal(false);

  upcomingAppointments = computed(() =>
    (this.doctor()?.appointments ?? []).filter(a => a.status !== 'CANCELLED'),
  );

  constructor() {
    this.refresh();
  }

  refresh() {
    this.loading.set(true);
    this.errorMessage.set(null);
    this.doctorService.getMe().subscribe({
      next: doctor => {
        this.doctor.set(doctor);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load your patients.');
        this.loading.set(false);
      },
    });
  }

  statusClass(status: DoctorAppointmentSummary['status']) {
    return status === 'CONFIRMED' ? 'bg-emerald-50 text-emerald-700' : 'bg-amber-50 text-amber-700';
  }

  startRecordVisit(appointment: DoctorAppointmentSummary) {
    this.recordingAppointmentId.set(appointment.id);
    this.visitDate.set(appointment.schedule.slice(0, 10));
    this.diagnosis.set('');
    this.notes.set('');
  }

  cancelRecordVisit() {
    this.recordingAppointmentId.set(null);
  }

  submitRecordVisit(appointment: DoctorAppointmentSummary) {
    const diagnosis = this.diagnosis().trim();
    const visit = this.visitDate();
    if (!diagnosis || !visit) return;

    this.submitting.set(true);
    this.medicalRecordService
      .create({
        patientId: appointment.patientId,
        appointmentId: appointment.id,
        visit,
        diagnosis,
        notes: this.notes().trim() || undefined,
      })
      .subscribe({
        next: () => {
          this.submitting.set(false);
          this.recordingAppointmentId.set(null);
          this.refresh();
        },
        error: err => {
          this.submitting.set(false);
          this.errorMessage.set(err.error?.message ?? 'Failed to record visit.');
        },
      });
  }
}
