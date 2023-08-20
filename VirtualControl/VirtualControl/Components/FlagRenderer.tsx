import React from "react";
import NorwayFlag from "../Components/Flags/NorwayFlag";
import SwedenFlag from "../Components/Flags/SwedenFlag";
import DenmarkFlag from "../Components/Flags/DenmarkFlag";

const FlagRenderer: React.FC<{ countryKey: string }> = ({ countryKey }) => {
  switch (countryKey) {
    case "SE":
      return <SwedenFlag />;
    case "DK":
      return <DenmarkFlag />;
    default:
      return <NorwayFlag />; // NO
  }
};

export default FlagRenderer;
