using Microsoft.EntityFrameworkCore;
using CheckPoint1.Models;

namespace CheckPoint1;

public class CheckpointContext : DbContext
{
    // DbSets para cada entidade do nosso modelo
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Produto> Produtos { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Pedido> Pedidos { get; set; }
    public DbSet<PedidoItem> PedidoItens { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Configura a conexão com o banco SQLite
        optionsBuilder.UseSqlite("Data Source=loja.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Relacionamento Categoria -> Produtos
        modelBuilder.Entity<Produto>()
            .HasOne(p => p.Categoria)
            .WithMany(c => c.Produtos)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Relacionamento Cliente -> Pedidos
        modelBuilder.Entity<Pedido>()
            .HasOne(p => p.Cliente)
            .WithMany(c => c.Pedidos)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Relacionamento Pedido -> PedidoItens
        modelBuilder.Entity<PedidoItem>()
            .HasOne(pi => pi.Pedido)
            .WithMany(p => p.Itens)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Relacionamento Produto -> PedidoItens
        modelBuilder.Entity<PedidoItem>()
            .HasOne(pi => pi.Produto)
            .WithMany(p => p.PedidoItens)
            .HasForeignKey(pi => pi.ProdutoId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // Índices únicos
        modelBuilder.Entity<Cliente>()
            .HasIndex(c => c.Email)
            .IsUnique();

        modelBuilder.Entity<Pedido>()
            .HasIndex(p => p.NumeroPedido)
            .IsUnique();

        // Dados iniciais
        // Categorias
        var categorias = new[]
        {
            new Categoria { Id = 1, Nome = "Eletrônicos" },
            new Categoria { Id = 2, Nome = "Livros" },
            new Categoria { Id = 3, Nome = "Jogos" }
        };
        modelBuilder.Entity<Categoria>().HasData(categorias);

        // Produtos (alguns com estoque zero para teste)
        var produtos = new[]
        {
            new Produto { Id = 1, Nome = "Smartphone", Preco = 1999.99m, Estoque = 10, CategoriaId = 1 },
            new Produto { Id = 2, Nome = "Notebook", Preco = 3999.99m, Estoque = 0, CategoriaId = 1 },
            new Produto { Id = 3, Nome = "Clean Code", Preco = 89.90m, Estoque = 5, CategoriaId = 2 },
            new Produto { Id = 4, Nome = "Domain-Driven Design", Preco = 129.90m, Estoque = 3, CategoriaId = 2 },
            new Produto { Id = 5, Nome = "The Last of Us", Preco = 199.90m, Estoque = 0, CategoriaId = 3 },
            new Produto { Id = 6, Nome = "God of War", Preco = 249.90m, Estoque = 8, CategoriaId = 3 }
        };
        modelBuilder.Entity<Produto>().HasData(produtos);

        // Clientes
        var clientes = new[]
        {
            new Cliente { Id = 1, Nome = "João Silva", Email = "joao@email.com" },
            new Cliente { Id = 2, Nome = "Maria Santos", Email = "maria@email.com" }
        };
        modelBuilder.Entity<Cliente>().HasData(clientes);

        // Pedidos
        var pedidos = new[]
        {
            new Pedido { Id = 1, ClienteId = 1, NumeroPedido = "P001", DataPedido = new DateTime(2025, 10, 1), Status = StatusPedido.Confirmado },
            new Pedido { Id = 2, ClienteId = 2, NumeroPedido = "P002", DataPedido = new DateTime(2025, 10, 2), Status = StatusPedido.Pendente }
        };
        modelBuilder.Entity<Pedido>().HasData(pedidos);

        // Itens de Pedido
        var itensPedido = new[]
        {
            new PedidoItem { Id = 1, PedidoId = 1, ProdutoId = 1, Quantidade = 1, PrecoUnitario = 1999.99m },
            new PedidoItem { Id = 2, PedidoId = 1, ProdutoId = 3, Quantidade = 2, PrecoUnitario = 89.90m },
            new PedidoItem { Id = 3, PedidoId = 2, ProdutoId = 4, Quantidade = 1, PrecoUnitario = 129.90m },
            new PedidoItem { Id = 4, PedidoId = 2, ProdutoId = 6, Quantidade = 1, PrecoUnitario = 249.90m }
        };
        modelBuilder.Entity<PedidoItem>().HasData(itensPedido);
    }
}