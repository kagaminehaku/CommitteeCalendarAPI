namespace CommitteeCalendarAPI.ActionModels
{
    public class LocationsMinimal
    {
        //public Guid LocationId { get; set; }

        public string LocationName { get; set; } = null!;

        public string LocationAddress { get; set; } = null!;

        public string LocationInfo { get; set; } = null!;

        public string LocationContact { get; set; } = null!;
    }
}
