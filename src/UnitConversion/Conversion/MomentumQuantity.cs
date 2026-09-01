using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents momentum.
    /// </summary>
    public partial class MomentumQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "p";
        public override string SIUnitName { get; } = "kilogram metre per second";
        public override string SIUnitLabelLatex { get; } = "\\frac{kg\\cdot m}{s}";
        public override double LengthDimension { get; } = 1;
        public override double TimeDimension { get; } = -1;
        public override double MassDimension { get; } = 1;

        private static MomentumQuantity instance_ = null;
        public static MomentumQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MomentumQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "kilogram metre per second",
                UnitLabel = "kg*m/s",
                ID = new Guid("68a56004-c3ef-47ff-8cfb-ddf8a18c57de"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "newton second",
                UnitLabel = "N*s",
                ID = new Guid("7f7dced7-0b3a-4125-9920-a9ac4e3cf219"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
            },
            new UnitChoice
            {
                UnitName = "pound foot per second",
                UnitLabel = "lb*ft/s",
                ID = new Guid("dd698492-45cc-45fc-9f9c-498daad59460"),
                ConversionFactorFromSIFormula = "1.0/(Factors.Pound*Factors.Foot)",
            }
        };

        public MomentumQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "momentum", "linear momentum" };
            ID = new Guid("cc3d8ab9-5131-43a7-84e5-52a553d2bf38");
            DescriptionMD = "**momentum** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is kilogram metre per second with unit label $\\frac{kg\\cdot m}{s}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
