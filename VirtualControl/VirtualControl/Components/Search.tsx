import React, { useState, useEffect } from "react";
import { SearchBox } from "@fluentui/react/lib/SearchBox";
import { Dropdown, IDropdownOption } from "@fluentui/react";
import { Dialog, DialogType, DialogFooter } from "@fluentui/react/lib/Dialog";
import { DefaultButton, PrimaryButton } from "@fluentui/react/lib/Button";
import {
  isAllDigits,
  removeWhitespaces,
  thousandSeparator,
} from "../utils/utils";
import { countryOptions, searchBoxStyles } from "../constants/constants";
import FlagRenderer from "./FlagRenderer";
import FlagOption from "./FlagOption";
import { CompanyData } from "../interfaces/CompanyData";
import "../css/searchcomponent.css";
import ProffIcon from "../Components/ProffIcon";
import {
  AZURE_FUNCTION_API_KEY,
  AZURE_FUNCTION_BASE_URL,
} from "../config/config";

interface SearchComponentProps {
  onCardClick: (item: CompanyData) => void;
  isAccountNameFilled: boolean;
  isOrgNumberFilled: boolean;
}

const allowCountryChoices = true;

/*----------------------------------------------------------------------------*/
/* METHODS */
/*----------------------------------------------------------------------------*/

const SearchComponent: React.FC<SearchComponentProps> = ({
  onCardClick,
  isAccountNameFilled,
  isOrgNumberFilled,
}) => {
  const MIN_ORGANISATIONNUMBER_LENGTH = 9;
  const [selectedItem, setSelectedItem] = useState<CompanyData | null>(null);
  const [cachedItem, setCachedItem] = useState<CompanyData | null>(null);
  const [resultsVisible, setResultsVisible] = useState<Boolean>(true);
  const [searchValue, setSearchValue] = useState<string>("");
  const [country, setCountry] = useState<string>("NO");
  const [data, setData] = useState<CompanyData[]>([]);
  const [debouncedSearchValue, setDebouncedSearchValue] = useState<string>("");
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

  const renderPlaceholder = () => {
    const selectedCountry = countryOptions.find(
      (option) => option.key === country
    );
    return (
      <div style={{ display: "flex", alignItems: "center" }}>
        <FlagRenderer countryKey={country} />
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
      setData([]);
    }
  }, [debouncedSearchValue]);

  useEffect(() => {
    const performConfirm = async () => {
      if (
        selectedItem &&
        selectedItem.Name &&
        selectedItem.OrganisationNumber
      ) {
        if (selectedItem.OrganisationNumber) {
          await handleSearch(
            "",
            selectedItem.OrganisationNumber,
            selectedItem.ProffCompanyId
          );
        }
        onCardClick(selectedItem);
        setResultsVisible(false);
      }
    };

    if (selectedItem) {
      performConfirm();
    }
  }, [selectedItem]);

  const handleSearch = async (
    query: string,
    organisationNumber: string = "",
    proffCompanyId: string = ""
  ) => {
    const domain: string = window.location.hostname;
    try {
      const response = await fetch(
        `${AZURE_FUNCTION_BASE_URL}?code=${AZURE_FUNCTION_API_KEY}&query=${query}&country=${country}&domain=${domain}&organisationNumber=${organisationNumber}`
      );

      if (response.ok) {
        const result = await response.json();

        // ProffCompanyId is the unique id in the "data" set wich we want to get in the clickedObject.
        if (proffCompanyId === "") {
          setData(result);
          return;
        }

        const clickedObject = data.find(
          (item) => item.ProffCompanyId === proffCompanyId
        );

        if (clickedObject) {
          clickedObject.Name == "" ? (clickedObject.Name = result.Name) : "";
          clickedObject.NumberOfEmployees == null
            ? (clickedObject.NumberOfEmployees = result.NumberOfEmployees)
            : "";
          clickedObject.Nace == null ? (clickedObject.Nace = result.Nace) : "";
          clickedObject.Profit == null
            ? (clickedObject.Profit = result.Profit)
            : "";
          clickedObject.Revenue == null
            ? (clickedObject.Revenue = result.Revenue)
            : "";
          clickedObject.Country == null
            ? (clickedObject.Country = result.Country)
            : "";
          clickedObject.TotalrentabilitetLoennsomhet == null
            ? (clickedObject.TotalrentabilitetLoennsomhet =
                result.TotalrentabilitetLoennsomhet)
            : "";
          clickedObject.Egenkapitalandel == null
            ? (clickedObject.Egenkapitalandel = result.Egenkapitalandel)
            : "";
          clickedObject.Likviditetsgrad == null
            ? (clickedObject.Likviditetsgrad = result.Likviditetsgrad)
            : "";

          setData([clickedObject]);
        }
      } else {
        console.error("Failed to fetch data from Azure Function");
      }
    } catch (error) {
      console.error("Error fetching data:", error);
    }
  };

  const handleCardClick = (item: CompanyData) => {
    if (isAccountNameFilled && isOrgNumberFilled) {
      setShowConfirmationDialog(true);
      setCachedItem(item);
    } else {
      setSelectedItem(item);
    }
    setSearchValue("");
    setDebouncedSearchValue("");
  };

  const handleConfirm = async () => {
    if (cachedItem) setSelectedItem(cachedItem);
    setShowConfirmationDialog(false);

    if (selectedItem) {
      if (selectedItem.OrganisationNumber) {
        await handleSearch(
          "",
          selectedItem.OrganisationNumber,
          selectedItem.ProffCompanyId
        );
      }
      onCardClick(selectedItem);
      setResultsVisible(false);
      setCachedItem(null);
    }
  };

  const handleCancel = () => {
    setShowConfirmationDialog(false);
  };

  /*----------------------------------------------------------------------------*/
  /* JSX */
  /*----------------------------------------------------------------------------*/

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
                    flag={<FlagRenderer countryKey={option.key as string} />}
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
            {data.map((item, index) => (
              <div
                key={item.Name + "_" + item.ProffCompanyId + "_" + index}
                className="search-result-card"
                onClick={() => handleCardClick(item)}
              >
                <div className="search-result-title">{item.Name}</div>
                <div className="search-result-id">{item.AddressLine}</div>
                <div className="search-result-subtext">
                  Org nr: {item.OrganisationNumber}
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
          title: "Advarsel",
          closeButtonAriaLabel: "Close",
        }}
        modalProps={{
          isBlocking: true,
        }}
      >
        <div>
          Dette valget vil overskrive ekisterende data ved lagring.
          <br></br>
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
