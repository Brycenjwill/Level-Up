﻿using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace LvlUpCs.Controllers
{
    using Microsoft.IdentityModel.Tokens;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;

    public class TokenService
    {
        public string GenerateToken(string username)
        {
            // Define token issuer, audience, expiration, and signing credentials
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("levelupbutitssixteenormorecharacters")); // Secret key
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var token = new JwtSecurityToken(
                issuer: "lvlup",       
                audience: "user",   
                claims: claims,
                expires: DateTime.Now.AddMinutes(30), 
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }


    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseController : Controller
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

        public class LogoutRequest
        {
            public int UserId { get; set; }
            public string Token { get; set; }
        }


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

        [HttpPost("ValidateUserLogin")]
        public IActionResult ValidateUserLogin([FromBody] LoginRequest loginRequest)
        {
            bool isValidUser = false;
            int userId = 0; // To store the user's ID
            string token = string.Empty; // To store the generated token

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // SQL query to find the user by email and get userid and password
                    string query = "SELECT userid, password FROM users WHERE email = @Email";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        // Add the email parameter
                        cmd.Parameters.AddWithValue("@Email", loginRequest.Email);

                        // Execute the query
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Get the stored hashed password and userid from the database
                                string storedHashedPassword = reader["password"].ToString();
                                userId = Convert.ToInt32(reader["userid"]);

                                // Use bcrypt to verify the password
                                if (BCrypt.Net.BCrypt.Verify(loginRequest.Password, storedHashedPassword))
                                {
                                    isValidUser = true; // Login success
                                }
                            }
                        }
                    }

                    if (isValidUser)
                    {
                        // Generate JWT token
                        var tokenService = new TokenService();
                        token = tokenService.GenerateToken(loginRequest.Email);

                        // Store the generated token in the sessionToken field of the user
                        string updateQuery = "UPDATE users SET sessionToken = @Token WHERE userid = @UserId";

                        using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, conn))
                        {
                            updateCmd.Parameters.AddWithValue("@Token", token);
                            updateCmd.Parameters.AddWithValue("@UserId", userId);
                            updateCmd.ExecuteNonQuery(); // Update the user's sessionToken
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle error by returning a 500 response with the error message
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
                // Return both the token and the userId
                return Ok(new { userid = userId, token = token });
            }
            else
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }
        }


        [HttpPost("Logout")]
        public IActionResult Logout([FromBody] LogoutRequest logoutRequest)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // SQL query to check if the provided token matches the stored token for the user
                    string query = "SELECT sessionToken FROM users WHERE userid = @UserId";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", logoutRequest.UserId);

                        string storedToken = null;

                        // Execute the query and get the session token for the user
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                storedToken = reader["sessionToken"] != DBNull.Value ? reader["sessionToken"].ToString() : null;
                            }
                            else
                            {
                                return NotFound(new { message = "User not found" });
                            }
                        }

                        if (storedToken == null || storedToken != logoutRequest.Token)
                        {
                            return Unauthorized(new { message = "Invalid token or user is not logged in" });
                        }

                        string updateQuery = "UPDATE users SET sessionToken = NULL WHERE userid = @UserId";

                        using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, conn))
                        {
                            updateCmd.Parameters.AddWithValue("@UserId", logoutRequest.UserId);
                            int rowsAffected = updateCmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                return Ok(new { message = "User logged out successfully" });
                            }
                            else
                            {
                                return StatusCode(500, new { message = "Failed to log out user" });
                            }
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