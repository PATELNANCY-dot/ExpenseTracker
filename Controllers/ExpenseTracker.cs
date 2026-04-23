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
            using (NpgsqlConnection con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                string query = "INSERT INTO users (name, email, password) VALUES (@Name, @Email, @Password)";

                NpgsqlCommand cmd = new NpgsqlCommand(query, con);

                cmd.Parameters.AddWithValue("@Name", model.Name ?? "");
                cmd.Parameters.AddWithValue("@Email", model.Email ?? "");
                cmd.Parameters.AddWithValue("@Password", model.Password ?? "");

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return Ok(new { message = "User Registered Successfully" });
        }

        // LOGIN USER
        [HttpPost("login")]
        public IActionResult Login(LoginDto model)
        {
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    con.Open();

                    string query = "SELECT * FROM users WHERE email = @Email AND password = @Password";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Email", model.Email);
                        cmd.Parameters.AddWithValue("@Password", model.Password);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var user = new
                                {
                                    Id = reader["id"],
                                    Name = reader["name"],
                                    Email = reader["email"]
                                };

                                return Ok(new
                                {
                                    success = true,
                                    message = "Login Successful",
                                    data = user
                                });
                            }
                        }
                    }
                }

                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid Credentials"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // ADD EXPENSE
        [HttpPost("add-expense")]
        public IActionResult AddExpense(ExpenseModel model)
        {
            using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var cmd = new NpgsqlCommand("SELECT sp_addexpense(@Title,@Amount,@Category,@ExpenseDate,@UserId,@Notes)", con);

            cmd.Parameters.AddWithValue("@Title", model.Title);
            cmd.Parameters.AddWithValue("@Amount", model.Amount);
            cmd.Parameters.AddWithValue("@Category", model.Category);
            cmd.Parameters.AddWithValue("@ExpenseDate", model.ExpenseDate);
            cmd.Parameters.AddWithValue("@UserId", model.UserId);
            cmd.Parameters.AddWithValue("@Notes", model.Notes ?? string.Empty);

            con.Open();
            cmd.ExecuteNonQuery();

            return Ok(new { message = "Added Successfully" });
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("API WORKING");
        }

        [HttpGet("db-test")]
        public IActionResult DbTest()
        {
            try
            {
                using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                con.Open();
                return Ok("DB CONNECTED");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ADD INCOME
        [HttpPost("add-income")]
        public IActionResult AddIncome(IncomeModel model)
        {
            using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var cmd = new NpgsqlCommand("SELECT sp_addincome(@_title,@_amount,@_incomedate,@_userid)", con);

            cmd.Parameters.AddWithValue("@_title", model.Title);
            cmd.Parameters.AddWithValue("@_amount", model.Amount);
            cmd.Parameters.AddWithValue("@_incomedate", model.IncomeDate);
            cmd.Parameters.AddWithValue("@_userid", model.UserId);

            con.Open();
            cmd.ExecuteNonQuery();

            return Ok(new { message = "Income Added Successfully" });
        }

        // DASHBOARD SUMMARY
        [HttpGet("dashboard-summary/{userId}")]
        public IActionResult GetDashboardSummary(long userId)
        {
            using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var cmd = new NpgsqlCommand("SELECT * FROM sp_getdashboardsummary(@_userid)", con);
            cmd.Parameters.AddWithValue("@_userid", userId);

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

        // USER PROFILE
        [HttpGet("user-profile/{userId}")]
        public IActionResult GetUserProfile(long userId)
        {
            using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var cmd = new NpgsqlCommand("SELECT * FROM sp_getuserprofile(@_userid)", con);
            cmd.Parameters.AddWithValue("@_userid", userId);

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

        // SAVE THEME
        [HttpPost("save-theme")]
        public IActionResult SaveTheme(ThemeModel model)
        {
            try
            {
                using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new NpgsqlCommand("SELECT sp_saveusertheme(@_userid,@_darkmode)", con);

                cmd.Parameters.AddWithValue("@_userid", model.UserId);
                cmd.Parameters.AddWithValue("@_darkmode", model.DarkMode);

                con.Open();
                cmd.ExecuteNonQuery();

                return Ok(new { message = "Theme Saved" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex.Message
                });
            }
        }

        // GET THEME
        [HttpGet("get-theme/{userId}")]
        public IActionResult GetTheme(long userId)
        {
            using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var cmd = new NpgsqlCommand("SELECT * FROM sp_getusertheme(@_userid)", con);
            cmd.Parameters.AddWithValue("@_userid", userId);

            con.Open();

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return Ok(new
                {
                    darkMode = reader["darkmode"]
                });
            }

            return Ok(new { darkMode = false });
        }

        // CHANGE PASSWORD
        [HttpPost("change-password")]
        public IActionResult ChangePassword(ChangePasswordDto model)
        {
            using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var cmd = new NpgsqlCommand("SELECT changepassword(@_userid,@_currentpassword,@_newpassword)", con);

            cmd.Parameters.AddWithValue("@_userid", model.Id);
            cmd.Parameters.AddWithValue("@_currentpassword", model.CurrentPassword);
            cmd.Parameters.AddWithValue("@_newpassword", model.NewPassword);

            con.Open();

            var result = cmd.ExecuteScalar();

            return Ok(new { message = result });
        }

        // DELETE ACCOUNT
        [HttpDelete("delete-account/{userId}")]
        public IActionResult DeleteAccount(long userId)
        {
            using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var cmd = new NpgsqlCommand("SELECT deleteuseraccount(@_userid)", con);
            cmd.Parameters.AddWithValue("@_userid", userId);

            con.Open();
            cmd.ExecuteNonQuery();

            return Ok(new { message = "Account Deleted" });
        }

        // CLEAR USER DATA
        [HttpDelete("clear-data/{userId}")]
        public IActionResult ClearUserData(long userId)
        {
            using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var cmd = new NpgsqlCommand("SELECT clearuserdata(@_userid)", con);
            cmd.Parameters.AddWithValue("@_userid", userId);

            con.Open();
            cmd.ExecuteNonQuery();

            return Ok(new { message = "User Data Cleared" });
        }
    }
}