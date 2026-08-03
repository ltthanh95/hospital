import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { roleGuard } from './core/auth/role.guard';
import { LoginComponent } from './auth/login/login';
import { RegisterComponent } from './auth/register/register';
import { DashboardComponent } from './dashboard/dashboard';
import { UnauthorizedComponent } from './shared/unauthorized/unauthorized';
import { AdminLayoutComponent } from './admin/admin-layout/admin-layout';
import { DepartmentsComponent } from './admin/departments/departments';
import { MedicinesComponent } from './admin/medicines/medicines';
import { InvoicesComponent } from './admin/invoices/invoices';
import { RevenueComponent } from './admin/revenue/revenue';
import { PatientChatComponent } from './chat/patient-chat/patient-chat';
import { DoctorChatComponent } from './chat/doctor-chat/doctor-chat';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'unauthorized', component: UnauthorizedComponent },
  {
    path: 'dashboard',
    component: DashboardComponent,
    canActivate: [authGuard],
  },
  {
    path: 'chat',
    component: PatientChatComponent,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['PATIENT'] },
  },
  {
    path: 'doctor/chat',
    component: DoctorChatComponent,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['DOCTOR'] },
  },
  {
    path: 'admin',
    component: AdminLayoutComponent,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN'] },
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'departments' },
      { path: 'departments', component: DepartmentsComponent },
      { path: 'medicines', component: MedicinesComponent },
      { path: 'invoices', component: InvoicesComponent },
      { path: 'revenue', component: RevenueComponent },
    ],
  },
];
