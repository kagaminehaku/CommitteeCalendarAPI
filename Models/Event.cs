namespace CommitteeCalendarAPI.Models;

public partial class Event
{
    public Guid EventId { get; set; }

    public string EventName { get; set; } = null!;

    public string HostPerson { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public int Duration { get; set; }

    public string Detail { get; set; } = null!;

    public Guid LocationId { get; set; }

    public bool IsAppoved { get; set; }

    public virtual ICollection<EventsParticipant> EventsParticipants { get; set; } = new List<EventsParticipant>();

    public virtual Location Location { get; set; } = null!;
}
