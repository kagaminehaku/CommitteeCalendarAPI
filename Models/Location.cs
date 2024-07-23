namespace CommitteeCalendarAPI.Models;

public partial class Location
{
    public Guid LocationId { get; set; }

    public string LocationName { get; set; } = null!;

    public string LocationAddress { get; set; } = null!;

    public string LocationInfo { get; set; } = null!;

    public string LocationContact { get; set; } = null!;

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}
