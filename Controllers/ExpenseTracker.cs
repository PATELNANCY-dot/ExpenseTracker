using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ExpenseTracker.Models;
using MimeKit;
using MailKit.Net.Smtp;

namespace ExpenseTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpenseTrackerController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ExpenseTrackerController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ===================== REGISTER =====================
        [HttpPost("register")]
        public IActionResult Register(UserModel model)
        {
            try
            {
                using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new SqlCommand(@"
                    INSERT INTO Users(Name,Email,Password)
                    VALUES(@n,@e,@p)", con);

                cmd.Parameters.AddWithValue("@n", model.Name ?? "");
                cmd.Parameters.AddWithValue("@e", model.Email ?? "");
                cmd.Parameters.AddWithValue("@p", model.Password ?? "");

                con.Open();
                cmd.ExecuteNonQuery();

                return Ok(new { message = "User Registered" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // ===================== LOGIN =====================
        [HttpPost("login")]
        public IActionResult Login(LoginDto model)
        {
            try
            {
                using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new SqlCommand(@"
                    SELECT Id,Name,Email
                    FROM Users
                    WHERE Email=@e AND Password=@p", con);

                cmd.Parameters.AddWithValue("@e", model.Email);
                cmd.Parameters.AddWithValue("@p", model.Password);

                con.Open();

                using var r = cmd.ExecuteReader();

                if (r.Read())
                {
                    return Ok(new
                    {
                        id = r["Id"],
                        name = r["Name"],
                        email = r["Email"]
                    });
                }

                return Unauthorized(new { message = "Invalid credentials" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // ===================== EXPENSES =====================

        [HttpGet("get-expenses/{userId}")]
        public IActionResult GetExpenses(long userId)
        {
            try
            {
                var list = new List<object>();

                using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new SqlCommand(@"
                    SELECT * FROM Expenses WHERE UserId=@id ORDER BY ExpenseDate DESC", con);

                cmd.Parameters.AddWithValue("@id", userId);

                con.Open();
                using var r = cmd.ExecuteReader();

                while (r.Read())
                {
                    list.Add(new
                    {
                        id = r["Id"],
                        title = r["Title"],
                        amount = r["Amount"],
                        category = r["Category"],
                        expenseDate = r["ExpenseDate"],
                        notes = r["Notes"]
                    });
                }

                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("add-expense")]
        public IActionResult AddExpense(ExpenseModel model)
        {
            try
            {
                using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new SqlCommand(@"
                    INSERT INTO Expenses(Title,Amount,Category,ExpenseDate,UserId,Notes)
                    VALUES(@t,@a,@c,@d,@u,@n)", con);

                cmd.Parameters.AddWithValue("@t", model.Title);
                cmd.Parameters.AddWithValue("@a", model.Amount);
                cmd.Parameters.AddWithValue("@c", model.Category ?? "");
                cmd.Parameters.AddWithValue("@d", model.ExpenseDate);
                cmd.Parameters.AddWithValue("@u", model.UserId);
                cmd.Parameters.AddWithValue("@n", model.Notes ?? "");

                con.Open();
                cmd.ExecuteNonQuery();

                return Ok(new { message = "Expense Added" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("update-expense/{id}")]
        public IActionResult UpdateExpense(long id, ExpenseModel model)
        {
            try
            {
                using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new SqlCommand(@"
                    UPDATE Expenses
                    SET Title=@t,Amount=@a,Category=@c,ExpenseDate=@d,Notes=@n
                    WHERE Id=@id", con);

                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@t", model.Title);
                cmd.Parameters.AddWithValue("@a", model.Amount);
                cmd.Parameters.AddWithValue("@c", model.Category ?? "");
                cmd.Parameters.AddWithValue("@d", model.ExpenseDate);
                cmd.Parameters.AddWithValue("@n", model.Notes ?? "");

                con.Open();
                cmd.ExecuteNonQuery();

                return Ok(new { message = "Updated" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("delete-expense/{id}")]
        public IActionResult DeleteExpense(long id)
        {
            try
            {
                using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new SqlCommand("DELETE FROM Expenses WHERE Id=@id", con);
                cmd.Parameters.AddWithValue("@id", id);

                con.Open();
                cmd.ExecuteNonQuery();

                return Ok(new { message = "Deleted" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // ===================== INCOME =====================

        [HttpGet("get-income/{userId}")]
        public IActionResult GetIncome(long userId)
        {
            try
            {
                var list = new List<object>();

                using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new SqlCommand(@"
                    SELECT * FROM Income WHERE UserId=@id ORDER BY IncomeDate DESC", con);

                cmd.Parameters.AddWithValue("@id", userId);

                con.Open();
                using var r = cmd.ExecuteReader();

                while (r.Read())
                {
                    list.Add(new
                    {
                        id = r["Id"],
                        title = r["Title"],
                        amount = r["Amount"],
                        incomeDate = r["IncomeDate"]
                    });
                }

                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("add-income")]
        public IActionResult AddIncome(IncomeModel model)
        {
            try
            {
                using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new SqlCommand(@"
                    INSERT INTO Income(Title,Amount,IncomeDate,UserId)
                    VALUES(@t,@a,@d,@u)", con);

                cmd.Parameters.AddWithValue("@t", model.Title);
                cmd.Parameters.AddWithValue("@a", model.Amount);
                cmd.Parameters.AddWithValue("@d", model.IncomeDate);
                cmd.Parameters.AddWithValue("@u", model.UserId);

                con.Open();
                cmd.ExecuteNonQuery();

                return Ok(new { message = "Income Added" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("update-income/{id}")]
        public IActionResult UpdateIncome(long id, IncomeModel model)
        {
            try
            {
                using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new SqlCommand(@"
                    UPDATE Income
                    SET Title=@t,Amount=@a,IncomeDate=@d
                    WHERE Id=@id", con);

                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@t", model.Title);
                cmd.Parameters.AddWithValue("@a", model.Amount);
                cmd.Parameters.AddWithValue("@d", model.IncomeDate);

                con.Open();
                cmd.ExecuteNonQuery();

                return Ok(new { message = "Updated" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("delete-income/{id}")]
        public IActionResult DeleteIncome(long id)
        {
            try
            {
                using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new SqlCommand("DELETE FROM Income WHERE Id=@id", con);
                cmd.Parameters.AddWithValue("@id", id);

                con.Open();
                cmd.ExecuteNonQuery();

                return Ok(new { message = "Deleted" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // ===================== DASHBOARD =====================

        [HttpGet("dashboard/{userId}")]
        public IActionResult Dashboard(long userId)
        {
            try
            {
                using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new SqlCommand(@"
                    SELECT 
                        (SELECT ISNULL(SUM(Amount),0) FROM Income WHERE UserId=@id) AS income,
                        (SELECT ISNULL(SUM(Amount),0) FROM Expenses WHERE UserId=@id) AS expense
                ", con);

                cmd.Parameters.AddWithValue("@id", userId);

                con.Open();

                using var r = cmd.ExecuteReader();

                if (r.Read())
                {
                    decimal income = Convert.ToDecimal(r["income"]);
                    decimal expense = Convert.ToDecimal(r["expense"]);

                    return Ok(new
                    {
                        totalIncome = income,
                        totalExpense = expense,
                        balance = income - expense
                    });
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // ===================== USER PROFILE =====================

        [HttpGet("get-user-profile/{id}")]
        public IActionResult GetUserProfile(long id)
        {
            try
            {
                using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new SqlCommand(@"
                    SELECT Id,Name,Email
                    FROM Users
                    WHERE Id=@id", con);

                cmd.Parameters.AddWithValue("@id", id);

                con.Open();

                using var r = cmd.ExecuteReader();

                if (r.Read())
                {
                    return Ok(new
                    {
                        id = r["Id"],
                        name = r["Name"],
                        email = r["Email"]
                    });
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("update-profile")]
        public IActionResult UpdateProfile([FromBody] UserModel model)
        {
            try
            {
                using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new SqlCommand(@"
                    UPDATE Users SET Name=@n, Email=@e WHERE Id=@id", con);

                cmd.Parameters.AddWithValue("@id", model.Id);
                cmd.Parameters.AddWithValue("@n", model.Name);
                cmd.Parameters.AddWithValue("@e", model.Email);

                con.Open();
                cmd.ExecuteNonQuery();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("change-password")]
        public IActionResult ChangePassword(ChangePasswordDto model)
        {
            try
            {
                using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                con.Open();

                // STEP 1: check old password
                var checkCmd = new SqlCommand(@"
            SELECT Password FROM Users WHERE Id=@id", con);

                checkCmd.Parameters.AddWithValue("@id", model.Id);

                var dbPassword = checkCmd.ExecuteScalar()?.ToString();

                if (dbPassword == null)
                    return NotFound(new { message = "User not found" });

                if (dbPassword != model.CurrentPassword)
                    return BadRequest(new { message = "Wrong current password" });

                // STEP 2: update password
                var updateCmd = new SqlCommand(@"
            UPDATE Users SET Password=@new WHERE Id=@id", con);

                updateCmd.Parameters.AddWithValue("@id", model.Id);
                updateCmd.Parameters.AddWithValue("@new", model.NewPassword);

                updateCmd.ExecuteNonQuery();

                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ===================== CLEAR DATA =====================

        [HttpDelete("clear-user-data/{userId}")]
        public IActionResult ClearData(long userId)
        {
            try
            {
                using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                con.Open();

                new SqlCommand("DELETE FROM Expenses WHERE UserId=@id", con)
                { Parameters = { new("@id", userId) } }.ExecuteNonQuery();

                new SqlCommand("DELETE FROM Income WHERE UserId=@id", con)
                { Parameters = { new("@id", userId) } }.ExecuteNonQuery();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // ===================== THEME =====================

        [HttpPost("save-theme")]
        public IActionResult SaveTheme(ThemeModel model)
        {
            try
            {
                using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new SqlCommand(@"
                    IF EXISTS (SELECT 1 FROM UserSettings WHERE UserId=@u)
                        UPDATE UserSettings SET DarkMode=@d WHERE UserId=@u
                    ELSE
                        INSERT INTO UserSettings(UserId,DarkMode) VALUES(@u,@d)
                ", con);

                cmd.Parameters.AddWithValue("@u", model.UserId);
                cmd.Parameters.AddWithValue("@d", model.DarkMode);

                con.Open();
                cmd.ExecuteNonQuery();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("get-theme/{userId}")]
        public IActionResult GetTheme(long userId)
        {
            try
            {
                using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new SqlCommand("SELECT DarkMode FROM UserSettings WHERE UserId=@id", con);
                cmd.Parameters.AddWithValue("@id", userId);

                con.Open();

                var result = cmd.ExecuteScalar();

                return Ok(new
                {
                    darkMode = result != null && result != DBNull.Value && (bool)result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // ===================== DELETE USER =====================

        [HttpDelete("delete-account/{id}")]
        public IActionResult Delete(long id)
        {
            try
            {
                using var con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new SqlCommand("DELETE FROM Users WHERE Id=@id", con);
                cmd.Parameters.AddWithValue("@id", id);

                con.Open();
                cmd.ExecuteNonQuery();

                return Ok(new { message = "Deleted" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPost("send-otp")]
        public IActionResult SendOTP(string email)
        {
            try
            {
                using var con = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                // CHECK EMAIL EXISTS
                var checkCmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Users WHERE Email=@e", con);

                checkCmd.Parameters.AddWithValue("@e", email);

                con.Open();

                int exists = (int)checkCmd.ExecuteScalar();

                if (exists == 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Email not found"
                    });
                }

                // GENERATE OTP
                string otp = new Random().Next(100000, 999999).ToString();

                // INSERT OTP
                var insertCmd = new SqlCommand(@"
            INSERT INTO PasswordResetOTP
            (Email, OTPCode, ExpiryTime, IsUsed)
            VALUES
            (@email,@otp,@time,0)", con);

                insertCmd.Parameters.AddWithValue("@email", email);
                insertCmd.Parameters.AddWithValue("@otp", otp);
                insertCmd.Parameters.AddWithValue("@time",
                    DateTime.Now.AddMinutes(5));

                insertCmd.ExecuteNonQuery();

                con.Close();

                // SEND EMAIL
                var message = new MimeMessage();

                message.From.Add(
                    new MailboxAddress(
                        "Expense Tracker",
                        "nancypatel8002@gmail.com"));

                message.To.Add(MailboxAddress.Parse(email));

                message.Subject = "Password Reset OTP";

                message.Body = new TextPart("plain")
                {
                    Text = $"Your OTP is: {otp}\n\nValid for 5 minutes."
                };

                using var client = new MailKit.Net.Smtp.SmtpClient();

                client.Connect(
                    "smtp.gmail.com",
                    587,
                    MailKit.Security.SecureSocketOptions.StartTls);

                client.Authenticate(
                    "nancypatel8002@gmail.com",
                    "fxvb dcqb ergw icks");

                client.Send(message);

                client.Disconnect(true);

                return Ok(new
                {
                    success = true,
                    message = "OTP sent successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPost("verify-otp")]
        public IActionResult VerifyOTP(string email, string otp)
        {
            try
            {
                using var con = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var cmd = new SqlCommand(@"
            SELECT *
            FROM PasswordResetOTP
            WHERE Email=@e
            AND OTPCode=@o
            AND IsUsed=0
            AND ExpiryTime > GETDATE()", con);

                cmd.Parameters.AddWithValue("@e", email);
                cmd.Parameters.AddWithValue("@o", otp);

                con.Open();

                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    reader.Close();

                    // MARK OTP USED
                    var updateCmd = new SqlCommand(@"
                UPDATE PasswordResetOTP
                SET IsUsed=1
                WHERE Email=@e
                AND OTPCode=@o", con);

                    updateCmd.Parameters.AddWithValue("@e", email);
                    updateCmd.Parameters.AddWithValue("@o", otp);

                    updateCmd.ExecuteNonQuery();

                    return Ok(new
                    {
                        success = true,
                        message = "OTP verified"
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    message = "Invalid or expired OTP"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }



        [HttpPut("reset-password")]
        public IActionResult ResetPassword(string email, string password)
        {
            try
            {
                using var con = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var cmd = new SqlCommand(@"
            UPDATE Users
            SET Password=@p
            WHERE Email=@e", con);

                cmd.Parameters.AddWithValue("@e", email);
                cmd.Parameters.AddWithValue("@p", password);

                con.Open();

                int rows = cmd.ExecuteNonQuery();

                if (rows > 0)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Password reset successful"
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    message = "User not found"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}