using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>The variation of displaced volume per variation of plane angle.</summary>
    public partial class VolumePerAngleQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "dV/dtheta";
        public override string SIUnitName { get; } = "cubic metre per radian";
        public override string SIUnitLabelLatex { get; } = "\\frac{m^3}{rad}";
        public override double LengthDimension { get; } = 3;
        public override double PlaneAngleDimension { get; } = -1;

        private static VolumePerAngleQuantity instance_ = null;
        public static VolumePerAngleQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new VolumePerAngleQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "cubic metre per radian", UnitLabel = "m^3/rad",
                ID = new Guid("e93fadc5-0e91-42b2-a7ec-faf8a7990c0e"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit", IsSI = true
            },
            new UnitChoice
            {
                UnitName = "cubic metre per revolution", UnitLabel = "m^3/rev",
                ID = new Guid("2cb98e09-e016-48de-a4e8-d778c0f03083"),
                ConversionFactorFromSIFormula = "Factors.Revolution"
            },
            new UnitChoice
            {
                UnitName = "litre per revolution", UnitLabel = "L/rev",
                ID = new Guid("fc1fc32a-3bc6-4d80-be6c-64f21328d428"),
                ConversionFactorFromSIFormula = "Factors.Revolution/Factors.Litre"
            },
            new UnitChoice
            {
                UnitName = "cubic inch per revolution", UnitLabel = "in^3/rev",
                ID = new Guid("c9690395-bcf5-4555-a5b3-d1f3c9adbad0"),
                ConversionFactorFromSIFormula = "Factors.Revolution/(Factors.Inch*Factors.Inch*Factors.Inch)"
            }
        };

        public VolumePerAngleQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "volume per angle", "volume per angle variation", "volume per rotation", "volume per revolution", "volume per cycle", "displacement per revolution" };
            ID = new Guid("b7289689-e15e-40c5-9d8c-7b07b1677881");
            DescriptionMD = "**Volume per angle** is the variation of displaced volume per variation of plane angle." + Environment.NewLine;
            DescriptionMD += "Its physical dimension is " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The coherent SI unit is cubic metre per radian; per-revolution units are practical alternatives." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
