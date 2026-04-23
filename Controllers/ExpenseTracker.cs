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
            using (NpgsqlConnection con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("sp_AddExpense", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Title", model.Title);
                cmd.Parameters.AddWithValue("@Amount", model.Amount);
                cmd.Parameters.AddWithValue("@Category", model.Category);
                cmd.Parameters.AddWithValue("@ExpenseDate", model.ExpenseDate);
                cmd.Parameters.AddWithValue("@UserId", model.UserId);
                cmd.Parameters.AddWithValue("@Notes", model.Notes ?? string.Empty);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return Ok(new { message = "Added Successfully" });
        }

        // TEST API
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("API WORKING");
        }

        // TEST DATABASE CONNECTION
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

        // GET USERS (FIXED VERSION)
        [HttpGet("test-users")]
        public IActionResult GetUsers()
        {
            try
            {
                var connStr = _configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrEmpty(connStr))
                {
                    return StatusCode(500, "Connection string is NULL or EMPTY");
                }

                List<object> users = new List<object>();

                using (NpgsqlConnection con = new NpgsqlConnection(connStr))
                {
                    con.Open();

                    using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT id, name, email FROM users", con))
                    {
                        cmd.CommandTimeout = 30;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                users.Add(new
                                {
                                    Id = reader["id"],
                                    Name = reader["name"],
                                    Email = reader["email"]
                                });
                            }
                        }
                    }
                }

                return Ok(new
                {
                    success = true,
                    count = users.Count,
                    data = users
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = ex.ToString()
                });
            }
        }

        // DEBUG CONNECTION
        [HttpGet("debug-full-conn")]
        public IActionResult DebugFullConn()
        {
            var conn = _configuration.GetConnectionString("DefaultConnection");

            return Ok(new
            {
                conn
            });
        }


        [HttpPost("add-income")]
        public IActionResult AddIncome(IncomeModel model)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("sp_addincome", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("_title", model.Title);
                cmd.Parameters.AddWithValue("_amount", model.Amount);
                cmd.Parameters.AddWithValue("_incomedate", model.IncomeDate);
                cmd.Parameters.AddWithValue("_userid", model.UserId);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return Ok(new { message = "Income Added Successfully" });
        }

        [HttpGet("dashboard-summary/{userId}")]
        public IActionResult GetDashboardSummary(long userId)
        {
            using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var cmd = new NpgsqlCommand("sp_getdashboardsummary", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("_userid", userId);

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


        [HttpGet("user-profile/{userId}")]
        public IActionResult GetUserProfile(long userId)
        {
            using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var cmd = new NpgsqlCommand("sp_getuserprofile", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("_userid", userId);

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


        [HttpPost("save-theme")]
        public IActionResult SaveTheme(ThemeModel model)
        {
            try
            {
                using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new NpgsqlCommand("sp_saveusertheme", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("_userid", model.UserId);
                cmd.Parameters.AddWithValue("_darkmode", model.DarkMode);

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

        [HttpGet("get-theme/{userId}")]
        public IActionResult GetTheme(long userId)
        {
            using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var cmd = new NpgsqlCommand("sp_getusertheme", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("_userid", userId);

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

        [HttpPost("change-password")]
        public IActionResult ChangePassword(ChangePasswordDto model)
        {
            using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var cmd = new NpgsqlCommand("changepassword", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("_userid", model.Id);
            cmd.Parameters.AddWithValue("_currentpassword", model.CurrentPassword);
            cmd.Parameters.AddWithValue("_newpassword", model.NewPassword);

            con.Open();

            var result = cmd.ExecuteScalar();

            return Ok(new { message = result });
        }

        [HttpDelete("delete-account/{userId}")]
        public IActionResult DeleteAccount(long userId)
        {
            using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var cmd = new NpgsqlCommand("deleteuseraccount", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("_userid", userId);

            con.Open();
            cmd.ExecuteNonQuery();

            return Ok(new { message = "Account Deleted" });
        }

        [HttpDelete("clear-data/{userId}")]
        public IActionResult ClearUserData(long userId)
        {
            using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var cmd = new NpgsqlCommand("clearuserdata", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("_userid", userId);

            con.Open();
            cmd.ExecuteNonQuery();

            return Ok(new { message = "User Data Cleared" });
        }
    }
}