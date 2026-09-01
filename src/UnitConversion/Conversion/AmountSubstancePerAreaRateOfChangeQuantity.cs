using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents amount of substance per area rate of change.
    /// </summary>
    public partial class AmountSubstancePerAreaRateOfChangeQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "d(n/A)/dt";
        public override string SIUnitName { get; } = "mole per square metre second";
        public override string SIUnitLabelLatex { get; } = "\\frac{mol}{m^2\\cdot s}";
        public override double LengthDimension { get; } = -2;
        public override double TimeDimension { get; } = -1;
        public override double AmountSubstanceDimension { get; } = 1;

        private static AmountSubstancePerAreaRateOfChangeQuantity instance_ = null;
        public static AmountSubstancePerAreaRateOfChangeQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new AmountSubstancePerAreaRateOfChangeQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "mole per square metre second",
                UnitLabel = "mol/(m^2*s)",
                ID = new Guid("c9dcb204-c8a6-4f0a-80df-680054f8d2d1"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "millimole per square metre second",
                UnitLabel = "mmol/(m^2*s)",
                ID = new Guid("bfa7e432-587b-4a31-b84f-950ce89ff6d2"),
                ConversionFactorFromSIFormula = "1.0/Factors.Milli",
            }
        };

        public AmountSubstancePerAreaRateOfChangeQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "amount of substance per area rate of change", "molar flux" };
            ID = new Guid("ad4557d8-15fc-46e5-821e-e908210b5040");
            DescriptionMD = "**amount of substance per area rate of change** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is mole per square metre second with unit label $\\frac{mol}{m^2\\cdot s}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
