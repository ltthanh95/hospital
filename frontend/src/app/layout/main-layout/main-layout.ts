import { Component, computed, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { LogoutButtonComponent } from '../../auth/logout/logout-button';
import { ChatBubbleComponent } from '../chat-bubble/chat-bubble';
import { Role } from '../../models/app.models';

interface NavItem {
  label: string;
  path: string;
  roles: Role[];
}

const NAV_ITEMS: NavItem[] = [
  { label: 'Dashboard', path: '/dashboard', roles: ['ADMIN', 'PATIENT', 'DOCTOR'] },

  // Patient
  { label: 'Book Appointment', path: '/book-appointment', roles: ['PATIENT'] },
  { label: 'My Appointments', path: '/my-appointments', roles: ['PATIENT'] },
  { label: 'My Records', path: '/my-records', roles: ['PATIENT'] },
  { label: 'My Invoices', path: '/my-invoices', roles: ['PATIENT'] },
  { label: 'My Profile', path: '/my-profile', roles: ['PATIENT'] },
  { label: 'Patient Chat', path: '/chat', roles: ['PATIENT'] },

  // Doctor
  { label: 'My Patients', path: '/my-patients', roles: ['DOCTOR'] },
  { label: 'Prescriptions', path: '/prescriptions', roles: ['DOCTOR'] },
  { label: 'Medicines', path: '/medicines', roles: ['DOCTOR'] },
  { label: 'Rooms', path: '/rooms', roles: ['DOCTOR'] },
  { label: 'Stays', path: '/stays', roles: ['DOCTOR'] },
  { label: 'Doctor Chat', path: '/doctor/chat', roles: ['DOCTOR'] },

  // Admin
  { label: 'Departments', path: '/departments', roles: ['ADMIN'] },
  { label: 'Doctors', path: '/doctors', roles: ['ADMIN'] },
  { label: 'Patients', path: '/patients', roles: ['ADMIN'] },
  { label: 'Appointments', path: '/appointments', roles: ['ADMIN'] },
  { label: 'Medicines', path: '/medicines', roles: ['ADMIN'] },
  { label: 'Invoices', path: '/invoices', roles: ['ADMIN'] },
  { label: 'Payments', path: '/payments', roles: ['ADMIN'] },
  { label: 'Rooms', path: '/rooms', roles: ['ADMIN'] },
  { label: 'Stays', path: '/stays', roles: ['ADMIN'] },
  { label: 'Revenue', path: '/revenue', roles: ['ADMIN'] },
];

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, RouterOutlet, LogoutButtonComponent, ChatBubbleComponent],
  templateUrl: './main-layout.html',
})
export class MainLayoutComponent {
  auth = inject(AuthService);

  navItems = computed(() => {
    const role = this.auth.user()?.role;
    if (!role) return [];
    return NAV_ITEMS.filter(item => item.roles.includes(role));
  });
}
