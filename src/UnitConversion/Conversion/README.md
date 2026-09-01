# Model for unit conversion of BasePhysicalQuantity

This nuget package hosts the C# model for converting units of a BasePhysicalQuantity.
Classes and methods are scoped within the namespace: ``UnitConversion.Conversion``

More info on:

https://github.com/Open-Source-Drilling-Community/UnitConversion

# Descriptive Definitions of UnitChoice Conversions
For each `UnitChoice`, it is possible to define a conversion factor and a conversion bias.
These are defined in the properties `ConversionFactorFromSIFormula` and `ConversionBiasFromSIFormula` respectively, which 
are string values. The content of these strings is the code to calculate the conversion factor and the conversion bias.
The code is in plain C#. If the code involves some mathematics operator like square root or Pi, these need to be fully
qualified, i.e., `System.Math.Sqrt` or `System.Math.Pi`. Otherwise, it is possible to use already defined `Factors`, like `Factors.Inch`.
These factors are also defined as formulas as a function of other definition. This symbolic definition of the conversion
factor and bias allows to generate a comprehensive description of the conversion factors and bias by calling the method `GetConversionDescription` of `UnitChoice`.

Here is the result obtained on calling `GetConversionDescription` on the Fahrenheit unit choice:
```
[v] = a * [SI] + b
where
[v] is the value in fahrenheit
[SI] is the value in SI, i.e., in kelvin
a = 1.0/FahrenheitSlope, i.e., 1.7999999999999998
b = -FahrenheitBias, i.e., -459.67
and
FahrenheitSlope = 5.0 / 9.0 reference: https://nn.wikipedia.org/wiki/Fahrenheit
FahrenheitBias = 459.67 reference: https://nn.wikipedia.org/wiki/Fahrenheit
```

and here is the result obtained with the Furlong unit choice:
```
[v] = a * [SI]
where
[v] is the value in furlong
[SI] is the value in SI, i.e., in metre
a = 1.0/Furlong, i.e., 0.004970969537898672
and
Furlong = 660.0 * Foot reference: https://www.britannica.com/science/furlong
Foot = 12.0 * Inch
Inch = 0.0254 reference: https://www.nist.gov/pml/owm/si-units-length
```

For efficiency, there is a conversion that is made from the symbolic description to code when calling the program `GenerateEnumerations` (see below).

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
quantity or when a new physical quantity is added or removed. 

# Contributors

**Eric Cayeux**, *NORCE Energy Modelling and Automation*

**Gilles Pelfrene**, *NORCE Energy Modelling and Automation*
