using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents heat capacity.
    /// </summary>
    public partial class HeatCapacityQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "C";
        public override string SIUnitName { get; } = "joule per kelvin";
        public override string SIUnitLabelLatex { get; } = "\\frac{J}{K}";
        public override double LengthDimension { get; } = 2;
        public override double TimeDimension { get; } = -2;
        public override double MassDimension { get; } = 1;
        public override double TemperatureDimension { get; } = -1;

        private static HeatCapacityQuantity instance_ = null;
        public static HeatCapacityQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new HeatCapacityQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "joule per kelvin",
                UnitLabel = "J/K",
                ID = new Guid("387b4ab8-4e40-4872-82f9-5e3ef827c059"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "kilojoule per kelvin",
                UnitLabel = "kJ/K",
                ID = new Guid("b72d91a9-56e8-4dce-a01e-6c250ee8ca99"),
                ConversionFactorFromSIFormula = "1.0/Factors.Kilo",
            },
            new UnitChoice
            {
                UnitName = "british thermal unit per degree fahrenheit",
                UnitLabel = "BTU/degF",
                ID = new Guid("9ae9182e-5d9a-41a5-b857-33db3ad4e7b0"),
                ConversionFactorFromSIFormula = "Factors.FahrenheitSlope/Factors.BTU",
            }
        };

        public HeatCapacityQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "heat capacity" };
            ID = new Guid("eb719933-d2de-4894-b72d-d5a66d89e54b");
            DescriptionMD = "**heat capacity** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is joule per kelvin with unit label $\\frac{J}{K}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
