using CampanhasAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampanhasAPI.Data.Configurations;

public class CampanhaConfiguration : IEntityTypeConfiguration<Campanha>
{
    public void Configure(EntityTypeBuilder<Campanha> builder)
    {
        builder.ToTable("campanhas");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Titulo).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Descricao).HasMaxLength(2000).IsRequired();
        builder.Property(c => c.MetaFinanceira).HasPrecision(18, 2);
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(c => c.Status); // o painel público filtra por Ativa
    }
}
