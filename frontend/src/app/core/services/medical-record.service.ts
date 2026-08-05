import { Injectable, inject } from '@angular/core';
import { ApiService } from '../http/api.service';
import { CreateMedicalRecordRequest, MedicalRecord, UpdateMedicalRecordRequest } from '../../models/app.models';

@Injectable({ providedIn: 'root' })
export class MedicalRecordService {
  private api = inject(ApiService);

  getAll() {
    return this.api.get<MedicalRecord[]>('/medicalrecord');
  }

  getMine() {
    return this.api.get<MedicalRecord[]>('/medicalrecord/me');
  }

  getById(id: number) {
    return this.api.get<MedicalRecord>(`/medicalrecord/${id}`);
  }

  create(request: CreateMedicalRecordRequest) {
    return this.api.post<MedicalRecord>('/medicalrecord', request);
  }

  update(id: number, request: UpdateMedicalRecordRequest) {
    return this.api.put<MedicalRecord>(`/medicalrecord/${id}`, request);
  }

  delete(id: number) {
    return this.api.delete(`/medicalrecord/${id}`);
  }
}
