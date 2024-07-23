namespace CommitteeCalendarAPI.ActionModels
{
    public class UserRegisterRequest
    {
        public string? Username { get; set; }

        public string? Password { get; set; }

        public string? RePassword { get; set; }

        public string? Info { get; set; }

        public string? Avatar { get; set; }

        public string? Email { get; set; }

        public string? Phonenumber { get; set; }
    }
}