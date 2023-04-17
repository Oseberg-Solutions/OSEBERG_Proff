- DOCUMENTATION:
  https://learn.microsoft.com/en-us/power-apps/developer/component-framework/import-custom-controls

- New Computers must set this to the enviorment variable
  C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin

1. msbuild /t:build /restore
2. pac pcf push --publisher-prefix os
