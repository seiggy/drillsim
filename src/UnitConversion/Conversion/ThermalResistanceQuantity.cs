using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents thermal resistance.
    /// </summary>
    public partial class ThermalResistanceQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "R_th";
        public override string SIUnitName { get; } = "kelvin per watt";
        public override string SIUnitLabelLatex { get; } = "\\frac{K}{W}";
        public override double LengthDimension { get; } = -2;
        public override double TimeDimension { get; } = 3;
        public override double MassDimension { get; } = -1;
        public override double TemperatureDimension { get; } = 1;

        private static ThermalResistanceQuantity instance_ = null;
        public static ThermalResistanceQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ThermalResistanceQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "kelvin per watt",
                UnitLabel = "K/W",
                ID = new Guid("33065071-5e8d-4b62-a3fd-66aaec43def6"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "kelvin per kilowatt",
                UnitLabel = "K/kW",
                ID = new Guid("3fd80f75-5fa8-406b-ad2a-076c6576f892"),
                ConversionFactorFromSIFormula = "Factors.Kilo",
            },
            new UnitChoice
            {
                UnitName = "hour degree fahrenheit per british thermal unit",
                UnitLabel = "h*degF/BTU",
                ID = new Guid("d4a9d58d-d91d-4bbc-9516-8010d44ea282"),
                ConversionFactorFromSIFormula = "Factors.BTU/(Factors.Hour*Factors.FahrenheitSlope)",
            }
        };

        public ThermalResistanceQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "thermal resistance" };
            ID = new Guid("72d2e7a8-926d-4a12-a1a1-34a8a4e591d3");
            DescriptionMD = "**thermal resistance** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is kelvin per watt with unit label $\\frac{K}{W}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
