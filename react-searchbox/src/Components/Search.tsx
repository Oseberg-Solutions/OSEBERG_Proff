import React, { useState } from "react";
import { SearchBox } from "@fluentui/react/lib/SearchBox";
import "../css/searchcomponent.css";

interface CompanyData {
  name: string;
  organisationNumber: string;
  email: string | null;
  homePage: string | null;
  mobilePhone: string | null;
  telephoneNumber: string | null;
  addressLine: string | null;
  boxAddressLine: string | null;
  postPlace: string | null;
  zipCode: string | null;
}

function isAllDigits(str: string) {
  const regex = /^\s*\d+(\s*\d+)*\s*$/;
  return regex.test(str);
}

function removeWhitespaces(str: string) {
  return str.replace(/\s+/g, "");
}

const SearchComponent: React.FC = () => {
  const MIN_ORGANISATIONNUMBER_LENGTH = 9;
  const [searchValue, setSearchValue] = useState<string>("");
  const [data, setData] = useState<CompanyData[]>([]);

  const handleSearch = async (query: string) => {
    try {
      const response = await fetch(
        `https://company-lookup.azurewebsites.net//api/ProffCompanySearch?code=zZSTDpXMqXTVRPIb7XL1lqb-ssnihlDbujQMBpr3RA42AzFuE86izg==&query=${query}`
      );
      if (response.ok) {
        const result = await response.json();
        setData(result);
      } else {
        console.error("Failed to fetch data from Azure Function");
      }
    } catch (error) {
      console.error("Error fetching data:", error);
    }
  };

  const handleCardClick = (id: string) => {
    alert(`Card with ID: ${id} clicked`);
  };

  return (
    <div>
      <SearchBox
        placeholder="Search..."
        onChange={(_, newValue) => {
          setSearchValue(newValue || "");
          if (newValue && newValue.length >= 3) {
            // if value is only digits, dont continue before we have a valid orgnr
            if (isAllDigits(newValue)) {
              setData([]);
              if (newValue.length < MIN_ORGANISATIONNUMBER_LENGTH) return;
              newValue = removeWhitespaces(newValue);
            }

            handleSearch(newValue);
          } else {
            setData([]); // Clear the data if the search value is less than 2 characters
          }
        }}
      />
      {searchValue && (
        <div className="search-results">
          {data.map((item) => (
            <div
              key={item.organisationNumber}
              className="search-result-card"
              onClick={() => handleCardClick(item.organisationNumber)}
            >
              <div className="search-result-title">{item.name}</div>
              <div className="search-result-id">
                Organisation Number: {item.organisationNumber}
              </div>
              <div className="search-result-subtext">
                {item.organisationNumber}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default SearchComponent;
