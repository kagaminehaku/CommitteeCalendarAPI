namespace CommitteeCalendarAPI.ActionModels
{
    public class PasswordChangeRequest
    {
        public string? RequesterUsername { get; set; }
        public string? TargetUsername { get; set; }
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}
