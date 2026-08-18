using System.Text.Json;
using backend.Mediator.Interfaces;
using backend.Models;
using backend.Models.Dtos;
using backend.Repositories.Interfaces;
using backend.Services.Features;

// This class handles the execution of various chatbot tools, such as retrieving medical records, 
// fetching patient profiles, and creating appointments. It integrates with repositories and the mediator 
// to perform these operations and returns results in JSON format.
namespace backend.Services.Chat
{
    public class ChatToolExecutor : IChatToolExecutor
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IMedicalRecordRepository _medicalRecordRepository;
        private readonly IMyMediator _mediator;


        /// Initializes a new instance of the <see cref="ChatToolExecutor"/> class.

        /// <param name="patientRepository">Repository for accessing patient data.</param>
        /// <param name="medicalRecordRepository">Repository for accessing medical records.</param>
        /// <param name="mediator">Mediator for handling commands.</param>
        public ChatToolExecutor(
            IPatientRepository patientRepository,
            IMedicalRecordRepository medicalRecordRepository,
            IMyMediator mediator)
        {
            _patientRepository = patientRepository;
            _medicalRecordRepository = medicalRecordRepository;
            _mediator = mediator;
        }


        /// Executes a chatbot tool based on the provided tool name and arguments.
        /// <param name="patientUserId">The ID of the patient user.</param>
        /// <param name="toolName">The name of the tool to execute.</param>
        /// <param name="argumentsJson">JSON string containing tool-specific arguments.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>A JSON string representing the result of the tool execution.</re
        public async Task<string> ExecuteAsync(int patientUserId, string toolName, string argumentsJson, CancellationToken cancellationToken)
        {
            //Choose option tools from users
            try
            {
                return toolName switch
                {
                    "get_my_medical_records" => await GetMyMedicalRecordsAsync(patientUserId),
                    "get_my_patient_profile" => await GetMyPatientProfileAsync(patientUserId),
                    "create_appointment" => await CreateAppointmentAsync(patientUserId, argumentsJson, cancellationToken),
                    _ => JsonSerializer.Serialize(new { error = $"Unknown tool '{toolName}'." }),
                };
            }
            catch (Exception ex)
            {
                // Tool failures are reported back to the model as data, not thrown,
                // so it can explain the problem to the patient instead of the request failing outright.
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        /// Retrieves the profile of the specified patient.
        private async Task<string> GetMyMedicalRecordsAsync(int patientUserId)
        {
            var patient = await _patientRepository.GetByUserIdAsync(patientUserId);
            if (patient is null)
            {
                return JsonSerializer.Serialize(new { error = "No patient profile found for the current account." });
            }

            var allRecords = await _medicalRecordRepository.GetAllAsync();
            var myRecords = allRecords
                .Where(record => record.PatientId == patient.Id)
                .Select(MedicalRecordResponse.FromEntity)
                .ToList();

            return JsonSerializer.Serialize(myRecords);
        }

        /// Creates an appointment for the specified patient based on the provided argument
        private async Task<string> GetMyPatientProfileAsync(int patientUserId)
        {
            var patient = await _patientRepository.GetByUserIdAsync(patientUserId);
            if (patient is null)
            {
                return JsonSerializer.Serialize(new { error = "No patient profile found for the current account." });
            }

            return JsonSerializer.Serialize(PatientResponse.FromEntity(patient));
        }
        /// Represents the arguments required for creating an appointment.
        private async Task<string> CreateAppointmentAsync(int patientUserId, string argumentsJson, CancellationToken cancellationToken)
        {
            var args = JsonSerializer.Deserialize<CreateAppointmentToolArgs>(
                argumentsJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (args is null || args.DoctorId <= 0 || string.IsNullOrWhiteSpace(args.Reason))
            {
                return JsonSerializer.Serialize(new { error = "doctorId, schedule, and reason are all required." });
            }

            if (!DateTime.TryParse(args.Schedule, out var schedule))
            {
                return JsonSerializer.Serialize(new { error = $"Could not parse schedule '{args.Schedule}' as a date/time." });
            }

            var result = await _mediator.SendAsync(new CreateAppointmentCommand
            {
                DoctorId = args.DoctorId,
                Schedule = schedule,
                Reason = args.Reason,
                RequestingUserId = patientUserId,
                RequestingUserRole = Role.PATIENT,
            }, cancellationToken);

            return JsonSerializer.Serialize(result);
        }

        private class CreateAppointmentToolArgs
        {
            public int DoctorId { get; set; }
            public string Schedule { get; set; } = string.Empty;
            public string Reason { get; set; } = string.Empty;
        }
    }
}
