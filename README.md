# ProjetoAWS - API de Clientes com Cache

Projeto ASP.NET Core Web API para gerenciamento de dados de clientes utilizando cache em memória (`IMemoryCache`).

A API permite cadastrar, consultar, atualizar e remover clientes armazenados temporariamente em cache.

## Tecnologias utilizadas

- .NET / ASP.NET Core Web API
- C#
- IMemoryCache
- Swagger / OpenAPI, caso esteja habilitado no projeto

## Estrutura principal

O projeto contém um controller chamado `ClientesController`, responsável pelas operações CRUD de clientes.

```csharp
[Route("api/[controller]")]
[ApiController]
public class ClientesController : ControllerBase
{
}
```

## Modelo de Cliente

```csharp
public class Cliente
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Telefone { get; set; } = string.Empty;

    public string Cpf { get; set; } = string.Empty;
}
```

## Configuração do cache

Para utilizar `IMemoryCache`, é necessário registrar o serviço no arquivo `Program.cs`.

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMemoryCache();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

## Endpoints da API

A URL base do controller é:

```http
/api/clientes
```

## Listar todos os clientes

```http
GET /api/clientes
```

### Resposta de exemplo

```json
[
  {
    "id": 1,
    "nome": "João Silva",
    "email": "joao@email.com",
    "telefone": "11999999999",
    "cpf": "12345678900"
  }
]
```

## Buscar cliente por ID

```http
GET /api/clientes/{id}
```

### Exemplo

```http
GET /api/clientes/1
```

### Possíveis respostas

- `200 OK` - Cliente encontrado
- `404 Not Found` - Cliente não encontrado

## Cadastrar cliente

```http
POST /api/clientes
```

### Corpo da requisição

```json
{
  "nome": "João Silva",
  "email": "joao@email.com",
  "telefone": "11999999999",
  "cpf": "12345678900"
}
```

### Possíveis respostas

- `201 Created` - Cliente cadastrado com sucesso
- `400 Bad Request` - Dados inválidos

## Atualizar cliente

```http
PUT /api/clientes/{id}
```

### Exemplo

```http
PUT /api/clientes/1
```

### Corpo da requisição

```json
{
  "nome": "João Silva Atualizado",
  "email": "joao.atualizado@email.com",
  "telefone": "11888888888",
  "cpf": "12345678900"
}
```

### Possíveis respostas

- `200 OK` - Cliente atualizado com sucesso
- `400 Bad Request` - Dados inválidos
- `404 Not Found` - Cliente não encontrado

## Remover cliente

```http
DELETE /api/clientes/{id}
```

### Exemplo

```http
DELETE /api/clientes/1
```

### Possíveis respostas

- `204 No Content` - Cliente removido com sucesso
- `404 Not Found` - Cliente não encontrado

## Como executar o projeto

1. Clone ou abra o projeto no Visual Studio ou Visual Studio Code.

2. Restaure os pacotes:

```bash
dotnet restore
```

3. Execute a aplicação:

```bash
dotnet run
```

4. Acesse a API pelo navegador, Postman, Insomnia ou Swagger.

Exemplo de URL local:

```http
https://localhost:5001/api/clientes
```

ou

```http
http://localhost:5000/api/clientes
```

## Observações importantes

- Os dados são armazenados apenas em cache de memória.
- Ao reiniciar a aplicação, os clientes cadastrados serão perdidos.
- Essa abordagem é útil para testes, protótipos e demonstrações.
- Para ambiente de produção, recomenda-se utilizar um banco de dados persistente, como SQL Server, PostgreSQL, MySQL ou DynamoDB.

## Exemplo de controller

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace ProjetoAWS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private const string CacheKey = "clientes";

        public ClientesController(IMemoryCache cache)
        {
            _cache = cache;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Cliente>> Get()
        {
            var clientes = ObterClientes();
            return Ok(clientes);
        }

        [HttpGet("{id:int}")]
        public ActionResult<Cliente> GetById(int id)
        {
            var clientes = ObterClientes();

            var cliente = clientes.FirstOrDefault(c => c.Id == id);

            if (cliente == null)
                return NotFound("Cliente não encontrado.");

            return Ok(cliente);
        }

        [HttpPost]
        public ActionResult<Cliente> Post([FromBody] Cliente cliente)
        {
            if (cliente == null)
                return BadRequest("Dados do cliente inválidos.");

            var clientes = ObterClientes();

            cliente.Id = clientes.Any()
                ? clientes.Max(c => c.Id) + 1
                : 1;

            clientes.Add(cliente);

            SalvarClientes(clientes);

            return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, cliente);
        }

        [HttpPut("{id:int}")]
        public ActionResult<Cliente> Put(int id, [FromBody] Cliente clienteAtualizado)
        {
            if (clienteAtualizado == null)
                return BadRequest("Dados do cliente inválidos.");

            var clientes = ObterClientes();

            var cliente = clientes.FirstOrDefault(c => c.Id == id);

            if (cliente == null)
                return NotFound("Cliente não encontrado.");

            cliente.Nome = clienteAtualizado.Nome;
            cliente.Email = clienteAtualizado.Email;
            cliente.Telefone = clienteAtualizado.Telefone;
            cliente.Cpf = clienteAtualizado.Cpf;

            SalvarClientes(clientes);

            return Ok(cliente);
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var clientes = ObterClientes();

            var cliente = clientes.FirstOrDefault(c => c.Id == id);

            if (cliente == null)
                return NotFound("Cliente não encontrado.");

            clientes.Remove(cliente);

            SalvarClientes(clientes);

            return NoContent();
        }

        private List<Cliente> ObterClientes()
        {
            if (!_cache.TryGetValue(CacheKey, out List<Cliente>? clientes))
            {
                clientes = new List<Cliente>();

                _cache.Set(CacheKey, clientes, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(30)
                });
            }

            return clientes;
        }

        private void SalvarClientes(List<Cliente> clientes)
        {
            _cache.Set(CacheKey, clientes, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(30)
            });
        }
    }
}
```

## Licença

Este projeto pode ser utilizado livremente para fins de estudo e demonstração.
