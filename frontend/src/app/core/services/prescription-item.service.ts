import { Injectable, inject } from '@angular/core';
import { ApiService } from '../http/api.service';
import { CreatePrescriptionItemRequest, PrescriptionItem, UpdatePrescriptionItemRequest } from '../../models/app.models';

@Injectable({ providedIn: 'root' })
export class PrescriptionItemService {
  private api = inject(ApiService);

  getAll() {
    return this.api.get<PrescriptionItem[]>('/prescriptionitem');
  }

  getById(id: number) {
    return this.api.get<PrescriptionItem>(`/prescriptionitem/${id}`);
  }

  create(request: CreatePrescriptionItemRequest) {
    return this.api.post<PrescriptionItem>('/prescriptionitem', request);
  }

  update(id: number, request: UpdatePrescriptionItemRequest) {
    return this.api.put<PrescriptionItem>(`/prescriptionitem/${id}`, request);
  }

  delete(id: number) {
    return this.api.delete(`/prescriptionitem/${id}`);
  }
}
