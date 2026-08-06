import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { forkJoin } from 'rxjs';
import { RevenueService } from '../../core/services/revenue.service';
import { InvoiceService } from '../../core/services/invoice.service';
import { PaymentService } from '../../core/services/payment.service';
import { Invoice, Payment, PaymentStatus, RevenueReport } from '../../models/app.models';
import { BarChartComponent, ChartSeries } from '../../shared/charts/bar-chart';
import { LineChartComponent } from '../../shared/charts/line-chart';

interface MonthBucket {
  key: string;
  label: string;
}

const STATUS_KEYS: PaymentStatus[] = ['COMPLETED', 'PENDING', 'FAILED', 'REFUNDED'];
const STATUS_LABELS = ['Completed', 'Pending', 'Failed', 'Refunded'];
const STATUS_COLORS = ['#0ca30c', '#fab219', '#d03b3b', '#4a3aa7'];

function monthKey(date: Date) {
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}`;
}

function buildTrailingMonths(count: number): MonthBucket[] {
  const now = new Date();
  const months: MonthBucket[] = [];
  for (let i = count - 1; i >= 0; i--) {
    const d = new Date(now.getFullYear(), now.getMonth() - i, 1);
    months.push({ key: monthKey(d), label: d.toLocaleDateString(undefined, { month: 'short', year: '2-digit' }) });
  }
  return months;
}

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, BarChartComponent, LineChartComponent],
  templateUrl: './admin-dashboard.html',
})
export class AdminDashboardComponent {
  private revenueService = inject(RevenueService);
  private invoiceService = inject(InvoiceService);
  private paymentService = inject(PaymentService);

  loading = signal(true);
  errorMessage = signal<string | null>(null);

  revenue = signal<RevenueReport | null>(null);
  invoices = signal<Invoice[]>([]);
  payments = signal<Payment[]>([]);

  readonly months = buildTrailingMonths(12);
  readonly monthLabels = this.months.map(m => m.label);
  readonly statusLabels = STATUS_LABELS;
  readonly statusColors = STATUS_COLORS;

  formatAmount = (value: number) =>
    value.toLocaleString(undefined, { minimumFractionDigits: 0, maximumFractionDigits: 0 });

  invoicedByMonth = computed(() => {
    const totals = new Map(this.months.map(m => [m.key, 0]));
    for (const invoice of this.invoices()) {
      const key = monthKey(new Date(invoice.issuedDate));
      if (totals.has(key)) totals.set(key, totals.get(key)! + invoice.total);
    }
    return this.months.map(m => totals.get(m.key) ?? 0);
  });

  collectedByMonth = computed(() => {
    const totals = new Map(this.months.map(m => [m.key, 0]));
    for (const payment of this.payments()) {
      if (payment.status !== 'COMPLETED') continue;
      const key = monthKey(new Date(payment.paymentDate));
      if (totals.has(key)) totals.set(key, totals.get(key)! + payment.amount);
    }
    return this.months.map(m => totals.get(m.key) ?? 0);
  });

  cumulativeRevenue = computed(() => {
    let running = 0;
    return this.collectedByMonth().map(v => (running += v));
  });

  monthlySeries = computed<ChartSeries[]>(() => [
    { name: 'Invoiced', color: '#2a78d6', values: this.invoicedByMonth() },
    { name: 'Collected', color: '#eb6834', values: this.collectedByMonth() },
  ]);

  statusAmounts = computed(() =>
    STATUS_KEYS.map(status =>
      this.payments()
        .filter(p => p.status === status)
        .reduce((sum, p) => sum + p.amount, 0),
    ),
  );

  statusSeries = computed<ChartSeries[]>(() => [{ name: 'Amount', color: '#2a78d6', values: this.statusAmounts() }]);

  totalInvoiced = computed(() => this.invoices().reduce((sum, i) => sum + i.total, 0));

  constructor() {
    forkJoin({
      revenue: this.revenueService.get(),
      invoices: this.invoiceService.getAll(),
      payments: this.paymentService.getAll(),
    }).subscribe({
      next: ({ revenue, invoices, payments }) => {
        this.revenue.set(revenue);
        this.invoices.set(invoices);
        this.payments.set(payments);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load dashboard data.');
        this.loading.set(false);
      },
    });
  }
}
