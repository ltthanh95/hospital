import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PatientService } from '../../core/services/patient.service';
import { Patient } from '../../models/app.models';

@Component({
  selector: 'app-patients',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './patients.html',
})
export class PatientsComponent {
  private patientService = inject(PatientService);

  patients = signal<Patient[]>([]);
  loading = signal(false);
  errorMessage = signal<string | null>(null);

  constructor() {
    this.refresh();
  }

  refresh() {
    this.loading.set(true);
    this.errorMessage.set(null);
    this.patientService.getAllDetailed().subscribe({
      next: patients => {
        this.patients.set(patients);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load patients.');
        this.loading.set(false);
      },
    });
  }

  statusClass(status: Patient['status']) {
    return status === 'ADMISSION' ? 'bg-amber-50 text-amber-700' : 'bg-slate-100 text-slate-600';
  }
}
