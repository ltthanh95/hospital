import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { LogoutButtonComponent } from '../../auth/logout/logout-button';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, RouterOutlet, LogoutButtonComponent],
  templateUrl: './admin-layout.html',
})
export class AdminLayoutComponent {}
