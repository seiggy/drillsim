using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents length per angle.
    /// </summary>
    public partial class LengthPerAngleQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "L/theta";
        public override string SIUnitName { get; } = "metre per radian";
        public override string SIUnitLabelLatex { get; } = "\\frac{m}{rad}";
        public override double PlaneAngleDimension { get; } = -1;
        public override double LengthDimension { get; } = 1;

        private static LengthPerAngleQuantity instance_ = null;
        public static LengthPerAngleQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new LengthPerAngleQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "metre per radian",
                UnitLabel = "m/rad",
                ID = new Guid("b694a909-4627-4cbd-ae36-e7e29a239a9c"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "metre per degree",
                UnitLabel = "m/deg",
                ID = new Guid("27ac407b-1b77-43da-8b46-d59e58c9ce60"),
                ConversionFactorFromSIFormula = "1.0/Factors.Degree",
            },
            new UnitChoice
            {
                UnitName = "foot per degree",
                UnitLabel = "ft/deg",
                ID = new Guid("4c88f88a-103d-40ce-a2bb-4e99423f8b81"),
                ConversionFactorFromSIFormula = "1.0/(Factors.Foot*Factors.Degree)",
            }
        };

        public LengthPerAngleQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "length per angle" };
            ID = new Guid("ffea77a1-f294-4c32-8cc2-6d6656071093");
            DescriptionMD = "**length per angle** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is metre per radian with unit label $\\frac{m}{rad}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
