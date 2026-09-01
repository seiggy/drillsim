using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents length per volume.
    /// </summary>
    public partial class LengthPerVolumeQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "L/V";
        public override string SIUnitName { get; } = "metre per cubic metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{m}{m^3}";
        public override double LengthDimension { get; } = -2;

        private static LengthPerVolumeQuantity instance_ = null;
        public static LengthPerVolumeQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new LengthPerVolumeQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "metre per cubic metre",
                UnitLabel = "m/m^3",
                ID = new Guid("c64123b3-159a-494c-82d4-83c502d54742"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "foot per cubic foot",
                UnitLabel = "ft/ft^3",
                ID = new Guid("af270ecd-b5a6-4ef5-b2b9-073a951b15a0"),
                ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot",
            }
        };

        public LengthPerVolumeQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "length per volume" };
            ID = new Guid("7cf7f819-1c5a-4cff-a003-7cf6f5d2d9c7");
            DescriptionMD = "**length per volume** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is metre per cubic metre with unit label $\\frac{m}{m^3}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
