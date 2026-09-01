using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents time per volume.
    /// </summary>
    public partial class TimePerVolumeQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "t/V";
        public override string SIUnitName { get; } = "second per cubic metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{s}{m^3}";
        public override double TimeDimension { get; } = 1;
        public override double LengthDimension { get; } = -3;

        private static TimePerVolumeQuantity instance_ = null;
        public static TimePerVolumeQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new TimePerVolumeQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "second per cubic metre",
                UnitLabel = "s/m^3",
                ID = new Guid("0639bb1c-8868-46ff-8854-d67c989ad0da"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "minute per litre",
                UnitLabel = "min/L",
                ID = new Guid("25053c0e-5373-46d1-a1de-c785480fdb94"),
                ConversionFactorFromSIFormula = "Factors.Litre/Factors.Minute",
            },
            new UnitChoice
            {
                UnitName = "second per cubic foot",
                UnitLabel = "s/ft^3",
                ID = new Guid("d84599be-f945-4440-b33a-6ce7b59bab96"),
                ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot*Factors.Foot",
            }
        };

        public TimePerVolumeQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "time per volume" };
            ID = new Guid("0c8dc346-29f5-4468-9259-5b93246113e6");
            DescriptionMD = "**time per volume** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is second per cubic metre with unit label $\\frac{s}{m^3}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
