import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PatientService } from '../../core/services/patient.service';
import { Patient } from '../../models/app.models';

@Component({
  selector: 'app-my-profile',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './my-profile.html',
})
export class MyProfileComponent {
  private patientService = inject(PatientService);

  patient = signal<Patient | null>(null);
  loading = signal(true);
  errorMessage = signal<string | null>(null);

  constructor() {
    this.patientService.getMe().subscribe({
      next: patient => {
        this.patient.set(patient);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load your profile.');
        this.loading.set(false);
      },
    });
  }
}
