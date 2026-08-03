import { Injectable, inject } from '@angular/core';
import { ApiService } from '../http/api.service';
import { PatientSummary } from '../../models/app.models';

@Injectable({ providedIn: 'root' })
export class PatientService {
  private api = inject(ApiService);

  getAll() {
    return this.api.get<PatientSummary[]>('/patient');
  }
}
