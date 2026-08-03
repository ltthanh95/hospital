import { Injectable, signal, computed } from '@angular/core';
import { catchError, of, tap } from 'rxjs';
import { ApiService } from '../http/api.service';
import { LoginRequest, RegisterRequest, User } from '../../models/app.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private currentUser = signal<User | null>(null);
  isLoggedIn = computed(() => !!this.currentUser());
  user = computed(() => this.currentUser());

  private initPromise: Promise<void> | null = null;

  constructor(private api: ApiService) {}

  ensureInitialized(): Promise<void> {
    if (!this.initPromise) {
      this.initPromise = new Promise(resolve => {
        this.fetchCurrentUser()
          .pipe(catchError(() => of(null)))
          .subscribe(() => resolve());
      });
    }
    return this.initPromise;
  }

  login(credentials: LoginRequest) {
    return this.api
      .post<User>('/auth/login', credentials)
      .pipe(tap(user => this.currentUser.set(user)));
  }

  register(request: RegisterRequest) {
    return this.api
      .post<User>('/auth/register', request)
      .pipe(tap(user => this.currentUser.set(user)));
  }

  logout() {
    return this.api.post('/auth/logout', {}).pipe(tap(() => this.currentUser.set(null)));
  }

  fetchCurrentUser() {
    return this.api.get<User>('/auth/me').pipe(tap(user => this.currentUser.set(user)));
  }
}
