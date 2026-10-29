using CozinhaApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CozinhaApp.Data;

public class CozinhaContext : DbContext
{
    public CozinhaContext(DbContextOptions<CozinhaContext> options) : base(options) { }

    public DbSet<Ingrediente> Ingredientes => Set<Ingrediente>();
    public DbSet<LoteIngrediente> Lotes => Set<LoteIngrediente>();
    public DbSet<Receita> Receitas => Set<Receita>();
    public DbSet<EtapaReceita> Etapas => Set<EtapaReceita>();
    public DbSet<EtapaIngrediente> EtapaIngredientes => Set<EtapaIngrediente>();
    public DbSet<Preparo> Preparos => Set<Preparo>();
    public DbSet<PreparoConsumo> Consumos => Set<PreparoConsumo>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Ingrediente>().HasIndex(i => i.Nome).IsUnique();

        b.Entity<EtapaIngrediente>().HasOne(e => e.Ingrediente).WithMany()
            .HasForeignKey(e => e.IngredienteId).OnDelete(DeleteBehavior.Restrict);

        b.Entity<PreparoConsumo>().HasOne(c => c.Lote).WithMany()
            .HasForeignKey(c => c.LoteId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<PreparoConsumo>().HasOne(c => c.Ingrediente).WithMany()
            .HasForeignKey(c => c.IngredienteId).OnDelete(DeleteBehavior.Restrict);

        b.Entity<Preparo>().HasIndex(p => p.Status);
    }
}
