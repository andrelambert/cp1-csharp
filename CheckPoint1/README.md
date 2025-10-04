# CP 1 - C# - Turma 3ESPY

## André Lambert - RM 99148 

## Sistema de Gerenciamento de Loja

Este é um projeto desenvolvido em C# usando Entity Framework Core que implementa um sistema de gerenciamento para uma loja, com funcionalidades de controle de estoque, pedidos, clientes e relatórios gerenciais.

## Funcionalidades

### Gestão de Cadastros
- Cadastro e manutenção de Categorias
- Cadastro e manutenção de Produtos
- Cadastro e manutenção de Clientes
- Gestão de Pedidos

### Controle de Estoque
- Atualização automática do estoque ao criar/cancelar pedidos
- Alerta de produtos com estoque baixo
- Atualização em lote do estoque por categoria

### Pedidos
- Criação de pedidos com múltiplos itens
- Validação de estoque disponível
- Gestão do status do pedido (Pendente, Confirmado, Em Andamento, Entregue, Cancelado)
- Cancelamento com devolução automática ao estoque

### Relatórios e Análises
- Dashboard executivo
- Faturamento por categoria
- Análise de vendas mensal
- Top clientes por valor
- Produtos sem venda
- Relatório de estoque

## Tecnologias Utilizadas
- .NET Core
- Entity Framework Core
- SQLite
- ADO.NET (para consultas complexas)

## Como Executar

### Pré-requisitos
- .NET Core SDK 9.0 ou superior

### Passos para Execução

1. Clone o repositório:
```bash
git clone [url-do-repositorio]
cd CheckPoint1
```

2. Restaure os pacotes:
```bash
dotnet restore
```

3. Execute o projeto:
```bash
dotnet run
```
ou
```bash
dotnet watch run  # Para executar com hot-reload
```

### Testando o Sistema

O banco de dados será criado automaticamente com dados iniciais para teste, incluindo:
- 3 categorias (Eletrônicos, Livros, Jogos)
- 6 produtos distribuídos entre as categorias
- 2 clientes com pedidos de exemplo

Para testar as funcionalidades:

1. **Cadastros Básicos**:
   - Crie uma nova categoria
   - Adicione produtos à categoria
   - Cadastre um novo cliente

2. **Gestão de Pedidos**:
   - Crie um novo pedido
   - Adicione múltiplos itens
   - Experimente atualizar o status do pedido
   - Tente cancelar um pedido e verifique o estoque

3. **Relatórios**:
   - Execute o dashboard executivo
   - Verifique o relatório de estoque
   - Analise o faturamento por categoria

## Estrutura do Projeto

- `/Models`: Classes de modelo (Categoria, Produto, Cliente, Pedido, PedidoItem)
- `/Context`: Configuração do Entity Framework
- `/Services`: Implementação dos serviços de dados
- `/Enums`: Enumerações do sistema

## Dados de Exemplo

O sistema já vem com dados iniciais para teste:

**Categorias:**
- Eletrônicos
- Livros
- Jogos

**Produtos:**
- Smartphone (Eletrônicos)
- Notebook (Eletrônicos)
- Clean Code (Livros)
- Domain-Driven Design (Livros)
- The Last of Us (Jogos)
- God of War (Jogos)

**Clientes:**
- João Silva (joao@email.com)
- Maria Santos (maria@email.com)

## Notas
- O projeto usa SQLite para simplicidade de demonstração
- Os dados iniciais incluem alguns produtos com estoque zero para teste
- As senhas e dados sensíveis não devem ser commitados em um projeto real
