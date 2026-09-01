using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents luminous exitance.
    /// </summary>
    public partial class LuminousExitanceQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "M_v";
        public override string SIUnitName { get; } = "lumen per square metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{lm}{m^2}";
        public override double SolidAngleDimension { get; } = 1;
        public override double LengthDimension { get; } = -2;
        public override double LuminousIntensityDimension { get; } = 1;

        private static LuminousExitanceQuantity instance_ = null;
        public static LuminousExitanceQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new LuminousExitanceQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "lumen per square metre",
                UnitLabel = "lm/m^2",
                ID = new Guid("eb2f91a6-23d6-41fc-939f-83612ba39adf"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "lumen per square foot",
                UnitLabel = "lm/ft^2",
                ID = new Guid("95b07233-6478-4b6a-ba51-1ecbdba2a52d"),
                ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot",
            }
        };

        public LuminousExitanceQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "luminous exitance" };
            ID = new Guid("bf6a07d5-0f5f-4397-b537-4748c13d373a");
            DescriptionMD = "**luminous exitance** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is lumen per square metre with unit label $\\frac{lm}{m^2}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
