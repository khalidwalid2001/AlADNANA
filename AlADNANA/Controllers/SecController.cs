using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AlADNANA.SqlConnectionManager;
 using System.Data.SqlClient;
using System.Data;
 using AlADNANA.Class;

namespace AlADNANA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class SecController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        [HttpPost]
        [Route("SaveData")]
        public async Task<IActionResult> SaveData([FromBody] List<SecTable> SecTable)
        {
            if (SecTable == null || SecTable.Count == 0)
            {
                return BadRequest("No data provided.");
            }

            try
            {
                SqlConnectionManag sqlConnectionManag = new SqlConnectionManag();
                using (SqlConnection connection = new SqlConnection(sqlConnectionManag.getstringConection()))
                {
                    await connection.OpenAsync();

                    // Create a DataTable to hold your data
                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("Name", typeof(string));
                    dataTable.Columns.Add("Nationality", typeof(string));
                    dataTable.Columns.Add("DateOfBirth", typeof(string)); // Changed from "Dob"
                    dataTable.Columns.Add("PassportNumber", typeof(string));
                    dataTable.Columns.Add("ExpirationDate", typeof(string));
                    dataTable.Columns.Add("ApprovalNumber", typeof(string)); // Assuming this replaces "Embassy"

                    // Populate the DataTable
                    foreach (var item in SecTable)
                    {
                        dataTable.Rows.Add(
                            item.Name,
                            item.Nationality,
                            item.DateOfBirth,
                            item.PassportNumber,
                            item.ExpirationDate,
                            item.ApprovalNumber
                        );
                    }

                    // Use SqlBulkCopy to perform bulk insert
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                    {
                        bulkCopy.DestinationTableName = "SecTable"; // Updated to your new table name

                        // Map columns in the DataTable to columns in the destination table
                        bulkCopy.ColumnMappings.Add("Name", "Name");
                        bulkCopy.ColumnMappings.Add("Nationality", "Nationality");
                        bulkCopy.ColumnMappings.Add("DateOfBirth", "DateOfBirth"); // Updated mapping
                        bulkCopy.ColumnMappings.Add("PassportNumber", "PassportNumber");
                        bulkCopy.ColumnMappings.Add("ExpirationDate", "ExpirationDate");
                        bulkCopy.ColumnMappings.Add("ApprovalNumber", "ApprovalNumber"); // New mapping

                        // Perform the bulk copy
                        await bulkCopy.WriteToServerAsync(dataTable);
                    }
                }

                return Ok("");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet]
        [Route("SearchData")]
        public async Task<IActionResult> SearchData([FromQuery] string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return BadRequest("No search query provided.");
            }

            try
            {
                SqlConnectionManag sqlConnectionManag = new SqlConnectionManag();
                using (SqlConnection connection = new SqlConnection(sqlConnectionManag.getstringConection()))
                {
                    await connection.OpenAsync();

                    string sqlQuery = @"
                SELECT Name, Nationality, DateOfBirth, PassportNumber, ExpirationDate, ApprovalNumber
                FROM SecTable
                WHERE Name LIKE @Query
                OR Nationality LIKE @Query
                OR DateOfBirth LIKE @Query
                OR PassportNumber LIKE @Query
                OR ExpirationDate LIKE @Query
                OR ApprovalNumber LIKE @Query";

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Query", "%" + query + "%");

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            var results = new List<SecTable>();
                            while (await reader.ReadAsync())
                            {
                                results.Add(new SecTable
                                {
                                    Name = reader["Name"].ToString(),
                                    Nationality = reader["Nationality"].ToString(),
                                    DateOfBirth = reader["DateOfBirth"].ToString(), // Adjusted for the new column name
                                    PassportNumber = reader["PassportNumber"].ToString(),
                                    ExpirationDate = reader["ExpirationDate"].ToString(),
                                    ApprovalNumber = reader["ApprovalNumber"].ToString() // Added based on the new table structure
                                });
                            }

                            return Ok(new { data = results });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

}

