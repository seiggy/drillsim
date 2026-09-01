using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents dose equivalent rate.
    /// </summary>
    public partial class DoseEquivalentRateQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "dH/dt";
        public override string SIUnitName { get; } = "sievert per second";
        public override string SIUnitLabelLatex { get; } = "\\frac{Sv}{s}";
        public override double TimeDimension { get; } = -3;
        public override double LengthDimension { get; } = 2;
        public override double? MeaningfulPrecisionInSI { get; } = 1E-09;

        private static DoseEquivalentRateQuantity instance_ = null;
        public static DoseEquivalentRateQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new DoseEquivalentRateQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "sievert per second",
                UnitLabel = "Sv/s",
                ID = new Guid("8e582959-40b2-4562-ae5f-37030a052f1c"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "millisievert per hour",
                UnitLabel = "mSv/h",
                ID = new Guid("80178739-e7ff-41a6-ab3a-74dc5f8fd9b6"),
                ConversionFactorFromSIFormula = "Factors.Hour/Factors.Milli",
            },
            new UnitChoice
            {
                UnitName = "microsievert per hour",
                UnitLabel = "uSv/h",
                ID = new Guid("44fee091-53cd-4c8f-8335-7ce6c755a220"),
                ConversionFactorFromSIFormula = "Factors.Hour/Factors.Micro",
            }
        };

        public DoseEquivalentRateQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "dose equivalent rate" };
            ID = new Guid("d94f46ab-9d1e-41b4-9e09-2172fdb085ff");
            DescriptionMD = "**dose equivalent rate** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is sievert per second with unit label $\\frac{Sv}{s}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
