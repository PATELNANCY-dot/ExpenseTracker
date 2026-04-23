using System.Data;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ExpenseTracker.Models;

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

        // REGISTER USER
        [HttpPost("register")]
        public IActionResult Register(UserModel model)
        {
            try
            {
                using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new NpgsqlCommand(
                    "INSERT INTO users(name,email,password) VALUES(@name,@email,@password)", con);

                cmd.Parameters.AddWithValue("@name", model.Name ?? "");
                cmd.Parameters.AddWithValue("@email", model.Email ?? "");
                cmd.Parameters.AddWithValue("@password", model.Password ?? "");

                con.Open();
                cmd.ExecuteNonQuery();

                return Ok(new { message = "User Registered Successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.ToString() });
            }
        }

        // LOGIN
        [HttpPost("login")]
        public IActionResult Login(LoginDto model)
        {
            try
            {
                using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new NpgsqlCommand(
                    "SELECT id,name,email FROM users WHERE email=@email AND password=@password", con);

                cmd.Parameters.AddWithValue("@email", model.Email);
                cmd.Parameters.AddWithValue("@password", model.Password);

                con.Open();

                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return Ok(new
                    {
                        success = true,
                        data = new
                        {
                            Id = reader["id"],
                            Name = reader["name"],
                            Email = reader["email"]
                        }
                    });
                }

                return Unauthorized(new { success = false });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.ToString() });
            }
        }

        // ADD EXPENSE
        [HttpPost("add-expense")]
        public IActionResult AddExpense(ExpenseModel model)
        {
            try
            {
                using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new NpgsqlCommand(@"
                    INSERT INTO expenses(title,amount,category,expensedate,userid,notes)
                    VALUES(@title,@amount,@category,@date,@userid,@notes)", con);

                cmd.Parameters.AddWithValue("@title", model.Title);
                cmd.Parameters.AddWithValue("@amount", model.Amount);
                cmd.Parameters.AddWithValue("@category", model.Category);
                cmd.Parameters.AddWithValue("@date", model.ExpenseDate);
                cmd.Parameters.AddWithValue("@userid", model.UserId);
                cmd.Parameters.AddWithValue("@notes", model.Notes ?? "");

                con.Open();
                cmd.ExecuteNonQuery();

                return Ok(new { message = "Added Successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.ToString() });
            }
        }

        // ADD INCOME
        [HttpPost("add-income")]
        public IActionResult AddIncome(IncomeModel model)
        {
            try
            {
                using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new NpgsqlCommand(@"
                    INSERT INTO incomes(title,amount,incomedate,userid)
                    VALUES(@title,@amount,@date,@userid)", con);

                cmd.Parameters.AddWithValue("@title", model.Title);
                cmd.Parameters.AddWithValue("@amount", model.Amount);
                cmd.Parameters.AddWithValue("@date", model.IncomeDate);
                cmd.Parameters.AddWithValue("@userid", model.UserId);

                con.Open();
                cmd.ExecuteNonQuery();

                return Ok(new { message = "Income Added Successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.ToString() });
            }
        }

        // DASHBOARD SUMMARY
        [HttpGet("dashboard-summary/{userId}")]
        public IActionResult GetDashboardSummary(long userId)
        {
            try
            {
                using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new NpgsqlCommand(@"
                    SELECT 
                        COALESCE(SUM(i.amount),0) AS totalIncome,
                        COALESCE(SUM(e.amount),0) AS totalExpense,
                        COALESCE(SUM(i.amount),0) - COALESCE(SUM(e.amount),0) AS balance
                    FROM incomes i
                    FULL JOIN expenses e ON i.userid = e.userid
                    WHERE COALESCE(i.userid,e.userid) = @userid", con);

                cmd.Parameters.AddWithValue("@userid", userId);

                con.Open();

                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return Ok(new
                    {
                        totalIncome = reader["totalincome"],
                        totalExpense = reader["totalexpense"],
                        balance = reader["balance"]
                    });
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.ToString() });
            }
        }

        // USER PROFILE
        [HttpGet("user-profile/{userId}")]
        public IActionResult GetUserProfile(long userId)
        {
            try
            {
                using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new NpgsqlCommand(
                    "SELECT id,name,email,createdat FROM users WHERE id=@id", con);

                cmd.Parameters.AddWithValue("@id", userId);

                con.Open();

                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return Ok(new
                    {
                        id = reader["id"],
                        name = reader["name"],
                        email = reader["email"],
                        createdAt = reader["createdat"]
                    });
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.ToString() });
            }
        }

        // SAVE THEME
        [HttpPost("save-theme")]
        public IActionResult SaveTheme(ThemeModel model)
        {
            try
            {
                using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new NpgsqlCommand(@"
                    INSERT INTO usersettings(userid,darkmode)
                    VALUES(@userid,@darkmode)
                    ON CONFLICT(userid)
                    DO UPDATE SET darkmode = EXCLUDED.darkmode", con);

                cmd.Parameters.AddWithValue("@userid", model.UserId);
                cmd.Parameters.AddWithValue("@darkmode", model.DarkMode);

                con.Open();
                cmd.ExecuteNonQuery();

                return Ok(new { message = "Theme Saved" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.ToString() });
            }
        }

        // GET THEME
        [HttpGet("get-theme/{userId}")]
        public IActionResult GetTheme(long userId)
        {
            try
            {
                using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new NpgsqlCommand(
                    "SELECT darkmode FROM usersettings WHERE userid=@userid", con);

                cmd.Parameters.AddWithValue("@userid", userId);

                con.Open();

                var result = cmd.ExecuteScalar();

                return Ok(new
                {
                    darkMode = result ?? false
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.ToString() });
            }
        }

        // CHANGE PASSWORD
        [HttpPost("change-password")]
        public IActionResult ChangePassword(ChangePasswordDto model)
        {
            try
            {
                using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new NpgsqlCommand(@"
                    UPDATE users 
                    SET password=@newPassword
                    WHERE id=@id AND password=@currentPassword", con);

                cmd.Parameters.AddWithValue("@id", model.Id);
                cmd.Parameters.AddWithValue("@currentPassword", model.CurrentPassword);
                cmd.Parameters.AddWithValue("@newPassword", model.NewPassword);

                con.Open();

                var rows = cmd.ExecuteNonQuery();

                return Ok(new { message = rows > 0 ? "Password Updated" : "Invalid Password" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.ToString() });
            }
        }

        // DELETE ACCOUNT
        [HttpDelete("delete-account/{userId}")]
        public IActionResult DeleteAccount(long userId)
        {
            try
            {
                using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new NpgsqlCommand("DELETE FROM users WHERE id=@id", con);
                cmd.Parameters.AddWithValue("@id", userId);

                con.Open();
                cmd.ExecuteNonQuery();

                return Ok(new { message = "Account Deleted" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.ToString() });
            }
        }

        // CLEAR USER DATA
        [HttpDelete("clear-data/{userId}")]
        public IActionResult ClearUserData(long userId)
        {
            try
            {
                using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                con.Open();

                new NpgsqlCommand("DELETE FROM expenses WHERE userid=@id", con)
                { Parameters = { new NpgsqlParameter("@id", userId) } }
                    .ExecuteNonQuery();

                new NpgsqlCommand("DELETE FROM incomes WHERE userid=@id", con)
                { Parameters = { new NpgsqlParameter("@id", userId) } }
                    .ExecuteNonQuery();

                return Ok(new { message = "User Data Cleared" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.ToString() });
            }
        }
    }
}