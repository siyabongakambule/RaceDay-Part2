using Microsoft.EntityFrameworkCore;
using RaceDay.Constants;
using RaceDay.Models;

namespace RaceDay.Data;

public class  ApplicationDbContext : DbContext
{
    public  ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventCategory> EventCategories => Set<EventCategory>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Roles 
        modelBuilder.Entity<Role>()
            .HasIndex(r => r.RoleName)
            .IsUnique();

        // Users 
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleID)
            .OnDelete(DeleteBehavior.Restrict);

        //  Events 
        modelBuilder.Entity<Event>()
            .HasOne(e => e.Organiser)
            .WithMany(u => u.OrganisedEvents)
            .HasForeignKey(e => e.OrganiserID)
            .OnDelete(DeleteBehavior.Restrict);

        //  EventCategories 
        modelBuilder.Entity<EventCategory>()
            .HasOne(c => c.Event)
            .WithMany(e => e.Categories)
            .HasForeignKey(c => c.EventID)
            .OnDelete(DeleteBehavior.Cascade);

        // Enrollments 
        modelBuilder.Entity<Enrollment>()
            .HasOne(en => en.Participant)
            .WithMany(u => u.Enrollments)
            .HasForeignKey(en => en.ParticipantID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrollment>()
            .HasOne(en => en.Category)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(en => en.CategoryID)
            .OnDelete(DeleteBehavior.Restrict);

        // A participant cannot enrol in the same category twice.
        modelBuilder.Entity<Enrollment>()
            .HasIndex(en => new { en.ParticipantID, en.CategoryID })
            .IsUnique();

        //  Results 
        modelBuilder.Entity<Result>()
            .HasOne(res => res.Enrollment)
            .WithOne(en => en.Result)
            .HasForeignKey<Result>(res => res.EnrollmentID)
            .OnDelete(DeleteBehavior.Restrict);

        // One result per enrolment (matches the ERD's 0..1 cardinality).
        modelBuilder.Entity<Result>()
            .HasIndex(res => res.EnrollmentID)
            .IsUnique();

        modelBuilder.Entity<Result>()
            .HasOne(res => res.CapturedByUser)
            .WithMany(u => u.CapturedResults)
            .HasForeignKey(res => res.CapturedByUserID)
            .OnDelete(DeleteBehavior.Restrict);

        //  Seed Roles (matches Part 1 SQL seed data) 
        modelBuilder.Entity<Role>().HasData(
            new Role { RoleID = 1, RoleName = RoleNames.Admin },
            new Role { RoleID = 2, RoleName = RoleNames.Organiser },
            new Role { RoleID = 3, RoleName = RoleNames.Participant }
        );
    }
}
