using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents thermal insulance.
    /// </summary>
    public partial class ThermalInsulanceQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "R_A";
        public override string SIUnitName { get; } = "square metre kelvin per watt";
        public override string SIUnitLabelLatex { get; } = "\\frac{m^2\\cdot K}{W}";
        public override double TemperatureDimension { get; } = 1;
        public override double TimeDimension { get; } = 3;
        public override double MassDimension { get; } = -1;

        private static ThermalInsulanceQuantity instance_ = null;
        public static ThermalInsulanceQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ThermalInsulanceQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "square metre kelvin per watt",
                UnitLabel = "m^2*K/W",
                ID = new Guid("4f7dd83c-dfbd-4dd4-ae9c-cf4f9e031c67"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "square foot hour degree fahrenheit per british thermal unit",
                UnitLabel = "ft^2*h*degF/BTU",
                ID = new Guid("d47cfe7d-beb9-4d2c-85ea-5ac42c1a482c"),
                ConversionFactorFromSIFormula = "Factors.BTU/(Factors.Foot*Factors.Foot*Factors.Hour*Factors.FahrenheitSlope)",
            }
        };

        public ThermalInsulanceQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "thermal insulance", "area specific thermal resistance" };
            ID = new Guid("f5776adf-201c-4222-8a8f-b4bed4184a79");
            DescriptionMD = "**thermal insulance** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is square metre kelvin per watt with unit label $\\frac{m^2\\cdot K}{W}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
