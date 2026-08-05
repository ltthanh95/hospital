import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MedicineService } from '../../core/services/medicine.service';
import { AuthService } from '../../core/auth/auth.service';
import { Medicine } from '../../models/app.models';

@Component({
  selector: 'app-medicines',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './medicines.html',
})
export class MedicinesComponent {
  private fb = inject(FormBuilder).nonNullable;
  private medicineService = inject(MedicineService);
  auth = inject(AuthService);

  medicines = signal<Medicine[]>([]);
  loading = signal(false);
  errorMessage = signal<string | null>(null);
  editingId = signal<number | null>(null);

  form = this.fb.group({
    name: ['', Validators.required],
    manufacturer: ['', Validators.required],
    unitPrice: [0, [Validators.required, Validators.min(0)]],
    stockQt: [0, [Validators.required, Validators.min(0)]],
    expiration: ['', Validators.required],
  });

  constructor() {
    this.refresh();
  }

  refresh() {
    this.loading.set(true);
    this.medicineService.getAll().subscribe({
      next: medicines => {
        this.medicines.set(medicines);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load medicines.');
        this.loading.set(false);
      },
    });
  }

  startCreate() {
    this.editingId.set(null);
    this.form.reset({ name: '', manufacturer: '', unitPrice: 0, stockQt: 0, expiration: '' });
  }

  startEdit(medicine: Medicine) {
    this.editingId.set(medicine.id);
    this.form.setValue({
      name: medicine.name,
      manufacturer: medicine.manufacturer,
      unitPrice: medicine.unitPrice,
      stockQt: medicine.stockQt,
      expiration: medicine.expiration.slice(0, 10),
    });
  }

  cancelEdit() {
    this.startCreate();
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const request = this.form.getRawValue();
    const id = this.editingId();
    this.errorMessage.set(null);

    const save$ = id ? this.medicineService.update(id, request) : this.medicineService.create(request);

    save$.subscribe({
      next: () => {
        this.cancelEdit();
        this.refresh();
      },
      error: err => this.errorMessage.set(err.error?.message ?? 'Save failed.'),
    });
  }

  remove(medicine: Medicine) {
    if (!confirm(`Delete medicine "${medicine.name}"?`)) return;

    this.medicineService.delete(medicine.id).subscribe({
      next: () => this.refresh(),
      error: err => this.errorMessage.set(err.error?.message ?? 'Delete failed.'),
    });
  }
}
