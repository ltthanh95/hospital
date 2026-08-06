import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DoctorService } from '../../core/services/doctor.service';
import { Doctor } from '../../models/app.models';
import { AppointmentCalendarComponent, CalendarAppointment } from '../../shared/calendar/appointment-calendar';

function startOfToday() {
  const d = new Date();
  d.setHours(0, 0, 0, 0);
  return d;
}

function isSameDay(a: Date, b: Date) {
  return a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate();
}

@Component({
  selector: 'app-doctor-dashboard',
  standalone: true,
  imports: [CommonModule, AppointmentCalendarComponent],
  templateUrl: './doctor-dashboard.html',
})
export class DoctorDashboardComponent {
  private doctorService = inject(DoctorService);

  doctor = signal<Doctor | null>(null);
  loading = signal(true);
  errorMessage = signal<string | null>(null);

  private today = startOfToday();

  calendarEvents = computed<CalendarAppointment[]>(() =>
    (this.doctor()?.appointments ?? [])
      .filter(a => new Date(a.schedule) >= this.today)
      .map(a => ({
        id: a.id,
        date: new Date(a.schedule),
        primaryLabel: a.patientName,
        secondaryLabel: a.reason,
        status: a.status,
      }))
      .sort((a, b) => a.date.getTime() - b.date.getTime()),
  );

  activeEvents = computed(() => this.calendarEvents().filter(e => e.status !== 'CANCELLED'));

  todayCount = computed(() => this.activeEvents().filter(e => isSameDay(e.date, this.today)).length);

  upcomingCount = computed(() => this.activeEvents().length);

  totalPatients = computed(() => {
    const appointments = this.doctor()?.appointments ?? [];
    return new Set(appointments.map(a => a.patientId)).size;
  });

  constructor() {
    this.doctorService.getMe().subscribe({
      next: doctor => {
        this.doctor.set(doctor);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load your schedule.');
        this.loading.set(false);
      },
    });
  }
}
