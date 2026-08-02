using backend.dbContext;
using backend.Mediator.Interfaces;
using backend.Models;
using backend.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.Features
{
    public class GetRevenueReportRequest : IRequest<RevenueResponse>
    {
    }

    public class GetRevenueReportHandler : IRequestHandler<GetRevenueReportRequest, RevenueResponse>
    {
        private readonly ApplicationDbContext _db;

        public GetRevenueReportHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<RevenueResponse> HandleAsync(GetRevenueReportRequest request, CancellationToken cancellationToken)
        {
            var totalPatientPayments = await _db.Payments
                .Where(payment => payment.Status == PaymentStatus.COMPLETED)
                .SumAsync(payment => payment.Amount, cancellationToken);

            var totalDoctorSalaries = await _db.Doctors
                .SumAsync(doctor => doctor.Salary, cancellationToken);

            return new RevenueResponse
            {
                TotalPatientPayments = totalPatientPayments,
                TotalDoctorSalaries = totalDoctorSalaries,
                NetRevenue = totalPatientPayments - totalDoctorSalaries,
            };
        }
    }
}
