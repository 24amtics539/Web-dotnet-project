using System.Threading.Tasks;

namespace CampusDeadline.Services
{
    public interface IPdfReportService
    {
        Task<byte[]> GenerateAcademicReportPdfAsync(int userId);
    }
}
