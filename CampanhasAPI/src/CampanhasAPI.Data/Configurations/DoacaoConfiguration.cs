using CampanhasAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampanhasAPI.Data.Configurations;

public class DoacaoConfiguration : IEntityTypeConfiguration<Doacao>
{
    public void Configure(EntityTypeBuilder<Doacao> builder)
    {
        builder.ToTable("doacoes");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Valor).HasPrecision(18, 2);
        builder.Property(d => d.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne<Campanha>().WithMany().HasForeignKey(d => d.IdCampanha).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Usuario>().WithMany().HasForeignKey(d => d.IdDoador).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => d.IdDoador);
    }
}
