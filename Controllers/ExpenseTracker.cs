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
            using (NpgsqlConnection con = new NpgsqlConnection(_configuration.GetConnectionString("Default")))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("sp_RegisterUser", con);
                cmd.CommandType = CommandType.StoredProcedure;

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
                DataTable dt = new DataTable();

                using (NpgsqlConnection con = new NpgsqlConnection(_configuration.GetConnectionString("Default")))
                {
                    con.Open();

                    string query = "SELECT * FROM users WHERE email = @Email AND password = @Password";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Email", model.Email);
                        cmd.Parameters.AddWithValue("@Password", model.Password);

                        using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }

                if (dt.Rows.Count > 0)
                {
                    var user = dt.AsEnumerable().Select(row => new
                    {
                        Id = row["id"],
                        Name = row["name"],
                        Email = row["email"]
                    }).FirstOrDefault();

                    return Ok(new
                    {
                        success = true,
                        message = "Login Successful",
                        data = user
                    });
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
            using (NpgsqlConnection con = new NpgsqlConnection(_configuration.GetConnectionString("Default")))
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
                using var con = new NpgsqlConnection(_configuration.GetConnectionString("Default"));
                con.Open();
                return Ok("DB CONNECTED");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("test-users")]
        public IActionResult GetUsers()
        {
            try
            {
                var connStr = _configuration.GetConnectionString("Default");

                if (string.IsNullOrEmpty(connStr))
                {
                    return StatusCode(500, "Connection string is NULL or EMPTY");
                }

                DataTable dt = new DataTable();

                using (NpgsqlConnection con = new NpgsqlConnection(connStr))
                {
                    con.Open();

                    using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM users", con))
                    using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                return Ok(new
                {
                    success = true,
                    count = dt.Rows.Count,
                    data = dt
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


        [HttpGet("debug-full-conn")]
        public IActionResult DebugFullConn()
        {
            var conn = _configuration.GetConnectionString("Default");

            return Ok(new
            {
                conn
            });
        }
    }  


}
