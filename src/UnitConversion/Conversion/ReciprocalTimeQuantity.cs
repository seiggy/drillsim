using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents reciprocal time.
    /// </summary>
    public partial class ReciprocalTimeQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "f";
        public override string SIUnitName { get; } = "reciprocal second";
        public override string SIUnitLabelLatex { get; } = "\\frac{1}{s}";
        public override double TimeDimension { get; } = -1;

        private static ReciprocalTimeQuantity instance_ = null;
        public static ReciprocalTimeQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ReciprocalTimeQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "reciprocal second",
                UnitLabel = "1/s",
                ID = new Guid("00da1610-4717-49e4-bc33-6547bf0654f5"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "reciprocal minute",
                UnitLabel = "1/min",
                ID = new Guid("9465e6c1-cbe9-413c-8702-9ccb64b9fee7"),
                ConversionFactorFromSIFormula = "Factors.Minute",
            },
            new UnitChoice
            {
                UnitName = "reciprocal hour",
                UnitLabel = "1/h",
                ID = new Guid("7fe15b16-6bc4-4865-a5df-1ff6ebcb7577"),
                ConversionFactorFromSIFormula = "Factors.Hour",
            }
        };

        public ReciprocalTimeQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "reciprocal time" };
            ID = new Guid("87c47f73-1e79-44c5-87ce-d81514305184");
            DescriptionMD = "**reciprocal time** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is reciprocal second with unit label $\\frac{1}{s}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
