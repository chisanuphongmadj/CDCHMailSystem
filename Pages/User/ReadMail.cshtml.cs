using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CDCHMailSystem.Models;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace CDCHMailSystem.Pages.User
{
    public class ReadMailModel : PageModel
    {
        public Mail Mail { get; set; }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await HttpContext.SignOutAsync();
            return RedirectToPage("/Account/Login");
        }

        public async Task OnGetAsync(int id)
        {
            string connectionString = "Server=tcp:cdchemail.database.windows.net,1433;Initial Catalog=cdchemailsystem;Persist Security Info=False;User ID=cdch1234;Password=1234@cdch;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT FromUsername, Subject, DateTime, Body FROM Mails WHERE Id = @Id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            Mail = new Mail
                            {
                                FromUsername = reader["FromUsername"].ToString(),
                                Subject = reader["Subject"].ToString(),
                                DateTime = reader["DateTime"].ToString(),
                                Body = reader["Body"].ToString()
                            };
                        }
                    }
                }
            }
        }
    }
}
