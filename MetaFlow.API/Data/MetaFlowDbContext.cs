using Microsoft.EntityFrameworkCore;
using MetaFlow.API.Models;

namespace MetaFlow.API.Data
{
    public class MetaFlowDbContext : DbContext
    {
        public MetaFlowDbContext(DbContextOptions<MetaFlowDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Meta> Metas { get; set; }
        public DbSet<RegistroDiario> RegistrosDiarios { get; set; }
        public DbSet<ResumoMensal> ResumosMensais { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurações para compatibilidade Oracle
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(bool))
                    {
                        property.SetColumnType("NUMBER(1)");
                    }
                }
            }

            // USUARIO
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Nome).HasMaxLength(100);
                entity.Property(u => u.Email).HasMaxLength(150);
                entity.Property(u => u.SenhaHash).HasMaxLength(256);
                entity.Property(u => u.Profissao).HasMaxLength(100);
                entity.Property(u => u.ObjetivoProfissional).HasMaxLength(200);

                entity.HasIndex(u => u.Email).IsUnique();

                // Relacionamentos
                entity.HasMany(u => u.Metas)
                    .WithOne(m => m.Usuario)
                    .HasForeignKey(m => m.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.RegistrosDiarios)
                    .WithOne(rd => rd.Usuario)
                    .HasForeignKey(rd => rd.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.ResumosMensais)
                    .WithOne(rm => rm.Usuario)
                    .HasForeignKey(rm => rm.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // META
            modelBuilder.Entity<Meta>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Titulo).HasMaxLength(200);
                entity.Property(m => m.Categoria).HasMaxLength(50);
                entity.Property(m => m.Descricao).HasMaxLength(1000);
                entity.Property(m => m.Status).HasMaxLength(20);

                entity.HasIndex(m => new { m.UsuarioId, m.Status });
                entity.HasIndex(m => m.Prazo);
            });

            // REGISTRO DIÁRIO
            modelBuilder.Entity<RegistroDiario>(entity =>
            {
                entity.HasKey(rd => rd.Id);
                entity.Property(rd => rd.Anotacoes).HasMaxLength(500);

                // Restrição única: 1 registro por usuário por dia
                entity.HasIndex(rd => new { rd.UsuarioId, rd.Data }).IsUnique();

                entity.HasIndex(rd => rd.Data);
            });

            // RESUMO MENSAL
            modelBuilder.Entity<ResumoMensal>(entity =>
            {
                entity.HasKey(rm => rm.Id);

                // Restrição única: 1 resumo por usuário por mês/ano
                entity.HasIndex(rm => new { rm.UsuarioId, rm.Ano, rm.Mes }).IsUnique();
            });
        }
    }
}