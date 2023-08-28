Løsningen er under utvikling og nyeste versjon vill alltid ligge på Suran.crm4 miljøet da det er det aktive endepunktet for React-PCF komponentet.

For PCF Komponentet, bruker vi React med Typscript og Fluent UI for style.

Vi bruker REST API tjenesten til Proff for å query data, men Proff tilatter ikke direkte kommunikasjon mellom nettleser og deres webtjenester, så vi opprettet en .NET Azure Function som en Proxy server som fungerer da som et mellomledd for data flyten.

Alle spørringer og responser går gjennom denne Azure Function applikasjonen. Se gjerne på flowchart visualiseringen under for en mer detaljert og visuell beskrivelse av løsningensflyten.

Flowchart av løsningen:

![Flow](https://github.com/Oseberg-Solutions/PCF-Component/assets/111337560/bfb038c7-562d-49ad-a8b7-8fbfa396c842)

Som vi ser på flowcharten, så gjør vi en ny spørring når vi først klikker på et kort for å hente ytterlig informasjon for det selskapet. Dette er for å spare API spørringer da, det er ulike endepunkter for å hente info som numberOfEmployees og NACE. Vi henter disse dataene da når brukeren først bestemmer seg for en kunde og klikker på en.

Felter vi henter fra proff:

- Navn
- Epost
- Webside
- Mobil
- Telefon
- Adresse
- Postboks addresse
- By
- Post nummer
- Orgnr
- Nace
- Antall ansatte

Vi må huske å mappe disse feltene riktig når vi setter opp PCF Komponentet:
