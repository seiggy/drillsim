using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents thermal conductance.
    /// </summary>
    public partial class ThermalConductanceQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "G_th";
        public override string SIUnitName { get; } = "watt per kelvin";
        public override string SIUnitLabelLatex { get; } = "\\frac{W}{K}";
        public override double LengthDimension { get; } = 2;
        public override double TimeDimension { get; } = -3;
        public override double MassDimension { get; } = 1;
        public override double TemperatureDimension { get; } = -1;

        private static ThermalConductanceQuantity instance_ = null;
        public static ThermalConductanceQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ThermalConductanceQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "watt per kelvin",
                UnitLabel = "W/K",
                ID = new Guid("1d8f78ba-b042-4e8a-a2a1-cab7e29240f6"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "kilowatt per kelvin",
                UnitLabel = "kW/K",
                ID = new Guid("5591c455-0985-42a8-93c1-d43204867ef9"),
                ConversionFactorFromSIFormula = "1.0/Factors.Kilo",
            },
            new UnitChoice
            {
                UnitName = "british thermal unit per hour degree fahrenheit",
                UnitLabel = "BTU/(h*degF)",
                ID = new Guid("ebf17506-af29-4a0f-a876-0ffc44ed5314"),
                ConversionFactorFromSIFormula = "Factors.Hour*Factors.FahrenheitSlope/Factors.BTU",
            }
        };

        public ThermalConductanceQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "thermal conductance" };
            ID = new Guid("65bb59df-6dfc-430d-b7a6-28405c56a73f");
            DescriptionMD = "**thermal conductance** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is watt per kelvin with unit label $\\frac{W}{K}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
