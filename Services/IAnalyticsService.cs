using System.Threading.Tasks;
using CampusDeadline.Models.ViewModels;

namespace CampusDeadline.Services
{
    public interface IAnalyticsService
    {
        Task<ProgressAnalyticsDto> GetProgressAnalyticsAsync(int userId, string period = "all");
    }
}
