import { useState } from 'react';
import Carousel from 'react-bootstrap/Carousel';
import 'bootstrap/dist/css/bootstrap.min.css';


function ControlledCarousel() {
  const [index, setIndex] = useState(0);

  const handleSelect = (selectedIndex) => {
    setIndex(selectedIndex);
  };

  return (
    <Carousel activeIndex={index} onSelect={handleSelect} interval={null} pause={null}>
      <Carousel.Item>
      <div className="d-block w-100">
          <h3>Physo</h3>
      </div>
      </Carousel.Item>
      <Carousel.Item>


      </Carousel.Item>
      <Carousel.Item>


      </Carousel.Item>
    </Carousel>
  );
}

export default ControlledCarousel;
