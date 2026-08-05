import { Injectable, inject } from '@angular/core';
import { ApiService } from '../http/api.service';
import { CreatePaymentRequest, Payment } from '../../models/app.models';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private api = inject(ApiService);

  getAll() {
    return this.api.get<Payment[]>('/payment');
  }

  getById(id: number) {
    return this.api.get<Payment>(`/payment/${id}`);
  }

  create(request: CreatePaymentRequest) {
    return this.api.post<Payment>('/payment', request);
  }
}
