using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Cross-standard terminology that supplements the concise names declared by
    /// individual quantities. Entries are intentionally semantic rather than
    /// merely dimensional and are shared by UI, service, and core lookup code.
    /// </summary>
    internal static class QuantitySynonymCatalog
    {
        private static readonly IReadOnlyDictionary<string, string[]> Synonyms =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["PowerPerArea"] = ["acoustic intensity", "heat flux", "heat transfer per unit area"],
                ["Pressure"] = ["acoustic pressure", "sound pressure", "capillary pressure", "capilliary pressure", "differential orifice pressure", "pressure buildup per cycle", "yield point", "gel strength"],
                ["Time"] = ["acoustic travel time", "duration", "annual time", "geologic time", "time hours", "time minutes", "time months"],
                ["Velocity"] = ["acoustic velocity", "corrosion rate", "linear velocity"],
                ["Energy"] = ["radiant energy", "impact energy"],
                ["Power"] = ["radiant flux", "radiant power", "heat exchange rate", "heat flow rate", "heat power"],
                ["Length"] = ["wavelength", "corrosion allowance", "vessel diameter", "fluid head", "height", "atomic length", "distance", "microscopic length", "optical length", "net pay thickness", "particle size"],
                ["PlaneAngle"] = ["bending azimuth"],
                ["MassDensityGradientPerLength"] = ["density gradient", "density per length"],
                ["MassDensityRateOfChange"] = ["density rate", "density per time"],
                ["MassDensityGradientPerTemperature"] = ["density temperature gradient", "density per temperature"],
                ["FrequencyRateOfChange"] = ["frequency rate", "frequency per time"],
                ["Proportion"] = ["gas ratio", "recovery per unit volume", "mass concentration fraction", "volume concentration fraction", "area ratio", "force ratio", "length ratio", "mass ratio", "molar fraction", "mole ratio", "power ratio", "time ratio", "volume ratio", "volumetric flow rate ratio"],
                ["VolumetricFlowRate"] = ["oil rate", "pumping rate", "gas volume flow rate"],
                ["StrokeFrequency"] = ["pump stroke rate"],
                ["PermeabilityLength"] = ["permeability thickness"],
                ["MassDensity"] = ["mass concentration", "concentration mass per volume", "gas density", "liquid density", "solid density", "mass per volume"],
                ["AmountSubstancePerVolume"] = ["mole concentration", "concentration mole per volume"],
                ["MolarVolume"] = ["concentration volume per mole", "molar specific volume", "volume per mole"],
                ["SpecificVolume"] = ["gas specific volume", "liquid specific volume", "solid specific volume", "volume per mass"],
                ["DataStorage"] = ["data storage words", "data storage words GB", "data storage words KB", "data storage words MB", "data storage words TB", "digital data unit", "digital word unit"],
                ["DataTransferRate"] = ["data transfer bits", "data transfer Gb", "data transfer Kb", "data transfer Mb", "data transfer Tb"],
                ["ElectricCapacitance"] = ["capacitance", "electrical capacitance"],
                ["ElectricDipoleMoment"] = ["dipole moment"],
                ["ElectricConductance"] = ["electrical conductance"],
                ["ElectricConductivity"] = ["electrical conductivity"],
                ["ElectricCurrent"] = ["electrical current"],
                ["ElectricCurrentDensity"] = ["electrical current density"],
                ["ElectricFieldStrength"] = ["electrical field strength", "electrical field gradient"],
                ["ElectricTension"] = ["electrical potential difference", "electrical tension"],
                ["ElectricResistance"] = ["electrical resistance"],
                ["ElectricResistivity"] = ["electrical resistivity", "formation resistivity"],
                ["ElectricResistanceGradientPerLength"] = ["electric resistance per unit length", "electrical resistance gradient"],
                ["MagneticFluxDensityGradientPerLength"] = ["magnetic flux density gradient"],
                ["WorkGradient"] = ["energy length per area"],
                ["Temperature"] = ["thermodynamic temperature"],
                ["MassRateGradientPerLength"] = ["mass flow rate gradient", "mass flow rate per length"],
                ["MassRatePerArea"] = ["mass flow rate per area"],
                ["AmountSubstanceRate"] = ["mole flow rate", "amount of substance per time"],
                ["VolumetricFlowRateGradientPerLength"] = ["volumetric flow rate gradient", "volumetric flow rate per length"],
                ["VolumetricFlowRateOfChange"] = ["volumetric flow rate change", "volume acceleration"],
                ["IsobaricSpecificHeatCapacity"] = ["specific heat capacity"],
                ["Diffusivity"] = ["thermal diffusivity", "diffusion coefficient"],
                ["AmountSubstancePerAreaRateOfChange"] = ["amount substance per area rate"],
                ["MassGradientPerLength"] = ["mass gradient", "mass per unit length"],
                ["ForceGradientPerLength"] = ["force gradient", "force per length"],
                ["LinearThermalExpansionCoefficient"] = ["linear thermal expansion"],
                ["VolumetricThermalExpansionCoefficient"] = ["volumetric thermal expansion"],
                ["ReciprocalElectricTension"] = ["reciprocal electric potential difference", "inverse voltage"],
                ["AreaRateOfChange"] = ["area rate", "area per time"],
                ["AngleGradientPerLength"] = ["plane angle gradient", "plane angle per length"],
                ["VolumeGradientPerLength"] = ["volume gradient", "volume per length"],
                ["VolumePerPressure"] = ["volumen per pressure"],
                ["VolumePerAreaRateOfChange"] = ["volume per area rate"],
                ["VolumePerLengthRateOfChange"] = ["volume per length rate"],
                ["VolumePerPressureRateOfChange"] = ["volume per pressure rate"],
                ["VolumePerVolumeRateOfChange"] = ["volume per volume rate"],
                ["TemperatureGradientPerLength"] = ["geothermal gradient", "temperature gradient", "relative temperature gradient"],
                ["LengthPerTemperature"] = ["geothermal step", "length per relative temperature"],
                ["PressureGradientPerLength"] = ["pressure gradient", "pressure drop per length"],
                ["PressureRateOfChange"] = ["pressure rate", "pressure per time"],
                ["VolumetricHeatTransferCoefficient"] = ["heat transfer coefficient volumetric"],
                ["PorousMediumPermeability"] = ["permeability", "rock permeability"]
            };

        internal static void Supplement(BasePhysicalQuantity quantity)
        {
            if (Synonyms.TryGetValue(quantity.Name, out string[]? synonyms))
            {
                quantity.AddUsualNames(synonyms);
            }

            string[] unsuffixedDrillingNames = (quantity.UsualNames ?? [])
                         .Where(name => name.EndsWith(" (drilling)", StringComparison.OrdinalIgnoreCase))
                         .Select(name => name[..^" (drilling)".Length])
                         .ToArray();
            quantity.AddUsualNames(unsuffixedDrillingNames);
        }
    }
}
