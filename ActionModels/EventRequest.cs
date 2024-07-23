using CommitteeCalendarAPI.Models;

namespace CommitteeCalendarAPI.ActionModels
{
    public class EventRequestPostPut
    {
        public string EventName { get; set; } = null!;

        public string HostPerson { get; set; } = null!;

        public DateOnly StartDate { get; set; }

        public TimeOnly StartTime { get; set; }

        public int Duration { get; set; }

        public string Detail { get; set; } = null!;

        public Guid LocationId { get; set; }

        public string Participants { get; set; } = null!;

        public bool IsAppoved { get; set; }
        public List<Guid> ParticipantIds { get; set; } = new List<Guid>();
    }

    public class EventRequestGet
    {
        public Guid EventId { get; set; }

        public string EventName { get; set; } = null!;

        public string HostPerson { get; set; } = null!;

        public DateOnly StartDate { get; set; }

        public TimeOnly StartTime { get; set; }

        public int Duration { get; set; }

        public string Detail { get; set; } = null!;

        public Guid LocationId { get; set; }

        public string Participants { get; set; } = null!;

        public bool IsAppoved { get; set; }
        public List<Guid> ParticipantIds { get; set; } = new List<Guid>();
    }
}
