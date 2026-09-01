using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents volume gradient per length.
    /// </summary>
    public partial class VolumeGradientPerLengthQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "dV/dL";
        public override string SIUnitName { get; } = "cubic metre per metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{m^3}{m}";
        public override double LengthDimension { get; } = 2;

        private static VolumeGradientPerLengthQuantity instance_ = null;
        public static VolumeGradientPerLengthQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new VolumeGradientPerLengthQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "cubic metre per metre",
                UnitLabel = "m^3/m",
                ID = new Guid("64db95fe-8310-4a99-a668-d68bf90b14ed"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "litre per metre",
                UnitLabel = "L/m",
                ID = new Guid("4bd8be1c-05ce-4d4a-b642-c429db1d11e5"),
                ConversionFactorFromSIFormula = "1.0/Factors.Litre",
            },
            new UnitChoice
            {
                UnitName = "cubic foot per foot",
                UnitLabel = "ft^3/ft",
                ID = new Guid("11aedb9f-ac7a-4dbf-8668-f17f48725d1c"),
                ConversionFactorFromSIFormula = "1.0/(Factors.Foot*Factors.Foot)",
            }
        };

        public VolumeGradientPerLengthQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "volume gradient per length" };
            ID = new Guid("a9efb630-0e03-4047-a777-f2277cc66d5b");
            DescriptionMD = "**volume gradient per length** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is " + SIUnitName + " with unit label $" + SIUnitLabelLatex + "$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
