import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../core/auth/auth.service';
import { LogoutButtonComponent } from '../auth/logout/logout-button';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [LogoutButtonComponent, RouterLink],
  templateUrl: "dashboard.html"
})
export class DashboardComponent {
  constructor(public auth: AuthService) {}
}
