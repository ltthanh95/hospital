import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { DepartmentService } from '../../core/services/department.service';
import { Department } from '../../models/app.models';

@Component({
  selector: 'app-departments',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './departments.html',
})
export class DepartmentsComponent {
  private fb = inject(FormBuilder).nonNullable;
  private departmentService = inject(DepartmentService);

  departments = signal<Department[]>([]);
  loading = signal(false);
  errorMessage = signal<string | null>(null);
  editingId = signal<number | null>(null);

  form = this.fb.group({
    name: ['', Validators.required],
  });

  constructor() {
    this.refresh();
  }

  refresh() {
    this.loading.set(true);
    this.departmentService.getAll().subscribe({
      next: departments => {
        this.departments.set(departments);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load departments.');
        this.loading.set(false);
      },
    });
  }

  startCreate() {
    this.editingId.set(null);
    this.form.reset({ name: '' });
  }

  startEdit(department: Department) {
    this.editingId.set(department.id);
    this.form.setValue({ name: department.name });
  }

  cancelEdit() {
    this.editingId.set(null);
    this.form.reset({ name: '' });
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const request = this.form.getRawValue();
    const id = this.editingId();
    this.errorMessage.set(null);

    const save$ = id ? this.departmentService.update(id, request) : this.departmentService.create(request);

    save$.subscribe({
      next: () => {
        this.cancelEdit();
        this.refresh();
      },
      error: err => this.errorMessage.set(err.error?.message ?? 'Save failed.'),
    });
  }

  remove(department: Department) {
    if (!confirm(`Delete department "${department.name}"?`)) return;

    this.departmentService.delete(department.id).subscribe({
      next: () => this.refresh(),
      error: err => this.errorMessage.set(err.error?.message ?? 'Delete failed.'),
    });
  }
}
