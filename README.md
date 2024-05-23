Løsningen er under utvikling og nyeste versjon vill alltid ligge på Suran.crm4 miljøet da det er det aktive endepunktet for React-PCF komponentet.

PCF Komponentet er et Virtual Control, utvikelt med React Typscript og Fluent UI.

Vi bruker REST API tjenesten til Proff for å query data, men Proff tilatter ikke direkte kommunikasjon mellom nettleser og deres webtjenester, så vi opprettet en .NET Azure Function som en Proxy server som fungerer da som et mellomledd for data flyten.

Alle spørringer og responser går gjennom denne Azure Function applikasjonen. Se gjerne på flowchart visualiseringen under for en mer detaljert og visuell beskrivelse av løsningensflyten.

Flowchart av løsningen:

![Flow](https://github.com/Oseberg-Solutions/PCF-Component/assets/111337560/bfb038c7-562d-49ad-a8b7-8fbfa396c842)

Som vi ser på flowcharten, så gjør vi en ny spørring når vi først klikker på et kort for å hente ytterlig informasjon for det selskapet. Dette er for å spare API spørringer da, det er ulike endepunkter for å hente info som numberOfEmployees og NACE. Vi henter disse dataene da når brukeren først bestemmer seg for en kunde og klikker på en.

Felter vi ber om fra proff (ikke sikkert alle felt har verdi):

Basic subscription (active subscription):

- Name
- ProffCompanyId
- CompanyTypeName
- NumberOfEmployees
- OrganisationNumber
- Email
- HomePage
- MobilePhone
- TelephoneNumber
- AddressLine
- BoxAddressLine
- PostPlace
- ZipCode
- VisitorAddressLine
- VisitorBoxAddressLine
- VisitorPostPlace
- VisitorZipCode

* Med Premium:

- Profit
- Revenue
- Likviditetsgrad
- TotalrentabilitetLoennsomhet
- Egenkapitalandel

Vi må huske å mappe disse feltene riktig når vi setter opp PCF Komponentet:
Gå til Løsningen:
![image](https://github.com/Oseberg-Solutions/PCF-Component/assets/111337560/0ce76636-b20e-4ead-b04a-2703cdccea5e)

Velg da Tabeller -> Så forretningsforbindelse -> Skjema så klikk å Hovedskjema:

![image](https://github.com/Oseberg-Solutions/PCF-Component/assets/111337560/256ae2da-8134-43cf-8e46-919fe6e731ba)

Når vi er inne på skjema, dobbelklikk på Søke feltet, velg så Komponenter for å utvide for å se flere valg i Egeneskaper. Klikk så på VirtualControl:

![image](https://github.com/Oseberg-Solutions/PCF-Component/assets/111337560/2a557c3d-9837-4df0-aa5a-6dc55a1f885f)

Når vi har klikket på VirtualControl, så får vi opp et vindu som viser alle feltene vi henter, og disse kan vi nå mappe til felter i Forretningsforbindelse felter:

![image](https://github.com/Oseberg-Solutions/PCF-Component/assets/111337560/6f9e6582-4ef6-436e-a2f5-bcfb89dd9a77)

I koden så har jeg allerede spesifisert de "Standard" feltene, og disse vil da automatisk bli feltet, men når det gjelder felter som ikke er "Standard" så må disse mappes.

Eks i CRM så har vi ingen standard NACE felt, så her må vi da opprette et tekst felt på forretningsforbindelse som heter noe med osb_nace.

Vi mapper dette da opp ved å finne nace feltet vi lagde:

![image](https://github.com/Oseberg-Solutions/PCF-Component/assets/111337560/f06990e0-4d5f-4d39-89b4-94803324ee15)

Det blir lagret hvilket domenet som gjør hvor mange kaller mot APIET:
![Alt text](image.png)
