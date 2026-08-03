import { Injectable, inject } from '@angular/core';
import { ApiService } from '../http/api.service';
import { Department, DepartmentRequest } from '../../models/app.models';

@Injectable({ providedIn: 'root' })
export class DepartmentService {
  private api = inject(ApiService);

  getAll() {
    return this.api.get<Department[]>('/department');
  }

  create(request: DepartmentRequest) {
    return this.api.post<Department>('/department', request);
  }

  update(id: number, request: DepartmentRequest) {
    return this.api.put<Department>(`/department/${id}`, request);
  }

  delete(id: number) {
    return this.api.delete(`/department/${id}`);
  }
}
