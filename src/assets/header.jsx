
import React, {useState} from "react";
import { useNavigate } from 'react-router-dom';


const Header = () => {
    const history = useNavigate();
    const [currentPage, setCurrentPage] = useState(0);
    const pages = ['/physical', '/social', '/intel'];

    const handleClick = () => {
        const nextPage = (currentPage + 1) % pages.length;
        setCurrentPage(nextPage);
        history(pages[nextPage]);
      };
      const handleBackClick = () => {
        const PrevPage = (currentPage - 1) % pages.length;
        setCurrentPage(PrevPage);
        history(pages[PrevPage]);
        if (PrevPage<=1) {
            setCurrentPage(pages[3])
        }
      };

    return (
      <header>
            <button className="forwordbutt" onClick={handleClick}>Cycle</button>
            <button onClick={handleBackClick}>Cycle Back</button>

            
      </header>
    )
  };

  
  export default Header;