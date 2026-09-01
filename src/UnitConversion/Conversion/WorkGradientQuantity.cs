using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class WorkGradientQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "dW/dl";
        public override string SIUnitName { get; } = "joule per metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{J}{m}";
        public override double LengthDimension { get; } = 1;
        public override double MassDimension { get; } = 1;
        public override double TimeDimension { get; } = -2;

        private static WorkGradientQuantity instance_ = null;
        public static WorkGradientQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new WorkGradientQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new()
        {
            new UnitChoice { UnitName = "joule per metre", UnitLabel = "J/m", ID = new Guid("b7488c2d-e76a-42ee-9da9-2729ad4fc5e7"), ConversionFactorFromSIFormula = "1.0/Factors.Unit", IsSI = true },
            new UnitChoice { UnitName = "kilojoule per metre", UnitLabel = "kJ/m", ID = new Guid("8fd19e40-e368-469d-b779-8283d0af3312"), ConversionFactorFromSIFormula = "1.0/Factors.Kilo" },
            new UnitChoice { UnitName = "joule per foot", UnitLabel = "J/ft", ID = new Guid("1d44f89c-7a70-4a84-82fd-cf7064c2e33f"), ConversionFactorFromSIFormula = "Factors.Foot" },
            new UnitChoice { UnitName = "foot pound force per foot", UnitLabel = "ft•lbf/ft", ID = new Guid("e535e2e9-33d9-4f6c-b62c-fdd21ca855c0"), ConversionFactorFromSIFormula = "1.0/Factors.PoundForce" }
        };

        public WorkGradientQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").First();
            UsualNames = new HashSet<string>() { "work gradient", "energy per length", "WorkGradient" };
            ID = new Guid("670a0017-e2ac-45c5-a6a4-965ab686a950");
            DescriptionMD = "**Work gradient** is the gradient of work with respect to length; the unsimplified unit preserves that meaning." + Environment.NewLine;
            DescriptionMD += "Its coherent SI unit is joule per metre with unit label $" + SIUnitLabelLatex + "$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
