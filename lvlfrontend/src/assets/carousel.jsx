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
      <div id="tree">
      <div id="str1">text</div>
        <div id="str2">text</div>
        <div id="str3">text</div>
      </div>
      </Carousel.Item>
      <Carousel.Item>
      <div id="tree">
      <div id="int1">text2</div>
        <div id="int2">text2</div>
        <div id="int3">text2</div>
      </div>

      </Carousel.Item>
      <Carousel.Item>
      <div id="tree">

        <div id="soc1">text3</div>
        <div id="soc2">text3</div>
        <div id="soc3">text3</div>
      </div>

      </Carousel.Item>
    </Carousel>
  );
}

export default ControlledCarousel;
