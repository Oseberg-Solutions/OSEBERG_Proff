import React, { useState, useEffect } from "react";
import { SearchBox, ISearchBoxStyles } from "@fluentui/react/lib/SearchBox";
import { Dropdown, IDropdownOption } from "@fluentui/react";
import { Dialog, DialogType, DialogFooter } from "@fluentui/react/lib/Dialog";
import { DefaultButton, PrimaryButton } from "@fluentui/react/lib/Button";
import FlagOption from "./FlagOption";
import { CompanyData } from "../interfaces/CompanyData";
import "../css/searchcomponent.css";
import NorwayFlag from "../Components/Flags/NorwayFlag";
import SwedenFlag from "../Components/Flags/SwedenFlag";
import DenmarkFlag from "../Components/Flags/DenmarkFlag";
import ProffIcon from "../Components/ProffIcon";
import {
  AZURE_FUNCTION_API_KEY,
  AZURE_FUNCTION_BASE_URL,
} from "../config/config";

interface SearchComponentProps {
  onCardClick: (item: CompanyData) => void;
}

const allowCountryChoices = true;

const countryOptions: IDropdownOption[] = [
  { key: "NO", text: "NO" },
  { key: "SE", text: "SE" },
  { key: "DK", text: "DK" },
];

const searchBoxStyles: Partial<ISearchBoxStyles> = {
  root: {
    border: "none",
    backgroundColor: "#f5f5f5",
  },
};

/*----------------------------------------------------------------------------*/
/* METHODS */
/*----------------------------------------------------------------------------*/
function isAllDigits(str: string) {
  const regex = /^\s*\d+(\s*\d+)*\s*$/;
  return regex.test(str);
}

function removeWhitespaces(str: string) {
  return str.replace(/\s+/g, "");
}

const SearchComponent: React.FC<SearchComponentProps> = ({ onCardClick }) => {
  const MIN_ORGANISATIONNUMBER_LENGTH = 9;
  const [selectedItem, setSelectedItem] = useState<CompanyData | null>(null);
  const [resultsVisible, setResultsVisible] = useState<Boolean>(true);
  const [searchValue, setSearchValue] = useState<string>("");
  const [country, setCountry] = useState<string>("NO");
  const [debouncedSearchValue, setDebouncedSearchValue] = useState<string>("");
  const [data, setData] = useState<CompanyData[]>([]);
  const [showConfirmationDialog, setShowConfirmationDialog] =
    useState<boolean>(false);
  const filteredCountryOptions = countryOptions.filter(
    (option) => option.key !== country
  );

  const handleCountryChange = (
    _: React.FormEvent<HTMLDivElement>,
    option?: IDropdownOption
  ) => {
    if (option) {
      setCountry(option.key as string);
    }
  };

  const renderFlag = (key: string) => {
    switch (key) {
      case "SE":
        return <SwedenFlag />;
      case "DK":
        return <DenmarkFlag />;
      default:
        return <NorwayFlag />; // NO
    }
  };

  const renderPlaceholder = () => {
    const selectedCountry = countryOptions.find(
      (option) => option.key === country
    );
    return (
      <div style={{ display: "flex", alignItems: "center" }}>
        {renderFlag(country)}
        <span style={{ marginLeft: 8 }}>{selectedCountry?.text}</span>
      </div>
    );
  };

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
    const domain: string = window.location.hostname;
    try {
      const response = await fetch(
        `${AZURE_FUNCTION_BASE_URL}?code=${AZURE_FUNCTION_API_KEY}&query=${query}&country=${country}&domain=${domain}`
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

  const renderSuffix = () => {
    return (
      <div style={{ height: "16px", width: "16px" }}>
        <ProffIcon />
      </div>
    );
  };

  const handleCardClick = (item: CompanyData) => {
    if (item.name && item.organisationNumber) {
      setSelectedItem(item);
      setShowConfirmationDialog(true);
    }
  };

  const handleConfirm = () => {
    setShowConfirmationDialog(false);
    if (selectedItem && selectedItem.name && selectedItem.organisationNumber) {
      onCardClick(selectedItem);
      setResultsVisible(false);
    }
  };

  const handleCancel = () => {
    setShowConfirmationDialog(false);
  };

  return (
    <div className="main-container">
      <div className="searchbox-container">
        <SearchBox
          styles={searchBoxStyles}
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
        {allowCountryChoices && (
          <div className="inner-container">
            <Dropdown
              options={filteredCountryOptions}
              selectedKey={country}
              onChange={handleCountryChange}
              onRenderPlaceholder={renderPlaceholder}
              onRenderOption={(props) => {
                const option = props as IDropdownOption;
                return (
                  <FlagOption
                    flag={renderFlag(option.key as string)}
                    text={option.text}
                  />
                );
              }}
            />
          </div>
        )}
      </div>
      <div className="search-results-wrapper">
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
      <Dialog
        hidden={!showConfirmationDialog}
        onDismiss={handleCancel}
        dialogContentProps={{
          type: DialogType.normal,
          title: "Confirmation",
          closeButtonAriaLabel: "Close",
        }}
        modalProps={{
          isBlocking: true,
        }}
      >
        <div>
          PS! Dette valget vil overskrive ekisterende data ved lagring.
          <br></br>
          Ønsker du å fortsette?
        </div>
        <DialogFooter>
          <PrimaryButton onClick={handleConfirm} text="Ok" />
          <DefaultButton onClick={handleCancel} text="Cancel" />
        </DialogFooter>
      </Dialog>
    </div>
  );
};

export default SearchComponent;
