using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CDCHMailSystem.Pages.User
{
    public class HomeModel : PageModel
    {
        public List<Mail> Mails { get; set; }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            // ź cookies �ͧ���������͡�ҡ�к�
            await HttpContext.SignOutAsync();
            return RedirectToPage("/Account/Login"); // ��Ѻ价��˹�� Login
        }

        public async Task OnGetAsync()
        {
            Mails = new List<Mail>();
            string connectionString = "Server=tcp:cdchemail.database.windows.net,1433;Initial Catalog=cdchemailsystem;Persist Security Info=False;User ID=cdch1234;Password=1234@cdch;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            string currentUsername = User.Identity?.Name; // �֧ username �ͧ���������͡�Թ����

            if (string.IsNullOrEmpty(currentUsername))
            {
                // �óշ�����������͡�Թ ��� redirect 价��˹�� Login
                RedirectToPage("/Account/Login");
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT Id, FromUsername, Subject, DateTime FROM Mails WHERE ToUsername = @CurrentUsername";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CurrentUsername", currentUsername);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Mails.Add(new Mail
                            {
                                Id = (int)reader["Id"],
                                FromUsername = reader["FromUsername"].ToString(),
                                Subject = reader["Subject"].ToString(),
                                DateTime = reader["DateTime"].ToString()
                            });
                        }
                    }
                }
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            string connectionString = "Server=tcp:cdchemail.database.windows.net,1433;Initial Catalog=cdchemailsystem;Persist Security Info=False;User ID=cdch1234;Password=1234@cdch;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string deleteQuery = "DELETE FROM Mails WHERE Id = @Id";

                using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }

            // ��Ŵ˹��������ѧ�ҡź����
            return RedirectToPage();
        }
    }

    public class Mail
    {
        public int Id { get; set; }
        public string FromUsername { get; set; }
        public string Subject { get; set; }
        public string DateTime { get; set; }
        public string Body { get; set; }
    }

}
