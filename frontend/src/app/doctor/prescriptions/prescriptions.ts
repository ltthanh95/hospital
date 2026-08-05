import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { DoctorService } from '../../core/services/doctor.service';
import { PrescriptionService } from '../../core/services/prescription.service';
import { PrescriptionItemService } from '../../core/services/prescription-item.service';
import { MedicineService } from '../../core/services/medicine.service';
import { Doctor, Medicine, Prescription, PrescriptionItemSummary } from '../../models/app.models';

interface DraftItem {
  medicineId: number;
  dosage: string;
  quantity: number;
  frequency: string;
  durationDays: number;
}

function emptyDraftItem(medicines: Medicine[]): DraftItem {
  return { medicineId: medicines[0]?.id ?? 0, dosage: '', quantity: 1, frequency: '', durationDays: 1 };
}

@Component({
  selector: 'app-doctor-prescriptions',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './prescriptions.html',
})
export class DoctorPrescriptionsComponent {
  private fb = inject(FormBuilder).nonNullable;
  private doctorService = inject(DoctorService);
  private prescriptionService = inject(PrescriptionService);
  private prescriptionItemService = inject(PrescriptionItemService);
  private medicineService = inject(MedicineService);

  doctor = signal<Doctor | null>(null);
  prescriptions = signal<Prescription[]>([]);
  medicines = signal<Medicine[]>([]);
  loading = signal(true);
  errorMessage = signal<string | null>(null);

  editingId = signal<number | null>(null);
  draftItems = signal<DraftItem[]>([]);
  saving = signal(false);

  editItemId = signal<number | null>(null);
  editItemDraft = signal<DraftItem>({ medicineId: 0, dosage: '', quantity: 1, frequency: '', durationDays: 1 });
  newItemDraft = signal<DraftItem>({ medicineId: 0, dosage: '', quantity: 1, frequency: '', durationDays: 1 });

  headerForm = this.fb.group({
    medicalRecordId: [0, Validators.required],
    issueDate: ['', Validators.required],
    instruction: ['', Validators.required],
  });

  myRecordIds = computed(() => new Set((this.doctor()?.medicalRecords ?? []).map(r => r.id)));
  myPrescriptions = computed(() => this.prescriptions().filter(p => this.myRecordIds().has(p.medicalRecordId)));

  constructor() {
    this.refresh();
  }

  refresh() {
    this.loading.set(true);
    this.errorMessage.set(null);
    forkJoin({
      doctor: this.doctorService.getMe(),
      prescriptions: this.prescriptionService.getAll(),
      medicines: this.medicineService.getAll(),
    }).subscribe({
      next: ({ doctor, prescriptions, medicines }) => {
        this.doctor.set(doctor);
        this.prescriptions.set(prescriptions);
        this.medicines.set(medicines);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load prescriptions.');
        this.loading.set(false);
      },
    });
  }

  patientNameFor(medicalRecordId: number) {
    return this.doctor()?.medicalRecords.find(r => r.id === medicalRecordId)?.patientName ?? '—';
  }

  startCreate() {
    this.editingId.set(null);
    this.headerForm.reset({ medicalRecordId: 0, issueDate: '', instruction: '' });
    this.draftItems.set([]);
  }

  startEdit(prescription: Prescription) {
    this.editingId.set(prescription.id);
    this.headerForm.setValue({
      medicalRecordId: prescription.medicalRecordId,
      issueDate: prescription.issueDate.slice(0, 10),
      instruction: prescription.instruction,
    });
    this.editItemId.set(null);
    this.newItemDraft.set(emptyDraftItem(this.medicines()));
  }

  cancelEdit() {
    this.startCreate();
  }

  addDraftItem() {
    this.draftItems.update(items => [...items, emptyDraftItem(this.medicines())]);
  }

  removeDraftItem(index: number) {
    this.draftItems.update(items => items.filter((_, i) => i !== index));
  }

  updateDraftItem(index: number, patch: Partial<DraftItem>) {
    this.draftItems.update(items => items.map((item, i) => (i === index ? { ...item, ...patch } : item)));
  }

  submitHeader() {
    if (this.headerForm.invalid) {
      this.headerForm.markAllAsTouched();
      return;
    }

    const { medicalRecordId, issueDate, instruction } = this.headerForm.getRawValue();
    const id = this.editingId();
    this.saving.set(true);
    this.errorMessage.set(null);

    if (id) {
      this.prescriptionService.update(id, { issueDate, instruction }).subscribe({
        next: () => {
          this.saving.set(false);
          this.cancelEdit();
          this.refresh();
        },
        error: err => {
          this.saving.set(false);
          this.errorMessage.set(err.error?.message ?? 'Failed to update prescription.');
        },
      });
      return;
    }

    this.prescriptionService.create({ medicalRecordId, issueDate, instruction }).subscribe({
      next: created => {
        const items = this.draftItems();
        if (items.length === 0) {
          this.saving.set(false);
          this.cancelEdit();
          this.refresh();
          return;
        }

        forkJoin(
          items.map(item => this.prescriptionItemService.create({ prescriptionId: created.id, ...item })),
        ).subscribe({
          next: () => {
            this.saving.set(false);
            this.cancelEdit();
            this.refresh();
          },
          error: err => {
            this.saving.set(false);
            this.errorMessage.set(err.error?.message ?? 'Prescription created, but some items failed to save.');
            this.refresh();
          },
        });
      },
      error: err => {
        this.saving.set(false);
        this.errorMessage.set(err.error?.message ?? 'Failed to create prescription.');
      },
    });
  }

  deletePrescription(prescription: Prescription) {
    if (!confirm('Delete this prescription and all its items?')) return;

    this.prescriptionService.delete(prescription.id).subscribe({
      next: () => {
        if (this.editingId() === prescription.id) this.cancelEdit();
        this.refresh();
      },
      error: err => this.errorMessage.set(err.error?.message ?? 'Failed to delete prescription.'),
    });
  }

  startEditItem(item: PrescriptionItemSummary) {
    this.editItemId.set(item.id);
    this.editItemDraft.set({
      medicineId: item.medicineId,
      dosage: item.dosage,
      quantity: item.quantity,
      frequency: item.frequency,
      durationDays: item.durationDays,
    });
  }

  cancelEditItem() {
    this.editItemId.set(null);
  }

  saveEditItem(item: PrescriptionItemSummary) {
    this.prescriptionItemService.update(item.id, this.editItemDraft()).subscribe({
      next: () => {
        this.editItemId.set(null);
        this.refresh();
      },
      error: err => this.errorMessage.set(err.error?.message ?? 'Failed to update item.'),
    });
  }

  deleteItem(item: PrescriptionItemSummary) {
    if (!confirm(`Remove ${item.medicineName} from this prescription?`)) return;

    this.prescriptionItemService.delete(item.id).subscribe({
      next: () => this.refresh(),
      error: err => this.errorMessage.set(err.error?.message ?? 'Failed to remove item.'),
    });
  }

  addItemToExisting(prescription: Prescription) {
    const draft = this.newItemDraft();
    if (!draft.dosage.trim() || !draft.frequency.trim()) return;

    this.prescriptionItemService.create({ prescriptionId: prescription.id, ...draft }).subscribe({
      next: () => {
        this.newItemDraft.set(emptyDraftItem(this.medicines()));
        this.refresh();
      },
      error: err => this.errorMessage.set(err.error?.message ?? 'Failed to add item.'),
    });
  }
}
