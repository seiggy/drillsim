using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents area rate of change.
    /// </summary>
    public partial class AreaRateOfChangeQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "dA/dt";
        public override string SIUnitName { get; } = "square metre per second";
        public override string SIUnitLabelLatex { get; } = "\\frac{m^2}{s}";
        public override double TimeDimension { get; } = -1;
        public override double LengthDimension { get; } = 2;

        private static AreaRateOfChangeQuantity instance_ = null;
        public static AreaRateOfChangeQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new AreaRateOfChangeQuantity();
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
                ID = new Guid("3b0a761b-3bb1-4b64-9540-2e5425ed8920"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "square metre per minute",
                UnitLabel = "m^2/min",
                ID = new Guid("937e0918-c2aa-445d-8a3b-179464b9ded2"),
                ConversionFactorFromSIFormula = "Factors.Minute",
            },
            new UnitChoice
            {
                UnitName = "square foot per second",
                UnitLabel = "ft^2/s",
                ID = new Guid("efaeef1f-5dfe-4b28-99b4-d7ff104ffb3e"),
                ConversionFactorFromSIFormula = "1.0/(Factors.Foot*Factors.Foot)",
            }
        };

        public AreaRateOfChangeQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "area rate of change" };
            ID = new Guid("922903d3-499c-4fc9-9d0e-da30abed1eff");
            DescriptionMD = "**area rate of change** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is square metre per second with unit label $\\frac{m^2}{s}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
