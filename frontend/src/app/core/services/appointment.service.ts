import { Injectable, inject } from '@angular/core';
import { ApiService } from '../http/api.service';
import { Appointment, CreateAppointmentRequest, RescheduleAppointmentRequest } from '../../models/app.models';

@Injectable({ providedIn: 'root' })
export class AppointmentService {
  private api = inject(ApiService);

  getAll() {
    return this.api.get<Appointment[]>('/appointment');
  }

  getById(id: number) {
    return this.api.get<Appointment>(`/appointment/${id}`);
  }

  create(request: CreateAppointmentRequest) {
    return this.api.post<Appointment>('/appointment', request);
  }

  reschedule(id: number, request: RescheduleAppointmentRequest) {
    return this.api.put<Appointment>(`/appointment/${id}/reschedule`, request);
  }

  cancel(id: number) {
    return this.api.patch<Appointment>(`/appointment/${id}/cancel`, {});
  }
}
