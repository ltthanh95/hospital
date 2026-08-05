import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DoctorService } from '../../core/services/doctor.service';
import { Doctor } from '../../models/app.models';

@Component({
  selector: 'app-doctors',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './doctors.html',
})
export class DoctorsComponent {
  private doctorService = inject(DoctorService);

  doctors = signal<Doctor[]>([]);
  loading = signal(false);
  errorMessage = signal<string | null>(null);

  constructor() {
    this.refresh();
  }

  refresh() {
    this.loading.set(true);
    this.errorMessage.set(null);
    this.doctorService.getAll().subscribe({
      next: doctors => {
        this.doctors.set(doctors);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load doctors.');
        this.loading.set(false);
      },
    });
  }

  statusClass(status: Doctor['status']) {
    return status === 'ACTIVE' ? 'bg-emerald-50 text-emerald-700' : 'bg-slate-100 text-slate-600';
  }
}
