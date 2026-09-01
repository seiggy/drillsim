using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents amount of substance rate.
    /// </summary>
    public partial class AmountSubstanceRateQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "q_n";
        public override string SIUnitName { get; } = "mole per second";
        public override string SIUnitLabelLatex { get; } = "\\frac{mol}{s}";
        public override double AmountSubstanceDimension { get; } = 1;
        public override double TimeDimension { get; } = -1;

        private static AmountSubstanceRateQuantity instance_ = null;
        public static AmountSubstanceRateQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new AmountSubstanceRateQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "mole per second",
                UnitLabel = "mol/s",
                ID = new Guid("c98f922e-202b-4089-8669-f1fcdced34e5"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "mole per minute",
                UnitLabel = "mol/min",
                ID = new Guid("9b687210-a450-4d7e-9f50-0314e0584990"),
                ConversionFactorFromSIFormula = "Factors.Minute",
            },
            new UnitChoice
            {
                UnitName = "kilomole per hour",
                UnitLabel = "kmol/h",
                ID = new Guid("3dec2a32-1d28-405d-bfde-569cd50a0982"),
                ConversionFactorFromSIFormula = "Factors.Hour/Factors.Kilo",
            }
        };

        public AmountSubstanceRateQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "amount of substance rate", "molar flow rate" };
            ID = new Guid("4356fa06-b519-4c6b-8f62-23c0f5a04fa1");
            DescriptionMD = "**amount of substance rate** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is mole per second with unit label $\\frac{mol}{s}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
