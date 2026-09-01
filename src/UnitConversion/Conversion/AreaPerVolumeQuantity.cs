using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents area per volume.
    /// </summary>
    public partial class AreaPerVolumeQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "A/V";
        public override string SIUnitName { get; } = "square metre per cubic metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{m^2}{m^3}";
        public override double LengthDimension { get; } = -1;

        private static AreaPerVolumeQuantity instance_ = null;
        public static AreaPerVolumeQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new AreaPerVolumeQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "square metre per cubic metre",
                UnitLabel = "m^2/m^3",
                ID = new Guid("da913573-b78c-4862-8404-65f5ee9c41ed"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "square centimetre per cubic centimetre",
                UnitLabel = "cm^2/cm^3",
                ID = new Guid("dde6d485-718e-417e-b460-805953a12c39"),
                ConversionFactorFromSIFormula = "Factors.Centi",
            },
            new UnitChoice
            {
                UnitName = "square foot per cubic foot",
                UnitLabel = "ft^2/ft^3",
                ID = new Guid("244f9a97-e0fe-48ad-9985-6e5ee3b80b1b"),
                ConversionFactorFromSIFormula = "Factors.Foot",
            }
        };

        public AreaPerVolumeQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "area per volume", "specific surface" };
            ID = new Guid("2b07d4f7-057b-44dd-9887-b54de6c73ea8");
            DescriptionMD = "**area per volume** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is square metre per cubic metre with unit label $\\frac{m^2}{m^3}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
