import { Component, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-logout-button',
  standalone: true,
  template: `
    <button
      type="button"
      (click)="logout()"
      [disabled]="loading()"
      class="rounded-lg border border-slate-300 px-3 py-1.5 text-sm font-medium text-slate-700 hover:bg-slate-100 disabled:opacity-60 transition-colors"
    >
      {{ loading() ? 'Signing out...' : 'Sign out' }}
    </button>
  `,
})
export class LogoutButtonComponent {
  loading = signal(false);

  constructor(
    private auth: AuthService,
    private router: Router,
  ) {}

  logout() {
    this.loading.set(true);
    this.auth.logout().subscribe({
      next: () => this.router.navigateByUrl('/login'),
      error: () => this.router.navigateByUrl('/login'),
    });
  }
}
