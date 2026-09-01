using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents electric resistance gradient per length.
    /// </summary>
    public partial class ElectricResistanceGradientPerLengthQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "dR/dL";
        public override string SIUnitName { get; } = "ohm per metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{ohm}{m}";
        public override double LengthDimension { get; } = 1;
        public override double TimeDimension { get; } = -3;
        public override double MassDimension { get; } = 1;
        public override double ElectricCurrentDimension { get; } = -2;

        private static ElectricResistanceGradientPerLengthQuantity instance_ = null;
        public static ElectricResistanceGradientPerLengthQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ElectricResistanceGradientPerLengthQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "ohm per metre",
                UnitLabel = "ohm/m",
                ID = new Guid("68043253-647a-478f-9265-e164ed2c9071"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "ohm per kilometre",
                UnitLabel = "ohm/km",
                ID = new Guid("f819762d-1772-42d8-8a77-d8806653fa98"),
                ConversionFactorFromSIFormula = "Factors.Kilo",
            },
            new UnitChoice
            {
                UnitName = "ohm per foot",
                UnitLabel = "ohm/ft",
                ID = new Guid("13202b93-795b-4fd4-88f0-bbff7cdf57cd"),
                ConversionFactorFromSIFormula = "Factors.Foot",
            }
        };

        public ElectricResistanceGradientPerLengthQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "electric resistance gradient per length", "resistance per length" };
            ID = new Guid("9d65e20d-c896-494a-9533-c32938fa65dc");
            DescriptionMD = "**electric resistance gradient per length** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is ohm per metre with unit label $\\frac{ohm}{m}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
