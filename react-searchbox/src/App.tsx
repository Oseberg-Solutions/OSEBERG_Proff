import React from "react";
import "./css/App.css";
import "../node_modules/@fluentui/react/dist/css/fabric.min.css";
import SearchComponent from "./Components/Search";
import { initializeIcons } from "@fluentui/font-icons-mdl2";

initializeIcons();

const App: React.FC = () => {
  return (
    <div className="App">
      <header className="App-header">
        <SearchComponent />
      </header>
    </div>
  );
};

export default App;
