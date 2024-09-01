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
    
    public class PrimaryController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        [HttpPost]
        [Route("SaveData")]
        public async Task<IActionResult> SaveData([FromBody] List<TableDataModel> tableData)
        {
            if (tableData == null || tableData.Count == 0)
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
                    dataTable.Columns.Add("Dob", typeof(string));
                    dataTable.Columns.Add("MotherName", typeof(string));
                    dataTable.Columns.Add("PassportNumber", typeof(string));
                    dataTable.Columns.Add("IssueDate", typeof(string));
                    dataTable.Columns.Add("ExpirationDate", typeof(string));
                    dataTable.Columns.Add("Embassy", typeof(string));

                    // Populate the DataTable
                    foreach (var item in tableData)
                    {
                        dataTable.Rows.Add(
                            item.Name,
                            item.Nationality,
                            item.Dob,
                            item.MotherName,
                            item.PassportNumber,
                            item.IssueDate,
                            item.ExpirationDate,
                            item.Embassy
                        );
                    }

                    // Use SqlBulkCopy to perform bulk insert
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                    {
                        bulkCopy.DestinationTableName = "TableData"; // Specify your destination table name

                        // Map columns in the DataTable to columns in the destination table
                        bulkCopy.ColumnMappings.Add("Name", "Name");
                        bulkCopy.ColumnMappings.Add("Nationality", "Nationality");
                        bulkCopy.ColumnMappings.Add("Dob", "Dob");
                        bulkCopy.ColumnMappings.Add("MotherName", "MotherName");
                        bulkCopy.ColumnMappings.Add("PassportNumber", "PassportNumber");
                        bulkCopy.ColumnMappings.Add("IssueDate", "IssueDate");
                        bulkCopy.ColumnMappings.Add("ExpirationDate", "ExpirationDate");
                        bulkCopy.ColumnMappings.Add("Embassy", "Embassy");

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
                        SELECT id, Name, Nationality, Dob, MotherName, PassportNumber, IssueDate, ExpirationDate, Embassy
                        FROM TableData
                        WHERE Name LIKE @Query
                        OR Nationality LIKE @Query
                        OR MotherName LIKE @Query
                        OR PassportNumber LIKE @Query
                        OR Embassy LIKE @Query
                        OR IssueDate LIKE @Query
                        OR ExpirationDate LIKE @Query
                          OR Dob LIKE @Query";

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Query", "%" + query + "%");

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            var results = new List<TableDataModel>();
                             while (await reader.ReadAsync())
                            {
                                results.Add(new TableDataModel
                                {
                                    Id = reader["Id"].ToString(),
                                     Name = reader["Name"].ToString(),
                                    Nationality = reader["Nationality"].ToString(),
                                    Dob = reader["Dob"].ToString(),
                                    MotherName = reader["MotherName"].ToString(),
                                    PassportNumber = reader["PassportNumber"].ToString(),
                                    IssueDate = reader["IssueDate"].ToString(),
                                    ExpirationDate = reader["ExpirationDate"].ToString(),
                                    Embassy = reader["Embassy"].ToString()
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
        [HttpPost]
        [Route("UpdateRowColor")]
        public IActionResult UpdateRowColor([FromHeader] int id, [FromHeader] string color)
        {
            SqlConnectionManag sqlConnectionManag = new SqlConnectionManag();

            using (SqlConnection connection = new SqlConnection(sqlConnectionManag.getstringConection()))
            {
                using (SqlCommand command = new SqlCommand("SaveOrUpdateRowColor", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@TableDataId", id);
                    command.Parameters.AddWithValue("@Color", color);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }

            return Ok(new { success = true, message = "Row color saved or updated successfully." });
        }
        [HttpPost]
        [Route("GetRowColors")]
        public IActionResult GetRowColors([FromBody] string idsString)
        {
            if (string.IsNullOrEmpty(idsString))
            {
                return BadRequest("idsString cannot be null or empty.");
            }
            SqlConnectionManag sqlConnectionManag = new SqlConnectionManag();

            var ids = idsString.Split(';').Select(id => int.Parse(id)).ToList();
            var colors = new List<RowColorModel>();

            using (SqlConnection connection = new SqlConnection(sqlConnectionManag.getstringConection()))
            {
                string query = "SELECT TableDataId, Color FROM RowColors WHERE TableDataId IN (" + string.Join(",", ids) + ")";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            colors.Add(new RowColorModel
                            {
                                TableDataId = reader.GetInt32(0),
                                Color = reader.GetString(1)
                            });
                        }
                    }
                }
            }

            return Ok(colors);
        }
        [HttpPost]
        [Route("upload")]

        public async Task<IActionResult> UploadFiles([FromBody] FileUploadDto fileUploadDto)
        {
            SqlConnectionManag sqlConnectionManag = new SqlConnectionManag();

            if (fileUploadDto == null || string.IsNullOrEmpty(fileUploadDto.ExcelFile) || string.IsNullOrEmpty(fileUploadDto.ExcelFileName))
            {
                return BadRequest("Excel file and Excel file name are required.");
            }

            // If ImageFile or ImageFileName is null, set them to DBNull.Value
            var imageFile = fileUploadDto.ImageFile ?? (object)DBNull.Value;
            var imageFileName = fileUploadDto.ImageFileName ?? (object)DBNull.Value;

            const string insertQuery = @"
            INSERT INTO UploadedFiles (ExcelFile, ExcelFileName, ImageFile, ImageFileName)
            VALUES (@ExcelFile, @ExcelFileName, @ImageFile, @ImageFileName);
            SELECT SCOPE_IDENTITY();";

            using (SqlConnection connection = new SqlConnection(sqlConnectionManag.getstringConection()))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@ExcelFile", fileUploadDto.ExcelFile);
                    command.Parameters.AddWithValue("@ExcelFileName", fileUploadDto.ExcelFileName);
                    command.Parameters.AddWithValue("@ImageFile", imageFile);
                    command.Parameters.AddWithValue("@ImageFileName", imageFileName);

                    var id = await command.ExecuteScalarAsync();
                    return Ok(new { id = id });
                }
            }
        }
        [HttpGet]
        [Route("GetFile")]
        public async Task<IActionResult> GetFile()
        {
            List<UploadedFile> files = new List<UploadedFile>();
            SqlConnectionManag sqlConnectionManag = new SqlConnectionManag();

            const string query = @"
            SELECT Id, ExcelFileName
            FROM UploadedFiles;";

            using (SqlConnection connection = new SqlConnection(sqlConnectionManag.getstringConection()))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        UploadedFile file = new UploadedFile
                        {
                            Id = reader.GetInt32(0),
                            ExcelFileName = reader.GetString(1),
                 
                        };

                        files.Add(file);
                    }

                    reader.Close();
                }
            }

            return Ok(files);
        }
        [HttpGet]
        [Route("GetFileId")]
        public async Task<IActionResult> GetFileId(int id)
        {
            SqlConnectionManag sqlConnectionManag = new SqlConnectionManag();

            const string query = @"
        SELECT Id, ExcelFile, ExcelFileName, ImageFile, ImageFileName
        FROM UploadedFiles
        WHERE Id = @Id;";

            UploadedFile file = null;

            using (SqlConnection connection = new SqlConnection(sqlConnectionManag.getstringConection()))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    if (await reader.ReadAsync())
                    {
                        file = new UploadedFile
                        {
                            Id = reader.GetInt32(0),
                            ExcelFile = reader.IsDBNull(1) ? null : reader.GetString(1),
                            ExcelFileName = reader.GetString(2),
                            ImageFile = reader.IsDBNull(3) ? null : reader.GetString(3),
                            ImageFileName = reader.IsDBNull(4) ? null : reader.GetString(4)
                        };
                    }

                    reader.Close();
                }
            }

            if (file == null)
            {
                return NotFound();
            }

            return Ok(file);
        }











    }

    }
 
