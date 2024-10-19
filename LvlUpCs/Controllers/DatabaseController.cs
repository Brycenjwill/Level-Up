using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace LvlUpCs.Controllers
{
    public class DatabaseController : Controller
    {
        

        private static string connectionString = "Server=levelup.cdkokcmcwbfz.us-east-2.rds.amazonaws.com;Database=levelup;User=admin;Password=ihack2024!;";

        [HttpPost("CheckEmailExists")]
        public static bool CheckEmailExists(string email)
        {
            bool emailExists = false;
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Example: Execute a SQL query (select or insert)
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
                    Console.WriteLine("An error occurred." + ex.Message);
  
                }

            }
            return emailExists;
        }
    }
}
