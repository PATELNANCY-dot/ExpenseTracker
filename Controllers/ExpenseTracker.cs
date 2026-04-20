using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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

        //  REGISTER USER
        [HttpPost("register")]
        public IActionResult Register(UserModel model)
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("Default")))
            {
                SqlCommand cmd = new SqlCommand("sp_RegisterUser", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Name", model.Name);
                cmd.Parameters.AddWithValue("@Email", model.Email);
                cmd.Parameters.AddWithValue("@Password", model.Password);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }

            return Ok(new
            {
                message = "User Registered Successfully"
            });
        }

        //  LOGIN USER
        [HttpPost("login")]
        public IActionResult Login(UserModel model)
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("Default")))
            {
                SqlCommand cmd = new SqlCommand("sp_LoginUser", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Email", model.Email);
                cmd.Parameters.AddWithValue("@Password", model.Password);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }

            if (dt.Rows.Count > 0)
            {
                var user = dt.AsEnumerable().Select(row => new
                {
                    Id = row["Id"],
                    Name = row["Name"],
                    Email = row["Email"]
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

        //  ADD EXPENSE
        [HttpPost("add-expense")]
        public IActionResult AddExpense(ExpenseModel model)
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("Default")))
            {
                SqlCommand cmd = new SqlCommand("sp_AddExpense", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Title", model.Title);
                cmd.Parameters.AddWithValue("@Amount", model.Amount);
                cmd.Parameters.AddWithValue("@Category", model.Category);
                cmd.Parameters.AddWithValue("@ExpenseDate", model.ExpenseDate);
                cmd.Parameters.AddWithValue("@UserId", model.UserId);
                cmd.Parameters.AddWithValue("@Notes" , model.Notes ?? string.Empty);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }

            return Ok(new
            {
                message = "Added Successfully"
            });
        }

        // GET EXPENSES BY USER
        [HttpGet("get-expenses/{userId}")]
        public IActionResult GetExpenses(int userId)
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("Default")))
            {
                SqlCommand cmd = new SqlCommand("sp_GetExpensesByUser", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@UserId", userId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }

            //Convert DataTable → List
            var expenses = new List<object>();

            foreach (DataRow row in dt.Rows)
            {
                expenses.Add(new
                {
                    Id = row["Id"],
                    Title = row["Title"],
                    Amount = row["Amount"],
                    Category = row["Category"],
                    ExpenseDate = row["ExpenseDate"],
                    UserId = row["UserId"],
                    Notes = row["Notes"],
                });
            }

            return Ok(expenses);
        }

        // DELETE EXPENSE
        [HttpDelete("delete-expense/{id}")]
        public IActionResult DeleteExpense(int id)
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("Default")))
            {
                SqlCommand cmd = new SqlCommand("sp_DeleteExpense", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", id);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            return Ok(new
            {
                message = "Expense Deleted Successfully"
            });
            
        }


        [HttpPut("update-expense/{id}")]
        public IActionResult UpdateExpense(int id, ExpenseModel model)
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("Default")))
            {
                SqlCommand cmd = new SqlCommand("sp_UpdateExpense", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Title", model.Title);
                cmd.Parameters.AddWithValue("@Amount", model.Amount);
                cmd.Parameters.AddWithValue("@Category", model.Category);
                cmd.Parameters.AddWithValue("@ExpenseDate", model.ExpenseDate);
                cmd.Parameters.AddWithValue("@Notes", model.Notes ?? string.Empty);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }

            return Ok(new
            {
                message = "Expense Updated Successfully"
            });
        }

        [HttpPost("add-income")]
        public IActionResult AddIncome(IncomeModel model)
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("Default")))
            {
                SqlCommand cmd = new SqlCommand("sp_AddIncome", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Title", model.Title);
                cmd.Parameters.AddWithValue("@Amount", model.Amount);
                cmd.Parameters.AddWithValue("@IncomeDate", model.IncomeDate);
                cmd.Parameters.AddWithValue("@UserId", model.UserId);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }

            return Ok(new { message = "Income Added Successfully" });
        }

        [HttpGet("get-income/{userId}")]
        public IActionResult GetIncome(int userId)
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("Default")))
            {
                SqlCommand cmd = new SqlCommand("sp_GetIncomeByUser", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@UserId", userId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }

            var income = new List<object>();

            foreach (DataRow row in dt.Rows)
            {
                income.Add(new
                {
                    Id = row["Id"],
                    Title = row["Title"],
                    Amount = row["Amount"],
                    IncomeDate = row["IncomeDate"],
                    UserId = row["UserId"]
                });
            }

            return Ok(income);
        }

        [HttpGet("dashboard/{userId}")]
        public IActionResult GetDashboard(int userId)
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("Default")))
            {
                SqlCommand cmd = new SqlCommand("sp_GetDashboardSummary", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@UserId", userId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }

            var result = dt.AsEnumerable().Select(row => new
            {
                TotalIncome = row["TotalIncome"],
                TotalExpense = row["TotalExpense"],
                Balance = row["Balance"]
            }).FirstOrDefault();

            return Ok(result);
        }

        [HttpPut("update-income/{id}")]
        public IActionResult UpdateIncome(int id, [FromBody] IncomeModel model)
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("Default")))
            {
                SqlCommand cmd = new SqlCommand("sp_UpdateIncome", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Title", model.Title);
                cmd.Parameters.AddWithValue("@Amount", model.Amount);
                cmd.Parameters.AddWithValue("@IncomeDate", model.IncomeDate);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return Ok();
        }

        [HttpDelete("delete-income/{id}")]
        public IActionResult DeleteIncome(int id)
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("Default")))
            {
                SqlCommand cmd = new SqlCommand("sp_DeleteIncome", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", id);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }

            return Ok(new { message = "Income Deleted" });
        }


        [HttpGet("get-user-profile/{userId}")]
        public IActionResult GetUserProfile(int userId)
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("Default")))
            {
                SqlCommand cmd = new SqlCommand("GetUserProfile", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@UserId", userId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }

            var user = dt.AsEnumerable().Select(row => new
            {
                Id = row["Id"],
                Name = row["Name"],
                Email = row["Email"],
                CreatedAt = row["CreatedAt"]
            }).FirstOrDefault();

            return Ok(user);
        }

        [HttpPut("update-profile")]
        public IActionResult UpdateProfile(UserModel model)
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("Default")))
            {
                SqlCommand cmd = new SqlCommand("UpdateUserProfile", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@UserId", model.Id);
                cmd.Parameters.AddWithValue("@Name", model.Name);
                cmd.Parameters.AddWithValue("@Email", model.Email);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }

            return Ok(new
            {
                message = "Profile Updated Successfully"
            });
        }

        [HttpPut("change-password")]
        public IActionResult ChangePassword(ChangePasswordDto model)
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("Default")))
            {
                SqlCommand cmd = new SqlCommand("ChangePassword", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@UserId", model.Id);
                cmd.Parameters.AddWithValue("@CurrentPassword", model.CurrentPassword);
                cmd.Parameters.AddWithValue("@NewPassword", model.NewPassword);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }

            return Ok(new
            {
                message = "Password Changed Successfully"
            });
        }


        [HttpDelete("clear-user-data/{userId}")]
        public IActionResult ClearUserData(int userId)
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("Default")))
            {
                SqlCommand cmd = new SqlCommand("ClearUserData", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@UserId", userId);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }

            return Ok(new
            {
                message = "All Expense and Income Data Cleared"
            });
        }

        [HttpDelete("delete-user/{userId}")]
        public IActionResult DeleteUser(int userId)
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("Default")))
            {
                SqlCommand cmd = new SqlCommand("DeleteUserAccount", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@UserId", userId);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }

            return Ok(new
            {
                message = "User Account Deleted Successfully"
            });
        }

        [HttpPost("save-theme")]
        public IActionResult SaveTheme(ThemeModel model)
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("Default")))
            {
                SqlCommand cmd = new SqlCommand("sp_SaveUserTheme", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@UserId", model.UserId);
                cmd.Parameters.AddWithValue("@DarkMode", model.DarkMode);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }

            return Ok(new
            {
                message = "Theme Saved Successfully"
            });
        }

        [HttpGet("get-theme/{userId}")]
        public IActionResult GetTheme(int userId)
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("Default")))
            {
                SqlCommand cmd = new SqlCommand("sp_GetUserTheme", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@UserId", userId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }

            var result = dt.AsEnumerable().Select(row => new
            {
                DarkMode = row["DarkMode"]
            }).FirstOrDefault();

            return Ok(result);
        }



    }
}