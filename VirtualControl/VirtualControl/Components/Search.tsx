import React, { useState, useEffect } from "react";
import { SearchBox, ISearchBoxStyles } from "@fluentui/react/lib/SearchBox";
import { testData } from "../testData";
import { CompanyData } from "../interfaces/CompanyData";
import "../css/searchcomponent.css";
import {
  AZURE_FUNCTION_API_KEY,
  AZURE_FUNCTION_BASE_URL,
} from "../config/config";

const searchBoxStyles: Partial<ISearchBoxStyles> = {
  root: {
    border: "none",
  },
};

interface SearchComponentProps {
  onCardClick: (item: CompanyData) => void;
}

function isAllDigits(str: string) {
  const regex = /^\s*\d+(\s*\d+)*\s*$/;
  return regex.test(str);
}

function removeWhitespaces(str: string) {
  return str.replace(/\s+/g, "");
}

const SearchComponent: React.FC<SearchComponentProps> = ({ onCardClick }) => {
  const MIN_ORGANISATIONNUMBER_LENGTH = 9;
  const [resultsVisible, setResultsVisible] = useState<Boolean>(true);
  const [searchValue, setSearchValue] = useState<string>("");
  const [country, setCountryValue] = useState<string>("NO");
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
    const queryLowerCase = query.toLowerCase();
    const filteredData = testData.filter((item) => {
      const nameMatches = item.name.toLowerCase().includes(queryLowerCase);
      const orgNumberMatches = item.organisationNumber.includes(query);

      return nameMatches || orgNumberMatches;
    });

    setData(filteredData);

    /*
    try {
      const response = await fetch(
        `${AZURE_FUNCTION_BASE_URL}?code=${AZURE_FUNCTION_API_KEY}&query=${query}&country=${query}`
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
    */
  };

  const handleCardClick = (item: CompanyData) => {
    if (item.name && item.organisationNumber) {
      onCardClick(item);
      setResultsVisible(false);
    }
  };

  return (
    <div>
      <SearchBox
        id="searchBox"
        placeholder="..."
        disableAnimation
        autoComplete="off"
        showIcon
        value={searchValue}
        onChange={(_, newValue) => {
          setSearchValue(newValue || "");
          setResultsVisible(true);
        }}
      />
      {searchValue && resultsVisible && (
        <div className="search-results">
          {data.map((item) => (
            <div
              key={item.name + item.organisationNumber}
              className="search-result-card"
              onClick={() => handleCardClick(item)}
            >
              <div className="search-result-title">{item.name}</div>
              <div className="search-result-id">{item.addressLine}</div>
              <div className="search-result-subtext">
                Org nr: {item.organisationNumber}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default SearchComponent;
