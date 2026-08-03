# Hospital Management System

A REST + real-time API for managing a small hospital's operations: patients,
doctors, appointments, medical records, prescriptions, pharmacy inventory,
room/stay assignment, billing, and an AI chatbot with live doctor handoff.

Built with **ASP.NET Core 9** (Web API), **Entity Framework Core** (SQL
Server), a hand-rolled mediator pattern for request/handler separation, JWT
authentication stored in an httpOnly cookie, and **SignalR** for real-time
chat.

## Tech stack

- **Backend:** ASP.NET Core 9 Web API (C#)
- **Database:** SQL Server (via EF Core + migrations)
- **Auth:** JWT, delivered as an httpOnly cookie (not exposed to JS)
- **Real-time:** SignalR (chat + live doctor handoff)
- **AI chatbot:** OpenRouter (OpenAI-compatible chat completions with tool/function calling)
- **Containerization:** Docker Compose (API + SQL Server)

## Functional overview

- **Auth & accounts** — register/login/logout as `ADMIN`, `PATIENT`, or
  `DOCTOR`; role-specific registration details (patient or doctor profile
  created alongside the account).
- **Patients & Doctors** — full CRUD. A patient can edit their own profile;
  a doctor can edit their own profile; `ADMIN` can edit/delete any. Doctor
  status (active/inactive) and salary are admin-only. Patient admission/
  discharge status is doctor/admin-only.
- **Departments** — admin-only CRUD; doctors are assigned to a department
  (created on the fly by name if it doesn't exist yet).
- **Appointments** — patients and doctors can both create appointments for
  each other; a doctor's availability is checked automatically and the
  appointment is auto-confirmed if the doctor is free on that date, or left
  pending if there's a conflict. Only the patient can reschedule or cancel
  their own appointment.
- **Medical records, prescriptions & prescription items** — doctor-only.
  Creating a prescription item decrements the linked medicine's stock
  (checked for availability first); updating/deleting restores stock
  correctly, including when a parent prescription or medical record is
  deleted.
- **Medicine (pharmacy inventory)** — admin-only for full management;
  doctors may additionally adjust stock quantity directly.
- **Rooms & stays** — admin assigns patients to rooms (capacity-checked),
  tracks check-in/check-out and length of stay.
- **Billing** — admin generates an invoice for a patient that auto-calculates
  charges from their completed room stays ($150/night), doctor consultation
  fees per medical record, and medicine costs from prescriptions; payments
  are recorded against invoices (overpayment blocked).
- **Revenue report** — total patient payments minus total doctor salaries.
- **AI chatbot** — patients can ask about their own medical records, ask
  general health questions (answered from their data when relevant,
  otherwise a hedged general answer), or book an appointment, all via a
  tool-calling LLM. A patient can also request a live doctor and the same
  chat window seamlessly switches from AI to a real human doctor in
  real time, queuing if none are available and auto-connecting as soon as
  one comes online.

## Non-functional characteristics

- **Security:** password hashing (BCrypt), JWT in an httpOnly/SameSite=Strict
  cookie, role-based authorization on every endpoint, ownership checks where
  relevant (e.g. a patient can't edit another patient's record), a global
  exception-handling middleware that maps domain errors to consistent HTTP
  status codes without leaking internals in production.
- **Consistency:** every API response uses a common `{ status, message,
  result }` envelope.
- **Data integrity:** EF Core relationships are explicit about delete
  behavior — `Restrict` on anything that would otherwise silently destroy
  clinical/financial history (appointments, medical records, invoices,
  payments, stays), `Cascade` only for true sub-documents (invoice line
  items, prescription items), `SetNull` for optional associations
  (department, doctor availability).
- **Real-time:** a single SignalR hub serves both the AI chat and the live
  doctor handoff, so the client never has to switch transport — only the
  session's mode changes.
- **Extensibility:** a custom, dependency-free mediator
  (request/handler pattern) and generic repository layer keep controllers
  thin and business logic testable in isolation.

## Entities

| Entity | Purpose |
|---|---|
| `User` | Login account (username, password hash, role: ADMIN/PATIENT/DOCTOR/NURSE) |
| `Patient` | Patient profile (1:1 with `User`); admission/discharge status |
| `Doctor` | Doctor profile (1:1 with `User`); license, specialization, consultation fee, salary, department |
| `Department` | Hospital department a doctor belongs to |
| `Appointment` | Scheduled visit between a doctor and patient; status PENDING/CONFIRMED/CANCELLED |
| `MedicalRecord` | A doctor's diagnosis/notes for a patient visit; optionally linked to the `Appointment` it came from |
| `Prescription` | A set of prescribed medicines tied to a medical record |
| `PrescriptionItem` | A single medicine + dosage/quantity/frequency within a prescription |
| `Medicine` | Pharmacy inventory item (name, manufacturer, unit price, stock quantity) |
| `Room` | A physical hospital room (number, type, capacity) |
| `Stay` | A patient's admission to a room (check-in/check-out) |
| `Invoice` / `InvoiceItem` | A generated bill for a patient and its line items |
| `Payment` | A payment recorded against an invoice |
| `ChatSession` / `ChatMessage` | A patient's chat thread (AI bot or live doctor) and its messages |

## Getting started with Docker

**Prerequisites:** Docker Desktop.

1. Copy the sample environment file and fill in real values:
   ```bash
   cp sample.env .env
   ```
   At minimum, set a strong `Jwt__Key` (32+ bytes) if you plan to expose this
   beyond localhost, and your `OpenRouter__ApiKey` if you want the chatbot to work
   (get a free key + pick a free model at https://openrouter.ai).

2. Start the database and API:
   ```bash
   docker compose up -d
   ```
   This builds the API image, starts SQL Server, and waits for the database
   health check before starting the API.

3. Apply database migrations (the API does not auto-migrate on startup).
   From the `backend/` folder, with the .NET SDK and `dotnet-ef` installed
   locally (`dotnet tool install --global dotnet-ef` if you don't have it):
   ```bash
   cd backend
   dotnet ef database update
   ```
   This connects to the SQL Server container via the port mapped to your
   host (`localhost,1433` by default, matching `appsettings.Development.json`).

4. The API is now available at `http://localhost:8080` (or whichever
   `ASPNETCORE_PORT` you set in `.env`), with Swagger UI at `/swagger` in
   development.

To stop everything: `docker compose down` (add `-v` to also drop the SQL
Server data volume).

### Running the API locally without Docker (for development)

`.env` is only read by `docker compose` — it is **not** loaded by `dotnet
run`. For local development, either run just the database via
`docker compose up -d sqlserver` and run the API with `dotnet run` from
`backend/` (using `appsettings.Development.json`), or set secrets via
`dotnet user-secrets` (e.g. `dotnet user-secrets set "OpenRouter:ApiKey" "..."`).
