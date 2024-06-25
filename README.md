Introduksjon
Denne dokumentasjonen beskriver en løsning utviklet for Dynamics 365 CRM ved hjelp av en PCF komponent som benytter React og Fluent UI i frontend, samt en Azure Function som håndterer integrasjon med Proff API, caching og data lagring. Løsningen validerer brukerens søk og viser resultatene i Dynamics 365 CRM.

Arkitektur Oversikt
Løsningen består av to hovedkomponenter:

Frontend: En PCF komponent laget med React (TypeSript) og Fluent UI.
Noe ressurser som kan brukes:

Scott Durow: https://www.youtube.com/watch?v=MYVmXdANC08&t
Microsoft: https://learn.microsoft.com/en-us/power-apps/developer/component-framework/import-custom-controls

Bruker Power Platform CLI (pac) for å deploye komponenter.
Backend: En Azure Function som integrerer med Proff API, håndterer caching og lagrer data.

  +-------------------+           +-----------------+
  | Dynamics 365 CRM  |           |    Proff API    |
  +-------------------+           +--------+--------+
            |                              |
            |                              |
            v                              v
  +-------------------+           +-----------------+
  |     Frontend      |           |  Azure Function |
  | PCF + React + UI  | <-------- |   (Backend)     |
  +-------------------+           +-----------------+

Utviklingen er knyttet opp mot suran.crm4 miljøet og utvikles direkte inn i løsningen "PowerAppsTools_os".

Vi benytter oss av REST tjenesten til Proff til å kommunisere.
Proff tillatter ikke direkte kommunikasjon mellom deres servere og nettlesere, så vi har da opprettet
en Azure function som håndtere dataflyten mellom crm og proff. 

![FlowChart](https://github.com/Oseberg-Solutions/OSEBERG_Proff/assets/111337560/c1a69d16-526c-4eba-8253-249d43bead2a)

Felter vi ber om fra proff (ikke sikkert alle felt har verdi):
Basis subscription (active subscription):

- Nace
- NumberOfEmployees
- VisitorAddressLine
- VisitorBoxAddressLine
- VisitorPostPlace
- VisitorZipCode
- numberOfEmployees
- visitorAddressLine
- visitorBoxAddressLine
- visitorPostPlace
- visitorZipCode
- homePage
- profit
- likviditetsgrad
- totalrentabilitetLoennsomhet
- egenkapitalandel
- revenue
- homePage

Med Premium subscription. Alt i basis +:

- economy
- leadOwnership
- organisationNumber
- rating
- ratingScore

Vi kan også mappe disse feltene til ønsket felt på et skjema. Gå til Søk feltet som har virtualcontrol komponenten og velg felter derfra.

Gå til Løsningen:
![image](https://github.com/Oseberg-Solutions/PCF-Component/assets/111337560/0ce76636-b20e-4ead-b04a-2703cdccea5e)

Velg da Tabeller -> Så forretningsforbindelse/emne -> Skjema så klikk å Hovedskjema:

![image](https://github.com/Oseberg-Solutions/PCF-Component/assets/111337560/256ae2da-8134-43cf-8e46-919fe6e731ba)

Når vi er inne på skjema, dobbelklikk på Søke feltet, velg så Komponenter for å utvide for å se flere valg i Egeneskaper. Klikk så på VirtualControl:

![image](https://github.com/Oseberg-Solutions/PCF-Component/assets/111337560/2a557c3d-9837-4df0-aa5a-6dc55a1f885f)

Når vi har klikket på VirtualControl, så får vi opp et vindu som viser alle feltene vi henter, og disse kan vi nå mappe til felter i Forretningsforbindelse felter:

![image](https://github.com/Oseberg-Solutions/PCF-Component/assets/111337560/6f9e6582-4ef6-436e-a2f5-bcfb89dd9a77)

I koden så har jeg allerede spesifisert de "Standard" feltene, og disse vil da automatisk bli feltet, men når det gjelder felter som ikke er "Standard" så må disse mappes.

Eks i CRM så har vi ingen standard NACE felt, så her må vi da opprette et tekst felt på forretningsforbindelse som heter noe med osb_nace.

Vi mapper dette da opp ved å finne nace feltet vi lagde:

![image](https://github.com/Oseberg-Solutions/PCF-Component/assets/111337560/f06990e0-4d5f-4d39-89b4-94803324ee15)

Azure:

- Resource group: dev-crm-rg

I denne ressursgruppen så er det 2 ting vi ønsker å ha kontroll på.

- Function App: OSB-proff-company-lookup
- Storage Account: proffwebstorage

Function appen er en serveless tjeneste som kjører alt av logikken i backend.

I proffwebstorage har vi alt av data i følgende tabeller:

- Proff Configuration (Holder info om hvilke kunder av oss som har aktiv lisens. Hver kunde må ha en rad her hvis det skal gå for dem.)
- Proff Request Activity (Holder oversikt over hvem som gjør hvor mange kall)
- Proff Premium Cache (Vi cacher premium data slik at vi sparer på API kall dersom kunder henter data fra samme organisasjon)
- Proff Premium Request Activity 

I ProffConfiguration tabellen så må lage en ny entitet (rad) med følgende eksempel data:
![AddEntity](https://github.com/Oseberg-Solutions/OSEBERG_Proff/assets/111337560/5db68a84-231a-48f3-b688-a0f13b1a21a3)

Partitionkey: suran.crm4.dynamics.com
RowKey: suran.crm4.dynamics.com
active_Subscription: true
domain: suran.crm4.dynamics.com
premium_subscription: false
![Oversikt](https://github.com/Oseberg-Solutions/OSEBERG_Proff/assets/111337560/32a2d9ae-bf1e-4ff2-9353-95d585920b31)


I ProffRequestActivity tabellen holder vi oversikt over hvem som gjør hvor mange kall.
Her skal ikke vi gjøre noe manuelt, denne tabellen blir styrt av logikken.

I denne tabellen har vi:

- PartitionKey
- RowKey
- Timestamp
- amount_of_request
- domain
- last_request

Merk at det blir laget en ny rad for hver mnd. Dette er for å kunne holde koll på hvem som gjør flest kall pr mnd.

I RowKey så kan vi se at verdien er: domain_år+måned.
Hvis vi tar røde kors for et eksempel, lager jeg et enkelt filter som filtrerer på deres domenet så får vi dette:
![Query](https://github.com/Oseberg-Solutions/OSEBERG_Proff/assets/111337560/b53b022a-d285-4c06-bdec-837206222014)

Resultatet gir en oversikt over hvor mange kall RKF har gjort disse månedene.

