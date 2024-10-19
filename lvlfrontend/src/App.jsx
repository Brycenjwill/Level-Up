
import {Routes, Route, BrowserRouter} from "react-router-dom";
import Home from "./pages/home";
import Login from "./pages/login";
import Header from "./assets/header";
import ControlledCarousel from "./assets/carousel";
import '../src/styles/App.css';


function App() {

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
