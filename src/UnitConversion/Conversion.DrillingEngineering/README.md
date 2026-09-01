# Model for unit conversion of PhysicalQuantity

This nuget package hosts the C# model for converting units of a PhysicalQuantity.
Classes and methods are scoped within the namespace: ``UnitConversion.Conversion.DrillingEngineering``

More info on:

https://github.com/Open-Source-Drilling-Community/UnitConversion

# Generation of Enumerations
This project has one file that is automaticaly generated `EnumerationQuantities.cs`. 
This file contains partial classes for each of the physical quantities. 
Each partial class definition defines an enumeration for each of the
unit choices that are defined for this physical quantity and a 
lookup table that defines the association between the enumeration
and the `GUID` of the unit choice.

To generate the file `EnumerationQuantities.cs`, one must run the program
`GenerateEnumerations`. The generation of the file is necessary each
time a modification is made in the list of unit choices of a physical
quantity or when a new physical quantity is added or removed. However,
there is a potential bootstrap problem with the definitions of the 
unit choices for the `UnitSystem`. As each `UnitSystem` has 
a list of unit choices and these choices are usually defined using
the unit choice enumeration, if the enumeration does not yet exist
or its name is changed, then the project does not compile and it is not
possible to generate the enumerations using the program `GenerateEnumerations`.
So it is advised to define the unit choice for the particular physical
quantity using its name or its `GUID`, then generate the enumerations by running
the program `GenerateEnumeration`and finally replace the name of the unit 
choice with its equivalent enumeration.
```csharp
Choices.Add(AccelerationDrillingQuantity.Instance.ID.ToString(), AccelerationDrillingQuantity.Instance.GetUnitChoice("metre per second squared").ID.ToString());
```

```
GenerateEnumerations
```

```csharp
Choices.Add(AccelerationDrillingQuantity.Instance.ID.ToString(), AccelerationDrillingQuantity.Instance.GetUnitChoice(AccelerationDrillingQuantity.UnitChoicesEnum.MetrePerSecondSquared).ID.ToString());
```

# Contributors

**Eric Cayeux**, *NORCE Energy Modelling and Automation*

**Gilles Pelfrene**, *NORCE Energy Modelling and Automation*
