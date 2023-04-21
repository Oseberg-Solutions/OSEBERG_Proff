import { IInputs, IOutputs } from "./generated/ManifestTypes";
import SearchComponent from './Components/Search';
import { CompanyData } from './interfaces/CompanyData'
import * as React from "react";

export class VirtualControl implements ComponentFramework.ReactControl<IInputs, IOutputs> {

    private _name: string | undefined;
    private _organisationNumber: string | undefined;
    private _email: string | undefined;
    private _homePage: string | undefined;
    private _mobilePhone: string | undefined;
    private _telephoneNumber: string | undefined;
    private _addressLine: string | undefined;
    private _boxAddressLine: string | undefined;
    private _postPlace: string | undefined;
    private _zipCode: string | undefined;

    private theComponent: ComponentFramework.ReactControl<IInputs, IOutputs>;

    private notifyOutputChanged: () => void;

    private saveClicked: boolean = false;

    constructor() { }

    public init(
        context: ComponentFramework.Context<IInputs>,
        notifyOutputChanged: () => void,
        state: ComponentFramework.Dictionary
    ): void {
        this.notifyOutputChanged = notifyOutputChanged;
    }

    public updateView(context: ComponentFramework.Context<IInputs>): React.ReactElement {
        const props = {
            onCardClick: this.handleCardClick,
        };
        return React.createElement(
            SearchComponent, props
        );
    }

    public handleCardClick = (item: CompanyData): void => {

        console.debug("index.ts: handleCardClick -> item.name", item.name);

        this._name = item.name;
        this._organisationNumber = item.organisationNumber;
        this._email = item.email || undefined;
        this._homePage = item.homePage || undefined;
        this._mobilePhone = item.mobilePhone || undefined;
        this._telephoneNumber = item.telephoneNumber || undefined;
        this._addressLine = item.addressLine || undefined;
        this._boxAddressLine = item.boxAddressLine || undefined;
        this._postPlace = item.postPlace || undefined;
        this._zipCode = item.zipCode || undefined;

        this.notifyOutputChanged();
    };

    public handleSaveClick = (): void => {
        this.saveClicked = true;
    }

    public getOutputs(): IOutputs {

        const outputs: IOutputs = {
            companyName: this._name,
            email: this._email,
            websiteurl: this._homePage,
            mobilePhone: this._mobilePhone,
            telephone1: this._telephoneNumber,
            address1_line1: this._addressLine,
            boxAddressLine: this._boxAddressLine,
            address1_city: this._postPlace,
            address1_postalcode: this._zipCode,
            cr41c_orgnr: this._organisationNumber
        };

        console.debug("getOutputs called:", outputs);

        return outputs;
    }

    public destroy(): void {
        // Add code to cleanup control if necessary
    }
}