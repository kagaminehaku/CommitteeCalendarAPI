using Microsoft.EntityFrameworkCore;

namespace CommitteeCalendarAPI.Models;

public partial class CommitteeCalendarContext : DbContext
{
    public CommitteeCalendarContext()
    {
    }

    public CommitteeCalendarContext(DbContextOptions<CommitteeCalendarContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<EventsParticipant> EventsParticipants { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<Participant> Participants { get; set; }

    public virtual DbSet<UserAccount> UserAccounts { get; set; }

    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //=> optionsBuilder.UseSqlServer("CS");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PK_event");

            entity.ToTable("events");

            entity.Property(e => e.EventId)
                .ValueGeneratedNever()
                .HasColumnName("event_id");
            entity.Property(e => e.Detail)
                .HasColumnType("text")
                .HasColumnName("detail");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.EventName)
                .HasMaxLength(256)
                .HasColumnName("event_name");
            entity.Property(e => e.HostPerson)
                .HasMaxLength(128)
                .HasColumnName("host_person");
            entity.Property(e => e.IsAppoved).HasColumnName("is_appoved");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.StartTime).HasColumnName("start_time");

            entity.HasOne(d => d.Location).WithMany(p => p.Events)
                .HasForeignKey(d => d.LocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_event_location");
        });

        modelBuilder.Entity<EventsParticipant>(entity =>
        {
            entity.HasKey(e => e.EvPartiId);

            entity.ToTable("events_participants");

            entity.Property(e => e.EvPartiId)
                .ValueGeneratedNever()
                .HasColumnName("ev_parti_id");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.ParticipantsId).HasColumnName("participants_id");

            entity.HasOne(d => d.Event).WithMany(p => p.EventsParticipants)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_events_participants_events");

            entity.HasOne(d => d.Participants).WithMany(p => p.EventsParticipants)
                .HasForeignKey(d => d.ParticipantsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_events_participants_participants");
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.LocationId).HasName("PK_location");

            entity.ToTable("locations");

            entity.Property(e => e.LocationId)
                .ValueGeneratedNever()
                .HasColumnName("location_id");
            entity.Property(e => e.LocationAddress)
                .HasColumnType("text")
                .HasColumnName("location_address");
            entity.Property(e => e.LocationContact)
                .HasColumnType("text")
                .HasColumnName("location_contact");
            entity.Property(e => e.LocationInfo)
                .HasColumnType("text")
                .HasColumnName("location_info");
            entity.Property(e => e.LocationName)
                .HasMaxLength(64)
                .HasColumnName("location_name");
        });

        modelBuilder.Entity<Participant>(entity =>
        {
            entity.HasKey(e => e.ParticipantsId).HasName("PK_participant");

            entity.ToTable("participants");

            entity.Property(e => e.ParticipantsId)
                .ValueGeneratedNever()
                .HasColumnName("participants_id");
            entity.Property(e => e.ParticipantsEmail)
                .HasMaxLength(128)
                .HasColumnName("participants_email");
            entity.Property(e => e.ParticipantsName)
                .HasMaxLength(256)
                .HasColumnName("participants_name");
            entity.Property(e => e.ParticipantsPhonenumber)
                .HasMaxLength(10)
                .HasColumnName("participants_phonenumber");
            entity.Property(e => e.ParticipantsRepresentative)
                .HasMaxLength(256)
                .HasColumnName("participants_representative");
        });

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.ToTable("user_account");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Adminpermission).HasColumnName("adminpermission");
            entity.Property(e => e.Avatar)
                .HasColumnType("text")
                .HasColumnName("avatar");
            entity.Property(e => e.Email)
                .HasMaxLength(128)
                .HasColumnName("email");
            entity.Property(e => e.Info)
                .HasColumnType("text")
                .HasColumnName("info");
            entity.Property(e => e.ParticipantsId).HasColumnName("participants_id");
            entity.Property(e => e.Password)
                .HasMaxLength(128)
                .HasColumnName("password");
            entity.Property(e => e.Phonenumber)
                .HasMaxLength(16)
                .HasColumnName("phonenumber");
            entity.Property(e => e.Username)
                .HasMaxLength(128)
                .HasColumnName("username");

            entity.HasOne(d => d.Participants).WithMany(p => p.UserAccounts)
                .HasForeignKey(d => d.ParticipantsId)
                .HasConstraintName("FK_user_account_participants");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
