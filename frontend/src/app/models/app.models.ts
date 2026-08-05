export type Role = 'ADMIN' | 'PATIENT' | 'DOCTOR';

export interface User {
    username: string;
    role: Role;
}

export interface ApiResponse<T> {
    status: number;
    message: string;
    result: T;
}

export interface LoginRequest {
    username: string;
    password: string;
}

export interface PatientRegistrationDetails {
    fName: string;
    lName: string;
    doB: string;
    gender: 'MALE' | 'FEMALE';
    address: string;
    phone: string;
    email: string;
    bloodType: string;
    emergencyContact: string;
}

export interface DoctorRegistrationDetails {
    fName: string;
    lName: string;
    doB: string;
    gender: 'MALE' | 'FEMALE';
    address: string;
    phone: string;
    email: string;
    licenseNumber: string;
    specialization: string;
    consulationFee: number;
    departmentName: string;
}

export interface RegisterRequest {
    username: string;
    password: string;
    role: Role;
    patient?: PatientRegistrationDetails;
    doctor?: DoctorRegistrationDetails;
}

export interface Department {
    id: number;
    name: string;
    doctorCount: number;
    doctorNames: string[];
}

export interface DepartmentRequest {
    name: string;
}

export interface Medicine {
    id: number;
    name: string;
    manufacturer: string;
    unitPrice: number;
    stockQt: number;
    expiration: string;
}

export interface MedicineRequest {
    name: string;
    manufacturer: string;
    unitPrice: number;
    stockQt: number;
    expiration: string;
}

export interface InvoiceItem {
    id: number;
    description: string;
    quantity: number;
    price: number;
    lineTotal: number;
}

export interface Invoice {
    id: number;
    patientId: number;
    patientName: string;
    issuedDate: string;
    total: number;
    items: InvoiceItem[];
}

export interface RevenueReport {
    totalPatientPayments: number;
    totalDoctorSalaries: number;
    netRevenue: number;
}

export interface PatientSummary {
    id: number;
    fName: string;
    lName: string;
}

export type Gender = 'MALE' | 'FEMALE';
export type PersonStatus = 'ADMISSION' | 'DISCHARGE' | 'ACTIVE' | 'INACTIVE';
export type AppointmentStatus = 'PENDING' | 'CONFIRMED' | 'CANCELLED';
export type PaymentMethod = 'CASH' | 'CARD' | 'BANK_TRANSFER' | 'INSURANCE';
export type PaymentStatus = 'PENDING' | 'COMPLETED' | 'FAILED' | 'REFUNDED';

export interface Appointment {
  id: number;
  schedule: string;
  status: AppointmentStatus;
  reason: string;
  doctorId: number;
  doctorName: string;
  patientId: number;
  patientName: string;
  medicalRecordId: number | null;
}

export interface CreateAppointmentRequest {
  patientId?: number;
  doctorId?: number;
  schedule: string;
  reason: string;
}

export interface RescheduleAppointmentRequest {
  schedule: string;
}

export interface DoctorAppointmentSummary {
  id: number;
  schedule: string;
  status: AppointmentStatus;
  reason: string;
  patientId: number;
  patientName: string;
}

export interface DoctorMedicalRecordSummary {
  id: number;
  visit: string;
  diagnosis: string;
  notes: string | null;
  patientName: string;
}

export interface Doctor {
  id: number;
  username: string;
  fName: string;
  lName: string;
  doB: string;
  gender: Gender;
  address: string;
  phone: string;
  email: string;
  licenseNumber: string;
  specialization: string;
  consulationFee: number;
  salary: number;
  status: PersonStatus;
  departmentName: string | null;
  appointments: DoctorAppointmentSummary[];
  medicalRecords: DoctorMedicalRecordSummary[];
}

export interface DoctorUpdateDetails {
  fName: string;
  lName: string;
  doB: string;
  gender: Gender;
  address: string;
  phone: string;
  email: string;
  licenseNumber: string;
  specialization: string;
  consulationFee: number;
  departmentName: string;
}

