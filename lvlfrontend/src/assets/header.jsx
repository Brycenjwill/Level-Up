import Button from 'react-bootstrap/Button';
import ProgressBar from 'react-bootstrap/ProgressBar';
import 'bootstrap/dist/css/bootstrap.min.css';

const Header = () => {
        const CustomProgressComponent = () => {
                const progress = 70; 
        }
    return (   
        <div>
                <Button id='LoginButt' href="./login">Login</Button>
                <div id='progressbox'>
                    <ProgressBar id='total_progress' now={50}/>    
                </div>
                
        </div>
    )
  };

  
  export default Header;