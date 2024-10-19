
import {Routes, Route, BrowserRouter} from "react-router-dom";
import Physical from "./pages/physical";
import Intel from "./pages/intel";
import Login from "./pages/login";
import Social from "./pages/social";
import Header from "./assets/header";
import Body from "./assets/body";
import '../src/styles/App.css';


function App() {
  
  

  return (

    <div>
    <BrowserRouter>
      <div className="header"><Header /></div>
          <Routes>
            <Route path="/login" element={<Login />}/>;
            <Route path="/intel" element={<Intel />}/>;
            <Route path="/social" element={<Social />}/>;
            <Route path="/physical" element={<Physical />}/>;
          </Routes>
    </BrowserRouter>
    <Body />
    </div>
    

    
  )
}

export default App;
