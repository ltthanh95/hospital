import { Injectable, inject } from '@angular/core';
import { ApiService } from '../http/api.service';
import { CreatePrescriptionRequest, Prescription, UpdatePrescriptionRequest } from '../../models/app.models';

@Injectable({ providedIn: 'root' })
export class PrescriptionService {
  private api = inject(ApiService);

  getAll() {
    return this.api.get<Prescription[]>('/prescription');
  }

  getMine() {
    return this.api.get<Prescription[]>('/prescription/me');
  }

  getById(id: number) {
    return this.api.get<Prescription>(`/prescription/${id}`);
  }

  create(request: CreatePrescriptionRequest) {
    return this.api.post<Prescription>('/prescription', request);
  }

  update(id: number, request: UpdatePrescriptionRequest) {
    return this.api.put<Prescription>(`/prescription/${id}`, request);
  }

  delete(id: number) {
    return this.api.delete(`/prescription/${id}`);
  }
}
