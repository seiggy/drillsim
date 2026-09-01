using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents reciprocal volume.
    /// </summary>
    public partial class ReciprocalVolumeQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "V^-1";
        public override string SIUnitName { get; } = "reciprocal cubic metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{1}{m^3}";
        public override double LengthDimension { get; } = -3;

        private static ReciprocalVolumeQuantity instance_ = null;
        public static ReciprocalVolumeQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ReciprocalVolumeQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "reciprocal cubic metre",
                UnitLabel = "1/m^3",
                ID = new Guid("f0af1387-39ca-4339-8b29-319b13e81fc3"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "reciprocal litre",
                UnitLabel = "1/L",
                ID = new Guid("32c42173-e7dc-43df-9725-703474c86c6e"),
                ConversionFactorFromSIFormula = "Factors.Litre",
            },
            new UnitChoice
            {
                UnitName = "reciprocal cubic foot",
                UnitLabel = "1/ft^3",
                ID = new Guid("1a5bf5ae-ea01-4702-92aa-edc1a645a2b9"),
                ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot*Factors.Foot",
            }
        };

        public ReciprocalVolumeQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "reciprocal volume" };
            ID = new Guid("9967a650-3116-464a-9746-6b2e7abec518");
            DescriptionMD = "**reciprocal volume** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is reciprocal cubic metre with unit label $\\frac{1}{m^3}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
