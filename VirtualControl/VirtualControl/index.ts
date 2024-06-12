import { IInputs, IOutputs } from "./generated/ManifestTypes";
import SearchComponent from "./Components/Search";
import { CompanyData } from "./interfaces/CompanyData";
import * as React from "react";

export class VirtualControl
  implements ComponentFramework.ReactControl<IInputs, IOutputs>
{
  private _name: string | undefined;
  private _organisationNumber: string | undefined;
  private _email: string | undefined;
  private _homePage: string | undefined;
  private _mobilePhone: string | undefined;
  private _telephoneNumber: string | undefined;
  private _addressLine: string | undefined;
  private _boxAddressLine: string | undefined;
  private _country: string | undefined;
  private _visitorAddressLine: string | undefined;
  private _vistiorPostPlace: string | undefined;
  private _visitorZipCode: string | undefined;
  private _visitorCountry: string | undefined;
  private _postPlace: string | undefined;
  private _zipCode: string | undefined;
  private _nace: string | undefined;
  private _numberOfEmployees: string | undefined;
  private _profit: string | undefined;
  private _revenue: string | undefined;
  private _sic: string | undefined;
  private _likviditetsgrad: string | undefined;
  private _totalrentabilitetLoennsomhet: string | undefined;
  private _egenkapitalandel: string | undefined;

  private context: ComponentFramework.Context<IInputs>;
  private theComponent: ComponentFramework.ReactControl<IInputs, IOutputs>;
  private saveClicked: boolean = false;

  private notifyOutputChanged: () => void;

  constructor() {}

  public init(
    context: ComponentFramework.Context<IInputs>,
    notifyOutputChanged: () => void,
    state: ComponentFramework.Dictionary
  ): void {
    this.context = context;
    this.notifyOutputChanged = notifyOutputChanged;
  }

  public updateView(
    context: ComponentFramework.Context<IInputs>
  ): React.ReactElement {
    const props = {
      onCardClick: this.handleCardClick,
      isAccountNameFilled: !!context.parameters.companyName.raw,
      isOrgNumberFilled: !!context.parameters.cr41c_orgnr.raw,
    };
    return React.createElement(SearchComponent, props);
  }

  public handleCardClick = (item: CompanyData): void => {
    this._name = item.Name;
    this._organisationNumber = item.OrganisationNumber || undefined;
    this._email = item.Email || undefined;
    this._homePage = item.HomePage || undefined;
    this._mobilePhone = item.MobilePhone || undefined;
    this._telephoneNumber = item.TelephoneNumber || undefined;
    this._country = item.Country || undefined;
    this._addressLine = item.AddressLine || undefined;
    this._boxAddressLine = item.BoxAddressLine || undefined;
    this._postPlace = item.PostPlace || undefined;
    this._zipCode = item.ZipCode || undefined;
    this._visitorAddressLine = item.VisitorAddressLine || undefined;
    this._vistiorPostPlace = item.VisitorPostPlace || undefined;
    this._visitorZipCode = item.VisitorZipCode || undefined;
    this._visitorCountry = item.Country || undefined;
    this._nace = item.Nace || undefined;
    this._numberOfEmployees = item.NumberOfEmployees || undefined;
    this._profit = item.Profit || undefined;
    this._revenue = item.Revenue || undefined;
    this._sic = item.Sic || undefined;
    this._likviditetsgrad = item.Likviditetsgrad || undefined;
    this._totalrentabilitetLoennsomhet =
      item.TotalrentabilitetLoennsomhet || undefined;
    this._egenkapitalandel = item.Egenkapitalandel || undefined;

    this.notifyOutputChanged();
  };

  public handleSaveClick = (): void => {
    this.saveClicked = true;
  };

  public getOutputs(): IOutputs {
    const outputs: IOutputs = {
      name: this._name,
      companyName: this._name,
      email: this._email,
      websiteurl: this._homePage,
      mobilePhone: this._mobilePhone,
      telephone1: this._telephoneNumber,
      address1_country: this._country,
      address1_line1: this._addressLine,
      boxAddressLine: this._boxAddressLine,
      address1_city: this._postPlace,
      address1_postalcode: this._zipCode,
      address2_line1: this._visitorAddressLine,
      address2_postalcode: this._visitorZipCode,
      address2_city: this._vistiorPostPlace,
      address2_country: this._visitorCountry,
      cr41c_orgnr: this._organisationNumber,
      os_nace: this._nace,
      numberofemployees: this._numberOfEmployees,
      os_driftsinntekter: this._revenue,
      revenue: this._revenue,
      os_profit: this._profit,
      os_likviditetsgrad: this._likviditetsgrad,
      os_totalrentabilitetlnnsomhet: this._totalrentabilitetLoennsomhet,
      os_egenkapitalandel: this._egenkapitalandel,
      os_sic: this._sic,
    };

    return outputs;
  }

  public destroy(): void {
    // Add code to cleanup control if necessary
  }
}

// Få den stabil.
