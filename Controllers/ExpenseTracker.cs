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

        // REGISTER
        [HttpPost("register")]
        public IActionResult Register(UserModel model)
        {
            try
            {
                using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new NpgsqlCommand(
                    "INSERT INTO users(name,email,password) VALUES(@n,@e,@p)", con);

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

        // LOGIN
        [HttpPost("login")]
        public IActionResult Login(LoginDto model)
        {
            try
            {
                using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new NpgsqlCommand(
                    "SELECT id,name,email FROM users WHERE email=@e AND password=@p", con);

                cmd.Parameters.AddWithValue("@e", model.Email);
                cmd.Parameters.AddWithValue("@p", model.Password);

                con.Open();

                using var r = cmd.ExecuteReader();

                if (r.Read())
                {
                    return Ok(new
                    {
                        id = r["id"],
                        name = r["name"],
                        email = r["email"]
                    });
                }

                return Unauthorized();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // EXPENSE
        [HttpPost("add-expense")]
        public IActionResult AddExpense(ExpenseModel model)
        {
            try
            {
                using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new NpgsqlCommand(@"
                    INSERT INTO expenses(title,amount,category,expensedate,userid,notes)
                    VALUES(@t,@a,@c,@d,@u,@n)", con);

                cmd.Parameters.AddWithValue("@t", model.Title);
                cmd.Parameters.AddWithValue("@a", model.Amount);
                cmd.Parameters.AddWithValue("@c", model.Category);
                cmd.Parameters.AddWithValue("@d", model.ExpenseDate);
                cmd.Parameters.AddWithValue("@u", model.UserId);
                cmd.Parameters.AddWithValue("@n", model.Notes ?? "");

                con.Open();
                cmd.ExecuteNonQuery();

                return Ok("Expense Added");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // INCOME  (FIXED TABLE NAME)
        [HttpPost("add-income")]
        public IActionResult AddIncome(IncomeModel model)
        {
            try
            {
                using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new NpgsqlCommand(@"
                    INSERT INTO income(title,amount,incomedate,userid)
                    VALUES(@t,@a,@d,@u)", con);

                cmd.Parameters.AddWithValue("@t", model.Title);
                cmd.Parameters.AddWithValue("@a", model.Amount);
                cmd.Parameters.AddWithValue("@d", model.IncomeDate);
                cmd.Parameters.AddWithValue("@u", model.UserId);

                con.Open();
                cmd.ExecuteNonQuery();

                return Ok("Income Added");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // DASHBOARD (FIXED - NO JOIN BUG)
        [HttpGet("dashboard-summary/{userId}")]
        public IActionResult Dashboard(long userId)
        {
            try
            {
                using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new NpgsqlCommand(@"
                    SELECT 
                        (SELECT COALESCE(SUM(amount),0) FROM income WHERE userid=@id) AS income,
                        (SELECT COALESCE(SUM(amount),0) FROM expenses WHERE userid=@id) AS expense
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

        // PROFILE
        [HttpGet("user-profile/{id}")]
        public IActionResult Profile(long id)
        {
            try
            {
                using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new NpgsqlCommand(
                    "SELECT id,name,email,createdat FROM users WHERE id=@id", con);

                cmd.Parameters.AddWithValue("@id", id);

                con.Open();

                using var r = cmd.ExecuteReader();

                if (r.Read())
                {
                    return Ok(new
                    {
                        id = r["id"],
                        name = r["name"],
                        email = r["email"]
                    });
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
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
                    VALUES(@u,@d)
                    ON CONFLICT(userid)
                    DO UPDATE SET darkmode=@d", con);

                cmd.Parameters.AddWithValue("@u", model.UserId);
                cmd.Parameters.AddWithValue("@d", model.DarkMode);

                con.Open();
                cmd.ExecuteNonQuery();

                return Ok("Theme Saved");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // GET THEME (FIXED NULL ISSUE)
        [HttpGet("get-theme/{userId}")]
        public IActionResult GetTheme(long userId)
        {
            try
            {
                using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new NpgsqlCommand(
                    "SELECT darkmode FROM usersettings WHERE userid=@id", con);

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

        // DELETE USER
        [HttpDelete("delete-account/{id}")]
        public IActionResult Delete(long id)
        {
            try
            {
                using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new NpgsqlCommand("DELETE FROM users WHERE id=@id", con);
                cmd.Parameters.AddWithValue("@id", id);

                con.Open();
                cmd.ExecuteNonQuery();

                return Ok("Deleted");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // CLEAR DATA
        [HttpDelete("clear-data/{id}")]
        public IActionResult Clear(long id)
        {
            try
            {
                using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                con.Open();

                new NpgsqlCommand("DELETE FROM expenses WHERE userid=@id", con)
                { Parameters = { new NpgsqlParameter("@id", id) } }.ExecuteNonQuery();

                new NpgsqlCommand("DELETE FROM income WHERE userid=@id", con)
                { Parameters = { new NpgsqlParameter("@id", id) } }.ExecuteNonQuery();

                return Ok("Data Cleared");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("get-expenses/{userId}")]
        public IActionResult GetExpenses(long userId)
        {
            try
            {
                using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new NpgsqlCommand(
                    "SELECT id,title,amount,category,expensedate,notes FROM expenses WHERE userid=@id ORDER BY expensedate DESC", con);

                cmd.Parameters.AddWithValue("@id", userId);

                con.Open();

                var list = new List<object>();

                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    list.Add(new
                    {
                        id = r["id"],
                        title = r["title"],
                        amount = r["amount"],
                        category = r["category"],
                        expenseDate = r["expensedate"],
                        notes = r["notes"]
                    });
                }

                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("get-income/{userId}")]
        public IActionResult GetIncome(long userId)
        {
            try
            {
                using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var cmd = new NpgsqlCommand(
                    "SELECT id,title,amount,incomedate FROM income WHERE userid=@id", con);

                cmd.Parameters.AddWithValue("@id", userId);

                con.Open();

                var list = new List<object>();

                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    list.Add(new
                    {
                        id = r["id"],
                        title = r["title"],
                        amount = r["amount"],
                        incomeDate = r["incomedate"]
                    });
                }

                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPut("update-expense/{id}")]
        public IActionResult UpdateExpense(long id, ExpenseModel model)
        {
            using var con = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var cmd = new NpgsqlCommand(@"
        UPDATE expenses 
        SET title=@t, amount=@a, category=@c, expensedate=@d, notes=@n
        WHERE id=@id", con);

            cmd.Parameters.AddWithValue("@t", model.Title);
            cmd.Parameters.AddWithValue("@a", model.Amount);
            cmd.Parameters.AddWithValue("@c", model.Category);
            cmd.Parameters.AddWithValue("@d", model.ExpenseDate);
            cmd.Parameters.AddWithValue("@n", model.Notes ?? "");
            cmd.Parameters.AddWithValue("@id", id);

            con.Open();
            cmd.ExecuteNonQuery();

            return Ok("Updated");
        }
    }
}