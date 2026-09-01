using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents luminous energy.
    /// </summary>
    public partial class LuminousEnergyQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "Q_v";
        public override string SIUnitName { get; } = "lumen second";
        public override string SIUnitLabelLatex { get; } = "lm\\cdot s";
        public override double SolidAngleDimension { get; } = 1;
        public override double TimeDimension { get; } = 1;
        public override double LuminousIntensityDimension { get; } = 1;

        private static LuminousEnergyQuantity instance_ = null;
        public static LuminousEnergyQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new LuminousEnergyQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "lumen second",
                UnitLabel = "lm*s",
                ID = new Guid("d7c4ef1e-1180-4c56-a845-64e9e6dffd26"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "lumen hour",
                UnitLabel = "lm*h",
                ID = new Guid("391bcaf6-3656-4821-bee2-27e92d7f7238"),
                ConversionFactorFromSIFormula = "1.0/Factors.Hour",
            }
        };

        public LuminousEnergyQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "luminous energy" };
            ID = new Guid("669c697c-b808-4873-a177-55c240c73619");
            DescriptionMD = "**luminous energy** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is lumen second with unit label $lm\\cdot s$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
