using AlADNANA.Class;
using AlADNANA.Helper;
using AlADNANA.SqlConnectionManager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Security.Cryptography.Xml;

namespace AlADNANA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                string decryptedPassword = Security.DecryptAES(loginRequest.Password);

                SqlConnectionManag sqlConnectionManag = new SqlConnectionManag();
                using (SqlConnection connection = new SqlConnection(sqlConnectionManag.getstringConection()))
                {
                    await connection.OpenAsync();

                    string query = "SELECT * FROM UserData WHERE Email = @Email AND Password = @Password";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", loginRequest.Email);
                        command.Parameters.AddWithValue("@Password", decryptedPassword);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                            {
                                var user = new
                                {
                                    Email = reader["Email"].ToString(),
                                 
                                    ExpirationDate = DateTime.UtcNow.AddMinutes(15)
                                };

                                string userJson = JsonConvert.SerializeObject(user);
                                string encryptedUserJson = Security.EncryptAES(userJson);

                                return Ok(new { encryptionKey = encryptedUserJson });
                            }
                            else
                            {
                                return NotFound("User not found");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}
 
