# Model for unit conversion of BasePhysicalQuantity

This nuget package hosts the C# model for managing sets of unit choices for physical quantities.
Such a set forms a BaseUnitSystem.

More info on:

https://github.com/Open-Source-Drilling-Community/UnitConversion

# Default UnitChoices for the Base Unit Systems
There are four default base unit systems: `SI`, `Metric`, `Imperial` and `US`.
It is mandatory to define the unit choice for every physical quantities for
each of these based unit systems. This is done by adding a line for each
 of the `BaseUnitSystem` in the file `BaseUnitSystem.cs`. This line looks like that:
```csharp
Choices.Add(AccelerationQuantity.Instance.ID.ToString(), AccelerationQuantity.Instance.GetUnitChoice(AccelerationQuantity.UnitChoicesEnum.MetrePerSecondSquared).ID.ToString());
```

# Contributors

**Eric Cayeux**, *NORCE Energy Modelling and Automation*

**Gilles Pelfrene**, *NORCE Energy Modelling and Automation*
