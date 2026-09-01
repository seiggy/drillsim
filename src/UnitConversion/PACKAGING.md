# Generating source and building the 3.4.1 packages

The physical-quantity enumerations are generated source files.  Build the solution with local project references while developing; otherwise restore can select older published packages and make new quantities appear to be missing.

From the repository root in PowerShell:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\Pack-All.ps1 -Version 3.4.1
```

The script performs the complete local release sequence: restore with local project references, generate the enumerations in **Debug** (the generated source is configuration-independent), build the release artifacts, run tests, and create the packages in `artifacts\packages`. Use `-GeneratorConfiguration Release` only when specifically needed. It uses this dependency order:

1. `OSDC.UnitConversion.Conversion`
2. `OSDC.UnitConversion.Conversion.DrillingEngineering`
3. `OSDC.UnitConversion.Conversion.UnitSystem`
4. `OSDC.UnitConversion.Conversion.UnitSystem.DrillingEngineering`
5. `OSDC.UnitConversion.DrillingRazorMudComponents`
6. `OSDC.UnitConversion.WebPages`

`ModelShared`, `Service`, and `Model` are built and tested as part of the solution, but are not published packages. `ModelShared` is a local OpenAPI client-model generator whose generated C# source is linked directly into `WebPages`; `Service` disables package-on-build and `Model` is not packable.

## Manual procedure

1. Open PowerShell in `C:\OSDC\UnitConversion`.
   If execution policy prevents direct execution of the helper script, use the command above; `Bypass` applies only to that PowerShell process.
2. Restore with local project references:

   ```powershell
   dotnet restore .\UnitConversion.sln -p:UseLocalUnitConversionProjects=true
   ```

3. Generate the source files.  The property is required here too because the generator references both conversion projects:

   ```powershell
   dotnet run --project .\GenerateEnumerations\GenerateEnumerations.csproj -c Debug --no-restore -p:UseLocalUnitConversionProjects=true
   ```

4. Build and test the local solution:

   ```powershell
   dotnet build .\UnitConversion.sln -c Release --no-restore -p:UseLocalUnitConversionProjects=true -p:GeneratePackageOnBuild=false
   dotnet test .\UnitConversion.sln -c Release --no-build -p:UseLocalUnitConversionProjects=true
   ```

5. Pack the projects in the listed dependency order, always passing both properties. For example:

   ```powershell
dotnet pack .\Conversion\Conversion.csproj -c Release --no-build -p:UseLocalUnitConversionProjects=true -p:PackageVersion=3.4.1 --output .\artifacts\packages
   ```

6. Inspect the resulting `.nupkg` files and publish them to the feed in that same dependency order.

Do not use `--no-restore` immediately after changing between package references and local project references. Run the restore command in step 2 first; that was the cause of the earlier missing-type failures.
