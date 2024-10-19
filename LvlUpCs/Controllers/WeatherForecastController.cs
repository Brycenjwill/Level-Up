using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;


namespace LvlUpCs.Controllers
{
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class CreateUserRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }


    [ApiController]
	[Route("[controller]")]
	public class WeatherForecastController : ControllerBase
	{
		private static readonly string[] Summaries = new[]
		{
			"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
		};

		private static string connectionString = "Server=levelup.cdkokcmcwbfz.us-east-2.rds.amazonaws.com;Database=levelup;User=admin;Password=ihack2024!;";

		private readonly ILogger<WeatherForecastController> _logger;

		public WeatherForecastController(ILogger<WeatherForecastController> logger)
		{
			_logger = logger;
		}

		[HttpGet(Name = "GetWeatherForecast")]
		public IEnumerable<WeatherForecast> Get()
		{
			return Enumerable.Range(1, 5).Select(index => new WeatherForecast
			{
				Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
				TemperatureC = Random.Shared.Next(-20, 55),
				Summary = Summaries[Random.Shared.Next(Summaries.Length)]
			})
			.ToArray();
		}

        [HttpPost("ValidateUserLogin")]
        public IActionResult ValidateUserLogin([FromBody] LoginRequest loginRequest)
        {
            bool isValidUser = false;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // SQL query to find the user by username
                    string query = "SELECT password FROM users WHERE email = @email";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        // Add the username parameter
                        cmd.Parameters.AddWithValue("@email", loginRequest.Email);

                        // Execute the query
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Get the stored hashed password from the database
                                string storedHashedPassword = reader["password"].ToString();

                                // Use bcrypt to verify the password
                                if (BCrypt.Net.BCrypt.Verify(loginRequest.Password, storedHashedPassword))
                                {
                                    isValidUser = true; // Login success
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle error by returning 500 response with error message
                    return StatusCode(500, new { message = "An error occurred: " + ex.Message });
                }
                finally
                {
                    conn.Close();
                }
            }

            // Return appropriate HTTP response based on login success
            if (isValidUser)
            {
                return Ok(new { message = "Login successful" });
            }
            else
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }
        }


        // DEBUG: REMOVE ME
        // Connect to database, query everything in user table.
        [HttpGet("ConnectToDatabase")]
		public string ConnectToDatabase()
		{
			string message = string.Empty;
			using (MySqlConnection conn = new MySqlConnection(connectionString))
			{
				try
				{
					// Open the connection
					conn.Open();
					Console.WriteLine("Database connected successfully!");

					// Example: Execute a SQL query (select or insert)
					string sql = "SELECT * FROM users";
					using (MySqlCommand cmd = new MySqlCommand(sql, conn))
					{
						using (MySqlDataReader reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								Console.WriteLine($"UserID: {reader["userid"]}, Username: {reader["username"]}");
								message += $"UserID: {reader["userid"]}, Username: {reader["username"]}\n";
							}
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine("An error occurred: " + ex.Message);
					message = "An error occurred: " + ex.Message;
				}
				finally
				{
					// Close the connection
					conn.Close();
				}
			}

			return message;
		}

		///////////////// Connect to database, insert new user into user table.
		[HttpPost("InsertNewUser")]
		public IActionResult InsertUser([FromBody] CreateUserRequest newUser)
		{

            var controller = new DatabaseController();
            var result = controller.CheckEmailExists(newUser.Email) as OkObjectResult;

            if (result != null && (bool)result.Value)
            {
                // Email exists, so return or handle it as needed
                return Conflict(new { message = "Email already exists." });
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newUser.Password);

			string connectionString = "Server=levelup.cdkokcmcwbfz.us-east-2.rds.amazonaws.com;Database=levelup;User=admin;Password=ihack2024!;";
			using (MySqlConnection conn = new MySqlConnection(connectionString))
			{
				try
				{
					conn.Open();

					// Prepare the SQL INSERT query using parameterized query
					string query = "INSERT INTO users (username, password, email) VALUES (@username, @password, @email)";

					using (MySqlCommand cmd = new MySqlCommand(query, conn))
					{
						// Add parameters to prevent SQL injection
						cmd.Parameters.AddWithValue("@username", newUser.Username);
						cmd.Parameters.AddWithValue("@password", hashedPassword);  // Ideally, hash the password before storing
						cmd.Parameters.AddWithValue("@email", newUser.Email);

						// Execute the query
						int rowsAffected = cmd.ExecuteNonQuery();

						// Check if the insert was successful
						if (rowsAffected > 0)
						{
                            return Ok(new { message = "User created successfully!" });
                        }
						else
						{
                            return BadRequest(new { message = "Failed to create user." });
                        }
					}
				}
				catch (Exception ex)
				{
                    return StatusCode(500, new { message = "An error occurred: " + ex.Message });
                }
				finally
				{
					conn.Close();
				}
			}
		}
	}
}
