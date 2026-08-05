import { Injectable, inject } from '@angular/core';
import { ApiService } from '../http/api.service';
import { CreateStayRequest, Stay } from '../../models/app.models';

@Injectable({ providedIn: 'root' })
export class StayService {
  private api = inject(ApiService);

  getAll() {
    return this.api.get<Stay[]>('/stay');
  }

  getById(id: number) {
    return this.api.get<Stay>(`/stay/${id}`);
  }

  create(request: CreateStayRequest) {
    return this.api.post<Stay>('/stay', request);
  }

  checkout(id: number) {
    return this.api.patch<Stay>(`/stay/${id}/checkout`, {});
  }

  delete(id: number) {
    return this.api.delete(`/stay/${id}`);
  }
}
