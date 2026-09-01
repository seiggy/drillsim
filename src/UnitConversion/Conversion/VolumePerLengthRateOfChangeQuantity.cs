using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents volume per length rate of change.
    /// </summary>
    public partial class VolumePerLengthRateOfChangeQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "d(V/L)/dt";
        public override string SIUnitName { get; } = "cubic metre per metre second";
        public override string SIUnitLabelLatex { get; } = "\\frac{m^3}{m\\cdot s}";
        public override double TimeDimension { get; } = -1;
        public override double LengthDimension { get; } = 2;

        private static VolumePerLengthRateOfChangeQuantity instance_ = null;
        public static VolumePerLengthRateOfChangeQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new VolumePerLengthRateOfChangeQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "cubic metre per metre second",
                UnitLabel = "m^3/(m*s)",
                ID = new Guid("1aef9de4-f583-4d25-b00c-0c7a06cd92d9"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "litre per metre minute",
                UnitLabel = "L/(m*min)",
                ID = new Guid("8a2f6b3d-2613-4c4f-8ec6-c32b84f24033"),
                ConversionFactorFromSIFormula = "Factors.Minute/Factors.Litre",
            }
        };

        public VolumePerLengthRateOfChangeQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "volume per length rate of change" };
            ID = new Guid("da543737-1db8-44c1-bfa6-785095ac4a67");
            DescriptionMD = "**volume per length rate of change** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is " + SIUnitName + " with unit label $" + SIUnitLabelLatex + "$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
