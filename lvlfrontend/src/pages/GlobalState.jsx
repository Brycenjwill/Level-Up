import React, { createContext, useReducer, useContext } from 'react';

// Create the global context
const GlobalStateContext = createContext();

// Define a reducer to handle state updates
const globalReducer = (state, action) => {
  switch (action.type) {
    case 'SET_USER':
      return { ...state, user: action.payload };
    case 'SET_TASKS':
      return { ...state, tasks: action.payload };
    default:
      return state;
  }
};

// Create the GlobalStateProvider component
export const GlobalStateProvider = ({ children }) => {
  const initialState = { user: null, tasks: [] };
  const [state, dispatch] = useReducer(globalReducer, initialState);

  return (
    <GlobalStateContext.Provider value={{ state, dispatch }}>
      {children}
    </GlobalStateContext.Provider>
  );
};

// Custom hook to access the global state
export const useGlobalState = () => {
  return useContext(GlobalStateContext);
};
