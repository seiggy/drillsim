using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents molar heat capacity.
    /// </summary>
    public partial class MolarHeatCapacityQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "C_m";
        public override string SIUnitName { get; } = "joule per mole kelvin";
        public override string SIUnitLabelLatex { get; } = "\\frac{J}{mol\\cdot K}";
        public override double LengthDimension { get; } = 2;
        public override double TimeDimension { get; } = -2;
        public override double MassDimension { get; } = 1;
        public override double TemperatureDimension { get; } = -1;
        public override double AmountSubstanceDimension { get; } = -1;

        private static MolarHeatCapacityQuantity instance_ = null;
        public static MolarHeatCapacityQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MolarHeatCapacityQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "joule per mole kelvin",
                UnitLabel = "J/(mol*K)",
                ID = new Guid("b66ae637-3fb8-4063-8e82-7e9001d2086f"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "kilojoule per kilomole kelvin",
                UnitLabel = "kJ/(kmol*K)",
                ID = new Guid("6e9d07ed-9840-44ba-b1f0-7a6a4349185c"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
            },
            new UnitChoice
            {
                UnitName = "kilojoule per mole kelvin",
                UnitLabel = "kJ/(mol*K)",
                ID = new Guid("43f78db6-d700-4830-b17c-a32f05c49f83"),
                ConversionFactorFromSIFormula = "1.0/Factors.Kilo",
            }
        };

        public MolarHeatCapacityQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "molar heat capacity" };
            ID = new Guid("c75d3d79-1040-4cb2-aa6a-9fe32d04a942");
            DescriptionMD = "**molar heat capacity** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is joule per mole kelvin with unit label $\\frac{J}{mol\\cdot K}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
