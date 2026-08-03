import { Injectable, inject } from '@angular/core';
import { ApiService } from '../http/api.service';
import { RevenueReport } from '../../models/app.models';

@Injectable({ providedIn: 'root' })
export class RevenueService {
  private api = inject(ApiService);

  get() {
    return this.api.get<RevenueReport>('/revenue');
  }
}
