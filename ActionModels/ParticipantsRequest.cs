namespace CommitteeCalendarAPI.ActionModels
{
    public class ParticipantGet
    {
        public Guid Id { get; set; }
        public string? ParticipantsName { get; set; }
    }

    public class ParticipantPutPost
    {
        public string ParticipantsName { get; set; } = null!;

        public string ParticipantsRepresentative { get; set; } = null!;

        public string ParticipantsPhonenumber { get; set; } = null!;

        public string ParticipantsEmail { get; set; } = null!;
    }

}
