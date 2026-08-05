import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InvoiceService } from '../../core/services/invoice.service';
import { Invoice } from '../../models/app.models';

@Component({
  selector: 'app-my-invoices',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './my-invoices.html',
})
export class MyInvoicesComponent {
  private invoiceService = inject(InvoiceService);

  invoices = signal<Invoice[]>([]);
  loading = signal(true);
  errorMessage = signal<string | null>(null);
  expandedId = signal<number | null>(null);

  constructor() {
    this.invoiceService.getMine().subscribe({
      next: invoices => {
        this.invoices.set(invoices);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load your invoices.');
        this.loading.set(false);
      },
    });
  }

  toggle(invoice: Invoice) {
    this.expandedId.set(this.expandedId() === invoice.id ? null : invoice.id);
  }
}
