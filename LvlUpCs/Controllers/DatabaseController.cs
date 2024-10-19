using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace LvlUpCs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseController : Controller
    {

        private static string connectionString = "Server=levelup.cdkokcmcwbfz.us-east-2.rds.amazonaws.com;Database=levelup;User=admin;Password=ihack2024!;";

        [HttpPost("CheckEmailExists")]
        public IActionResult CheckEmailExists(string email)
        {
            bool emailExists = false;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT userid FROM users WHERE email = @email";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("email", email);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                emailExists = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(new { error = ex.Message });
                }
            }

            return Ok(emailExists);
        }

        [HttpPost("MarkTaskAsCompleted")]
        public IActionResult MarkTaskAsCompleted(int userid, int taskid)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string sql = "INSERT INTO user_completed_tasks (userid, taskid) VALUES (@userid, @taskid);";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("userid", userid);
                        cmd.Parameters.AddWithValue("taskid", taskid);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Ok(true);  // Task marked as completed
                        }
                        else
                        {
                            return BadRequest("Couldn't add completed task to the database.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(new { error = ex.Message });
                }
            }
        }
    }
}