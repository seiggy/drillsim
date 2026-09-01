using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents luminous efficacy.
    /// </summary>
    public partial class LuminousEfficacyQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "eta_v";
        public override string SIUnitName { get; } = "lumen per watt";
        public override string SIUnitLabelLatex { get; } = "\\frac{lm}{W}";
        public override double SolidAngleDimension { get; } = 1;
        public override double LengthDimension { get; } = -2;
        public override double TimeDimension { get; } = 3;
        public override double MassDimension { get; } = -1;
        public override double LuminousIntensityDimension { get; } = 1;

        private static LuminousEfficacyQuantity instance_ = null;
        public static LuminousEfficacyQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new LuminousEfficacyQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "lumen per watt",
                UnitLabel = "lm/W",
                ID = new Guid("b886e335-2192-4f7b-b04d-0003f9b0b24b"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "lumen per kilowatt",
                UnitLabel = "lm/kW",
                ID = new Guid("9ea10827-647e-4208-8495-93f794603212"),
                ConversionFactorFromSIFormula = "Factors.Kilo",
            }
        };

        public LuminousEfficacyQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "luminous efficacy" };
            ID = new Guid("cea058e0-f4a8-4f46-966c-f460eff93031");
            DescriptionMD = "**luminous efficacy** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is lumen per watt with unit label $\\frac{lm}{W}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
