using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Curated explanatory text for the quantities introduced with the expanded
    /// catalogue. Keeping it here lets a drilling specialization inherit the
    /// same definition while adding its own measurement precision.
    /// </summary>
    internal static class PhysicalQuantityDescriptionCatalog
    {
        private static readonly IReadOnlyDictionary<string, string> Definitions =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["AbsorbedDose"] = "is the energy imparted by ionising radiation per unit mass of material.",
                ["AmountSubstancePerArea"] = "expresses an amount of substance distributed over a surface, for example a surface concentration or areal loading.",
                ["AmountSubstancePerAreaRateOfChange"] = "expresses how an areal amount-of-substance density changes with time.",
                ["AmountSubstancePerVolume"] = "expresses the amount of substance contained in a unit volume; it is commonly called amount concentration or molar concentration.",
                ["AmountSubstanceRate"] = "is the flow of amount of substance through a process or boundary per unit time.",
                ["AngularVelocityPerVolumetricFlowRate"] = "relates rotational speed to volumetric flow rate, as used when characterising rotating-flow equipment.",
                ["AreaPerAmountSubstance"] = "expresses surface area available per amount of substance, the reciprocal of areal amount concentration.",
                ["AreaPerMass"] = "expresses surface area per unit mass, for example the specific surface area of a material.",
                ["AreaPerVolume"] = "expresses surface area per unit volume, for example an interfacial or specific area density.",
                ["AreaRateOfChange"] = "is the rate at which an area grows, contracts, or is swept out with time.",
                ["BendingMomentGradient"] = "is the spatial gradient of bending moment with respect to length; the unsimplified form preserves that mechanical interpretation.",
                ["DataStorage"] = "measures the amount of digital information that can be stored.",
                ["DataTransferRate"] = "measures the rate at which digital information is transmitted or processed.",
                ["Diffusivity"] = "characterises the rate at which a quantity spreads through a medium by diffusion; thermal and mass diffusivities share this dimension.",
                ["DigitalSymbolRate"] = "is the number of modulation symbols conveyed per unit time; it is distinct from bit rate when a symbol carries several bits.",
                ["DoseEquivalent"] = "is a radiation-protection quantity that weights absorbed dose for the biological effectiveness of the radiation.",
                ["DoseEquivalentRate"] = "is the time rate of dose equivalent, used to describe radiation-protection exposure intensity.",
                ["ElectricCharge"] = "quantifies electric charge carried by matter or transferred in an electrical process.",
                ["ElectricChargePerArea"] = "expresses surface charge density.",
                ["ElectricChargePerMass"] = "expresses charge per unit mass, often called specific charge.",
                ["ElectricChargePerVolume"] = "expresses volume charge density.",
                ["ElectricConductance"] = "measures the ability of a component or path to conduct electric current; it is the reciprocal of resistance.",
                ["ElectricConductivity"] = "measures a material's ability to conduct electric current; it is the reciprocal of electrical resistivity.",
                ["ElectricCurrentDensity"] = "is electric current flowing through a unit cross-sectional area.",
                ["ElectricDipoleMoment"] = "quantifies the separation and magnitude of opposite electric charges in a system.",
                ["ElectricFieldStrength"] = "is electric force per unit charge, equivalently the electric-potential gradient.",
                ["ElectricResistanceGradientPerLength"] = "is the change of electrical resistance per unit length of a conductor or formation path.",
                ["ElectricalMobility"] = "is charge-carrier drift velocity per unit electric-field strength.",
                ["ElectromagneticMoment"] = "quantifies the strength and orientation of a magnetic dipole produced by a current distribution.",
                ["EnergyPerArea"] = "expresses energy distributed over an area, for example radiant exposure or surface work; its named form preserves that interpretation.",
                ["ForceArea"] = "is the product of force and area, retained where that product has engineering significance.",
                ["ForcePerVolume"] = "is force distributed through a unit volume, often called force density or body-force density.",
                ["HeatCapacity"] = "is the heat required to raise the temperature of a body by one unit, without normalising by mass or amount of substance.",
                ["Illuminance"] = "is luminous flux incident on a surface per unit area and describes how strongly a surface is illuminated.",
                ["Inductance"] = "relates magnetic flux linkage to electric current and determines a circuit's opposition to changing current.",
                ["KinematicViscosity"] = "is dynamic viscosity divided by mass density and characterises momentum diffusion in a fluid.",
                ["LengthPerAngle"] = "relates a linear displacement or arc length to a change in plane angle.",
                ["LengthPerMass"] = "expresses length per unit mass, for example a material's linear yield per mass.",
                ["LengthPerPressure"] = "expresses displacement or length change per unit pressure change, a compliance-like quantity.",
                ["LengthPerTemperature"] = "expresses length change per unit temperature change before normalisation by original length.",
                ["LengthPerVolume"] = "expresses length per unit volume, useful for lineal density or geometry-related measures.",
                ["LightExposure"] = "is luminous energy received by a surface per unit area over an exposure interval.",
                ["LinearThermalExpansionCoefficient"] = "is the fractional change in length per unit temperature change.",
                ["Luminance"] = "describes luminous intensity emitted, reflected, or transmitted by a surface in a given direction per projected area.",
                ["LuminousEfficacy"] = "relates luminous flux to radiant power and indicates how efficiently radiation produces visible light.",
                ["LuminousEnergy"] = "is luminous flux integrated over time and represents the perceived light output of an event.",
                ["LuminousExitance"] = "is luminous flux leaving a surface per unit area.",
                ["LuminousFlux"] = "is the total visible-light power weighted by the sensitivity of the human eye.",
                ["MagneticFieldStrength"] = "describes the magnetising field produced by currents or magnetisation, independently of the medium's permeability.",
                ["MagneticFluxDensityGradientPerLength"] = "is the spatial gradient of magnetic flux density along a length.",
                ["MagneticPermeability"] = "relates magnetic flux density to magnetic-field strength in a material.",
                ["MagneticVectorPotential"] = "is a vector field whose curl gives magnetic flux density.",
                ["MassLength"] = "is the product of mass and length, retained for quantities whose dimensional expression has this form.",
                ["MassPerArea"] = "expresses mass distributed over an area, for example areal density or surface loading.",
                ["MassPerEnergy"] = "expresses mass per unit energy, the reciprocal of specific energy.",
                ["MassRateGradientPerLength"] = "is the spatial gradient of mass flow rate along a length.",
                ["MassRatePerArea"] = "is mass flow rate through a unit area, commonly called mass flux.",
                ["Mobility"] = "is the permeability-related fluid mobility, conventionally permeability divided by dynamic viscosity.",
                ["MolarEnergy"] = "is energy per amount of substance, commonly used for molar enthalpy and related thermodynamic properties.",
                ["MolarHeatCapacity"] = "is heat capacity per amount of substance.",
                ["MolarMass"] = "is mass per amount of substance.",
                ["MolarVolume"] = "is volume per amount of substance.",
                ["Momentum"] = "is mass multiplied by velocity and describes the quantity of motion of a body or flow.",
                ["PermeabilityLength"] = "is permeability multiplied by length, often used as permeability-thickness in reservoir and flow calculations.",
                ["Permittivity"] = "relates electric displacement to electric-field strength in a material.",
                ["PlaneAnglePerVolume"] = "expresses a plane-angle change distributed through a volume.",
                ["PowerPerArea"] = "is power transferred, emitted, or received per unit area; depending on context it may describe heat flux or intensity.",
                ["PowerPerVolume"] = "is power generated, dissipated, or transferred per unit volume.",
                ["PowerRateOfChange"] = "is the change in power per unit time, for example the ramp rate of a generator or drive.",
                ["PressurePerVolume"] = "expresses pressure change per unit volume change, for example a stiffness-like volumetric response.",
                ["PressureTimePerVolume"] = "combines pressure, time, and volume in flow-resistance and transient-flow relationships.",
                ["ProductivityIndex"] = "relates volumetric production rate to pressure drawdown and characterises well or reservoir productivity.",
                ["ProportionRateOfChange"] = "is the rate at which a dimensionless proportion or opening changes with time.",
                ["Radiance"] = "is radiant power emitted, reflected, transmitted, or received per projected area per solid angle.",
                ["RadiantIntensity"] = "is radiant power emitted by a source per unit solid angle.",
                ["Radioactivity"] = "is the rate of nuclear transformations in a radioactive sample.",
                ["ReciprocalArea"] = "is inverse area and is used for areal number densities and geometry factors.",
                ["ReciprocalElectricTension"] = "is inverse electric potential difference.",
                ["ReciprocalForce"] = "is inverse force.",
                ["ReciprocalMass"] = "is inverse mass.",
                ["ReciprocalTime"] = "is inverse time and is the dimensional basis of frequency and rates.",
                ["ReciprocalVolume"] = "is inverse volume and is used for volumetric concentrations and densities.",
                ["RelativeTemperaturePerPressure"] = "is relative-temperature change per pressure change.",
                ["Reluctance"] = "opposes magnetic flux in a magnetic circuit, analogous to electrical resistance in an electric circuit.",
                ["SectionModulus"] = "is a geometric property of a cross-section used to relate bending moment to maximum bending stress.",
                ["SpecificActivity"] = "is radioactivity per unit mass of radioactive material.",
                ["SpecificEnergy"] = "is energy per unit mass.",
                ["SpecificProductivityIndex"] = "normalises productivity index by a further reference quantity, retaining the named reservoir-engineering interpretation.",
                ["TemperatureRateOfChange"] = "is the rate at which temperature changes with time.",
                ["ThermalConductance"] = "is the heat-transfer rate per temperature difference for a complete thermal path.",
                ["ThermalInsulance"] = "is the area-normalised resistance to heat flow through a material or layer.",
                ["ThermalResistance"] = "is the temperature difference required to sustain a unit heat-transfer rate.",
                ["TimePerLength"] = "expresses travel time or delay per unit length, the reciprocal of velocity.",
                ["TimePerMass"] = "expresses time per unit mass, the reciprocal of mass rate.",
                ["TimePerVolume"] = "expresses time per unit volume, the reciprocal of volumetric flow rate.",
                ["VolumeGradientPerLength"] = "is the spatial gradient of volume with respect to length.",
                ["VolumePerAngle"] = "relates a volume change to a plane-angle variation; the angle-based form is retained for rotational geometry and displacement applications.",
                ["VolumePerArea"] = "expresses volume per unit area, equivalent to a characteristic thickness.",
                ["VolumePerAreaRateOfChange"] = "is the rate of change of volume per unit area.",
                ["VolumePerEnergy"] = "expresses volume per unit energy.",
                ["VolumePerLengthRateOfChange"] = "is the rate of change of volume per unit length.",
                ["VolumePerPressure"] = "expresses volume change per unit pressure change, a compliance-like volumetric quantity.",
                ["VolumePerPressureRateOfChange"] = "is the rate of change of volume-per-pressure with time.",
                ["VolumePerVolumeRateOfChange"] = "is the time rate of relative volume change, for example a volumetric strain rate.",
                ["VolumetricFlowRateGradientPerLength"] = "is the spatial gradient of volumetric flow rate along a length.",
                ["VolumetricFlowRatePerArea"] = "is volumetric flow rate through a unit area, commonly called superficial velocity or flux.",
                ["VolumetricHeatTransferCoefficient"] = "is heat-transfer capacity per unit volume and temperature difference.",
                ["VolumetricThermalExpansionCoefficient"] = "is the fractional change in volume per unit temperature change.",
                ["WorkGradient"] = "is the spatial gradient of work or energy; the unsimplified expression preserves this interpretation."
            };

        internal static void Supplement(BasePhysicalQuantity quantity)
        {
            bool drilling = quantity.Name.EndsWith("Drilling", StringComparison.Ordinal);
            string baseName = drilling ? quantity.Name[..^"Drilling".Length] : quantity.Name;
            if (!Definitions.TryGetValue(baseName, out string? definition)) return;

            string displayName = quantity.UsualNames?.FirstOrDefault() ?? baseName;
            string description = "**" + displayName + "** " + definition + Environment.NewLine;
            description += "Its physical dimension is " + quantity.GetDimensionsEnclosed() + "." + Environment.NewLine;
            description += "The coherent SI unit is " + quantity.SIUnitName + " with unit label $" + quantity.SIUnitLabelLatex + "$." + Environment.NewLine;
            if (drilling && quantity.MeaningfulPrecisionInSI != null)
            {
                description += "For the drilling specialization, the meaningful precision is " + quantity.MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            }
            quantity.ReplaceDescription(description);
        }
    }
}
