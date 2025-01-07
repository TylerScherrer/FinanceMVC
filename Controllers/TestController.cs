using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;

[Route("api/test-db")]
[ApiController]
public class TestController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> TestDatabaseConnection()
    {
        string connectionString = "Server=financemvc.mysql.database.azure.com; Port=3306; Database=financedb; Uid=TylerS00; Pwd=Blink182!; SslMode=Required; SslCa=D:\\home\\site\\wwwroot\\DigiCertGlobalRootCA.crt.pem;";

        try
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            return Ok("Connected to MySQL database successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Failed to connect: {ex.Message}");
        }
    }
}
