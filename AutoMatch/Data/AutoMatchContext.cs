using Microsoft.EntityFrameworkCore;
using AutoMatch.Models;

namespace AutoMatch.Data
{
    public class AutoMatchContext : DbContext
    {
        public AutoMatchContext(DbContextOptions<AutoMatchContext> options)
            : base(options)
        { }

        public DbSet<Utilizador> Utilizadores { get; set; }
        public DbSet<CodigoPostal> CodigoPostais { get; set; }
        public DbSet<Administrador> Administradores { get; set; }
        public DbSet<Comprador> Compradores { get; set; }
        public DbSet<Preferencias> Preferencias { get; set; }
        public DbSet<Vendedor> Vendedores { get; set; }
        public DbSet<DadosFaturacao> DadosFaturacoes { get; set; }
        public DbSet<Modelo> Modelos { get; set; }
        public DbSet<Anuncio> Anuncios { get; set; }
        public DbSet<Documento> Documentos { get; set; }
        public DbSet<Imagens> Imagens { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<Visita> Visitas { get; set; }
        public DbSet<Notificacoes> Notificacoes { get; set; }
        public DbSet<Compra> Compras { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Desativar todas as cascatas automáticas
            foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // Vendedor (1) -> (Muitos) Anuncios
            modelBuilder.Entity<Anuncio>()
                .HasOne(a => a.Vendedor)
                .WithMany(v => v.Anuncios)
                .HasForeignKey(a => a.Id_Vendedor)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
