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