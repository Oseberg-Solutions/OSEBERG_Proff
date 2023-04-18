import { IInputs, IOutputs } from "./generated/ManifestTypes";
import SearchComponent from './Components/Search';
import { CompanyData } from './interfaces/CompanyData'
import * as React from "react";

export class VirtualControl implements ComponentFramework.ReactControl<IInputs, IOutputs> {
    
    private name: string | undefined;
    private organisationNumber: string | undefined;
    private email: string | undefined;
    private homePage: string | undefined;
    private mobilePhone: string | undefined;
    private telephoneNumber: string | undefined;
    private addressLine: string | undefined;
    private boxAddressLine: string | undefined;
    private postPlace: string | undefined;
    private zipCode: string | undefined;

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
        this.name = item.name;
        this.organisationNumber = item.organisationNumber;
        this.email = item.email || undefined;
        this.homePage = item.homePage || undefined;
        this.mobilePhone = item.mobilePhone || undefined;
        this.telephoneNumber = item.telephoneNumber || undefined;
        this.addressLine = item.addressLine || undefined;
        this.boxAddressLine = item.boxAddressLine || undefined;
        this.postPlace = item.postPlace || undefined;
        this.zipCode = item.zipCode || undefined;
      
        this.notifyOutputChanged();
    };

    public handleSaveClick = (): void => {
        this.saveClicked = true;
    }

    public getOutputs(): IOutputs {
        return {
            name: this.name,
            email: this.email,
            websiteurl: this.homePage,
            mobilePhone: this.mobilePhone,
            telephone1: this.telephoneNumber,
            address1_line1: this.addressLine,
            boxAddressLine: this.boxAddressLine,
            address1_city: this.postPlace,
            address1_postalcode: this.zipCode,
            cr41c_orgnr: this.organisationNumber
        };
    }

    public destroy(): void {
        // Add code to cleanup control if necessary
    }
}