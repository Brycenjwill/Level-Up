import { useState } from 'react';
import Carousel from 'react-bootstrap/Carousel';
import 'bootstrap/dist/css/bootstrap.min.css';

function ControlledCarousel() {
  const [index, setIndex] = useState(0);

  // State to track which elements have been clicked
  const [clicked, setClicked] = useState({
    str1: false,
    str2: false,
    str3: false,
    int1: false,
    int2: false,
    int3: false,
    soc1: false,
    soc2: false,
    soc3: false,
  });

  // Function to handle click and toggle text color
  const handleClick = (id) => {
    setClicked((prevState) => ({
      ...prevState,
      [id]: !prevState[id],  // Toggle the clicked state
    }));
  };

  // Function to return conditional styling for text color
  const getTextStyle = (id) => ({
    color: clicked[id] ? 'green' : 'white',  // Set text color to green if clicked, otherwise white
    cursor: 'pointer',
    backgroundColor: 'black',  // Keep the div's background color black
    height: '100%',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
  });

  const handleSelect = (selectedIndex) => {
    setIndex(selectedIndex);
  };

  return (
    <Carousel activeIndex={index} onSelect={handleSelect} interval={null} pause={null}>
      <Carousel.Item>
        <div id="tree">
          <div id="str1" style={getTextStyle('str1')} onClick={() => handleClick('str1')}>text</div>
          <div id="str2" style={getTextStyle('str2')} onClick={() => handleClick('str2')}>text</div>
          <div id="str3" style={getTextStyle('str3')} onClick={() => handleClick('str3')}>text</div>
        </div>
      </Carousel.Item>
      <Carousel.Item>
        <div id="tree">
          <div id="int1" style={getTextStyle('int1')} onClick={() => handleClick('int1')}>text2</div>
          <div id="int2" style={getTextStyle('int2')} onClick={() => handleClick('int2')}>text2</div>
          <div id="int3" style={getTextStyle('int3')} onClick={() => handleClick('int3')}>text2</div>
        </div>
      </Carousel.Item>
      <Carousel.Item>
        <div id="tree">
          <div id="soc1" style={getTextStyle('soc1')} onClick={() => handleClick('soc1')}>text3</div>
          <div id="soc2" style={getTextStyle('soc2')} onClick={() => handleClick('soc2')}>text3</div>
          <div id="soc3" style={getTextStyle('soc3')} onClick={() => handleClick('soc3')}>text3</div>
        </div>
      </Carousel.Item>
    </Carousel>
  );
}

export default ControlledCarousel;
