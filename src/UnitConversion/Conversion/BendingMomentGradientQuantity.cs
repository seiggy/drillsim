using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class BendingMomentGradientQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "dM/dl";
        public override string SIUnitName { get; } = "newton metre per metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{N\\cdot m}{m}";
        public override double LengthDimension { get; } = 1;
        public override double MassDimension { get; } = 1;
        public override double TimeDimension { get; } = -2;

        private static BendingMomentGradientQuantity instance_ = null;
        public static BendingMomentGradientQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new BendingMomentGradientQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new()
        {
            new UnitChoice { UnitName = "newton metre per metre", UnitLabel = "N•m/m", ID = new Guid("8544e5b8-f182-4833-bbb4-4262f4f3c55f"), ConversionFactorFromSIFormula = "1.0/Factors.Unit", IsSI = true },
            new UnitChoice { UnitName = "kilonewton metre per metre", UnitLabel = "kN•m/m", ID = new Guid("807087da-e38c-4a04-97c9-cd8bb3ce1d98"), ConversionFactorFromSIFormula = "1.0/Factors.Kilo" },
            new UnitChoice { UnitName = "foot pound force per foot", UnitLabel = "ft•lbf/ft", ID = new Guid("604de5ca-be77-4f34-8160-7b39303352bc"), ConversionFactorFromSIFormula = "1.0/Factors.PoundForce" },
            new UnitChoice { UnitName = "inch pound force per inch", UnitLabel = "in•lbf/in", ID = new Guid("8036588e-ff60-4176-bc61-4cf91ac271d8"), ConversionFactorFromSIFormula = "1.0/Factors.PoundForce" }
        };

        public BendingMomentGradientQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").First();
            UsualNames = new HashSet<string>() { "bending moment gradient", "moment of force gradient", "BendingMomentGradient" };
            ID = new Guid("5be8f51c-b97c-4a8e-a859-08459bd26f55");
            DescriptionMD = "**Bending moment gradient** is the gradient of bending moment with respect to length; the unsimplified unit preserves that meaning." + Environment.NewLine;
            DescriptionMD += "Its coherent SI unit is newton metre per metre with unit label $" + SIUnitLabelLatex + "$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
