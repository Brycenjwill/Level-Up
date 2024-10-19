using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;


namespace LvlUpCs.Controllers
{
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

		// Method to validate user login
		[HttpPost("ValidateUserLogin")]
		public bool ValidateUserLogin(string username, string password)
		{
			bool isValidUser = false;

			using (MySqlConnection conn = new MySqlConnection(connectionString))
			{
				try
				{
					conn.Open();

					// SQL query to find the user by username
					string query = "SELECT password FROM users WHERE username = @username";

					using (MySqlCommand cmd = new MySqlCommand(query, conn))
					{
						// Add the username parameter
						cmd.Parameters.AddWithValue("@username", username);

						// Execute the query
						using (MySqlDataReader reader = cmd.ExecuteReader())
						{
							if (reader.Read())
							{
								// Get the stored hashed password from the database
								string storedHashedPassword = reader["password"].ToString();

								// Use bcrypt to verify the password
								if (BCrypt.Net.BCrypt.Verify(password, storedHashedPassword))
								{
									isValidUser = true; // Login success
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine("An error occurred: " + ex.Message);
				}
				finally
				{
					conn.Close();
				}
			}

			Console.WriteLine($"Login attempt succeeded: {isValidUser}");
			return isValidUser;
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
		public void InsertUser(string username, string password, string email)
		{
			string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

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
						cmd.Parameters.AddWithValue("@username", username);
						cmd.Parameters.AddWithValue("@password", hashedPassword);  // Ideally, hash the password before storing
						cmd.Parameters.AddWithValue("@email", email);

						// Execute the query
						int rowsAffected = cmd.ExecuteNonQuery();

						// Check if the insert was successful
						if (rowsAffected > 0)
						{
							Console.WriteLine("User created successfully!");
						}
						else
						{
							Console.WriteLine("Failed to create user.");
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine("An error occurred: " + ex.Message);
				}
				finally
				{
					conn.Close();
				}
			}
		}
	}
}
