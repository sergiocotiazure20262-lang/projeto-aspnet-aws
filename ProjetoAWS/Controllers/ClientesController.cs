using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace ProjetoAWS.Controllers
{
    [Route("api/v1/clientes")]
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

    public class Cliente
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Telefone { get; set; } = string.Empty;

        public string Cpf { get; set; } = string.Empty;
    }
}