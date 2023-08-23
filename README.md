Løsningen er under utvikling og nyeste versjon vill alltid ligge på Suran.crm4 miljøet da det er det aktive endepunktet for React-PCF komponentet.

For PCF Komponentet, bruker vi React med Typscript og Fluent UI for style.

Vi bruker REST API tjenesten til Proff for å query data, men Proff tilatter ikke direkte kommunikasjon mellom nettleser og deres webtjenester, så vi opprettet en .NET Azure Function som en Proxy server som fungerer da som et mellomledd for data flyten.

Alle spørringer og responser går gjennom denne Azure Function applikasjonen. Se gjerne på flowchart visualiseringen under for en mer detaljert og visuell beskrivelse av løsningensflyten.

Flowchart av løsningen:

![image](https://github.com/Oseberg-Solutions/PCF-Component/assets/111337560/b30fa934-a4ee-4e0a-8bb2-72edd121d39a)
