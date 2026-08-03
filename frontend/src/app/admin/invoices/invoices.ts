import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { InvoiceService } from '../../core/services/invoice.service';
import { PatientService } from '../../core/services/patient.service';
import { Invoice, PatientSummary } from '../../models/app.models';

@Component({
  selector: 'app-invoices',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './invoices.html',
})
export class InvoicesComponent {
  private fb = inject(FormBuilder).nonNullable;
  private invoiceService = inject(InvoiceService);
  private patientService = inject(PatientService);

  invoices = signal<Invoice[]>([]);
  patients = signal<PatientSummary[]>([]);
  loading = signal(false);
  generating = signal(false);
  errorMessage = signal<string | null>(null);
  expandedId = signal<number | null>(null);

  form = this.fb.group({
    patientId: [0, [Validators.required, Validators.min(1)]],
  });

  constructor() {
    this.refresh();
    this.patientService.getAll().subscribe({
      next: patients => this.patients.set(patients),
      error: () => this.errorMessage.set('Failed to load patients.'),
    });
  }

  refresh() {
    this.loading.set(true);
    this.invoiceService.getAll().subscribe({
      next: invoices => {
        this.invoices.set(invoices);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load invoices.');
        this.loading.set(false);
      },
    });
  }

  toggleExpand(invoice: Invoice) {
    this.expandedId.set(this.expandedId() === invoice.id ? null : invoice.id);
  }

  generate() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.generating.set(true);
    this.errorMessage.set(null);

    this.invoiceService.generate(this.form.getRawValue().patientId).subscribe({
      next: () => {
        this.generating.set(false);
        this.refresh();
      },
      error: err => {
        this.generating.set(false);
        this.errorMessage.set(err.error?.message ?? 'Failed to generate invoice.');
      },
    });
  }

  remove(invoice: Invoice) {
    if (!confirm(`Delete invoice #${invoice.id} for ${invoice.patientName}?`)) return;

    this.invoiceService.delete(invoice.id).subscribe({
      next: () => this.refresh(),
      error: err => this.errorMessage.set(err.error?.message ?? 'Delete failed.'),
    });
  }
}
