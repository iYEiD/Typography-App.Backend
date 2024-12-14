using Microsoft.EntityFrameworkCore;
using Models;


public class ApplicationDbContext : DbContext
{
    public DbSet<User>? Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        
        optionsBuilder.UseMySql(builder.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0)),
        options => options.EnableRetryOnFailure());
    }
}