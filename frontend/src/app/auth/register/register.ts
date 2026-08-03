import { Component, computed, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { RegisterRequest } from '../../models/app.models';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.html',
})
export class RegisterComponent {
  private fb = inject(FormBuilder).nonNullable;
  private auth = inject(AuthService);
  private router = inject(Router);

  form = this.fb.group({
    username: ['', Validators.required],
    password: ['', [Validators.required, Validators.minLength(6)]],
    role: this.fb.control<'PATIENT' | 'DOCTOR' | 'ADMIN'>('PATIENT'),

    patient: this.fb.group({
      fName: ['', Validators.required],
      lName: ['', Validators.required],
      doB: ['', Validators.required],
      gender: this.fb.control<'MALE' | 'FEMALE'>('MALE'),
      address: ['', Validators.required],
      phone: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      bloodType: ['', Validators.required],
      emergencyContact: ['', Validators.required],
    }),

    doctor: this.fb.group({
      fName: ['', Validators.required],
      lName: ['', Validators.required],
      doB: ['', Validators.required],
      gender: this.fb.control<'MALE' | 'FEMALE'>('MALE'),
      address: ['', Validators.required],
      phone: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      licenseNumber: ['', Validators.required],
      specialization: ['', Validators.required],
      consulationFee: [0, [Validators.required, Validators.min(0)]],
      departmentName: ['', Validators.required],
    })
  });

  

  private roleSignal = signal<'PATIENT' | 'DOCTOR' | 'ADMIN'>('PATIENT');
  role = this.roleSignal.asReadonly();
  isPatient = computed(() => this.role() === 'PATIENT');
  isDoctor = computed(() => this.role() === 'DOCTOR');
  isAdmin= computed(() => this.role() === 'ADMIN');
  loading = signal(false);
  errorMessage = signal<string | null>(null);

  constructor() {
    this.form.controls.role.valueChanges.subscribe(value => {
      this.roleSignal.set(value);
      this.syncGroupValidity(value);
    });
    this.syncGroupValidity(this.role());
  }

  private syncGroupValidity(role: 'PATIENT' | 'DOCTOR' | 'ADMIN') {
    const { patient, doctor } = this.form.controls;

    patient.disable({ emitEvent: false });
    doctor.disable({ emitEvent: false });

    if (role === 'PATIENT') patient.enable({ emitEvent: false });
    if (role === 'DOCTOR') doctor.enable({ emitEvent: false });
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const request: RegisterRequest = {
      username: raw.username,
      password: raw.password,
      role: raw.role,
      patient: raw.role === 'PATIENT' ? raw.patient : undefined,
      doctor: raw.role === 'DOCTOR' ? raw.doctor : undefined,
    };

    this.loading.set(true);
    this.errorMessage.set(null);

    this.auth.register(request).subscribe({
      next: () => this.router.navigateByUrl('/dashboard'),
      error: err => {
        this.loading.set(false);
        this.errorMessage.set(err.error?.message ?? 'Registration failed.');
      },
    });
  }
}
