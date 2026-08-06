import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PatientService } from '../../core/services/patient.service';
import { Patient } from '../../models/app.models';
import { AppointmentCalendarComponent, CalendarAppointment } from '../../shared/calendar/appointment-calendar';

function startOfToday() {
  const d = new Date();
  d.setHours(0, 0, 0, 0);
  return d;
}

@Component({
  selector: 'app-patient-dashboard',
  standalone: true,
  imports: [CommonModule, AppointmentCalendarComponent],
  templateUrl: './patient-dashboard.html',
})
export class PatientDashboardComponent {
  private patientService = inject(PatientService);

  patient = signal<Patient | null>(null);
  loading = signal(true);
  errorMessage = signal<string | null>(null);

  private today = startOfToday();

  calendarEvents = computed<CalendarAppointment[]>(() =>
    (this.patient()?.appointments ?? [])
      .filter(a => new Date(a.schedule) >= this.today)
      .map(a => ({
        id: a.id,
        date: new Date(a.schedule),
        primaryLabel: `Dr. ${a.doctorName}`,
        secondaryLabel: a.reason,
        status: a.status,
      }))
      .sort((a, b) => a.date.getTime() - b.date.getTime()),
  );

  upcomingCount = computed(
    () => this.calendarEvents().filter(e => e.status !== 'CANCELLED').length,
  );

  nextAppointment = computed(() => {
    const active = this.calendarEvents().filter(e => e.status !== 'CANCELLED');
    return active.length ? active[0] : null;
  });

  constructor() {
    this.patientService.getMe().subscribe({
      next: patient => {
        this.patient.set(patient);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load your appointments.');
        this.loading.set(false);
      },
    });
  }
}
