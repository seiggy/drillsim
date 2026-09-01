using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents luminous flux.
    /// </summary>
    public partial class LuminousFluxQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "Phi_v";
        public override string SIUnitName { get; } = "lumen";
        public override string SIUnitLabelLatex { get; } = "lm";
        public override double LuminousIntensityDimension { get; } = 1;
        public override double SolidAngleDimension { get; } = 1;

        private static LuminousFluxQuantity instance_ = null;
        public static LuminousFluxQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new LuminousFluxQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "lumen",
                UnitLabel = "lm",
                ID = new Guid("d8fbbfd8-cb93-4d36-bad7-3cb254f8ab48"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "kilolumen",
                UnitLabel = "klm",
                ID = new Guid("2fc41212-2edb-492e-81ff-a0e04e196ecd"),
                ConversionFactorFromSIFormula = "1.0/Factors.Kilo",
            }
        };

        public LuminousFluxQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "luminous flux" };
            ID = new Guid("e294517a-2b69-46f0-ae14-bb43b3f679bb");
            DescriptionMD = "**luminous flux** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is lumen with unit label $lm$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
