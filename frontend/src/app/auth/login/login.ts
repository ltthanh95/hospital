import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
})
export class LoginComponent {
  private fb = inject(FormBuilder).nonNullable;
  private auth = inject(AuthService);
  private router = inject(Router);

  form = this.fb.group({
    username: ['', Validators.required],
    password: ['', Validators.required],
  });

  loading = signal(false);
  errorMessage = signal<string | null>(null);

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);

    this.auth.login(this.form.getRawValue() as { username: string; password: string }).subscribe({
      next: () => this.router.navigateByUrl('/dashboard'),
      error: err => {
        this.loading.set(false);
        this.errorMessage.set(err.error?.message ?? 'Invalid username or password.');
      },
    });
  }
}
