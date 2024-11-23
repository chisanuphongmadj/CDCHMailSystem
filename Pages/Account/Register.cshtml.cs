using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace CDCHMailSystem.Pages.Account
{
    public class RegisterModel : PageModel
    {
        [BindProperty]
        [Required]
        public string FirstName { get; set; }

        [BindProperty]
        [Required]
        public string LastName { get; set; }

        [BindProperty]
        [Required]
        public string Phone { get; set; }

        [BindProperty]
        [Required]
        public string Username { get; set; }

        [BindProperty]
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // ź��ͧ��ҧ�ҡ�Թ�ص������
            FirstName = FirstName?.Trim();
            LastName = LastName?.Trim();
            Phone = Phone?.Trim();
            Username = Username?.Trim();
            Password = Password?.Trim();

            // ��Ǩ�ͺ��� FirstName ��� LastName �������ѡ�� A-Z ���� a-z
            if (!Regex.IsMatch(FirstName ?? string.Empty, @"^[A-Za-z]+$"))
            {
                ModelState.AddModelError(nameof(FirstName), "First name must contain only alphabetic characters.");
                return Page();
            }

            if (!Regex.IsMatch(LastName ?? string.Empty, @"^[A-Za-z]+$"))
            {
                ModelState.AddModelError(nameof(LastName), "Last name must contain only alphabetic characters.");
                return Page();
            }

            // ��Ǩ�ͺ��� Phone �繵���Ţ 10 ��ѡ
            if (!Regex.IsMatch(Phone ?? string.Empty, @"^\d{10}$"))
            {
                ModelState.AddModelError(nameof(Phone), "Phone number must be exactly 10 numeric digits.");
                return Page();
            }

            // ��Ǩ�ͺ��� Username ��੾�е���ѡ�� ASCII (�鹾����)
            if (!Regex.IsMatch(Username ?? string.Empty, @"^[\x20-\x7E]+$"))
            {
                ModelState.AddModelError(nameof(Username), "Username must contain only printable ASCII characters.");
                return Page();
            }

            // ��Ǩ�ͺ��� Password ��੾�е���ѡ�� ASCII (�鹾����)
            if (!Regex.IsMatch(Password ?? string.Empty, @"^[\x20-\x7E]+$"))
            {
                ModelState.AddModelError(nameof(Password), "Password must contain only printable ASCII characters.");
                return Page();
            }

            string connectionString = "Server=tcp:cdchemail.database.windows.net,1433;Initial Catalog=cdchemailsystem;Persist Security Info=False;User ID=cdch1234;Password=1234@cdch;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // ��Ǩ�ͺ��� Username �����������������
                    string checkQuery = "SELECT COUNT(1) FROM Users WHERE Username = @Username";
                    using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@Username", Username);
                        int usernameExists = (int)await checkCommand.ExecuteScalarAsync();

                        if (usernameExists > 0)
                        {
                            ModelState.AddModelError(nameof(Username), "The username already exists. Please choose a different username.");
                            return Page();
                        }
                    }

                    // ��� Username �ѧ�����㹰ҹ������ ���ӡ�� INSERT
                    string query = "INSERT INTO Users (FirstName, LastName, Phone, Username, Password) VALUES (@FirstName, @LastName, @Phone, @Username, @Password)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FirstName", FirstName);
                        command.Parameters.AddWithValue("@LastName", LastName);
                        command.Parameters.AddWithValue("@Phone", Phone);
                        command.Parameters.AddWithValue("@Username", Username);
                        command.Parameters.AddWithValue("@Password", Password);

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (SqlException ex)
            {
                // �Ѵ��â�ͼԴ��Ҵ����������Ͱҹ������
                ModelState.AddModelError(string.Empty, "An error occurred while processing your request. Please try again later.");
                Console.WriteLine($"SQL Error: {ex.Message}");
                return Page();
            }

            return RedirectToPage("/Account/Login");
        }
    }
}
