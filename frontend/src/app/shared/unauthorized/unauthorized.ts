import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-unauthorized',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-slate-50 px-4">
      <div class="text-center">
        <h1 class="text-2xl font-semibold text-slate-900">Access denied</h1>
        <p class="mt-2 text-sm text-slate-500">You don't have permission to view this page.</p>
        <a routerLink="/dashboard" class="mt-4 inline-block text-blue-600 font-medium hover:underline">
          Back to dashboard
        </a>
      </div>
    </div>
  `,
})
export class UnauthorizedComponent {}
