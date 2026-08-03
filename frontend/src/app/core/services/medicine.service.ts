import { Injectable, inject } from '@angular/core';
import { ApiService } from '../http/api.service';
import { Medicine, MedicineRequest } from '../../models/app.models';

@Injectable({ providedIn: 'root' })
export class MedicineService {
  private api = inject(ApiService);

  getAll() {
    return this.api.get<Medicine[]>('/medicine');
  }

  create(request: MedicineRequest) {
    return this.api.post<Medicine>('/medicine', request);
  }

  update(id: number, request: MedicineRequest) {
    return this.api.put<Medicine>(`/medicine/${id}`, request);
  }

  updateStock(id: number, stockQt: number) {
    return this.api.patch<Medicine>(`/medicine/${id}/stock`, { stockQt });
  }

  delete(id: number) {
    return this.api.delete(`/medicine/${id}`);
  }
}
