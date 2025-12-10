using System.ComponentModel.DataAnnotations;

namespace WorkSessionTrackerAPI.DTOs
{
    public class StudentConnectToCompanyDto
    {
        [Required] public int CompanyId { get; set; }
        [Required] public string TotpCode { get; set; } = string.Empty;
    }
}
