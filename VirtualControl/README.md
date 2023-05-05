- DOCUMENTATION:
  https://learn.microsoft.com/en-us/power-apps/developer/component-framework/import-custom-controls

- New Computers must set this to the enviorment (PATH) variable (To to run msbuild)
  C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin

// Devops setup:

pac auth create --url https://suran.crm4.dynamics.com/

1. msbuild /t:build /restore
2. pac pcf push --publisher-prefix os
   https://suran.crm4.dynamics.com/
