import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PaymentService } from '../../core/services/payment.service';
import { Payment } from '../../models/app.models';

@Component({
  selector: 'app-payments',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './payments.html',
})
export class PaymentsComponent {
  private paymentService = inject(PaymentService);

  payments = signal<Payment[]>([]);
  loading = signal(false);
  errorMessage = signal<string | null>(null);

  constructor() {
    this.refresh();
  }

  refresh() {
    this.loading.set(true);
    this.errorMessage.set(null);
    this.paymentService.getAll().subscribe({
      next: payments => {
        this.payments.set(payments);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load payments.');
        this.loading.set(false);
      },
    });
  }

  statusClass(status: Payment['status']) {
    switch (status) {
      case 'COMPLETED':
        return 'bg-emerald-50 text-emerald-700';
      case 'FAILED':
        return 'bg-red-50 text-red-700';
      case 'REFUNDED':
        return 'bg-slate-100 text-slate-600';
      default:
        return 'bg-amber-50 text-amber-700';
    }
  }
}
