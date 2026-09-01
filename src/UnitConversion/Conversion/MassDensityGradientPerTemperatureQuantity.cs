using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class MassDensityGradientPerTemperatureQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "kilogram per cubic metre per kelvin";
        public override string SIUnitLabelLatex { get; } = "\\frac{\\frac{kg}{m^{3}}}{K}";
        public override double MassDimension { get; } = 1;
        public override double LengthDimension { get; } = -3;
        public override double TemperatureDimension { get; } = -1;
        private static MassDensityGradientPerTemperatureQuantity instance_ = null;

        public static MassDensityGradientPerTemperatureQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MassDensityGradientPerTemperatureQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
                {
                    UnitName = "kilogram per cubic metre per kelvin",
                    UnitLabel = "kg/m³/K",
                    ID = new Guid("8b947453-ebe8-4fa9-b59a-87557150e1cf"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                    IsSI = true
                },
                new UnitChoice
                {
                    UnitName = "specific gravity per celsius",
                    UnitLabel = "sg/°C",
                    ID = new Guid("2b1d68c0-4e75-4e9d-92a1-37d501e7cb3e"),
                    ConversionFactorFromSIFormula = "Factors.SpecificGavity4degC"
                },
                new UnitChoice
                {
                    UnitName = "gram per cubic centimetre per celsius",
                    UnitLabel = "g/cm³/°C",
                    ID = new Guid("e78e2b25-e0a7-4c06-b6df-60f97f767a20"),
                    ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi*Factors.Centi/Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (UK) per celsius",
                    UnitLabel = "ppgUK/°C",
                    ID = new Guid("edac57eb-7535-447f-bcf9-0c6709b6ae3b"),
                    ConversionFactorFromSIFormula = "Factors.GallonUK/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (US) per fahrenheit",
                    UnitLabel = "ppgUS/°F",
                    ID = new Guid("397a5f98-842e-4d86-8fb7-f4f2f82e720b"),
                    ConversionFactorFromSIFormula = "Factors.GallonUS*Factors.FahrenheitSlope/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (UK) per fahrenheit",
                    UnitLabel = "ppgUK/°F",
                    ID = new Guid("24485846-7944-4903-a5c5-975298daf450"),
                    ConversionFactorFromSIFormula = "Factors.GallonUK*Factors.FahrenheitSlope/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (US) per celsius",
                    UnitLabel = "ppgUS/°C",
                    ID = new Guid("8b642465-acee-4a4a-9cb5-6fc16ace5bc3"),
                    ConversionFactorFromSIFormula = "Factors.GallonUS/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic foot per celsius",
                    UnitLabel = "lb/ft³/°C",
                    ID = new Guid("592a60a7-71e1-40dc-bfe0-573e407b893c"),
                    ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot*Factors.Foot/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic foot per fahrenheit",
                    UnitLabel = "lb/ft³/°F",
                    ID = new Guid("64b97848-5c42-49ec-a09e-05c7bd0cea6b"),
                    ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot*Factors.Foot*Factors.FahrenheitSlope/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic inch per celsius",
                    UnitLabel = "lb/in³/°C",
                    ID = new Guid("10e845fe-c8c1-4847-bf6a-874c1f746325"),
                    ConversionFactorFromSIFormula = "Factors.Inch*Factors.Inch*Factors.Inch/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic inch per fahrenheit",
                    UnitLabel = "lb/in³/°F",
                    ID = new Guid("586d023b-3c87-4354-bce9-5704c8d1ae0a"),
                    ConversionFactorFromSIFormula = "Factors.Inch*Factors.Inch*Factors.Inch*Factors.FahrenheitSlope/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic yard per celsius",
                    UnitLabel = "lb/yd³/°C",
                    ID = new Guid("ea5147c2-d35b-4e0c-8c47-e9f04a6e0fa1"),
                    ConversionFactorFromSIFormula = "Factors.Yard*Factors.Yard*Factors.Yard/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic yeard per fahrenheit",
                    UnitLabel = "lb/yd³/°F",
                    ID = new Guid("e15f8f82-0d58-487a-9d70-8f14f3606177"),
                    ConversionFactorFromSIFormula = "Factors.Yard*Factors.Yard*Factors.Yard*Factors.FahrenheitSlope/Factors.Pound"
                }
        };
        public MassDensityGradientPerTemperatureQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "mass density gradient per temperature" };
            ID = new Guid("2d788f1e-db66-49c3-8eb6-313152cd8e3c");
            DescriptionMD = string.Empty;
            DescriptionMD += @"A mass density gradient per temperature is the first derivative of a mass density compared to temperature: $\frac{d\rho}{dT}$, where $\rho$ is a mass density and $T$ is temperature." + Environment.NewLine;
            DescriptionMD += @"The dimension of mass density gradient per temperature is:" + Environment.NewLine;
            DescriptionMD += "$" + GetDimensionsEnclosed() + "$." + Environment.NewLine;
            if (!string.IsNullOrEmpty(SIUnitLabelLatex) && !string.IsNullOrEmpty(SIUnitName) && UsualNames != null && UsualNames.Count > 0)
            {
                DescriptionMD += Environment.NewLine;
                DescriptionMD += @"The SI unit for **" + UsualNames.First() + "** is: " + SIUnitName + " with the associated unit label $" + SIUnitLabelLatex + "$" + Environment.NewLine;
            }
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}