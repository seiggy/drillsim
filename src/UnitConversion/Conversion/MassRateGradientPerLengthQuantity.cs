using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents mass rate gradient per length.
    /// </summary>
    public partial class MassRateGradientPerLengthQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "d(q_m)/dL";
        public override string SIUnitName { get; } = "kilogram per second metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{kg}{s\\cdot m}";
        public override double LengthDimension { get; } = -1;
        public override double TimeDimension { get; } = -1;
        public override double MassDimension { get; } = 1;

        private static MassRateGradientPerLengthQuantity instance_ = null;
        public static MassRateGradientPerLengthQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MassRateGradientPerLengthQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "kilogram per second metre",
                UnitLabel = "kg/(s*m)",
                ID = new Guid("dead3854-9f7f-488b-a6bf-6ce64fa54219"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "kilogram per minute metre",
                UnitLabel = "kg/(min*m)",
                ID = new Guid("dc6e06ea-52ce-46f3-9733-6e0ac54a8706"),
                ConversionFactorFromSIFormula = "Factors.Minute",
            },
            new UnitChoice
            {
                UnitName = "pound per minute foot",
                UnitLabel = "lb/(min*ft)",
                ID = new Guid("05ee15e6-5ef3-403c-82f8-bae268834aa9"),
                ConversionFactorFromSIFormula = "Factors.Minute*Factors.Foot/Factors.Pound",
            }
        };

        public MassRateGradientPerLengthQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "mass rate gradient per length" };
            ID = new Guid("05891b6e-c096-4901-b941-1c23697153d9");
            DescriptionMD = "**mass rate gradient per length** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is kilogram per second metre with unit label $\\frac{kg}{s\\cdot m}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
