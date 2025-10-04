using Microsoft.Data.Sqlite;

namespace CheckPoint1.Services;

public class AdoNetService
{
    private readonly string _connectionString;
        
    public AdoNetService()
    {
        // Connection string para SQLite usando o mesmo arquivo do EF
        _connectionString = "Data Source=loja.db";
    }
        
    // ========== CONSULTAS COMPLEXAS ==========
        
    public void RelatorioVendasCompleto()
    {
        Console.WriteLine("=== RELATÓRIO VENDAS COMPLETO (ADO.NET) ===");
        
        var query = @"
            SELECT 
                p.NumeroPedido,
                c.Nome as NomeCliente,
                pr.Nome as NomeProduto,
                pi.Quantidade,
                pi.PrecoUnitario,
                (pi.Quantidade * pi.PrecoUnitario) as Subtotal
            FROM Pedidos p
            INNER JOIN Clientes c ON p.ClienteId = c.Id
            INNER JOIN PedidoItens pi ON p.Id = pi.PedidoId
            INNER JOIN Produtos pr ON pi.ProdutoId = pr.Id
            ORDER BY p.DataPedido";

        using (var conn = GetConnection())
        {
            conn.Open();
            using (var cmd = new SqliteCommand(query, conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine($"Pedido: {reader["NumeroPedido"]}");
                        Console.WriteLine($"Cliente: {reader["NomeCliente"]}");
                        Console.WriteLine($"Produto: {reader["NomeProduto"]}");
                        Console.WriteLine($"Quantidade: {reader["Quantidade"]}");
                        Console.WriteLine($"Preço Unit.: {reader["PrecoUnitario"]:C}");
                        Console.WriteLine($"Subtotal: {reader["Subtotal"]:C}");
                        Console.WriteLine("-------------------");
                    }
                }
            }
        }
    }
        
    public void FaturamentoPorCliente()
    {
        Console.WriteLine("=== FATURAMENTO POR CLIENTE ===");
        
        var query = @"
            SELECT 
                c.Nome,
                COUNT(DISTINCT p.Id) as TotalPedidos,
                CAST(SUM(pi.Quantidade * pi.PrecoUnitario) AS REAL) as Faturamento,
                CAST(SUM(pi.Quantidade * pi.PrecoUnitario) AS REAL) / COUNT(DISTINCT p.Id) as TicketMedio
            FROM Clientes c
            LEFT JOIN Pedidos p ON c.Id = p.ClienteId
            LEFT JOIN PedidoItens pi ON p.Id = pi.PedidoId
            GROUP BY c.Id, c.Nome
            ORDER BY Faturamento DESC";

        using (var conn = GetConnection())
        {
            conn.Open();
            using (var cmd = new SqliteCommand(query, conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine($"Cliente: {reader["Nome"]}");
                        Console.WriteLine($"Total Pedidos: {reader["TotalPedidos"]}");
                        Console.WriteLine($"Faturamento: {reader["Faturamento"]:C}");
                        Console.WriteLine($"Ticket Médio: {reader["TicketMedio"]:C}");
                        Console.WriteLine("-------------------");
                    }
                }
            }
        }
    }
        
    public void ProdutosSemVenda()
    {
        Console.WriteLine("=== PRODUTOS SEM VENDAS ===");
        
        var query = @"
            SELECT 
                c.Nome as Categoria,
                p.Nome as Produto,
                CAST(p.Preco AS REAL) as Preco,
                p.Estoque,
                CAST(p.Preco * p.Estoque AS REAL) as ValorEstoque
            FROM Produtos p
            INNER JOIN Categorias c ON p.CategoriaId = c.Id
            LEFT JOIN PedidoItens pi ON p.Id = pi.ProdutoId
            WHERE pi.Id IS NULL";

        using (var conn = GetConnection())
        {
            conn.Open();
            using (var cmd = new SqliteCommand(query, conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    var totalValorParado = 0m;
                    while (reader.Read())
                    {
                        Console.WriteLine($"Categoria: {reader["Categoria"]}");
                        Console.WriteLine($"Produto: {reader["Produto"]}");
                        Console.WriteLine($"Preço: {reader["Preco"]:C}");
                        Console.WriteLine($"Estoque: {reader["Estoque"]}");
                        Console.WriteLine($"Valor em Estoque: {reader["ValorEstoque"]:C}");
                        Console.WriteLine("-------------------");
                        
                        totalValorParado += Convert.ToDecimal(reader["ValorEstoque"]);
                    }
                    Console.WriteLine($"Total Valor Parado em Estoque: {totalValorParado:C}");
                }
            }
        }
    }
        
    // ========== OPERAÇÕES DE DADOS ==========
        
    public void AtualizarEstoqueLote()
    {
        Console.WriteLine("=== ATUALIZAR ESTOQUE EM LOTE ===");
        
        // Listar categorias disponíveis
        using (var conn = GetConnection())
        {
            conn.Open();
            
            // Listar categorias
            var queryCategories = "SELECT Id, Nome FROM Categorias";
            using (var cmd = new SqliteCommand(queryCategories, conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    Console.WriteLine("\nCategorias disponíveis:");
                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader["Id"]} - {reader["Nome"]}");
                    }
                }
            }
            
            Console.Write("\nInforme o ID da categoria para atualização: ");
            var categoriaId = int.Parse(Console.ReadLine() ?? "0");
            
            // Listar produtos da categoria
            var queryProdutos = "SELECT Id, Nome, Estoque FROM Produtos WHERE CategoriaId = @CategoriaId";
            using (var cmd = new SqliteCommand(queryProdutos, conn))
            {
                cmd.Parameters.AddWithValue("@CategoriaId", categoriaId);
                using (var reader = cmd.ExecuteReader())
                {
                    Console.WriteLine("\nProdutos da categoria:");
                    while (reader.Read())
                    {
                        Console.WriteLine($"ID: {reader["Id"]} - {reader["Nome"]} (Estoque atual: {reader["Estoque"]})");
                        Console.Write("Nova quantidade em estoque: ");
                        var novoEstoque = int.Parse(Console.ReadLine() ?? "0");
                        
                        // Atualizar estoque
                        using (var updateCmd = new SqliteCommand(
                            "UPDATE Produtos SET Estoque = @Estoque WHERE Id = @Id", conn))
                        {
                            updateCmd.Parameters.AddWithValue("@Estoque", novoEstoque);
                            updateCmd.Parameters.AddWithValue("@Id", reader["Id"]);
                            var affected = updateCmd.ExecuteNonQuery();
                            Console.WriteLine($"Registro atualizado com sucesso! ({affected} registro afetado)");
                        }
                    }
                }
            }
        }
    }
        
    public void InserirPedidoCompleto()
    {
        Console.WriteLine("=== INSERIR PEDIDO COMPLETO ===\n");
        
        using (var conn = GetConnection())
        {
            conn.Open();
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    // 1. Listar clientes
                    var queryClientes = "SELECT Id, Nome FROM Clientes ORDER BY Nome";
                    Console.WriteLine("Clientes disponíveis:");
                    using (var cmd = new SqliteCommand(queryClientes, conn))
                    {
                        cmd.Transaction = transaction;
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Console.WriteLine($"{reader["Id"]} - {reader["Nome"]}");
                            }
                        }
                    }

                    // 2. Selecionar cliente
                    Console.Write("\nInforme o ID do cliente: ");
                    var clienteId = int.Parse(Console.ReadLine() ?? "0");

                    // 3. Criar pedido
                    var numeroPedido = DateTime.Now.ToString("yyyyMMddHHmmss");
                    var insertPedido = @"
                        INSERT INTO Pedidos (ClienteId, NumeroPedido, DataPedido, Status, ValorTotal) 
                        VALUES (@ClienteId, @NumeroPedido, @DataPedido, @Status, @ValorTotal);
                        SELECT last_insert_rowid();";

                    int pedidoId;
                    using (var cmd = new SqliteCommand(insertPedido, conn))
                    {
                        cmd.Transaction = transaction;
                        cmd.Parameters.AddWithValue("@ClienteId", clienteId);
                        cmd.Parameters.AddWithValue("@NumeroPedido", numeroPedido);
                        cmd.Parameters.AddWithValue("@DataPedido", DateTime.Now);
                        cmd.Parameters.AddWithValue("@Status", "Pendente");
                        cmd.Parameters.AddWithValue("@ValorTotal", 0m); // Inicializa com zero
                        pedidoId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // 4. Adicionar itens
                    var continuarAdicionando = true;
                    while (continuarAdicionando)
                    {
                        // 4.1 Listar produtos
                        var queryProdutos = @"
                            SELECT p.Id, p.Nome, p.Preco, p.Estoque, c.Nome as Categoria 
                            FROM Produtos p
                            JOIN Categorias c ON p.CategoriaId = c.Id
                            WHERE p.Estoque > 0
                            ORDER BY c.Nome, p.Nome";
                        
                        Console.WriteLine("\nProdutos disponíveis:");
                        using (var cmd = new SqliteCommand(queryProdutos, conn))
                        {
                            cmd.Transaction = transaction;
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    Console.WriteLine($"\nID: {reader["Id"]}");
                                    Console.WriteLine($"Produto: {reader["Nome"]}");
                                    Console.WriteLine($"Categoria: {reader["Categoria"]}");
                                    Console.WriteLine($"Preço: {reader["Preco"]:C}");
                                    Console.WriteLine($"Estoque: {reader["Estoque"]}");
                                }
                            }
                        }

                        // 4.2 Selecionar produto
                        Console.Write("\nInforme o ID do produto (0 para finalizar): ");
                        var produtoId = int.Parse(Console.ReadLine() ?? "0");
                        if (produtoId == 0) break;

                        Console.Write("Quantidade: ");
                        var quantidade = int.Parse(Console.ReadLine() ?? "1");

                        // 4.3 Verificar estoque
                        var queryEstoque = "SELECT Estoque, Preco FROM Produtos WHERE Id = @ProdutoId";
                        using (var cmd = new SqliteCommand(queryEstoque, conn))
                        {
                            cmd.Transaction = transaction;
                            cmd.Parameters.AddWithValue("@ProdutoId", produtoId);
                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    var estoque = Convert.ToInt32(reader["Estoque"]);
                                    var preco = Convert.ToDecimal(reader["Preco"]);

                                    if (quantidade > estoque)
                                    {
                                        Console.WriteLine($"Estoque insuficiente! Disponível: {estoque}");
                                        continue;
                                    }

                                    // 4.4 Inserir item
                                    reader.Close();
                                    var insertItem = @"
                                        INSERT INTO PedidoItens (PedidoId, ProdutoId, Quantidade, PrecoUnitario)
                                        VALUES (@PedidoId, @ProdutoId, @Quantidade, @PrecoUnitario)";

                                    using (var cmdItem = new SqliteCommand(insertItem, conn))
                                    {
                                        cmdItem.Transaction = transaction;
                                        cmdItem.Parameters.AddWithValue("@PedidoId", pedidoId);
                                        cmdItem.Parameters.AddWithValue("@ProdutoId", produtoId);
                                        cmdItem.Parameters.AddWithValue("@Quantidade", quantidade);
                                        cmdItem.Parameters.AddWithValue("@PrecoUnitario", preco);
                                        cmdItem.ExecuteNonQuery();
                                    }

                                    // 4.5 Atualizar estoque
                                    var updateEstoque = "UPDATE Produtos SET Estoque = Estoque - @Quantidade WHERE Id = @ProdutoId";
                                    using (var cmdEstoque = new SqliteCommand(updateEstoque, conn))
                                    {
                                        cmdEstoque.Transaction = transaction;
                                        cmdEstoque.Parameters.AddWithValue("@ProdutoId", produtoId);
                                        cmdEstoque.Parameters.AddWithValue("@Quantidade", quantidade);
                                        cmdEstoque.ExecuteNonQuery();
                                    }

                                    // 4.6 Atualizar valor total do pedido
                                    var updateValorTotal = @"
                                        UPDATE Pedidos 
                                        SET ValorTotal = (
                                            SELECT COALESCE(SUM(Quantidade * PrecoUnitario), 0)
                                            FROM PedidoItens
                                            WHERE PedidoId = @PedidoId
                                        )
                                        WHERE Id = @PedidoId";
                                    
                                    using (var cmdValorTotal = new SqliteCommand(updateValorTotal, conn))
                                    {
                                        cmdValorTotal.Transaction = transaction;
                                        cmdValorTotal.Parameters.AddWithValue("@PedidoId", pedidoId);
                                        cmdValorTotal.ExecuteNonQuery();
                                    }
                                }
                            }
                        }

                        Console.Write("\nDeseja adicionar mais itens? (S/N): ");
                        continuarAdicionando = (Console.ReadLine()?.ToUpper() == "S");
                    }

                    // 5. Finalizar pedido
                    Console.Write("\nConfirmar pedido? (S/N): ");
                    if (Console.ReadLine()?.ToUpper() == "S")
                    {
                        transaction.Commit();
                        Console.WriteLine($"\nPedido {numeroPedido} criado com sucesso!");
                    }
                    else
                    {
                        transaction.Rollback();
                        Console.WriteLine("\nPedido cancelado.");
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"Erro ao criar pedido: {ex.Message}");
                }
            }
        }
    }
        
    public void ExcluirDadosAntigos()
    {
        Console.WriteLine("=== EXCLUIR DADOS ANTIGOS ===\n");
        
        using (var conn = GetConnection())
        {
            conn.Open();
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    // 1. Identificar pedidos antigos
                    var queryPedidosAntigos = @"
                        SELECT 
                            p.Id, 
                            p.NumeroPedido,
                            p.DataPedido,
                            c.Nome as Cliente,
                            CAST(SUM(pi.Quantidade * pi.PrecoUnitario) AS REAL) as ValorTotal
                        FROM Pedidos p
                        JOIN Clientes c ON p.ClienteId = c.Id
                        JOIN PedidoItens pi ON p.Id = pi.PedidoId
                        WHERE p.Status = 'Cancelado'
                        AND date(p.DataPedido) < date('now', '-6 months')
                        GROUP BY p.Id, p.NumeroPedido, p.DataPedido, c.Nome";

                    var pedidosParaExcluir = new List<int>();
                    using (var cmd = new SqliteCommand(queryPedidosAntigos, conn))
                    {
                        cmd.Transaction = transaction;
                        using (var reader = cmd.ExecuteReader())
                        {
                            Console.WriteLine("Pedidos cancelados há mais de 6 meses:\n");
                            var encontrouPedidos = false;
                            
                            while (reader.Read())
                            {
                                encontrouPedidos = true;
                                var pedidoId = Convert.ToInt32(reader["Id"]);
                                var dataPedido = DateTime.Parse(reader["DataPedido"].ToString() ?? DateTime.Now.ToString());
                                
                                Console.WriteLine($"Pedido: {reader["NumeroPedido"]}");
                                Console.WriteLine($"Data: {dataPedido:dd/MM/yyyy}");
                                Console.WriteLine($"Cliente: {reader["Cliente"]}");
                                Console.WriteLine($"Valor: {reader["ValorTotal"]:C}");
                                Console.WriteLine("-------------------");
                                
                                pedidosParaExcluir.Add(pedidoId);
                            }
                            
                            if (!encontrouPedidos)
                            {
                                Console.WriteLine("Não foram encontrados pedidos antigos para excluir.");
                                return;
                            }
                        }
                    }

                    Console.Write("\nConfirma a exclusão dos pedidos listados? (S/N): ");
                    if (Console.ReadLine()?.ToUpper() != "S")
                    {
                        Console.WriteLine("Operação cancelada pelo usuário.");
                        return;
                    }

                    // 2. Excluir itens dos pedidos
                    var deleteItens = "DELETE FROM PedidoItens WHERE PedidoId IN (SELECT Id FROM Pedidos WHERE Status = 'Cancelado' AND date(DataPedido) < date('now', '-6 months'))";
                    using (var cmd = new SqliteCommand(deleteItens, conn))
                    {
                        cmd.Transaction = transaction;
                        var itensExcluidos = cmd.ExecuteNonQuery();
                        Console.WriteLine($"\nItens de pedido excluídos: {itensExcluidos}");
                    }

                    // 3. Excluir pedidos
                    var deletePedidos = "DELETE FROM Pedidos WHERE Status = 'Cancelado' AND date(DataPedido) < date('now', '-6 months')";
                    using (var cmd = new SqliteCommand(deletePedidos, conn))
                    {
                        cmd.Transaction = transaction;
                        var pedidosExcluidos = cmd.ExecuteNonQuery();
                        Console.WriteLine($"Pedidos excluídos: {pedidosExcluidos}");
                    }

                    transaction.Commit();
                    Console.WriteLine("\nDados antigos excluídos com sucesso!");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"Erro ao excluir dados: {ex.Message}");
                }
            }
        }
    }
        
    public void ProcessarDevolucao()
    {
        Console.WriteLine("=== PROCESSAR DEVOLUÇÃO ===\n");
        
        // Solicitar número do pedido
        Console.Write("Digite o número do pedido para devolução: ");
        var numeroPedido = Console.ReadLine();
        
        using (var conn = GetConnection())
        {
            conn.Open();
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    // 1. Verificar se o pedido existe e pode ser devolvido
                    var queryPedido = @"
                        SELECT p.Id, p.Status, p.DataPedido, c.Nome as Cliente
                        FROM Pedidos p
                        JOIN Clientes c ON p.ClienteId = c.Id
                        WHERE p.NumeroPedido = @numeroPedido";

                    using (var cmd = new SqliteCommand(queryPedido, conn))
                    {
                        cmd.Transaction = transaction;
                        cmd.Parameters.AddWithValue("@numeroPedido", numeroPedido);
                        
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                Console.WriteLine("Pedido não encontrado!");
                                return;
                            }

                            if (reader["Status"].ToString() == "Cancelado")
                            {
                                Console.WriteLine("Pedido já está cancelado!");
                                return;
                            }

                            var dataPedido = DateTime.Parse(reader["DataPedido"].ToString());
                            if ((DateTime.Now - dataPedido).TotalDays > 30)
                            {
                                Console.WriteLine("Pedido com mais de 30 dias não pode ser devolvido!");
                                return;
                            }

                            Console.WriteLine($"\nPedido encontrado:");
                            Console.WriteLine($"Cliente: {reader["Cliente"]}");
                            Console.WriteLine($"Data: {dataPedido:dd/MM/yyyy}");
                            Console.WriteLine($"Status: {reader["Status"]}");
                            
                            var pedidoId = Convert.ToInt32(reader["Id"]);
                            
                            // 2. Listar itens do pedido
                            reader.Close();
                            var queryItens = @"
                                SELECT pi.Id, p.Nome as Produto, pi.Quantidade, pi.PrecoUnitario,
                                    (pi.Quantidade * pi.PrecoUnitario) as Subtotal,
                                    p.Id as ProdutoId
                                FROM PedidoItens pi
                                JOIN Produtos p ON pi.ProdutoId = p.Id
                                WHERE pi.PedidoId = @pedidoId";
                            
                            cmd.CommandText = queryItens;
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@pedidoId", pedidoId);
                            
                            using (var readerItens = cmd.ExecuteReader())
                            {
                                Console.WriteLine("\nItens do pedido:");
                                while (readerItens.Read())
                                {
                                    Console.WriteLine($"\nProduto: {readerItens["Produto"]}");
                                    Console.WriteLine($"Quantidade: {readerItens["Quantidade"]}");
                                    Console.WriteLine($"Preço: {readerItens["PrecoUnitario"]:C}");
                                    Console.WriteLine($"Subtotal: {readerItens["Subtotal"]:C}");
                                }
                            }
                            
                            // 3. Confirmar devolução
                            Console.Write("\nConfirma a devolução? (S/N): ");
                            if (Console.ReadLine()?.ToUpper() != "S")
                            {
                                Console.WriteLine("Devolução cancelada pelo usuário.");
                                return;
                            }
                            
                            // 4. Atualizar status do pedido
                            var updatePedido = "UPDATE Pedidos SET Status = 'Cancelado' WHERE Id = @pedidoId";
                            cmd.CommandText = updatePedido;
                            cmd.ExecuteNonQuery();
                            
                            // 5. Devolver itens ao estoque
                            var updateEstoque = @"
                                UPDATE Produtos 
                                SET Estoque = Estoque + (
                                    SELECT Quantidade 
                                    FROM PedidoItens 
                                    WHERE PedidoId = @pedidoId AND ProdutoId = Produtos.Id
                                )
                                WHERE Id IN (
                                    SELECT ProdutoId 
                                    FROM PedidoItens 
                                    WHERE PedidoId = @pedidoId
                                )";
                            
                            cmd.CommandText = updateEstoque;
                            cmd.ExecuteNonQuery();
                            
                            transaction.Commit();
                            Console.WriteLine("\nDevolução processada com sucesso!");
                        }
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"Erro ao processar devolução: {ex.Message}");
                }
            }
        }
    }

    public void CalcularComissao()
    {
        Console.WriteLine("=== CÁLCULO DE COMISSÃO ===");
        
        var query = @"
            SELECT 
                v.Vendedor,
                COUNT(DISTINCT p.Id) as TotalPedidos,
                CAST(SUM(pi.Quantidade * pi.PrecoUnitario) AS REAL) as TotalVendas,
                CAST(SUM(pi.Quantidade * pi.PrecoUnitario * 0.05) AS REAL) as Comissao
            FROM (
                SELECT 
                    c.Id as ClienteId,
                    CASE 
                        WHEN c.Id % 2 = 0 THEN 'João Silva'
                        ELSE 'Maria Santos'
                    END as Vendedor
                FROM Clientes c
            ) v
            JOIN Pedidos p ON p.ClienteId = v.ClienteId
            JOIN PedidoItens pi ON pi.PedidoId = p.Id
            WHERE p.Status != 'Cancelado'
            GROUP BY v.Vendedor
            ORDER BY TotalVendas DESC";

        using (var conn = GetConnection())
        {
            conn.Open();
            using (var cmd = new SqliteCommand(query, conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine($"\nVendedor: {reader["Vendedor"]}");
                        Console.WriteLine($"Total de Pedidos: {reader["TotalPedidos"]}");
                        Console.WriteLine($"Total em Vendas: {reader["TotalVendas"]:C}");
                        Console.WriteLine($"Comissão (5%): {reader["Comissao"]:C}");
                        Console.WriteLine("-------------------");
                    }
                }
            }
        }
    }
        
    // ========== ANÁLISES PERFORMANCE ==========
        
    public void AnalisarPerformanceVendas()
    {
        Console.WriteLine("=== ANÁLISE PERFORMANCE VENDAS ===\n");
        
        var query = @"
            WITH VendasMensais AS (
                SELECT 
                    strftime('%Y', p.DataPedido) as Ano,
                    strftime('%m', p.DataPedido) as Mes,
                    COUNT(DISTINCT p.Id) as TotalPedidos,
                    CAST(SUM(pi.Quantidade) AS INTEGER) as QuantidadeVendida,
                    CAST(SUM(pi.Quantidade * pi.PrecoUnitario) AS REAL) as ValorTotal,
                    CAST(SUM(pi.Quantidade * pi.PrecoUnitario) / COUNT(DISTINCT p.Id) AS REAL) as TicketMedio
                FROM Pedidos p
                JOIN PedidoItens pi ON p.Id = pi.PedidoId
                WHERE p.Status != 'Cancelado'
                GROUP BY 
                    strftime('%Y', p.DataPedido),
                    strftime('%m', p.DataPedido)
                ORDER BY Ano DESC, Mes DESC
            )
            SELECT 
                v.*,
                CAST((ValorTotal - LAG(ValorTotal) OVER (ORDER BY Ano, Mes)) / LAG(ValorTotal) OVER (ORDER BY Ano, Mes) * 100 AS REAL) as CrescimentoPercentual
            FROM VendasMensais v";

        using (var conn = GetConnection())
        {
            conn.Open();
            using (var cmd = new SqliteCommand(query, conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    decimal? maiorValor = null;
                    decimal? menorValor = null;
                    var totalMeses = 0;
                    var somaValores = 0m;
                    
                    Console.WriteLine("VENDAS MENSAIS:\n");
                    Console.WriteLine("Período          Pedidos    Qtde    Valor Total    Ticket Médio    Crescimento");
                    Console.WriteLine("--------------------------------------------------------------------------------");
                    
                    while (reader.Read())
                    {
                        var mes = int.Parse(reader["Mes"]?.ToString() ?? "1");
                        var ano = int.Parse(reader["Ano"]?.ToString() ?? DateTime.Now.Year.ToString());
                        var valorTotal = Convert.ToDecimal(reader["ValorTotal"]);
                        var crescimento = reader["CrescimentoPercentual"] == DBNull.Value 
                            ? "      -    "
                            : $"{Convert.ToDouble(reader["CrescimentoPercentual"]):F1}%";
                        
                        Console.WriteLine($"{mes:00}/{ano}            {reader["TotalPedidos"]}        {reader["QuantidadeVendida"]}     {valorTotal:C}    {reader["TicketMedio"]:C}      {crescimento}");
                        
                        // Atualiza estatísticas
                        maiorValor = maiorValor == null ? valorTotal : Math.Max(maiorValor.Value, valorTotal);
                        menorValor = menorValor == null ? valorTotal : Math.Min(menorValor.Value, valorTotal);
                        somaValores += valorTotal;
                        totalMeses++;
                    }
                    
                    if (totalMeses > 0)
                    {
                        Console.WriteLine("\nRESUMO ESTATÍSTICO:");
                        Console.WriteLine($"Melhor mês: {maiorValor:C}");
                        Console.WriteLine($"Pior mês: {menorValor:C}");
                        Console.WriteLine($"Média mensal: {(somaValores / totalMeses):C}");
                        if (totalMeses >= 2)
                        {
                            var crescimentoTotal = ((maiorValor - menorValor) / menorValor * 100);
                            Console.WriteLine($"Crescimento total no período: {crescimentoTotal:F1}%");
                        }
                    }
                }
            }
        }
    }
        
    // ========== UTILIDADES ==========
        
    private SqliteConnection GetConnection()
    {
        return new SqliteConnection(_connectionString);
    }
        

    public void TestarConexao()
    {
        Console.WriteLine("=== TESTE DE CONEXÃO ===");
        
        try
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                Console.WriteLine("Conexão estabelecida com sucesso!");
                
                // Informações do banco
                using (var cmd = new SqliteCommand("SELECT sqlite_version()", conn))
                {
                    var version = cmd.ExecuteScalar()?.ToString();
                    Console.WriteLine($"Versão SQLite: {version}");
                }
                
                // Contagem de registros nas tabelas principais
                var tables = new[] { "Categorias", "Produtos", "Clientes", "Pedidos", "PedidoItens" };
                foreach (var table in tables)
                {
                    using (var cmd = new SqliteCommand($"SELECT COUNT(*) FROM {table}", conn))
                    {
                        var count = cmd.ExecuteScalar();
                        Console.WriteLine($"Total de registros em {table}: {count}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao conectar: {ex.Message}");
        }
    }
}