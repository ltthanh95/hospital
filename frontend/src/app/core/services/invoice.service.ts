import { Injectable, inject } from '@angular/core';
import { ApiService } from '../http/api.service';
import { Invoice } from '../../models/app.models';

@Injectable({ providedIn: 'root' })
export class InvoiceService {
  private api = inject(ApiService);

  getAll() {
    return this.api.get<Invoice[]>('/invoice');
  }

  getMine() {
    return this.api.get<Invoice[]>('/invoice/me');
  }

  generate(patientId: number) {
    return this.api.post<Invoice>('/invoice/generate', { patientId });
  }

  delete(id: number) {
    return this.api.delete(`/invoice/${id}`);
  }
}
