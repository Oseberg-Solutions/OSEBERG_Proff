Løsningen er under utvikling og nyeste versjon vill alltid ligge på Suran.crm4 miljøet da det er det aktive endepunktet for React-PCF komponentet.

For PCF Komponentet, bruker vi React med Typscript og Fluent UI for style.

Vi bruker REST API tjenesten til Proff for å query data, men Proff tilatter ikke direkte kommunikasjon mellom nettleser og deres webtjenester, så vi opprettet en .NET Azure Function som en Proxy server som fungerer da som et mellomledd for data flyten.

Alle spørringer og responser går gjennom denne Azure Function applikasjonen. Se gjerne på flowchart visualiseringen under for en mer detaljert og visuell beskrivelse av løsningensflyten.

Flowchart av løsningen:

![Flow](https://github.com/Oseberg-Solutions/PCF-Component/assets/111337560/bfb038c7-562d-49ad-a8b7-8fbfa396c842)