export interface PatientAppointmentSummary {
  id: number;
  schedule: string;
  status: AppointmentStatus;
  reason: string;
  doctorName: string;
}

export interface PatientMedicalRecordSummary {
  id: number;
  visit: string;
  diagnosis: string;
  notes: string | null;
  doctorName: string;
}

export interface Patient {
  id: number;
  username: string;
  fName: string;
  lName: string;
  doB: string;
  gender: Gender;
  address: string;
  phone: string;
  email: string;
  bloodType: string;
  emergencyContact: string;
  admissionDate: string;
  dischargeDate: string | null;
  status: PersonStatus;
  appointments: PatientAppointmentSummary[];
  medicalRecords: PatientMedicalRecordSummary[];
}

export interface PatientUpdateDetails {
  fName: string;
  lName: string;
  doB: string;
  gender: Gender;
  address: string;
  phone: string;
  email: string;
  bloodType: string;
  emergencyContact: string;
}

export interface MedicalRecord {
  id: number;
  doctorId: number;
  doctorName: string;
  patientId: number;
  patientName: string;
  appointmentId: number | null;
  visit: string;
  diagnosis: string;
  notes: string | null;
  prescriptionIds: number[];
}

export interface CreateMedicalRecordRequest {
  patientId: number;
  appointmentId?: number;
  visit: string;
  diagnosis: string;
  notes?: string;
}

export interface UpdateMedicalRecordRequest {
  visit: string;
  diagnosis: string;
  notes?: string;
}

export interface Payment {
  id: number;
  invoiceId: number;
  patientId: number;
  patientName: string;
  amount: number;
  paymentDate: string;
  method: PaymentMethod;
  status: PaymentStatus;
}

export interface CreatePaymentRequest {
  invoiceId: number;
  amount: number;
  method: PaymentMethod;
}

export interface PrescriptionItemSummary {
  id: number;
  medicineId: number;
  medicineName: string;
  dosage: string;
  quantity: number;
  frequency: string;
  durationDays: number;
}

export interface Prescription {
  id: number;
  medicalRecordId: number;
  issueDate: string;
  instruction: string;
  prescriptionItemIds: number[];
  items: PrescriptionItemSummary[];
}

export interface CreatePrescriptionRequest {
  medicalRecordId: number;
  issueDate: string;
  instruction: string;
}

export interface UpdatePrescriptionRequest {
  issueDate: string;
  instruction: string;
}

export interface PrescriptionItem {
  id: number;
  prescriptionId: number;
  medicineId: number;
  medicineName: string;
  dosage: string;
  quantity: number;
  frequency: string;
  durationDays: number;
}

export interface CreatePrescriptionItemRequest {
  prescriptionId: number;
  medicineId: number;
  dosage: string;
  quantity: number;
  frequency: string;
  durationDays: number;
}

export interface UpdatePrescriptionItemRequest {
  medicineId: number;
  dosage: string;
  quantity: number;
  frequency: string;
  durationDays: number;
}

export interface Room {
  id: number;
  roomNumber: string;
  type: string;
  capacity: number;
  currentOccupancy: number;
}

export interface RoomRequest {
  roomNumber: string;
  type: string;
  capacity: number;
}

export interface Stay {
  id: number;
  patientId: number;
  patientName: string;
  roomId: number;
  roomNumber: string;
  checkIn: string;
  checkOut: string | null;
  nights: number;
}

export interface CreateStayRequest {
  patientId: number;
  roomId: number;
  checkIn: string;
}

export type ChatMode = 'BOT' | 'WAITING_FOR_DOCTOR' | 'LIVE';
export type ChatSenderRole = 'PATIENT' | 'DOCTOR' | 'BOT' | 'SYSTEM';

export interface ChatMessage {
    id: number;
    senderRole: ChatSenderRole;
    senderUserId: number | null;
    content: string;
    sentAt: string;
}

export interface ChatSession {
    id: number;
    patientId: number;
    patientName: string;
    doctorId: number | null;
    doctorName: string | null;
    mode: ChatMode;
    createdAt: string;
    messages: ChatMessage[];
}