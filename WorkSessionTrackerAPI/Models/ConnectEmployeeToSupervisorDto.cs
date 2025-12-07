using System.ComponentModel.DataAnnotations;

namespace WorkSessionTrackerAPI.DTOs
{
    public class ConnectEmployeeToSupervisorDto
    {
        [Required] public int EmployeeId { get; set; }
        [Required] public int SupervisorId { get; set; }
        [Required] public string TotpCode { get; set; } = string.Empty; // TOTP code from the supervisor
    }
}
