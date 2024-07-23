namespace CommitteeCalendarAPI.Models;

public partial class Participant
{
    public Guid ParticipantsId { get; set; }

    public string ParticipantsName { get; set; } = null!;

    public string ParticipantsRepresentative { get; set; } = null!;

    public string ParticipantsPhonenumber { get; set; } = null!;

    public string ParticipantsEmail { get; set; } = null!;

    public virtual ICollection<EventsParticipant> EventsParticipants { get; set; } = new List<EventsParticipant>();

    public virtual ICollection<UserAccount> UserAccounts { get; set; } = new List<UserAccount>();
}
