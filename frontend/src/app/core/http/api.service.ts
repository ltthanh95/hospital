// api.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { ApiResponse } from '../../models/app.models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  get<T>(path: string, params?: any): Observable<T> {
    return this.http
      .get<ApiResponse<T>>(`${this.baseUrl}${path}`, { params, withCredentials: true })
      .pipe(map(res => res.result));
  }

  post<T>(path: string, body: any): Observable<T> {
    return this.http
      .post<ApiResponse<T>>(`${this.baseUrl}${path}`, body, { withCredentials: true })
      .pipe(map(res => res.result));
  }

  put<T>(path: string, body: any): Observable<T> {
    return this.http
      .put<ApiResponse<T>>(`${this.baseUrl}${path}`, body, { withCredentials: true })
      .pipe(map(res => res.result));
  }

  patch<T>(path: string, body: any): Observable<T> {
    return this.http
      .patch<ApiResponse<T>>(`${this.baseUrl}${path}`, body, { withCredentials: true })
      .pipe(map(res => res.result));
  }

  delete<T>(path: string): Observable<T> {
    return this.http
      .delete<ApiResponse<T>>(`${this.baseUrl}${path}`, { withCredentials: true })
      .pipe(map(res => res.result));
  }
}