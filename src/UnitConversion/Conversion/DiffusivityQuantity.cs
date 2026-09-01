using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents diffusivity.
    /// </summary>
    public partial class DiffusivityQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "D";
        public override string SIUnitName { get; } = "square metre per second";
        public override string SIUnitLabelLatex { get; } = "\\frac{m^2}{s}";
        public override double TimeDimension { get; } = -1;
        public override double LengthDimension { get; } = 2;

        private static DiffusivityQuantity instance_ = null;
        public static DiffusivityQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new DiffusivityQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "square metre per second",
                UnitLabel = "m^2/s",
                ID = new Guid("52e4a981-1639-4f44-81b6-123ceb2ad229"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "square millimetre per second",
                UnitLabel = "mm^2/s",
                ID = new Guid("7f17e101-5abf-4d1a-abc8-0ade006a9e68"),
                ConversionFactorFromSIFormula = "1.0/(Factors.Milli*Factors.Milli)",
            },
            new UnitChoice
            {
                UnitName = "square foot per hour",
                UnitLabel = "ft^2/h",
                ID = new Guid("e2fe8fb4-aff4-411a-8862-59d20290d6a5"),
                ConversionFactorFromSIFormula = "Factors.Hour/(Factors.Foot*Factors.Foot)",
            }
        };

        public DiffusivityQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "diffusivity" };
            ID = new Guid("a88883dc-2db9-4f2d-b34b-61f0f4da910d");
            DescriptionMD = "**diffusivity** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is square metre per second with unit label $\\frac{m^2}{s}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
