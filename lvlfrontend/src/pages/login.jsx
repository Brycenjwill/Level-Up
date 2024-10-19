import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useGlobalState } from './GlobalState';  // Assuming you have a GlobalState setup

const Login = () => {
  const navigate = useNavigate();
  const { dispatch } = useGlobalState();  // Get the dispatch from global state

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
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(requestBody)
      });

      const result = await response.json();
      if (response.ok) {
        const { userid, token } = result;  // Extract userid and token from the response

        // Fetch user tasks
        const taskResponse = await fetch('https://lvlupcs.azurewebsites.net/api/Database/GetUserTasks', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`  // Assuming your API requires the token as a Bearer token
          },
          body: JSON.stringify({ userid, sessionToken: token })
        });

        const taskResult = await taskResponse.json();
        if (taskResponse.ok) {
          // Store user tasks in global state
          dispatch({
            type: 'SET_USER_TASKS',
            payload: taskResult,
          });

          // Navigate to home page after successful login and task fetch
          navigate('/');
        } else {
          console.error('Error fetching tasks:', taskResult);
        }
      } else {
        console.error('Login error:', result);
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
