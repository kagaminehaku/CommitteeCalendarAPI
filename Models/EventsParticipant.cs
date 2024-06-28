using System;
using System.Collections.Generic;

namespace CommitteeCalendarAPI.Models;

public partial class EventsParticipant
{
    public Guid EvPartiId { get; set; }

    public Guid EventId { get; set; }

    public Guid ParticipantsId { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual Participant Participants { get; set; } = null!;
}
