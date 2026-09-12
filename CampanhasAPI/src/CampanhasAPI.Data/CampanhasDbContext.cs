using CampanhasAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CampanhasAPI.Data;

public class CampanhasDbContext(DbContextOptions<CampanhasDbContext> options) : DbContext(options)
{
    public DbSet<Campanha> Campanhas => Set<Campanha>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Doacao> Doacoes => Set<Doacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CampanhasDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
