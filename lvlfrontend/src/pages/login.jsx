


import React, { useState } from 'react';

const Login = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const handleSubmit = async (event) => {
    event.preventDefault();

    const requestBody = {
      Email: email,
      Password: password
    };

    try {
      const response = await fetch('https://lvlupcs.azurewebsites.net/api/Database/ValidateUserLogin', {
        method: 'POST', // Specifies the request method
        headers: {
          'Content-Type': 'application/json' // Ensure the request is JSON
        },
        body: JSON.stringify(requestBody) // Stringify the body of the request
      });

      const result = await response.json(); // Parse the JSON response
      if (response.ok) {
        console.log('Success:', result);
      } else {
        console.error('Error:', result);
      }
    } catch (error) {
      console.error('Network error:', error);
    }
  };

  return (
    <div className="login-container">
      <form onSubmit={handleSubmit}>
        <h2>Login</h2>
        <div>
          <label htmlFor="username">Username:</label>
          <input
            type="text"
            id="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />
        </div>
        <div>
          <label htmlFor="password">Password:</label>
          <input
            type="password"
            id="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </div>
        <button type="submit">Login</button>
      </form>
    </div>
  );
};

export default Login;