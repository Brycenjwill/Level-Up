using Microsoft.AspNetCore.Mvc;
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

        public class MarkTaskCompletedRequest 
        {
            public int UserId { get; set; }
            public int TaskId { get; set; }
            public string Token { get; set; }
        }


        private static string connectionString = "Server=levelup.cdkokcmcwbfz.us-east-2.rds.amazonaws.com;Database=levelup;User=admin;Password=ihack2024!;";

        private IActionResult CheckEmailExists(string email)
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
        public IActionResult MarkTaskAsCompleted([FromBody] MarkTaskCompletedRequest request)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // First, check if the provided token matches the one stored in the database for the given userid
                    string checkTokenQuery = "SELECT sessionToken FROM users WHERE userid = @UserId";

                    using (MySqlCommand checkTokenCmd = new MySqlCommand(checkTokenQuery, conn))
                    {
                        checkTokenCmd.Parameters.AddWithValue("@UserId", request.UserId);
                        string storedToken = null;

                        // Execute the query to get the session token from the database
                        using (MySqlDataReader reader = checkTokenCmd.ExecuteReader())
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

                        // Verify the token
                        if (storedToken == null || storedToken != request.Token)
                        {
                            return Unauthorized(new { message = "Invalid token or user is not logged in" });
                        }

                        // If token is valid, proceed to mark the task as completed
                        string insertQuery = "INSERT INTO user_completed_tasks (userid, taskid) VALUES (@UserId, @TaskId)";

                        using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, conn))
                        {
                            insertCmd.Parameters.AddWithValue("@UserId", request.UserId);
                            insertCmd.Parameters.AddWithValue("@TaskId", request.TaskId);

                            int rowsAffected = insertCmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                return Ok(new { message = "Task marked as completed" });
                            }
                            else
                            {
                                return BadRequest(new { message = "Couldn't add completed task to the database." });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { error = ex.Message });
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

        [HttpPost("GetUserTasks")]
        public IActionResult GetUserTasks(int userid, string sessionToken)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // First, check if the provided token matches the one stored in the database for the given userid
                    string checkTokenQuery = "SELECT sessionToken FROM users WHERE userid = @UserId";

                    using (MySqlCommand checkTokenCmd = new MySqlCommand(checkTokenQuery, conn))
                    {
                        checkTokenCmd.Parameters.AddWithValue("@UserId", userid);
                        string storedToken = null;

                        // Execute the query to get the session token from the database
                        using (MySqlDataReader reader = checkTokenCmd.ExecuteReader())
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

                        // Verify the token
                        if (storedToken == null || storedToken != sessionToken)
                        {
                            return Unauthorized(new { message = "Invalid token or user is not logged in" });
                        }
                    }

                    // Now, query all tasks from the database including title and tree_type (lowercase)
                    string tasksQuery = @"
                SELECT t.taskid, t.title, t.info, t.xp, 
                       LOWER(t.tree_type) AS tree_type,  -- Convert tree_type to lowercase
                       CASE WHEN uct.userid IS NOT NULL THEN true ELSE false END AS completed
                FROM task t
                LEFT JOIN user_completed_tasks uct ON t.taskid = uct.taskid AND uct.userid = @UserId";

                    using (MySqlCommand tasksCmd = new MySqlCommand(tasksQuery, conn))
                    {
                        tasksCmd.Parameters.AddWithValue("@UserId", userid);

                        using (MySqlDataReader reader = tasksCmd.ExecuteReader())
                        {
                            var tasks = new List<Dictionary<string, object>>();

                            while (reader.Read())
                            {
                                var task = new Dictionary<string, object>
                        {
                            { "id", reader["taskid"] },
                            { "title", reader["title"] },
                            { "info", reader["info"] },
                            { "xp", reader["xp"] },
                            { "tree_type", reader["tree_type"] },  // Return lowercased tree_type
                            { "completed", Convert.ToBoolean(reader["completed"]) }
                        };

                                tasks.Add(task);
                            }

                            return Ok(tasks);  // Return the tasks as JSON
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