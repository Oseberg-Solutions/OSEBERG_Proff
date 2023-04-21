import React from "react";

interface FlagOptionProps {
  flag: React.ReactNode;
  text: string;
}

const FlagOption: React.FC<FlagOptionProps> = ({ flag, text }) => (
  <div style={{ display: "flex", alignItems: "center" }}>
    {flag}
    <span style={{ marginLeft: 8 }}>{text}</span>
  </div>
);

export default FlagOption;
