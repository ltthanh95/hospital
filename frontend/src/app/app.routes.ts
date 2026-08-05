import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { roleGuard } from './core/auth/role.guard';
import { LoginComponent } from './auth/login/login';
import { RegisterComponent } from './auth/register/register';
import { DashboardComponent } from './dashboard/dashboard';
import { UnauthorizedComponent } from './shared/unauthorized/unauthorized';
import { MainLayoutComponent } from './layout/main-layout/main-layout';
import { DepartmentsComponent } from './admin/departments/departments';
import { MedicinesComponent } from './admin/medicines/medicines';
import { InvoicesComponent } from './admin/invoices/invoices';
import { RevenueComponent } from './admin/revenue/revenue';
import { PatientChatComponent } from './chat/patient-chat/patient-chat';
import { DoctorChatComponent } from './chat/doctor-chat/doctor-chat';
import { AppointmentsComponent } from './features/appointments/appointments';
import { DoctorsComponent } from './features/doctors/doctors';
import { PatientsComponent } from './features/patients/patients';
import { PaymentsComponent } from './features/payments/payments';
import { RoomsComponent } from './features/rooms/rooms';
import { StaysComponent } from './features/stays/stays';
import { MyProfileComponent } from './patient/my-profile/my-profile';
import { BookAppointmentComponent } from './patient/book-appointment/book-appointment';
import { MyAppointmentsComponent } from './patient/my-appointments/my-appointments';
import { MyInvoicesComponent } from './patient/my-invoices/my-invoices';
import { MyRecordsComponent } from './patient/my-records/my-records';
import { MyPatientsComponent } from './doctor/my-patients/my-patients';
import { DoctorPrescriptionsComponent } from './doctor/prescriptions/prescriptions';

const ALL_ROLES = ['ADMIN', 'PATIENT', 'DOCTOR'];
const ADMIN_DOCTOR = ['ADMIN', 'DOCTOR'];

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'unauthorized', component: UnauthorizedComponent },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        component: DashboardComponent,
        canActivate: [roleGuard],
        data: { roles: ALL_ROLES },
      },

      // Patient
      {
        path: 'book-appointment',
        component: BookAppointmentComponent,
        canActivate: [roleGuard],
        data: { roles: ['PATIENT'] },
      },
      {
        path: 'my-appointments',
        component: MyAppointmentsComponent,
        canActivate: [roleGuard],
        data: { roles: ['PATIENT'] },
      },
      {
        path: 'my-records',
        component: MyRecordsComponent,
        canActivate: [roleGuard],
        data: { roles: ['PATIENT'] },
      },
      {
        path: 'my-invoices',
        component: MyInvoicesComponent,
        canActivate: [roleGuard],
        data: { roles: ['PATIENT'] },
      },
      {
        path: 'my-profile',
        component: MyProfileComponent,
        canActivate: [roleGuard],
        data: { roles: ['PATIENT'] },
      },

      // Doctor
      {
        path: 'my-patients',
        component: MyPatientsComponent,
        canActivate: [roleGuard],
        data: { roles: ['DOCTOR'] },
      },
      {
        path: 'prescriptions',
        component: DoctorPrescriptionsComponent,
        canActivate: [roleGuard],
        data: { roles: ['DOCTOR'] },
      },

      // Shared admin/doctor
      {
        path: 'medicines',
        component: MedicinesComponent,
        canActivate: [roleGuard],
        data: { roles: ADMIN_DOCTOR },
      },
      {
        path: 'rooms',
        component: RoomsComponent,
        canActivate: [roleGuard],
        data: { roles: ADMIN_DOCTOR },
      },
      {
        path: 'stays',
        component: StaysComponent,
        canActivate: [roleGuard],
        data: { roles: ADMIN_DOCTOR },
      },

      // Admin
      {
        path: 'departments',
        component: DepartmentsComponent,
        canActivate: [roleGuard],
        data: { roles: ['ADMIN'] },
      },
      {
        path: 'doctors',
        component: DoctorsComponent,
        canActivate: [roleGuard],
        data: { roles: ['ADMIN'] },
      },
      {
        path: 'patients',
        component: PatientsComponent,
        canActivate: [roleGuard],
        data: { roles: ['ADMIN'] },
      },
      {
        path: 'appointments',
        component: AppointmentsComponent,
        canActivate: [roleGuard],
        data: { roles: ['ADMIN'] },
      },
      {
        path: 'invoices',
        component: InvoicesComponent,
        canActivate: [roleGuard],
        data: { roles: ['ADMIN'] },
      },
      {
        path: 'payments',
        component: PaymentsComponent,
        canActivate: [roleGuard],
        data: { roles: ['ADMIN'] },
      },
      {
        path: 'revenue',
        component: RevenueComponent,
        canActivate: [roleGuard],
        data: { roles: ['ADMIN'] },
      },

      // Chat
      {
        path: 'chat',
        component: PatientChatComponent,
        canActivate: [roleGuard],
        data: { roles: ['PATIENT'] },
      },
      {
        path: 'doctor/chat',
        component: DoctorChatComponent,
        canActivate: [roleGuard],
        data: { roles: ['DOCTOR'] },
      },
    ],
  },
];
