import { Component, computed, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppointmentStatus } from '../../models/app.models';

export interface CalendarAppointment {
  id: number;
  date: Date;
  primaryLabel: string;
  secondaryLabel: string;
  status: AppointmentStatus;
}

interface DayCell {
  date: Date;
  inMonth: boolean;
  isToday: boolean;
  items: CalendarAppointment[];
}

function isSameDay(a: Date, b: Date) {
  return a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate();
}

function startOfDay(d: Date) {
  const c = new Date(d);
  c.setHours(0, 0, 0, 0);
  return c;
}

const WEEKDAY_LABELS = ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'];

@Component({
  selector: 'app-appointment-calendar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './appointment-calendar.html',
})
export class AppointmentCalendarComponent {
  events = input<CalendarAppointment[]>([]);
  emptyLabel = input('No appointments scheduled.');

  weekdayLabels = WEEKDAY_LABELS;

  private today = startOfDay(new Date());
  viewMonth = signal(new Date(this.today.getFullYear(), this.today.getMonth(), 1));
  selectedDate = signal<Date>(this.today);

  monthLabel = computed(() =>
    this.viewMonth().toLocaleDateString(undefined, { month: 'long', year: 'numeric' }),
  );

  upcoming = computed(() =>
    [...this.events()]
      .filter(e => e.date >= this.today)
      .sort((a, b) => a.date.getTime() - b.date.getTime())
      .slice(0, 5),
  );

  weeks = computed<DayCell[][]>(() => {
    const month = this.viewMonth();
    const year = month.getFullYear();
    const monthIndex = month.getMonth();
    const startWeekday = new Date(year, monthIndex, 1).getDay();
    const daysInMonth = new Date(year, monthIndex + 1, 0).getDate();
    const events = this.events();

    const cells: DayCell[] = [];
    for (let i = 0; i < startWeekday; i++) {
      const d = new Date(year, monthIndex, 1 - (startWeekday - i));
      cells.push({ date: d, inMonth: false, isToday: false, items: [] });
    }
    for (let day = 1; day <= daysInMonth; day++) {
      const d = new Date(year, monthIndex, day);
      const items = events.filter(e => isSameDay(e.date, d));
      cells.push({ date: d, inMonth: true, isToday: isSameDay(d, this.today), items });
    }
    while (cells.length % 7 !== 0) {
      const last = cells[cells.length - 1].date;
      const d = new Date(last);
      d.setDate(d.getDate() + 1);
      cells.push({ date: d, inMonth: false, isToday: false, items: [] });
    }

    const weeks: DayCell[][] = [];
    for (let i = 0; i < cells.length; i += 7) weeks.push(cells.slice(i, i + 7));
    return weeks;
  });

  selectedDayItems = computed(() => {
    const date = this.selectedDate();
    return this.events()
      .filter(e => isSameDay(e.date, date))
      .sort((a, b) => a.date.getTime() - b.date.getTime());
  });

  prevMonth() {
    const m = this.viewMonth();
    this.viewMonth.set(new Date(m.getFullYear(), m.getMonth() - 1, 1));
  }

  nextMonth() {
    const m = this.viewMonth();
    this.viewMonth.set(new Date(m.getFullYear(), m.getMonth() + 1, 1));
  }

  goToday() {
    this.viewMonth.set(new Date(this.today.getFullYear(), this.today.getMonth(), 1));
    this.selectedDate.set(this.today);
  }

  selectDay(cell: DayCell) {
    this.selectedDate.set(cell.date);
  }

  isSelected(cell: DayCell) {
    return isSameDay(cell.date, this.selectedDate());
  }

  statusDotClass(status: AppointmentStatus) {
    switch (status) {
      case 'CONFIRMED':
        return 'bg-emerald-500';
      case 'CANCELLED':
        return 'bg-red-400';
      default:
        return 'bg-amber-500';
    }
  }

  statusBadgeClass(status: AppointmentStatus) {
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
