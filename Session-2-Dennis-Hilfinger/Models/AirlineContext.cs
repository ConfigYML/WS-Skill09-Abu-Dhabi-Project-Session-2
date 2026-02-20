using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Session_2_Dennis_Hilfinger;

public partial class AirlineContext : DbContext
{
    public AirlineContext()
    {
    }

    public AirlineContext(DbContextOptions<AirlineContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Aircraft> Aircrafts { get; set; }

    public virtual DbSet<Airport> Airports { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Office> Offices { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Route> Routes { get; set; }

    public virtual DbSet<Schedule> Schedules { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Session2;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Aircraft>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_AirPlan");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.MakeModel).HasMaxLength(10);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Airport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_AirPort");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CountryId).HasColumnName("CountryID");
            entity.Property(e => e.Iatacode)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("IATACode");
            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasOne(d => d.Country).WithMany(p => p.Airports)
                .HasForeignKey(d => d.CountryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AirPort_Country");
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Country");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Office>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Office");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Contact).HasMaxLength(250);
            entity.Property(e => e.CountryId).HasColumnName("CountryID");
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(50);

            entity.HasOne(d => d.Country).WithMany(p => p.Offices)
                .HasForeignKey(d => d.CountryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Office_Country");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_UserRole");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Title).HasMaxLength(50);
        });

        modelBuilder.Entity<Route>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ArrivalAirportId).HasColumnName("ArrivalAirportID");
            entity.Property(e => e.DepartureAirportId).HasColumnName("DepartureAirportID");

            entity.HasOne(d => d.ArrivalAirport).WithMany(p => p.RouteArrivalAirports)
                .HasForeignKey(d => d.ArrivalAirportId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Routes_Airports3");

            entity.HasOne(d => d.DepartureAirport).WithMany(p => p.RouteDepartureAirports)
                .HasForeignKey(d => d.DepartureAirportId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Routes_Airports2");
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Serivec");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AircraftId).HasColumnName("AircraftID");
            entity.Property(e => e.EconomyPrice).HasColumnType("money");
            entity.Property(e => e.FlightNumber).HasMaxLength(10);
            entity.Property(e => e.RouteId).HasColumnName("RouteID");
            entity.Property(e => e.Time).HasPrecision(5);

            entity.HasOne(d => d.Aircraft).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.AircraftId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Schedule_AirCraft");

            entity.HasOne(d => d.Route).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.RouteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Schedule_Routes");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_User");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.OfficeId).HasColumnName("OfficeID");
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");

            entity.HasOne(d => d.Office).WithMany(p => p.Users)
                .HasForeignKey(d => d.OfficeId)
                .HasConstraintName("FK_Users_Offices");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Roles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
