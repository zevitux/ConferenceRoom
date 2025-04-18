using ConferenceRoomApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ConferenceRoomApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.User) //One booking have one user
            .WithMany(b => b.Bookings) //One user have many bookings
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Room) //One booking have one room
            .WithMany(b => b.Bookings) //One room have many bookings
            .HasForeignKey(b => b.RoomId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}