import { postLogin } from './api';
import {Routes, Route, BrowserRouter} from "react-router-dom";
import Home from "./pages/home";
import Login from "./pages/login";
import '../src/styles/App.css';
import '../src/styles/Header.css';
import '../src/styles/Login.css';
import ReactDOM from 'react-dom/client'; // Import createRoot from 'react-dom/client'

function App() {
// Get the root element where the app will be mounted

  return (

    

      <BrowserRouter>    
          <Routes>
            <Route path="/" element={<Home />} />;
            <Route path="/login" element={<Login />}/>;
          </Routes>
    </BrowserRouter>


 

    
  )  

    

}

export default App;
