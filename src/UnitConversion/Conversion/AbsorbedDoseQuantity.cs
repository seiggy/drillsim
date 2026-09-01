using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents absorbed dose.
    /// </summary>
    public partial class AbsorbedDoseQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "D";
        public override string SIUnitName { get; } = "gray";
        public override string SIUnitLabelLatex { get; } = "Gy";
        public override double TimeDimension { get; } = -2;
        public override double LengthDimension { get; } = 2;
        public override double? MeaningfulPrecisionInSI { get; } = 1E-06;

        private static AbsorbedDoseQuantity instance_ = null;
        public static AbsorbedDoseQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new AbsorbedDoseQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "gray",
                UnitLabel = "Gy",
                ID = new Guid("4ee9a141-da57-40fd-90c2-f224da907d70"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "milligray",
                UnitLabel = "mGy",
                ID = new Guid("35957208-54e9-407d-b4b8-781ac093036b"),
                ConversionFactorFromSIFormula = "1.0/Factors.Milli",
            },
            new UnitChoice
            {
                UnitName = "rad",
                UnitLabel = "rad",
                ID = new Guid("d367e5fd-3943-4a44-b4a7-c2c4d0eec004"),
                ConversionFactorFromSIFormula = "1.0/Factors.Centi",
            }
        };

        public AbsorbedDoseQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "absorbed dose" };
            ID = new Guid("230dfcc5-efaa-4d09-b1c2-d64557bc59ef");
            DescriptionMD = "**absorbed dose** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is gray with unit label $Gy$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
