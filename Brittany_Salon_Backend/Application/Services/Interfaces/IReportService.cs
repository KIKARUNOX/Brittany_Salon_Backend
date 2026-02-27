using Brittany_Salon_Backend.Application.DTOs.Report;

namespace Brittany_Salon_Backend.Application.Services.Interfaces
{
    public interface IReportService
    {
        Task<List<TopClientDto>> GetTop5LoyalClientsAsync();
        Task<List<TopProductDto>> GetTopSellingProductsAsync();
        Task<List<DailyPaymentDto>> GetDailyPaymentsAsync(DateTime? date = null);
        Task<DailyTotalDto?> GetDailyTotalAsync(DateTime? date = null);
        Task<List<MonthlyRevenueDto>> GetMonthlyRevenueAsync(int? year = null);
        Task<BusinessSummaryDto> GetBusinessSummaryAsync();
    }
}
