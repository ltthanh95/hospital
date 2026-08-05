import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MedicalRecordService } from '../../core/services/medical-record.service';
import { PrescriptionService } from '../../core/services/prescription.service';
import { MedicalRecord, Prescription } from '../../models/app.models';

@Component({
  selector: 'app-my-records',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './my-records.html',
})
export class MyRecordsComponent {
  private medicalRecordService = inject(MedicalRecordService);
  private prescriptionService = inject(PrescriptionService);

  records = signal<MedicalRecord[]>([]);
  prescriptions = signal<Prescription[]>([]);
  loading = signal(true);
  errorMessage = signal<string | null>(null);
  private loadedCount = 0;

  constructor() {
    this.medicalRecordService.getMine().subscribe({
      next: records => {
        this.records.set(records);
        this.checkDone();
      },
      error: () => {
        this.errorMessage.set('Failed to load your medical records.');
        this.checkDone();
      },
    });

    this.prescriptionService.getMine().subscribe({
      next: prescriptions => {
        this.prescriptions.set(prescriptions);
        this.checkDone();
      },
      error: () => {
        this.errorMessage.set('Failed to load your prescriptions.');
        this.checkDone();
      },
    });
  }

  private checkDone() {
    this.loadedCount += 1;
    if (this.loadedCount >= 2) this.loading.set(false);
  }
}
