using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using System;

namespace CDCHMailSystem.Pages.User
{
    public class ComposeModel : PageModel
    {
        [BindProperty]
        [Required]
        public string ToUsername { get; set; }

        [BindProperty]
        [Required]
        public string Subject { get; set; }

        [BindProperty]
        [Required]
        public string Body { get; set; }

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await HttpContext.SignOutAsync();
            return RedirectToPage("/Account/Login");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // ��Ǩ�ͺ��Ҽ���������������ҵ���ͧ
            if (ToUsername.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
            {
                ErrorMessage = "You cannot send an email to yourself.";
                return Page();
            }

            string connectionString = "Server=tcp:cdchemail.database.windows.net,1433;Initial Catalog=cdchemailsystem;Persist Security Info=False;User ID=cdch1234;Password=1234@cdch;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            bool userExists = false;

            // ��˹�ࢵ������
            var bangkokTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // ��Ǩ�ͺ��Ҽ�������觶֧�������ԧ�������
                string checkUserQuery = "SELECT COUNT(1) FROM Users WHERE Username = @ToUsername";
                using (SqlCommand command = new SqlCommand(checkUserQuery, connection))
                {
                    command.Parameters.AddWithValue("@ToUsername", ToUsername);
                    int count = (int)await command.ExecuteScalarAsync();
                    userExists = count > 0;
                }

                if (!userExists)
                {
                    ErrorMessage = "The specified username does not exist.";
                    return Page();
                }

                // �ѹ�֡�����ŧ㹰ҹ������ ������ѹ�֡���ҵ��ࢵ������
                string insertQuery = "INSERT INTO Mails (FromUsername, ToUsername, Subject, Body, DateTime) VALUES (@FromUsername, @ToUsername, @Subject, @Body, @DateTime)";

                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@FromUsername", User.Identity.Name);
                    command.Parameters.AddWithValue("@ToUsername", ToUsername);
                    command.Parameters.AddWithValue("@Subject", Subject);
                    command.Parameters.AddWithValue("@Body", Body);
                    command.Parameters.AddWithValue("@DateTime", bangkokTime);

                    await command.ExecuteNonQueryAsync();
                }
            }

            // ����¹��鹷ҧ�˹�� Home ��ѧ�ҡ���������
            return RedirectToPage("/User/Home");
        }
    }
}
