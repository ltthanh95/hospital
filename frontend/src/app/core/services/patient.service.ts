import { Injectable, inject } from '@angular/core';
import { ApiService } from '../http/api.service';
import { Patient, PatientSummary, PatientUpdateDetails } from '../../models/app.models';

@Injectable({ providedIn: 'root' })
export class PatientService {
  private api = inject(ApiService);

  getAll() {
    return this.api.get<PatientSummary[]>('/patient');
  }

  getAllDetailed() {
    return this.api.get<Patient[]>('/patient');
  }

  getMe() {
    return this.api.get<Patient>('/patient/me');
  }

  getById(id: number) {
    return this.api.get<Patient>(`/patient/${id}`);
  }

  update(id: number, request: PatientUpdateDetails) {
    return this.api.put<Patient>(`/patient/${id}`, request);
  }

  delete(id: number) {
    return this.api.delete(`/patient/${id}`);
  }

  updateStatus(id: number, status: 'ADMISSION' | 'DISCHARGE') {
    return this.api.patch<Patient>(`/patient/${id}/status`, { status });
  }
}
