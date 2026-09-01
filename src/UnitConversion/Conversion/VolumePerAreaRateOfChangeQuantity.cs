using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents volume per area rate of change.
    /// </summary>
    public partial class VolumePerAreaRateOfChangeQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "d(V/A)/dt";
        public override string SIUnitName { get; } = "cubic metre per square metre second";
        public override string SIUnitLabelLatex { get; } = "\\frac{m^3}{m^2\\cdot s}";
        public override double TimeDimension { get; } = -1;
        public override double LengthDimension { get; } = 1;

        private static VolumePerAreaRateOfChangeQuantity instance_ = null;
        public static VolumePerAreaRateOfChangeQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new VolumePerAreaRateOfChangeQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "cubic metre per square metre second",
                UnitLabel = "m^3/(m^2*s)",
                ID = new Guid("9b2b7fd7-cc5d-493d-9d84-d75c4a9dee2a"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "litre per square metre minute",
                UnitLabel = "L/(m^2*min)",
                ID = new Guid("b3ef43d1-af6a-4d50-b8bd-900fe293d045"),
                ConversionFactorFromSIFormula = "Factors.Minute/Factors.Litre",
            }
        };

        public VolumePerAreaRateOfChangeQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "volume per area rate of change" };
            ID = new Guid("bae8948e-2a16-4664-b426-fd29f3548c2b");
            DescriptionMD = "**volume per area rate of change** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is " + SIUnitName + " with unit label $" + SIUnitLabelLatex + "$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
