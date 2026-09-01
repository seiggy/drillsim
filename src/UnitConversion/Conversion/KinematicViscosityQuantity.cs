using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents kinematic viscosity.
    /// </summary>
    public partial class KinematicViscosityQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "nu";
        public override string SIUnitName { get; } = "square metre per second";
        public override string SIUnitLabelLatex { get; } = "\\frac{m^2}{s}";
        public override double TimeDimension { get; } = -1;
        public override double LengthDimension { get; } = 2;

        private static KinematicViscosityQuantity instance_ = null;
        public static KinematicViscosityQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new KinematicViscosityQuantity();
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
                ID = new Guid("3565c99f-6367-4c5b-abca-2f7c08533eba"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "stokes",
                UnitLabel = "St",
                ID = new Guid("cdec3bb5-8d0b-4ffc-910f-2cdc987f1b4c"),
                ConversionFactorFromSIFormula = "1.0/(Factors.Centi*Factors.Centi)",
            },
            new UnitChoice
            {
                UnitName = "centistokes",
                UnitLabel = "cSt",
                ID = new Guid("37a10a66-c11f-4254-b939-a770aca4ec47"),
                ConversionFactorFromSIFormula = "1.0/Factors.Micro",
            }
        };

        public KinematicViscosityQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "kinematic viscosity" };
            ID = new Guid("19c3e97f-6b3f-43b4-9201-5a91db440ae2");
            DescriptionMD = "**kinematic viscosity** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is square metre per second with unit label $\\frac{m^2}{s}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
