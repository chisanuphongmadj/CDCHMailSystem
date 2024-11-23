using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CDCHMailSystem.Pages.User
{
    public class HomeModel : PageModel
    {
        public List<Mail> Mails { get; set; }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            // ลบ cookies ของผู้ใช้และออกจากระบบ
            await HttpContext.SignOutAsync();
            return RedirectToPage("/Account/Login"); // กลับไปที่หน้า Login
        }

        public async Task OnGetAsync()
        {
            Mails = new List<Mail>();
            string connectionString = "Server=tcp:cdchemail.database.windows.net,1433;Initial Catalog=cdchemailsystem;Persist Security Info=False;User ID=cdch1234;Password=1234@cdch;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            string currentUsername = User.Identity?.Name; // ดึง username ของผู้ใช้ที่ล็อกอินอยู่

            if (string.IsNullOrEmpty(currentUsername))
            {
                // กรณีที่ผู้ใช้ไม่ล็อกอิน ให้ redirect ไปที่หน้า Login
                RedirectToPage("/Account/Login");
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT Id, FromUsername, Subject, DateTime, IsRead, Body FROM Mails WHERE ToUsername = @CurrentUsername";

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
                                DateTime = reader["DateTime"].ToString(),
                                IsRead = (bool)reader["IsRead"],
                                Body = reader["Body"].ToString() // ดึง Body มาใช้งาน
                            });
                        }
                    }
                }
            }
        }

        public async Task<IActionResult> OnPostMarkAsReadAsync(int id)
        {
            string connectionString = "Server=tcp:cdchemail.database.windows.net,1433;Initial Catalog=cdchemailsystem;Persist Security Info=False;User ID=cdch1234;Password=1234@cdch;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string updateQuery = "UPDATE Mails SET IsRead = 1 WHERE Id = @Id"; // อัปเดตสถานะเป็นอ่านแล้ว

                using (SqlCommand command = new SqlCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }

            return RedirectToPage(); // โหลดหน้าใหม่หลังอัปเดตสถานะ
        }

        public async Task<IActionResult> OnPostMarkAsUnreadAsync(int id)
        {
            string connectionString = "Server=tcp:cdchemail.database.windows.net,1433;Initial Catalog=cdchemailsystem;Persist Security Info=False;User ID=cdch1234;Password=1234@cdch;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string updateQuery = "UPDATE Mails SET IsRead = 0 WHERE Id = @Id"; // อัปเดตสถานะเป็นยังไม่อ่าน

                using (SqlCommand command = new SqlCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }

            return RedirectToPage(); // โหลดหน้าใหม่หลังอัปเดตสถานะ
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

            // โหลดหน้าใหม่หลังจากลบเสร็จ
            return RedirectToPage();
        }
    }

    public class Mail
    {
        public int Id { get; set; }
        public string FromUsername { get; set; }
        public string Subject { get; set; }
        public string DateTime { get; set; }
        public string Body { get; set; } // เพิ่มพร็อพเพอร์ตี้ Body
        public bool IsRead { get; set; } // เพิ่มสถานะการอ่านเมล
    }
}
