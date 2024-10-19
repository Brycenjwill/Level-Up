import Button from 'react-bootstrap/Button';
import ProgressBar from 'react-bootstrap/ProgressBar';
import 'bootstrap/dist/css/bootstrap.min.css';
import norm from './normal.png';
import ground from './grassy_dirt_bk.jpg'

const Header = () => {
        const CustomProgressComponent = () => {
                const progress = 70; 
        }
    return (   
        <div>
                <Button id='LoginButt' href="./login">Login</Button>

                <div>
                    <ProgressBar id='total_progress' now={50}/>
                    <image src={ground} alt="norm" className='image'></image>    
                </div>
                 
        </div>
    )
  };

  
  export default Header;