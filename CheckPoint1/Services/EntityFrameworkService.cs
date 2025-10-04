using Microsoft.EntityFrameworkCore;
using CheckPoint1.Models;

namespace CheckPoint1.Services;

    public class EntityFrameworkService
    {
        private readonly CheckpointContext _context;

        public EntityFrameworkService()
        {
            _context = new CheckpointContext();
        }

        // ========== CRUD CATEGORIAS ==========

        public void CriarCategoria()
        {
            Console.WriteLine("=== CRIAR CATEGORIA ===");
            
            Console.Write("Nome da categoria: ");
            var nome = Console.ReadLine();
            
            if (string.IsNullOrEmpty(nome))
            {
                Console.WriteLine("Nome é obrigatório!");
                return;
            }
            
            try
            {
                var categoria = new Categoria { Nome = nome };
                _context.Categorias.Add(categoria);
                _context.SaveChanges();
                
                Console.WriteLine($"Categoria '{nome}' criada com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao criar categoria: {ex.Message}");
            }
        }

        public void ListarCategorias()
        {
            Console.WriteLine("=== CATEGORIAS ===");
            
            var categorias = _context.Categorias
                .Include(c => c.Produtos)
                .OrderBy(c => c.Nome)
                .ToList();
                
            foreach (var categoria in categorias)
            {
                Console.WriteLine($"Categoria: {categoria.Nome}");
                Console.WriteLine($"Total de produtos: {categoria.Produtos.Count}");
                Console.WriteLine("-------------------");
            }
        }

        // ========== CRUD PRODUTOS ==========

        public void CriarProduto()
        {
            Console.WriteLine("=== CRIAR PRODUTO ===");
            
            // Listar categorias disponíveis
            var categorias = _context.Categorias.OrderBy(c => c.Nome).ToList();
            Console.WriteLine("\nCategorias disponíveis:");
            foreach (var categoria in categorias)
            {
                Console.WriteLine($"{categoria.Id} - {categoria.Nome}");
            }
            
            Console.Write("\nID da categoria: ");
            if (!int.TryParse(Console.ReadLine(), out int categoriaId))
            {
                Console.WriteLine("ID inválido!");
                return;
            }
            
            if (!_context.Categorias.Any(c => c.Id == categoriaId))
            {
                Console.WriteLine("Categoria não encontrada!");
                return;
            }
            
            Console.Write("Nome do produto: ");
            var nome = Console.ReadLine();
            
            Console.Write("Preço: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal preco))
            {
                Console.WriteLine("Preço inválido!");
                return;
            }
            
            Console.Write("Estoque inicial: ");
            if (!int.TryParse(Console.ReadLine(), out int estoque))
            {
                Console.WriteLine("Estoque inválido!");
                return;
            }
            
            try
            {
                var produto = new Produto
                {
                    Nome = nome,
                    Preco = preco,
                    Estoque = estoque,
                    CategoriaId = categoriaId
                };
                
                _context.Produtos.Add(produto);
                _context.SaveChanges();
                
                Console.WriteLine($"Produto '{nome}' criado com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao criar produto: {ex.Message}");
            }
        }

        public void ListarProdutos()
        {
            Console.WriteLine("=== PRODUTOS ===");
            
            var produtos = _context.Produtos
                .Include(p => p.Categoria)
                .OrderBy(p => p.Categoria.Nome)
                .ThenBy(p => p.Nome)
                .ToList();
                
            string? categoriaAtual = null;
            foreach (var produto in produtos)
            {
                if (categoriaAtual != produto.Categoria.Nome)
                {
                    categoriaAtual = produto.Categoria.Nome;
                    Console.WriteLine($"\nCategoria: {categoriaAtual}");
                    Console.WriteLine("-------------------");
                }
                
                Console.WriteLine($"Nome: {produto.Nome}");
                Console.WriteLine($"Preço: {produto.Preco:C}");
                Console.WriteLine($"Estoque: {produto.Estoque}");
                Console.WriteLine("-------------------");
            }
        }

        public void AtualizarProduto()
        {
            Console.WriteLine("=== ATUALIZAR PRODUTO ===");
            
            Console.Write("ID do produto: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("ID inválido!");
                return;
            }
            
            var produto = _context.Produtos
                .Include(p => p.Categoria)
                .FirstOrDefault(p => p.Id == id);
                
            if (produto == null)
            {
                Console.WriteLine("Produto não encontrado!");
                return;
            }
            
            Console.WriteLine($"Produto atual: {produto.Nome}");
            Console.WriteLine($"Categoria: {produto.Categoria.Nome}");
            Console.WriteLine($"Preço atual: {produto.Preco:C}");
            Console.WriteLine($"Estoque atual: {produto.Estoque}");
            
            Console.Write("\nNovo nome (vazio para manter): ");
            var nome = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(nome))
                produto.Nome = nome;
            
            Console.Write("Novo preço (vazio para manter): ");
            var precoStr = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(precoStr) && decimal.TryParse(precoStr, out decimal preco))
                produto.Preco = preco;
            
            Console.Write("Novo estoque (vazio para manter): ");
            var estoqueStr = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(estoqueStr) && int.TryParse(estoqueStr, out int estoque))
                produto.Estoque = estoque;
            
            try
            {
                _context.SaveChanges();
                Console.WriteLine("Produto atualizado com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao atualizar produto: {ex.Message}");
            }
        }

        // ========== CRUD CLIENTES ==========

        public void CriarCliente()
        {
            Console.WriteLine("=== CRIAR CLIENTE ===");
            
            Console.Write("Nome do cliente: ");
            var nome = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(nome))
            {
                Console.WriteLine("Nome é obrigatório!");
                return;
            }
            
            Console.Write("Email: ");
            var email = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(email))
            {
                Console.WriteLine("Email é obrigatório!");
                return;
            }
            
            // Verificar se email já existe
            if (_context.Clientes.Any(c => c.Email == email))
            {
                Console.WriteLine("Email já cadastrado!");
                return;
            }
            
            Console.Write("CPF (somente números): ");
            var cpf = Console.ReadLine()?.Replace(".", "").Replace("-", "");
            if (string.IsNullOrWhiteSpace(cpf) || !cpf.All(char.IsDigit) || cpf.Length != 11)
            {
                Console.WriteLine("CPF inválido! Digite apenas os 11 números.");
                return;
            }
            
            try
            {
                var cliente = new Cliente
                {
                    Nome = nome,
                    Email = email,
                    CPF = cpf
                };
                
                _context.Clientes.Add(cliente);
                _context.SaveChanges();
                
                Console.WriteLine($"Cliente '{nome}' criado com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao criar cliente: {ex.Message}");
            }
        }

        public void ListarClientes()
        {
            Console.WriteLine("=== CLIENTES ===");
            
            var clientes = _context.Clientes
                .Include(c => c.Pedidos)
                .OrderBy(c => c.Nome)
                .ToList();
                
            foreach (var cliente in clientes)
            {
                Console.WriteLine($"Nome: {cliente.Nome}");
                Console.WriteLine($"Email: {cliente.Email}");
                Console.WriteLine($"CPF: {cliente.CPF}");
                Console.WriteLine($"Total de pedidos: {cliente.Pedidos.Count}");
                if (cliente.Pedidos.Any())
                {
                    var valorTotal = cliente.Pedidos
                        .SelectMany(p => p.Itens)
                        .Sum(i => i.Quantidade * i.PrecoUnitario);
                    Console.WriteLine($"Valor total em pedidos: {valorTotal:C}");
                }
                Console.WriteLine("-------------------");
            }
        }

        public void AtualizarCliente()
        {
            Console.WriteLine("=== ATUALIZAR CLIENTE ===");
            
            Console.Write("Email do cliente: ");
            var email = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(email))
            {
                Console.WriteLine("Email é obrigatório!");
                return;
            }
            
            var cliente = _context.Clientes.FirstOrDefault(c => c.Email == email);
            if (cliente == null)
            {
                Console.WriteLine("Cliente não encontrado!");
                return;
            }
            
            Console.WriteLine($"Cliente atual: {cliente.Nome}");
            
            Console.Write("Novo nome (vazio para manter): ");
            var nome = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(nome))
                cliente.Nome = nome;
            
            Console.Write("Novo email (vazio para manter): ");
            var novoEmail = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(novoEmail))
            {
                if (_context.Clientes.Any(c => c.Email == novoEmail && c.Id != cliente.Id))
                {
                    Console.WriteLine("Email já cadastrado para outro cliente!");
                    return;
                }
                cliente.Email = novoEmail;
            }
            
            Console.Write("Novo CPF (somente números, vazio para manter): ");
            var cpf = Console.ReadLine()?.Replace(".", "").Replace("-", "");
            if (!string.IsNullOrWhiteSpace(cpf))
            {
                if (!cpf.All(char.IsDigit) || cpf.Length != 11)
                {
                    Console.WriteLine("CPF inválido! Digite apenas os 11 números.");
                    return;
                }
                cliente.CPF = cpf;
            }
            
            try
            {
                _context.SaveChanges();
                Console.WriteLine("Cliente atualizado com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao atualizar cliente: {ex.Message}");
            }
        }

        // ========== CRUD PEDIDOS ==========

        public void CriarPedido()
        {
            Console.WriteLine("=== CRIAR PEDIDO ===");
            
            // Solicitar cliente
            Console.Write("ID do cliente: ");
            if (!int.TryParse(Console.ReadLine(), out int clienteId))
            {
                Console.WriteLine("ID inválido!");
                return;
            }
            
            var cliente = _context.Clientes.FirstOrDefault(c => c.Id == clienteId);
            if (cliente == null)
            {
                Console.WriteLine("Cliente não encontrado!");
                return;
            }
            
            // Gerar número do pedido
            var ultimoPedido = _context.Pedidos
                .OrderByDescending(p => p.NumeroPedido)
                .FirstOrDefault();
                
            string numeroPedido;
            if (ultimoPedido == null)
            {
                numeroPedido = "P001";
            }
            else
            {
                var numero = int.Parse(ultimoPedido.NumeroPedido[1..]) + 1;
                numeroPedido = $"P{numero:000}";
            }
            
            // Criar pedido
            var pedido = new Pedido
            {
                ClienteId = clienteId,
                NumeroPedido = numeroPedido,
                DataPedido = DateTime.Now,
                Status = StatusPedido.Pendente
            };
            
            _context.Pedidos.Add(pedido);
            _context.SaveChanges();
            
            // Adicionar itens
            while (true)
            {
                Console.WriteLine("\nAdicionar item ao pedido:");
                Console.WriteLine("0 - Finalizar pedido");
                
                // Listar produtos disponíveis
                var produtos = _context.Produtos
                    .Include(p => p.Categoria)
                    .Where(p => p.Estoque > 0)
                    .OrderBy(p => p.Categoria.Nome)
                    .ThenBy(p => p.Nome)
                    .ToList();
                    
                foreach (var prod in produtos)
                {
                    Console.WriteLine($"{prod.Id} - {prod.Nome} ({prod.Categoria.Nome}) - {prod.Preco:C} - Estoque: {prod.Estoque}");
                }
                
                Console.Write("\nID do produto (0 para finalizar): ");
                if (!int.TryParse(Console.ReadLine(), out int produtoId))
                {
                    Console.WriteLine("ID inválido!");
                    continue;
                }
                
                if (produtoId == 0)
                    break;
                
                var produto = produtos.FirstOrDefault(p => p.Id == produtoId);
                if (produto == null)
                {
                    Console.WriteLine("Produto não encontrado!");
                    continue;
                }
                
                Console.Write($"Quantidade (disponível: {produto.Estoque}): ");
                if (!int.TryParse(Console.ReadLine(), out int quantidade) || quantidade <= 0)
                {
                    Console.WriteLine("Quantidade inválida!");
                    continue;
                }
                
                if (quantidade > produto.Estoque)
                {
                    Console.WriteLine("Quantidade maior que o estoque disponível!");
                    continue;
                }
                
                var item = new PedidoItem
                {
                    PedidoId = pedido.Id,
                    ProdutoId = produto.Id,
                    Quantidade = quantidade,
                    PrecoUnitario = produto.Preco
                };
                
                _context.PedidoItens.Add(item);
                
                // Atualizar estoque
                produto.Estoque -= quantidade;
                
                _context.SaveChanges();
            }
            
            var totalPedido = _context.PedidoItens
                .Where(pi => pi.PedidoId == pedido.Id)
                .Sum(pi => pi.Quantidade * pi.PrecoUnitario);
                
            Console.WriteLine($"\nPedido {numeroPedido} criado com sucesso!");
            Console.WriteLine($"Total do pedido: {totalPedido:C}");
        }

        public void ListarPedidos()
        {
            Console.WriteLine("=== PEDIDOS ===");
            
            var pedidos = _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Itens)
                    .ThenInclude(i => i.Produto)
                .OrderByDescending(p => p.DataPedido)
                .ToList();
                
            foreach (var pedido in pedidos)
            {
                Console.WriteLine($"\nPedido: {pedido.NumeroPedido}");
                Console.WriteLine($"Data: {pedido.DataPedido:dd/MM/yyyy HH:mm}");
                Console.WriteLine($"Cliente: {pedido.Cliente.Nome}");
                Console.WriteLine($"Status: {pedido.Status}");
                Console.WriteLine("\nItens:");
                
                decimal total = 0;
                foreach (var item in pedido.Itens)
                {
                    var subtotal = item.Quantidade * item.PrecoUnitario;
                    total += subtotal;
                    
                    Console.WriteLine($"- {item.Produto.Nome}");
                    Console.WriteLine($"  Quantidade: {item.Quantidade}");
                    Console.WriteLine($"  Preço: {item.PrecoUnitario:C}");
                    Console.WriteLine($"  Subtotal: {subtotal:C}");
                }
                
                Console.WriteLine($"\nTotal do pedido: {total:C}");
                Console.WriteLine("-------------------");
            }
        }

        public void AtualizarStatusPedido()
        {
            Console.WriteLine("=== ATUALIZAR STATUS PEDIDO ===");
            
            Console.Write("Número do pedido: ");
            var numeroPedido = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(numeroPedido))
            {
                Console.WriteLine("Número do pedido é obrigatório!");
                return;
            }
            
            var pedido = _context.Pedidos.FirstOrDefault(p => p.NumeroPedido == numeroPedido);
            if (pedido == null)
            {
                Console.WriteLine("Pedido não encontrado!");
                return;
            }
            
            Console.WriteLine($"\nStatus atual: {pedido.Status}");
            Console.WriteLine("\nStatus disponíveis:");
            
            var statusValues = Enum.GetValues(typeof(StatusPedido))
                .Cast<StatusPedido>()
                .ToList();
                
            foreach (var status in statusValues)
            {
                Console.WriteLine($"{(int)status} - {status}");
            }
            
            Console.Write("\nNovo status: ");
            if (!int.TryParse(Console.ReadLine(), out int novoStatus))
            {
                Console.WriteLine("Status inválido!");
                return;
            }
            
            if (!Enum.IsDefined(typeof(StatusPedido), novoStatus))
            {
                Console.WriteLine("Status inválido!");
                return;
            }
            
            // Validar transições válidas
            var statusAtual = pedido.Status;
            var novoStatusEnum = (StatusPedido)novoStatus;
            
            bool transicaoValida = (statusAtual, novoStatusEnum) switch
            {
                (StatusPedido.Pendente, StatusPedido.Confirmado) => true,
                (StatusPedido.Pendente, StatusPedido.Cancelado) => true,
                (StatusPedido.Confirmado, StatusPedido.EmAndamento) => true,
                (StatusPedido.Confirmado, StatusPedido.Cancelado) => true,
                (StatusPedido.EmAndamento, StatusPedido.Entregue) => true,
                _ => false
            };
            
            if (!transicaoValida)
            {
                Console.WriteLine($"Transição de status inválida: {statusAtual} -> {novoStatusEnum}");
                return;
            }
            
            pedido.Status = novoStatusEnum;
            
            try
            {
                _context.SaveChanges();
                Console.WriteLine("Status atualizado com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao atualizar status: {ex.Message}");
            }
        }

        public void CancelarPedido()
        {
            Console.WriteLine("=== CANCELAR PEDIDO ===");
            
            Console.Write("Número do pedido: ");
            var numeroPedido = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(numeroPedido))
            {
                Console.WriteLine("Número do pedido é obrigatório!");
                return;
            }
            
            var pedido = _context.Pedidos
                .Include(p => p.Itens)
                    .ThenInclude(i => i.Produto)
                .FirstOrDefault(p => p.NumeroPedido == numeroPedido);
                
            if (pedido == null)
            {
                Console.WriteLine("Pedido não encontrado!");
                return;
            }
            
            if (pedido.Status != StatusPedido.Pendente && pedido.Status != StatusPedido.Confirmado)
            {
                Console.WriteLine($"Não é possível cancelar um pedido com status {pedido.Status}!");
                return;
            }
            
            try
            {
                // Devolver produtos ao estoque
                foreach (var item in pedido.Itens)
                {
                    item.Produto.Estoque += item.Quantidade;
                }
                
                pedido.Status = StatusPedido.Cancelado;
                _context.SaveChanges();
                
                Console.WriteLine("Pedido cancelado com sucesso!");
                Console.WriteLine("Produtos devolvidos ao estoque.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao cancelar pedido: {ex.Message}");
            }
            // TODO: Implementar cancelamento 
            // - Pedir id do pedido
            // - Validar se o pedido existe
            // - Só permite cancelar se status = Pendente ou Confirmado
            // - Devolver estoque dos produtos
            Console.WriteLine("=== CANCELAR PEDIDO ===");
        }

        // ========== CONSULTAS LINQ AVANÇADAS ==========

        public void ConsultasAvancadas()
        {
            Console.WriteLine("=== CONSULTAS LINQ ===");
            Console.WriteLine("1. Produtos mais vendidos");
            Console.WriteLine("2. Clientes com mais pedidos");
            Console.WriteLine("3. Faturamento por categoria");
            Console.WriteLine("4. Pedidos por período");
            Console.WriteLine("5. Produtos em estoque baixo");
            

            var opcao = Console.ReadLine();

            switch (opcao)
            {
                case "1": ProdutosMaisVendidos(); break;
                case "2": ClientesComMaisPedidos(); break;
                case "3": FaturamentoPorCategoria(); break;
                case "4": PedidosPorPeriodo(); break;
                case "5": ProdutosEstoqueBaixo(); break;
                case "6": AnaliseVendasMensal(); break;
                case "7": TopClientesPorValor(); break;
            }
        }

        private void ProdutosMaisVendidos()
        {
            var produtos = _context.PedidoItens
                .Include(pi => pi.Produto)
                    .ThenInclude(p => p.Categoria)
                .AsEnumerable() // Força execução no lado do cliente
                .GroupBy(pi => pi.Produto)
                .Select(g => new
                {
                    Produto = g.Key,
                    Categoria = g.Key.Categoria.Nome,
                    QuantidadeVendida = g.Sum(pi => pi.Quantidade),
                    ValorTotal = g.Sum(pi => pi.Quantidade * pi.PrecoUnitario)
                })
                .OrderByDescending(x => x.QuantidadeVendida)
                .ToList();
                
            Console.WriteLine("\nProdutos mais vendidos:");
            foreach (var item in produtos)
            {
                Console.WriteLine($"\nProduto: {item.Produto.Nome}");
                Console.WriteLine($"Categoria: {item.Categoria}");
                Console.WriteLine($"Quantidade vendida: {item.QuantidadeVendida}");
                Console.WriteLine($"Valor total: {item.ValorTotal:C}");
                Console.WriteLine("-------------------");
            }
        }

        private void ClientesComMaisPedidos()
        {
            var clientes = _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Itens)
                .AsEnumerable() // Força execução no lado do cliente
                .GroupBy(p => p.Cliente)
                .Select(g => new
                {
                    Cliente = g.Key,
                    TotalPedidos = g.Count(),
                    ValorTotal = g.SelectMany(p => p.Itens)
                        .Sum(i => i.Quantidade * i.PrecoUnitario),
                    TicketMedio = g.SelectMany(p => p.Itens)
                        .Sum(i => i.Quantidade * i.PrecoUnitario) / g.Count()
                })
                .OrderByDescending(x => x.TotalPedidos)
                .ToList();
                
            Console.WriteLine("\nClientes com mais pedidos:");
            foreach (var item in clientes)
            {
                Console.WriteLine($"\nCliente: {item.Cliente.Nome}");
                Console.WriteLine($"Total de pedidos: {item.TotalPedidos}");
                Console.WriteLine($"Valor total: {item.ValorTotal:C}");
                Console.WriteLine($"Ticket médio: {item.TicketMedio:C}");
                Console.WriteLine("-------------------");
            }
        }

        private void FaturamentoPorCategoria()
        {
            var pedidoItens = _context.PedidoItens
                .Include(pi => pi.Produto)
                    .ThenInclude(p => p.Categoria)
                .AsEnumerable();
                
            var totalGeral = pedidoItens.Sum(pi => pi.Quantidade * pi.PrecoUnitario);
            
            var categorias = pedidoItens
                .GroupBy(pi => pi.Produto.Categoria)
                .Select(g => new
                {
                    Categoria = g.Key.Nome,
                    ValorTotal = g.Sum(pi => pi.Quantidade * pi.PrecoUnitario),
                    QuantidadeVendida = g.Sum(pi => pi.Quantidade),
                    ParticipacaoPercentual = g.Sum(pi => pi.Quantidade * pi.PrecoUnitario) / totalGeral * 100
                })
                .OrderByDescending(x => x.ValorTotal)
                .ToList();
                
            Console.WriteLine("\nFaturamento por categoria:");
            foreach (var cat in categorias)
            {
                Console.WriteLine($"\nCategoria: {cat.Categoria}");
                Console.WriteLine($"Valor total: {cat.ValorTotal:C}");
                Console.WriteLine($"Quantidade vendida: {cat.QuantidadeVendida}");
                Console.WriteLine($"Participação: {cat.ParticipacaoPercentual:F2}%");
                Console.WriteLine("-------------------");
            }
        }

        private void PedidosPorPeriodo()
        {
            Console.Write("\nData inicial (dd/MM/yyyy): ");
            if (!DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime dataInicial))
            {
                Console.WriteLine("Data inválida!");
                return;
            }
            
            Console.Write("Data final (dd/MM/yyyy): ");
            if (!DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime dataFinal))
            {
                Console.WriteLine("Data inválida!");
                return;
            }
            
            var pedidos = _context.Pedidos
                .Include(p => p.Itens)
                .Where(p => p.DataPedido.Date >= dataInicial.Date && p.DataPedido.Date <= dataFinal.Date)
                .AsEnumerable()
                .GroupBy(p => p.DataPedido.Date)
                .Select(g => new
                {
                    Data = g.Key,
                    QuantidadePedidos = g.Count(),
                    ValorTotal = g.SelectMany(p => p.Itens).Sum(i => i.Quantidade * i.PrecoUnitario),
                    MediaPorPedido = g.SelectMany(p => p.Itens).Sum(i => i.Quantidade * i.PrecoUnitario) / g.Count()
                })
                .OrderBy(x => x.Data)
                .ToList();
                
            Console.WriteLine("\nPedidos por período:");
            foreach (var dia in pedidos)
            {
                Console.WriteLine($"\nData: {dia.Data:dd/MM/yyyy}");
                Console.WriteLine($"Quantidade de pedidos: {dia.QuantidadePedidos}");
                Console.WriteLine($"Valor total: {dia.ValorTotal:C}");
                Console.WriteLine($"Média por pedido: {dia.MediaPorPedido:C}");
                Console.WriteLine("-------------------");
            }
            
            // Resumo do período
            if (pedidos.Any())
            {
                var resumo = new
                {
                    TotalPedidos = pedidos.Sum(x => x.QuantidadePedidos),
                    ValorTotal = pedidos.Sum(x => x.ValorTotal),
                    MediaDiaria = pedidos.Average(x => x.ValorTotal)
                };
                
                Console.WriteLine("\nResumo do período:");
                Console.WriteLine($"Total de pedidos: {resumo.TotalPedidos}");
                Console.WriteLine($"Valor total: {resumo.ValorTotal:C}");
                Console.WriteLine($"Média diária: {resumo.MediaDiaria:C}");
            }
        }

        private void ProdutosEstoqueBaixo()
        {
            var produtosDb = _context.Produtos
                .Include(p => p.Categoria)
                .Where(p => p.Estoque < 5)
                .AsEnumerable();
                
            var produtos = produtosDb
                .Select(p => new
                {
                    p.Nome,
                    CategoriaNome = p.Categoria.Nome,
                    p.Estoque,
                    p.Preco,
                    ValorParaReposicao = (5 - p.Estoque) * p.Preco
                })
                .OrderBy(x => x.Estoque)
                .ThenByDescending(x => x.ValorParaReposicao)
                .ToList();
                
            Console.WriteLine("\nProdutos com estoque baixo (menos de 5 unidades):");
            foreach (var prod in produtos)
            {
                Console.WriteLine($"\nProduto: {prod.Nome}");
                Console.WriteLine($"Categoria: {prod.CategoriaNome}");
                Console.WriteLine($"Estoque atual: {prod.Estoque}");
                Console.WriteLine($"Preço unitário: {prod.Preco:C}");
                Console.WriteLine($"Valor para repor até 5 unidades: {prod.ValorParaReposicao:C}");
                Console.WriteLine("-------------------");
            }
            
            if (produtos.Any())
            {
                var totalReposicao = produtos.Sum(p => p.ValorParaReposicao);
                Console.WriteLine($"\nValor total necessário para reposição: {totalReposicao:C}");
            }
            else
            {
                Console.WriteLine("\nNão há produtos com estoque baixo.");
            }
        }

        private void AnaliseVendasMensal()
        {
            var pedidoItens = _context.PedidoItens
                .Include(pi => pi.Pedido)
                .AsEnumerable();
                
            var vendas = pedidoItens
                .GroupBy(pi => new { Ano = pi.Pedido.DataPedido.Year, Mes = pi.Pedido.DataPedido.Month })
                .Select(g => new
                {
                    g.Key.Ano,
                    g.Key.Mes,
                    QuantidadeVendida = g.Sum(pi => pi.Quantidade),
                    ValorTotal = g.Sum(pi => pi.Quantidade * pi.PrecoUnitario),
                    QtdPedidos = g.Select(pi => pi.PedidoId).Distinct().Count()
                })
                .OrderBy(x => x.Ano)
                .ThenBy(x => x.Mes)
                .ToList();
                
            Console.WriteLine("\nAnálise de vendas mensal:");
            
            decimal? valorAnterior = null;
            var mediasMoveis = new Queue<decimal>(3);
            
            foreach (var venda in vendas)
            {
                Console.WriteLine($"\nPeríodo: {venda.Mes:00}/{venda.Ano}");
                Console.WriteLine($"Quantidade vendida: {venda.QuantidadeVendida}");
                Console.WriteLine($"Quantidade de pedidos: {venda.QtdPedidos}");
                Console.WriteLine($"Ticket médio: {venda.ValorTotal / venda.QtdPedidos:C}");
                Console.WriteLine($"Valor total: {venda.ValorTotal:C}");
                
                if (valorAnterior.HasValue)
                {
                    var variacao = (venda.ValorTotal - valorAnterior.Value) / valorAnterior.Value * 100;
                    Console.WriteLine($"Variação em relação ao mês anterior: {variacao:F2}%");
                }
                
                mediasMoveis.Enqueue(venda.ValorTotal);
                if (mediasMoveis.Count > 3)
                    mediasMoveis.Dequeue();
                    
                if (mediasMoveis.Count == 3)
                {
                    var mediaMovel = mediasMoveis.Average();
                    Console.WriteLine($"Média móvel (3 meses): {mediaMovel:C}");
                }
                
                Console.WriteLine("-------------------");
                valorAnterior = venda.ValorTotal;
            }
            
            if (vendas.Any())
            {
                var primeiroMes = vendas.First();
                var ultimoMes = vendas.Last();
                var crescimento = (ultimoMes.ValorTotal - primeiroMes.ValorTotal) / primeiroMes.ValorTotal * 100;
                
                Console.WriteLine($"\nCrescimento total no período: {crescimento:F2}%");
                Console.WriteLine($"Média mensal de vendas: {vendas.Average(v => v.ValorTotal):C}");
                Console.WriteLine($"Melhor mês: {vendas.MaxBy(v => v.ValorTotal)?.Mes:00}/{vendas.MaxBy(v => v.ValorTotal)?.Ano} - {vendas.Max(v => v.ValorTotal):C}");
                Console.WriteLine($"Pior mês: {vendas.MinBy(v => v.ValorTotal)?.Mes:00}/{vendas.MinBy(v => v.ValorTotal)?.Ano} - {vendas.Min(v => v.ValorTotal):C}");
            }
        }

        private void TopClientesPorValor()
        {
            var pedidos = _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Itens)
                .Where(p => p.Status != StatusPedido.Cancelado)
                .AsEnumerable();
                
            var totalGeral = pedidos.SelectMany(p => p.Itens).Sum(pi => pi.Quantidade * pi.PrecoUnitario);
            
            var topClientes = pedidos
                .GroupBy(p => p.Cliente)
                .Select(g => new
                {
                    Cliente = g.Key,
                    ValorTotal = g.SelectMany(p => p.Itens).Sum(i => i.Quantidade * i.PrecoUnitario),
                    QuantidadePedidos = g.Count(),
                    TicketMedio = g.SelectMany(p => p.Itens).Sum(i => i.Quantidade * i.PrecoUnitario) / g.Count()
                })
                .OrderByDescending(x => x.ValorTotal)
                .Take(10)
                .ToList();
                
            Console.WriteLine("\nTop 10 clientes por valor:");
            foreach (var cliente in topClientes)
            {
                var participacao = cliente.ValorTotal / totalGeral * 100;
                
                Console.WriteLine($"\nCliente: {cliente.Cliente.Nome}");
                Console.WriteLine($"Valor total: {cliente.ValorTotal:C}");
                Console.WriteLine($"Quantidade de pedidos: {cliente.QuantidadePedidos}");
                Console.WriteLine($"Ticket médio: {cliente.TicketMedio:C}");
                Console.WriteLine($"Participação no faturamento: {participacao:F2}%");
                Console.WriteLine("-------------------");
            }
            
            if (topClientes.Any())
            {
                var totalTop10 = topClientes.Sum(c => c.ValorTotal);
                var participacaoTop10 = totalTop10 / totalGeral * 100;
                Console.WriteLine($"\nConcentração do faturamento:");
                Console.WriteLine($"Top 10 clientes: {participacaoTop10:F2}% do faturamento total");
            }
        }

        // ========== RELATÓRIOS GERAIS ==========

        public void RelatoriosGerais()
        {
            Console.WriteLine("=== RELATÓRIOS GERAIS ===");
            Console.WriteLine("1. Dashboard executivo");
            Console.WriteLine("2. Relatório de estoque");
            Console.WriteLine("3. Análise de clientes");

            var opcao = Console.ReadLine();

            switch (opcao)
            {
                case "1": DashboardExecutivo(); break;
                case "2": RelatorioEstoque(); break;
                case "3": AnaliseClientes(); break;
            }
        }

        private void DashboardExecutivo()
        {
            Console.WriteLine("\n=== DASHBOARD EXECUTIVO ===\n");
            
            // Indicadores gerais
            var totalPedidos = _context.Pedidos.Count();
            var pedidosCancelados = _context.Pedidos.Count(p => p.Status == StatusPedido.Cancelado);
            var taxaCancelamento = totalPedidos > 0 ? (decimal)pedidosCancelados / totalPedidos * 100 : 0;
            
            var pedidoItens = _context.PedidoItens.AsEnumerable();
            var valorTotal = pedidoItens.Sum(pi => pi.Quantidade * pi.PrecoUnitario);
            var ticketMedio = totalPedidos > 0 ? valorTotal / totalPedidos : 0;
            
            Console.WriteLine("INDICADORES GERAIS");
            Console.WriteLine($"Total de pedidos: {totalPedidos}");
            Console.WriteLine($"Pedidos cancelados: {pedidosCancelados} ({taxaCancelamento:F2}%)");
            Console.WriteLine($"Valor total vendido: {valorTotal:C}");
            Console.WriteLine($"Ticket médio: {ticketMedio:C}");
            
            // Estoque
            var produtos = _context.Produtos.AsEnumerable();
            var produtosEstoque = produtos.Count();
            var produtosZerados = produtos.Count(p => p.Estoque == 0);
            var produtosBaixos = produtos.Count(p => p.Estoque > 0 && p.Estoque < 5);
            var valorEstoque = produtos.Sum(p => p.Estoque * p.Preco);
            
            Console.WriteLine("\nESTOQUE");
            Console.WriteLine($"Total de produtos: {produtosEstoque}");
            Console.WriteLine($"Produtos zerados: {produtosZerados}");
            Console.WriteLine($"Produtos em estoque baixo: {produtosBaixos}");
            Console.WriteLine($"Valor total em estoque: {valorEstoque:C}");
            
            // Clientes
            var totalClientes = _context.Clientes.Count();
            var clientesAtivos = _context.Clientes
                .Count(c => c.Pedidos.Any(p => p.Status != StatusPedido.Cancelado));
            var clientesInativos = totalClientes - clientesAtivos;
            
            Console.WriteLine("\nCLIENTES");
            Console.WriteLine($"Total de clientes: {totalClientes}");
            Console.WriteLine($"Clientes ativos: {clientesAtivos}");
            Console.WriteLine($"Clientes inativos: {clientesInativos}");
            
            // Faturamento mensal (últimos 6 meses)
            var pedidosUltimosSeisMeses = _context.Pedidos
                .Where(p => p.Status != StatusPedido.Cancelado)
                .Include(p => p.Itens)
                .AsEnumerable();
                
            var ultimosSeisMeses = pedidosUltimosSeisMeses
                .GroupBy(p => new { p.DataPedido.Year, p.DataPedido.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Valor = g.SelectMany(p => p.Itens).Sum(i => i.Quantidade * i.PrecoUnitario)
                })
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .Take(6)
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();
                
            Console.WriteLine("\nFATURAMENTO MENSAL");
            foreach (var mes in ultimosSeisMeses)
            {
                Console.WriteLine($"{mes.Month:00}/{mes.Year}: {mes.Valor:C}");
            }
        }

        private void RelatorioEstoque()
        {
            Console.WriteLine("\n=== RELATÓRIO DE ESTOQUE ===\n");
            
            // Produtos por categoria
            var categorias = _context.Categorias
                .Include(c => c.Produtos)
                .AsEnumerable()
                .Select(c => new
                {
                    c.Nome,
                    QtdProdutos = c.Produtos.Count,
                    ValorEstoque = c.Produtos.Sum(p => p.Estoque * p.Preco),
                    ProdutosZerados = c.Produtos.Count(p => p.Estoque == 0),
                    ProdutosBaixos = c.Produtos.Count(p => p.Estoque > 0 && p.Estoque < 5),
                    ProdutosBaixosList = c.Produtos.Where(p => p.Estoque > 0 && p.Estoque < 5)
                        .Select(p => new { p.Nome, p.Estoque, p.Preco, ValorReposicao = (5 - p.Estoque) * p.Preco })
                        .OrderBy(p => p.Estoque)
                })
                .OrderBy(x => x.Nome)
                .ToList();
                
            Console.WriteLine("PRODUTOS POR CATEGORIA");
            foreach (var cat in categorias)
            {
                Console.WriteLine($"\nCategoria: {cat.Nome}");
                Console.WriteLine($"Quantidade de produtos: {cat.QtdProdutos}");
                Console.WriteLine($"Valor em estoque: {cat.ValorEstoque:C}");
                Console.WriteLine($"Produtos zerados: {cat.ProdutosZerados}");
                Console.WriteLine($"Produtos em estoque baixo: {cat.ProdutosBaixos}");
            }
            
            // Produtos zerados
            var zerados = _context.Produtos
                .Include(p => p.Categoria)
                .Where(p => p.Estoque == 0)
                .OrderBy(p => p.Categoria.Nome)
                .ThenBy(p => p.Nome)
                .ToList();
                
            Console.WriteLine("\nPRODUTOS COM ESTOQUE ZERADO");
            foreach (var prod in zerados)
            {
                Console.WriteLine($"{prod.Nome} ({prod.Categoria.Nome})");
                Console.WriteLine($"Preço: {prod.Preco:C}");
            }
            
            // Produtos em estoque baixo
            var baixos = _context.Produtos
                .Include(p => p.Categoria)
                .Where(p => p.Estoque > 0 && p.Estoque < 5)
                .OrderBy(p => p.Estoque)
                .ThenBy(p => p.Nome)
                .ToList();
                
            Console.WriteLine("\nPRODUTOS COM ESTOQUE BAIXO");
            foreach (var prod in baixos)
            {
                Console.WriteLine($"{prod.Nome} ({prod.Categoria.Nome})");
                Console.WriteLine($"Estoque atual: {prod.Estoque}");
                Console.WriteLine($"Preço: {prod.Preco:C}");
                Console.WriteLine($"Valor para repor até 5 unidades: {(5 - prod.Estoque) * prod.Preco:C}");
            }
            
            // Resumo geral
            var resumo = new
            {
                TotalProdutos = _context.Produtos.Count(),
                ValorTotalEstoque = _context.Produtos.Sum(p => p.Estoque * p.Preco),
                TotalZerados = zerados.Count,
                TotalBaixos = baixos.Count,
                ValorReposicao = baixos.Sum(p => (5 - p.Estoque) * p.Preco)
            };
            
            Console.WriteLine("\nRESUMO GERAL");
            Console.WriteLine($"Total de produtos: {resumo.TotalProdutos}");
            Console.WriteLine($"Valor total em estoque: {resumo.ValorTotalEstoque:C}");
            Console.WriteLine($"Produtos zerados: {resumo.TotalZerados}");
            Console.WriteLine($"Produtos em estoque baixo: {resumo.TotalBaixos}");
            Console.WriteLine($"Valor necessário para reposição: {resumo.ValorReposicao:C}");
        }

        private void AnaliseClientes()
        {
            Console.WriteLine("\n=== ANÁLISE DE CLIENTES ===\n");
            
            // Análise geral
            var analiseGeral = _context.Clientes
                .AsEnumerable()
                .Select(c => new
                {
                    TotalClientes = _context.Clientes.Count(),
                    ClientesAtivos = _context.Clientes.Count(c => c.Pedidos.Any(p => p.DataPedido >= DateTime.Now.AddMonths(-3))),
                    MediaCompras = _context.Clientes
                        .Where(c => c.Pedidos.Any(p => p.Status != StatusPedido.Cancelado))
                        .Average(c => c.Pedidos
                            .Where(p => p.Status != StatusPedido.Cancelado)
                            .SelectMany(p => p.Itens)
                            .Sum(i => i.Quantidade * i.PrecoUnitario))
                })
                .FirstOrDefault();

            if (analiseGeral != null)
            {
                Console.WriteLine("\nANÁLISE GERAL DE CLIENTES");
                Console.WriteLine($"Total de clientes cadastrados: {analiseGeral.TotalClientes}");
                Console.WriteLine($"Clientes ativos (últimos 3 meses): {analiseGeral.ClientesAtivos}");
                Console.WriteLine($"Média de compras por cliente: {analiseGeral.MediaCompras:C}");
                Console.WriteLine("-------------------");
            }

            // Top Clientes
            var topClientes = _context.Clientes
                .AsEnumerable()
                .Select(c => new
                {
                    c.Nome,
                    TotalPedidos = c.Pedidos.Count(p => p.Status != StatusPedido.Cancelado),
                    UltimoPedido = c.Pedidos.Max(p => p.DataPedido),
                    TotalCompras = c.Pedidos
                        .Where(p => p.Status != StatusPedido.Cancelado)
                        .SelectMany(p => p.Itens)
                        .Sum(i => i.Quantidade * i.PrecoUnitario)
                })
                .OrderByDescending(x => x.TotalCompras)
                .Take(5)
                .ToList();

            Console.WriteLine("\nTOP 5 CLIENTES");
            foreach (var cliente in topClientes)
            {
                Console.WriteLine($"\nCliente: {cliente.Nome}");
                Console.WriteLine($"Total de pedidos: {cliente.TotalPedidos}");
                Console.WriteLine($"Total em compras: {cliente.TotalCompras:C}");
                Console.WriteLine($"Último pedido: {cliente.UltimoPedido:dd/MM/yyyy}");
                if (cliente.TotalPedidos > 0)
                {
                    Console.WriteLine($"Ticket médio: {(cliente.TotalCompras / cliente.TotalPedidos):C}");
                }
                Console.WriteLine("-------------------");
            }

            // Resumo geral
            var resumo = _context.Clientes
                .AsEnumerable()
                .Select(c => new
                {
                    TotalClientes = _context.Clientes.Count(),
                    ClientesAtivos = _context.Clientes.Count(c => c.Pedidos.Any(p => p.DataPedido >= DateTime.Now.AddMonths(-3))),
                    MediaCompras = _context.Clientes
                        .Where(c => c.Pedidos.Any(p => p.Status != StatusPedido.Cancelado))
                        .Average(c => c.Pedidos
                            .Where(p => p.Status != StatusPedido.Cancelado)
                            .SelectMany(p => p.Itens)
                            .Sum(i => i.Quantidade * i.PrecoUnitario))
                })
                .FirstOrDefault();

            if (resumo != null)
            {
                Console.WriteLine("\nRESUMO GERAL");
                Console.WriteLine($"Total de clientes cadastrados: {resumo.TotalClientes}");
                Console.WriteLine($"Clientes ativos (últimos 3 meses): {resumo.ClientesAtivos}");
                Console.WriteLine($"Taxa de ativação: {(decimal)resumo.ClientesAtivos / resumo.TotalClientes:P1}");
                Console.WriteLine($"Média de compras por cliente: {resumo.MediaCompras:C}");
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
