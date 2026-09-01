# OSDC.Drilling.EarthMagneticField.WebPages

Reusable Blazor pages for stateless Earth magnetic-field evaluation:

- `/EarthMagneticFieldCalculation`: unit-aware WMM2025/IGRF14 evaluation with explicit UTC and north-east-down output.
- `/EarthMagneticFieldModel`: installed-model validity and provenance.
- `/StatisticsEarthMagneticField`: process-replica REST counters.

Consumers register an `HttpClient` named `EarthMagneticFieldHostURL`, MudBlazor, and the OSDC unit-system services.

Package ID: `OSDC.Drilling.EarthMagneticField.WebPages`

Author: Eric Cayeux

Company: NORCE Research
