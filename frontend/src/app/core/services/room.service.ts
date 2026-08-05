import { Injectable, inject } from '@angular/core';
import { ApiService } from '../http/api.service';
import { Room, RoomRequest } from '../../models/app.models';

@Injectable({ providedIn: 'root' })
export class RoomService {
  private api = inject(ApiService);

  getAll() {
    return this.api.get<Room[]>('/room');
  }

  getById(id: number) {
    return this.api.get<Room>(`/room/${id}`);
  }

  create(request: RoomRequest) {
    return this.api.post<Room>('/room', request);
  }

  update(id: number, request: RoomRequest) {
    return this.api.put<Room>(`/room/${id}`, request);
  }

  delete(id: number) {
    return this.api.delete(`/room/${id}`);
  }
}
