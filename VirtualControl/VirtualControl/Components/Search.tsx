import React, { useState, useEffect } from "react";
import { SearchBox, ISearchBoxStyles } from "@fluentui/react/lib/SearchBox";
import "../css/searchcomponent.css";

const searchBoxStyles: Partial<ISearchBoxStyles> = {
  root: {
    border: "none",
  },
};

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
  const [debouncedSearchValue, setDebouncedSearchValue] = useState<string>("");
  const [data, setData] = useState<CompanyData[]>([]);

  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedSearchValue(searchValue);
    }, 1000);

    return () => {
      clearTimeout(handler);
    };
  }, [searchValue]);

  useEffect(() => {
    if (debouncedSearchValue && debouncedSearchValue.length >= 3) {
      let searchValueToUse = debouncedSearchValue;

      // if value is only digits, don't continue before we have a valid orgnr
      if (isAllDigits(debouncedSearchValue)) {
        setData([]);
        if (debouncedSearchValue.length < MIN_ORGANISATIONNUMBER_LENGTH) return;
        searchValueToUse = removeWhitespaces(debouncedSearchValue);
      }

      handleSearch(searchValueToUse);
    } else {
      setData([]); // Clear the data if the debounced search value is less than 3 characters
    }
  }, [debouncedSearchValue]);

  const handleSearch = async (query: string) => {
    console.log("Hadnle Search...");
    return;
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
        styles={searchBoxStyles}
        placeholder="Search..."
        onChange={(_, newValue) => {
          setSearchValue(newValue || "");
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
