import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DoctorService } from '../../core/services/doctor.service';
import { AppointmentService } from '../../core/services/appointment.service';
import { Doctor } from '../../models/app.models';

interface DayCell {
  date: Date;
  inMonth: boolean;
  isPast: boolean;
  count: number;
}

interface Slot {
  hour: number;
  taken: boolean;
  disabled: boolean;
}

const CLINIC_HOURS = [9, 10, 11, 12, 13, 14, 15, 16];

function isSameDay(a: Date, b: Date) {
  return a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate();
}

// Mirrors the naive local-time convention the backend already uses for appointment scheduling
// (no timezone offset), matching how reschedule/cancel already round-trip `datetime-local` values.
function formatLocalDateTime(date: Date, hour: number): string {
  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, '0');
  const d = String(date.getDate()).padStart(2, '0');
  const h = String(hour).padStart(2, '0');
  return `${y}-${m}-${d}T${h}:00:00`;
}

@Component({
  selector: 'app-book-appointment',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './book-appointment.html',
})
export class BookAppointmentComponent {
  private doctorService = inject(DoctorService);
  private appointmentService = inject(AppointmentService);

  doctors = signal<Doctor[]>([]);
  doctorsLoading = signal(true);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  selectedDoctorId = signal<number | null>(null);
  selectedDoctor = signal<Doctor | null>(null);
  doctorLoading = signal(false);

  today = new Date();
  viewMonth = signal(new Date(this.today.getFullYear(), this.today.getMonth(), 1));
  selectedDate = signal<Date | null>(null);
  selectedSlotHour = signal<number | null>(null);
  reason = signal('');
  submitting = signal(false);

  monthLabel = computed(() =>
    this.viewMonth().toLocaleDateString(undefined, { month: 'long', year: 'numeric' }),
  );

  calendarWeeks = computed(() => {
    const month = this.viewMonth();
    const year = month.getFullYear();
    const monthIndex = month.getMonth();
    const startWeekday = new Date(year, monthIndex, 1).getDay();
    const daysInMonth = new Date(year, monthIndex + 1, 0).getDate();
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const appointments = (this.selectedDoctor()?.appointments ?? []).filter(a => a.status !== 'CANCELLED');

    const cells: DayCell[] = [];
    for (let i = 0; i < startWeekday; i++) {
      const d = new Date(year, monthIndex, 1 - (startWeekday - i));
      cells.push({ date: d, inMonth: false, isPast: true, count: 0 });
    }
    for (let day = 1; day <= daysInMonth; day++) {
      const d = new Date(year, monthIndex, day);
      const count = appointments.filter(a => isSameDay(new Date(a.schedule), d)).length;
      cells.push({ date: d, inMonth: true, isPast: d < today, count });
    }
    while (cells.length % 7 !== 0) {
      const last = cells[cells.length - 1].date;
      const d = new Date(last);
      d.setDate(d.getDate() + 1);
      cells.push({ date: d, inMonth: false, isPast: true, count: 0 });
    }

    const weeks: DayCell[][] = [];
    for (let i = 0; i < cells.length; i += 7) weeks.push(cells.slice(i, i + 7));
    return weeks;
  });

  daySlots = computed<Slot[]>(() => {
    const date = this.selectedDate();
    if (!date) return [];

    const appointments = (this.selectedDoctor()?.appointments ?? []).filter(a => a.status !== 'CANCELLED');
    const now = new Date();
    const isToday = isSameDay(date, now);

    return CLINIC_HOURS.map(hour => {
      const taken = appointments.some(a => {
        const d = new Date(a.schedule);
        return isSameDay(d, date) && d.getHours() === hour;
      });
      const disabled = taken || (isToday && hour <= now.getHours());
      return { hour, taken, disabled };
    });
  });

  constructor() {
    this.doctorService.getAll().subscribe({
      next: doctors => {
        this.doctors.set(doctors);
        this.doctorsLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load doctors.');
        this.doctorsLoading.set(false);
      },
    });
  }

  selectDoctor(idString: string) {
    const id = Number(idString);
    if (!id) {
      this.selectedDoctorId.set(null);
      this.selectedDoctor.set(null);
      return;
    }

    this.selectedDoctorId.set(id);
    this.selectedDate.set(null);
    this.selectedSlotHour.set(null);
    this.doctorLoading.set(true);
    this.doctorService.getById(id).subscribe({
      next: doctor => {
        this.selectedDoctor.set(doctor);
        this.doctorLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load that doctor.');
        this.doctorLoading.set(false);
      },
    });
  }

  prevMonth() {
    const m = this.viewMonth();
    this.viewMonth.set(new Date(m.getFullYear(), m.getMonth() - 1, 1));
    this.selectedDate.set(null);
    this.selectedSlotHour.set(null);
  }

  nextMonth() {
    const m = this.viewMonth();
    this.viewMonth.set(new Date(m.getFullYear(), m.getMonth() + 1, 1));
    this.selectedDate.set(null);
    this.selectedSlotHour.set(null);
  }

  selectDay(cell: DayCell) {
    if (!this.selectedDoctorId() || cell.isPast) return;
    this.selectedDate.set(cell.date);
    this.selectedSlotHour.set(null);
  }

  pickSlot(slot: Slot) {
    if (slot.disabled) return;
    this.selectedSlotHour.set(slot.hour);
  }

  submit() {
    const date = this.selectedDate();
    const hour = this.selectedSlotHour();
    const doctorId = this.selectedDoctorId();
    const reason = this.reason().trim();
    if (!date || hour === null || !doctorId || !reason) return;

    this.submitting.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.appointmentService.create({ doctorId, schedule: formatLocalDateTime(date, hour), reason }).subscribe({
      next: () => {
        this.successMessage.set('Appointment booked.');
        this.submitting.set(false);
        this.reason.set('');
        this.selectedSlotHour.set(null);
        this.doctorService.getById(doctorId).subscribe(doctor => this.selectedDoctor.set(doctor));
      },
      error: err => {
        this.errorMessage.set(err.error?.message ?? 'Failed to book appointment.');
        this.submitting.set(false);
      },
    });
  }
}
