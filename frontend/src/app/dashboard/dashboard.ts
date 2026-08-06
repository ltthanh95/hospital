import { Component } from '@angular/core';
import { AuthService } from '../core/auth/auth.service';
import { PatientDashboardComponent } from '../patient/patient-dashboard/patient-dashboard';
import { DoctorDashboardComponent } from '../doctor/doctor-dashboard/doctor-dashboard';
import { AdminDashboardComponent } from '../admin/admin-dashboard/admin-dashboard';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [PatientDashboardComponent, DoctorDashboardComponent, AdminDashboardComponent],
  templateUrl: 'dashboard.html',
})
export class DashboardComponent {
  constructor(public auth: AuthService) {}
}
