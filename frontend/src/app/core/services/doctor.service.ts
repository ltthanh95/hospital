import { Injectable, inject } from '@angular/core';
import { ApiService } from '../http/api.service';
import { Doctor, DoctorUpdateDetails } from '../../models/app.models';

@Injectable({ providedIn: 'root' })
export class DoctorService {
  private api = inject(ApiService);

  getAll() {
    return this.api.get<Doctor[]>('/doctor');
  }

  getMe() {
    return this.api.get<Doctor>('/doctor/me');
  }

  getById(id: number) {
    return this.api.get<Doctor>(`/doctor/${id}`);
  }

  update(id: number, request: DoctorUpdateDetails) {
    return this.api.put<Doctor>(`/doctor/${id}`, request);
  }

  delete(id: number) {
    return this.api.delete(`/doctor/${id}`);
  }

  updateSalary(id: number, salary: number) {
    return this.api.patch<Doctor>(`/doctor/${id}/salary`, { salary });
  }

  updateStatus(id: number, status: 'ACTIVE' | 'INACTIVE') {
    return this.api.patch<Doctor>(`/doctor/${id}/status`, { status });
  }
}
