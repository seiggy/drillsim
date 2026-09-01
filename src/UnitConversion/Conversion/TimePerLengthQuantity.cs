using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents time per length.
    /// </summary>
    public partial class TimePerLengthQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "t/L";
        public override string SIUnitName { get; } = "second per metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{s}{m}";
        public override double TimeDimension { get; } = 1;
        public override double LengthDimension { get; } = -1;

        private static TimePerLengthQuantity instance_ = null;
        public static TimePerLengthQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new TimePerLengthQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "second per metre",
                UnitLabel = "s/m",
                ID = new Guid("a70fe7ae-dd6f-4e44-ac3b-a7c53f287d26"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "minute per metre",
                UnitLabel = "min/m",
                ID = new Guid("aad1e3d6-1dd6-4aed-8a3f-a96613d6c701"),
                ConversionFactorFromSIFormula = "1.0/Factors.Minute",
            },
            new UnitChoice
            {
                UnitName = "second per foot",
                UnitLabel = "s/ft",
                ID = new Guid("a660c9aa-3930-44bb-873a-1cc58b633f9c"),
                ConversionFactorFromSIFormula = "Factors.Foot",
            }
        };

        public TimePerLengthQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "time per length" };
            ID = new Guid("6ae01714-16a5-48d0-82f3-a466478d51e4");
            DescriptionMD = "**time per length** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is second per metre with unit label $\\frac{s}{m}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
