using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents light exposure.
    /// </summary>
    public partial class LightExposureQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "H_v";
        public override string SIUnitName { get; } = "lux second";
        public override string SIUnitLabelLatex { get; } = "lx\\cdot s";
        public override double SolidAngleDimension { get; } = 1;
        public override double LengthDimension { get; } = -2;
        public override double TimeDimension { get; } = 1;
        public override double LuminousIntensityDimension { get; } = 1;

        private static LightExposureQuantity instance_ = null;
        public static LightExposureQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new LightExposureQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "lux second",
                UnitLabel = "lx*s",
                ID = new Guid("0c43758f-fcae-4081-81d8-6cca69c752de"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "lux hour",
                UnitLabel = "lx*h",
                ID = new Guid("e7284ffa-74aa-40ff-a1b8-9f8bb879698e"),
                ConversionFactorFromSIFormula = "1.0/Factors.Hour",
            },
            new UnitChoice
            {
                UnitName = "foot candle hour",
                UnitLabel = "fc*h",
                ID = new Guid("7067a899-4fb3-4d12-afa5-a0b217652659"),
                ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot/Factors.Hour",
            }
        };

        public LightExposureQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "light exposure", "luminous exposure" };
            ID = new Guid("14e2d5f4-f868-46eb-8101-8a4867b57b4f");
            DescriptionMD = "**light exposure** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is lux second with unit label $lx\\cdot s$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
