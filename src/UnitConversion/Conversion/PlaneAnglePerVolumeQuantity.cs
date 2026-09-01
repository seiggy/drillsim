using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents plane angle per volume.
    /// </summary>
    public partial class PlaneAnglePerVolumeQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "theta/V";
        public override string SIUnitName { get; } = "radian per cubic metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{rad}{m^3}";
        public override double PlaneAngleDimension { get; } = 1;
        public override double LengthDimension { get; } = -3;

        private static PlaneAnglePerVolumeQuantity instance_ = null;
        public static PlaneAnglePerVolumeQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new PlaneAnglePerVolumeQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "radian per cubic metre",
                UnitLabel = "rad/m^3",
                ID = new Guid("20daaf82-ae4d-4591-90d1-d6b199b802e6"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "degree per litre",
                UnitLabel = "deg/L",
                ID = new Guid("ff2a7ea1-102f-4197-b4fe-987dc7317abe"),
                ConversionFactorFromSIFormula = "Factors.Litre*Factors.Degree",
            }
        };

        public PlaneAnglePerVolumeQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "plane angle per volume" };
            ID = new Guid("79958c90-85a4-463e-88be-4e328d86907e");
            DescriptionMD = "**plane angle per volume** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is radian per cubic metre with unit label $\\frac{rad}{m^3}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
