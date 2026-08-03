import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RevenueService } from '../../core/services/revenue.service';
import { RevenueReport } from '../../models/app.models';

@Component({
  selector: 'app-revenue',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './revenue.html',
})
export class RevenueComponent {
  private revenueService = inject(RevenueService);

  report = signal<RevenueReport | null>(null);
  loading = signal(false);
  errorMessage = signal<string | null>(null);

  constructor() {
    this.refresh();
  }

  refresh() {
    this.loading.set(true);
    this.errorMessage.set(null);
    this.revenueService.get().subscribe({
      next: report => {
        this.report.set(report);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load revenue report.');
        this.loading.set(false);
      },
    });
  }
}
