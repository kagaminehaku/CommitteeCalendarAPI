namespace CommitteeCalendarAPI.Models;

public partial class UserAccount
{
    public Guid Id { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Info { get; set; } = null!;

    public string Avatar { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phonenumber { get; set; } = null!;

    public bool Adminpermission { get; set; }

    public Guid? ParticipantsId { get; set; }

    public virtual Participant? Participants { get; set; }
}
