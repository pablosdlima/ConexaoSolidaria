using CampanhasAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampanhasAPI.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.NomeCompleto).HasMaxLength(200).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(254).IsRequired();
        builder.Property(u => u.Cpf).HasMaxLength(11).IsFixedLength().IsRequired();
        builder.Property(u => u.SenhaHash).HasMaxLength(100).IsRequired(); // BCrypt gera 60 chars
        builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(u => u.Email).IsUnique(); // RF-3: e-mail único no banco
        builder.HasIndex(u => u.Cpf).IsUnique();
    }
}
