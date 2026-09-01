using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents volume per pressure rate of change.
    /// </summary>
    public partial class VolumePerPressureRateOfChangeQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "d(V/p)/dt";
        public override string SIUnitName { get; } = "cubic metre per pascal second";
        public override string SIUnitLabelLatex { get; } = "\\frac{m^3}{Pa\\cdot s}";
        public override double LengthDimension { get; } = 4;
        public override double TimeDimension { get; } = 1;
        public override double MassDimension { get; } = -1;

        private static VolumePerPressureRateOfChangeQuantity instance_ = null;
        public static VolumePerPressureRateOfChangeQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new VolumePerPressureRateOfChangeQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "cubic metre per pascal second",
                UnitLabel = "m^3/(Pa*s)",
                ID = new Guid("7c3ec170-776e-4d3e-9c99-28e2ca6d10ba"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "litre per bar minute",
                UnitLabel = "L/(bar*min)",
                ID = new Guid("bca9770f-64db-490f-b9d1-f4caa16e7732"),
                ConversionFactorFromSIFormula = "Factors.Bar*Factors.Minute/Factors.Litre",
            }
        };

        public VolumePerPressureRateOfChangeQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "volume per pressure rate of change" };
            ID = new Guid("ecb46cc0-4de0-4ca4-88b9-d1051dea3579");
            DescriptionMD = "**volume per pressure rate of change** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is cubic metre per pascal second with unit label $\\frac{m^3}{Pa\\cdot s}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
