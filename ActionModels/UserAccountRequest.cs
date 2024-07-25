namespace CommitteeCalendarAPI.Models
{
    public class UserAccountMinimal
    {
        public string? Username { get; set; }
        public string? Info { get; set; }
        public string? Avatar { get; set; }
        public string? Email { get; set; }
        public string? Phonenumber { get; set; }
        public string? ParticipantsName { get; set; }
    }

    public class UserAccountUpdate
    {
        public string? Info { get; set; }
        public IFormFile? Avatar { get; set; }
        public string? Email { get; set; }
        public string? Phonenumber { get; set; }
    }
}
