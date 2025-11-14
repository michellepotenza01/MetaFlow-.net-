using Microsoft.EntityFrameworkCore;
using MetaFlow.API.Models;
using MetaFlow.API.Enums;

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

            ConfigureForOracle(modelBuilder);

            ConfigureUsuario(modelBuilder);
            ConfigureMeta(modelBuilder);
            ConfigureRegistroDiario(modelBuilder);
            ConfigureResumoMensal(modelBuilder);
            ConfigureRelationships(modelBuilder);
        }

        private void ConfigureForOracle(ModelBuilder modelBuilder)
        {
            if (Database.IsOracle())
            {
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
            }
        }

        private void ConfigureUsuario(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.ToTable("USUARIOS");
                
                entity.Property(u => u.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedOnAdd();
                    
                entity.Property(u => u.Nome)
                    .HasColumnName("NOME")
                    .HasMaxLength(100)
                    .IsRequired();
                    
                entity.Property(u => u.Email)
                    .HasColumnName("EMAIL")
                    .HasMaxLength(150)
                    .IsRequired();
                    
                entity.Property(u => u.SenhaHash)
                    .HasColumnName("SENHA_HASH")
                    .HasMaxLength(256)
                    .IsRequired();
                    
                entity.Property(u => u.Profissao)
                    .HasColumnName("PROFISSAO")
                    .HasMaxLength(100);
                    
                entity.Property(u => u.ObjetivoProfissional)
                    .HasColumnName("OBJETIVO_PROFISSIONAL")
                    .HasMaxLength(200);
                    
                entity.Property(u => u.CriadoEm)
                    .HasColumnName("CRIADO_EM")
                    .HasDefaultValueSql("SYSTIMESTAMP");
                    
                entity.Property(u => u.AtualizadoEm)
                    .HasColumnName("ATUALIZADO_EM")
                    .HasDefaultValueSql("SYSTIMESTAMP");

                entity.HasIndex(u => u.Email)
                    .HasDatabaseName("IX_USUARIOS_EMAIL")
                    .IsUnique();
            });
        }

        private void ConfigureMeta(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Meta>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.ToTable("METAS");
                
                entity.Property(m => m.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedOnAdd();
                    
                entity.Property(m => m.UsuarioId)
                    .HasColumnName("USUARIO_ID")
                    .IsRequired();
                    
                entity.Property(m => m.Titulo)
                    .HasColumnName("TITULO")
                    .HasMaxLength(200)
                    .IsRequired();
                    
                entity.Property(m => m.Categoria)
                    .HasColumnName("CATEGORIA")
                    .HasConversion<int>()
                    .IsRequired();
                    
                entity.Property(m => m.Descricao)
                    .HasColumnName("DESCRICAO")
                    .HasMaxLength(1000);
                    
                entity.Property(m => m.Status)
                    .HasColumnName("STATUS")
                    .HasConversion<int>()
                    .IsRequired();
                    
                entity.Property(m => m.Progresso)
                    .HasColumnName("PROGRESSO")
                    .HasColumnType("NUMBER(5,2)")
                    .IsRequired();
                    
                entity.Property(m => m.Prazo)
                    .HasColumnName("PRAZO")
                    .IsRequired();
                    
                entity.Property(m => m.CriadoEm)
                    .HasColumnName("CRIADO_EM")
                    .HasDefaultValueSql("SYSTIMESTAMP");

                entity.HasIndex(m => new { m.UsuarioId, m.Status })
                    .HasDatabaseName("IX_METAS_USUARIO_STATUS");
                    
                entity.HasIndex(m => m.Prazo)
                    .HasDatabaseName("IX_METAS_PRAZO");
                    
                entity.HasIndex(m => m.Categoria)
                    .HasDatabaseName("IX_METAS_CATEGORIA");
            });
        }

        private void ConfigureRegistroDiario(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RegistroDiario>(entity =>
            {
                entity.HasKey(rd => rd.Id);
                entity.ToTable("REGISTROS_DIARIOS");
                
                entity.Property(rd => rd.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedOnAdd();
                    
                entity.Property(rd => rd.UsuarioId)
                    .HasColumnName("USUARIO_ID")
                    .IsRequired();
                    
                entity.Property(rd => rd.Data)
                    .HasColumnName("DATA")
                    .IsRequired();
                    
                entity.Property(rd => rd.Humor)
                    .HasColumnName("HUMOR")
                    .HasConversion<int>()
                    .IsRequired();
                    
                entity.Property(rd => rd.Produtividade)
                    .HasColumnName("PRODUTIVIDADE")
                    .HasColumnType("NUMBER(3)")
                    .IsRequired();
                    
                entity.Property(rd => rd.TempoFoco)
                    .HasColumnName("TEMPO_FOCO")
                    .HasColumnType("NUMBER(5)")
                    .HasDefaultValue(0);
                    
                entity.Property(rd => rd.Anotacoes)
                    .HasColumnName("ANOTACOES")
                    .HasMaxLength(500);
                    
                entity.Property(rd => rd.CriadoEm)
                    .HasColumnName("CRIADO_EM")
                    .HasDefaultValueSql("SYSTIMESTAMP");

                entity.HasIndex(rd => new { rd.UsuarioId, rd.Data })
                    .HasDatabaseName("IX_REGISTROS_USUARIO_DATA")
                    .IsUnique();
                    
                entity.HasIndex(rd => rd.Data)
                    .HasDatabaseName("IX_REGISTROS_DATA");
            });
        }

        private void ConfigureResumoMensal(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ResumoMensal>(entity =>
            {
                entity.HasKey(rm => rm.Id);
                entity.ToTable("RESUMOS_MENSAIS");
                
                entity.Property(rm => rm.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedOnAdd();
                    
                entity.Property(rm => rm.UsuarioId)
                    .HasColumnName("USUARIO_ID")
                    .IsRequired();
                    
                entity.Property(rm => rm.Ano)
                    .HasColumnName("ANO")
                    .HasColumnType("NUMBER(4)")
                    .IsRequired();
                    
                entity.Property(rm => rm.Mes)
                    .HasColumnName("MES")
                    .HasColumnType("NUMBER(2)")
                    .IsRequired();
                    
                entity.Property(rm => rm.TotalRegistros)
                    .HasColumnName("TOTAL_REGISTROS")
                    .HasColumnType("NUMBER(10)")
                    .IsRequired();
                    
                entity.Property(rm => rm.MetasConcluidas)
                    .HasColumnName("METAS_CONCLUIDAS")
                    .HasColumnType("NUMBER(10)")
                    .IsRequired();
                    
                entity.Property(rm => rm.MediaHumor)
                    .HasColumnName("MEDIA_HUMOR")
                    .HasColumnType("NUMBER(4,2)")
                    .IsRequired();
                    
                entity.Property(rm => rm.MediaProdutividade)
                    .HasColumnName("MEDIA_PRODUTIVIDADE")
                    .HasColumnType("NUMBER(4,2)")
                    .IsRequired();
                    
                entity.Property(rm => rm.TaxaConclusao)
                    .HasColumnName("TAXA_CONCLUSAO")
                    .HasColumnType("NUMBER(5,2)")
                    .IsRequired();
                    
                entity.Property(rm => rm.CalculadoEm)
                    .HasColumnName("CALCULADO_EM")
                    .HasDefaultValueSql("SYSTIMESTAMP");

                 entity.HasIndex(rm => new { rm.UsuarioId, rm.Ano, rm.Mes })
                    .HasDatabaseName("IX_RESUMOS_USUARIO_PERIODO")
                    .IsUnique();
            });
        }

        private void ConfigureRelationships(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>()
                .HasMany(u => u.Metas)
                .WithOne(m => m.Usuario)
                .HasForeignKey(m => m.UsuarioId)
                .HasConstraintName("FK_METAS_USUARIO")
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Usuario>()
                .HasMany(u => u.RegistrosDiarios)
                .WithOne(rd => rd.Usuario)
                .HasForeignKey(rd => rd.UsuarioId)
                .HasConstraintName("FK_REGISTROS_USUARIO")
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Usuario>()
                .HasMany(u => u.ResumosMensais)
                .WithOne(rm => rm.Usuario)
                .HasForeignKey(rm => rm.UsuarioId)
                .HasConstraintName("FK_RESUMOS_USUARIO")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}