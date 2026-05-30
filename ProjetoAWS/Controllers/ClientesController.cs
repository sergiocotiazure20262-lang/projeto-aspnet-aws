using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ProjetoAWS.Controllers
{
    [Route("api/v1/clientes")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly string _connectionString;

        public ClientesController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("BDClientes")
                ?? throw new InvalidOperationException("Connection string 'BDClientes' não encontrada.");
        }

        [HttpGet]
        public ActionResult<IEnumerable<Cliente>> Get()
        {
            var clientes = new List<Cliente>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(@"
                SELECT Id, Nome, Email, Telefone, Cpf
                FROM Clientes
                ORDER BY Id", connection);

            connection.Open();

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                clientes.Add(new Cliente
                {
                    Id = reader.GetInt32("Id"),
                    Nome = reader.GetString("Nome"),
                    Email = reader.GetString("Email"),
                    Telefone = reader.GetString("Telefone"),
                    Cpf = reader.GetString("Cpf")
                });
            }

            return Ok(clientes);
        }

        [HttpGet("{id:int}")]
        public ActionResult<Cliente> GetById(int id)
        {
            Cliente? cliente = null;

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(@"
                SELECT Id, Nome, Email, Telefone, Cpf
                FROM Clientes
                WHERE Id = @Id", connection);

            command.Parameters.Add("@Id", SqlDbType.Int).Value = id;

            connection.Open();

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                cliente = new Cliente
                {
                    Id = reader.GetInt32("Id"),
                    Nome = reader.GetString("Nome"),
                    Email = reader.GetString("Email"),
                    Telefone = reader.GetString("Telefone"),
                    Cpf = reader.GetString("Cpf")
                };
            }

            if (cliente == null)
                return NotFound("Cliente não encontrado.");

            return Ok(cliente);
        }

        [HttpPost]
        public ActionResult<Cliente> Post([FromBody] Cliente cliente)
        {
            if (cliente == null)
                return BadRequest("Dados do cliente inválidos.");

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(@"
                INSERT INTO Clientes (Nome, Email, Telefone, Cpf)
                OUTPUT INSERTED.Id
                VALUES (@Nome, @Email, @Telefone, @Cpf)", connection);

            command.Parameters.Add("@Nome", SqlDbType.VarChar, 150).Value = cliente.Nome;
            command.Parameters.Add("@Email", SqlDbType.VarChar, 150).Value = cliente.Email;
            command.Parameters.Add("@Telefone", SqlDbType.VarChar, 30).Value = cliente.Telefone;
            command.Parameters.Add("@Cpf", SqlDbType.VarChar, 20).Value = cliente.Cpf;

            connection.Open();

            cliente.Id = (int)command.ExecuteScalar()!;

            return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, cliente);
        }

        [HttpPut("{id:int}")]
        public ActionResult<Cliente> Put(int id, [FromBody] Cliente clienteAtualizado)
        {
            if (clienteAtualizado == null)
                return BadRequest("Dados do cliente inválidos.");

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(@"
                UPDATE Clientes
                SET 
                    Nome = @Nome,
                    Email = @Email,
                    Telefone = @Telefone,
                    Cpf = @Cpf
                WHERE Id = @Id", connection);

            command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
            command.Parameters.Add("@Nome", SqlDbType.VarChar, 150).Value = clienteAtualizado.Nome;
            command.Parameters.Add("@Email", SqlDbType.VarChar, 150).Value = clienteAtualizado.Email;
            command.Parameters.Add("@Telefone", SqlDbType.VarChar, 30).Value = clienteAtualizado.Telefone;
            command.Parameters.Add("@Cpf", SqlDbType.VarChar, 20).Value = clienteAtualizado.Cpf;

            connection.Open();

            var linhasAfetadas = command.ExecuteNonQuery();

            if (linhasAfetadas == 0)
                return NotFound("Cliente não encontrado.");

            clienteAtualizado.Id = id;

            return Ok(clienteAtualizado);
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(@"
                DELETE FROM Clientes
                WHERE Id = @Id", connection);

            command.Parameters.Add("@Id", SqlDbType.Int).Value = id;

            connection.Open();

            var linhasAfetadas = command.ExecuteNonQuery();

            if (linhasAfetadas == 0)
                return NotFound("Cliente não encontrado.");

            return NoContent();
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