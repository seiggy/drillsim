using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents length per temperature.
    /// </summary>
    public partial class LengthPerTemperatureQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "L/T";
        public override string SIUnitName { get; } = "metre per kelvin";
        public override string SIUnitLabelLatex { get; } = "\\frac{m}{K}";
        public override double TemperatureDimension { get; } = -1;
        public override double LengthDimension { get; } = 1;

        private static LengthPerTemperatureQuantity instance_ = null;
        public static LengthPerTemperatureQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new LengthPerTemperatureQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "metre per kelvin",
                UnitLabel = "m/K",
                ID = new Guid("5eb0aaab-92e3-408c-a381-4eb929a6e2b2"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "millimetre per kelvin",
                UnitLabel = "mm/K",
                ID = new Guid("8bb8ac76-86b4-49bc-9794-a50c8a18c88b"),
                ConversionFactorFromSIFormula = "1.0/Factors.Milli",
            },
            new UnitChoice
            {
                UnitName = "foot per degree fahrenheit",
                UnitLabel = "ft/degF",
                ID = new Guid("899cedf7-5ddc-4f4c-a8eb-01f97a78b853"),
                ConversionFactorFromSIFormula = "Factors.FahrenheitSlope/Factors.Foot",
            }
        };

        public LengthPerTemperatureQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "length per temperature" };
            ID = new Guid("c6bd0e8f-1707-46e1-8d4a-7073b3b710e9");
            DescriptionMD = "**length per temperature** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is metre per kelvin with unit label $\\frac{m}{K}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
