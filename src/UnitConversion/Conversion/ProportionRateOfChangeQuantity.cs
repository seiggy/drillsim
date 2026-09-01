using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// The rate at which a dimensionless proportion changes with time.
    /// </summary>
    public partial class ProportionRateOfChangeQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "dx/dt";
        public override string SIUnitName { get; } = "proportion per second";
        public override string SIUnitLabelLatex { get; } = "\\frac{1}{s}";
        public override double TimeDimension { get; } = -1;

        private static ProportionRateOfChangeQuantity instance_ = null;
        public static ProportionRateOfChangeQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ProportionRateOfChangeQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "proportion per second",
                UnitLabel = "1/s",
                ID = new Guid("5f0cc602-309c-479d-9c9c-9488675dcba3"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true
            },
            new UnitChoice
            {
                UnitName = "percent per second",
                UnitLabel = "%/s",
                ID = new Guid("9dbadbb9-de09-4fdc-b383-ffbd01a28f1c"),
                ConversionFactorFromSIFormula = "1.0/Factors.Centi"
            },
            new UnitChoice
            {
                UnitName = "per thousand per second",
                UnitLabel = "‰/s",
                ID = new Guid("fa64e5ee-e321-4b0f-91b7-ae04c704bbcf"),
                ConversionFactorFromSIFormula = "1.0/Factors.Milli"
            },
            new UnitChoice
            {
                UnitName = "part per million per second",
                UnitLabel = "ppm/s",
                ID = new Guid("518c5571-4415-4687-99bf-fc2e2db1b924"),
                ConversionFactorFromSIFormula = "1.0/Factors.Micro"
            }
        };

        public ProportionRateOfChangeQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "proportion rate of change", "proportion rate", "rate of change of proportion" };
            ID = new Guid("ea1d1297-7726-474c-8bc0-804b8f3b1f4a");
            DescriptionMD = "**Proportion rate of change** is the change in a dimensionless proportion per unit time." + Environment.NewLine;
            DescriptionMD += "Its physical dimension is " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The coherent SI unit is reciprocal second with unit label $" + SIUnitLabelLatex + "$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
