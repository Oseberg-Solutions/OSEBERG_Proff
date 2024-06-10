- DOCUMENTATION:
  https://learn.microsoft.com/en-us/power-apps/developer/component-framework/import-custom-controls

- New Computers must set this to the enviorment (PATH) variable (To to run msbuild)
  C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin

#

npm start watch

// Devops setup:

Install:
Using Pac version 1.23.3

pac auth create --environment https://suran.crm4.dynamics.com/

1. cd VirtualControl
2. msbuild /t:build /restore
3. pac pcf push --publisher-prefix os

Domain: https://suran.crm4.dynamics.com/
