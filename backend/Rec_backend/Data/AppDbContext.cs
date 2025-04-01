using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Rec_backend.Models;

namespace Rec_backend.Data;

public class RecDbContext : DbContext
{
    private readonly IConfiguration _conf;
    public RecDbContext(IConfiguration configuration)
    {
        _conf = configuration;
    }
    public DbSet<RequestLog> RequestLogs => Set<RequestLog>();
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_conf.GetConnectionString("DataBase"));
    }
}