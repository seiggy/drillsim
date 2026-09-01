using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents dose equivalent.
    /// </summary>
    public partial class DoseEquivalentQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "H";
        public override string SIUnitName { get; } = "sievert";
        public override string SIUnitLabelLatex { get; } = "Sv";
        public override double TimeDimension { get; } = -2;
        public override double LengthDimension { get; } = 2;
        public override double? MeaningfulPrecisionInSI { get; } = 1E-06;

        private static DoseEquivalentQuantity instance_ = null;
        public static DoseEquivalentQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new DoseEquivalentQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "sievert",
                UnitLabel = "Sv",
                ID = new Guid("d4bcea8d-2603-4574-9f54-a4ca8b4b7c3c"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "millisievert",
                UnitLabel = "mSv",
                ID = new Guid("b5d1391d-d278-4577-8420-8329e7a06ac6"),
                ConversionFactorFromSIFormula = "1.0/Factors.Milli",
            },
            new UnitChoice
            {
                UnitName = "microsievert",
                UnitLabel = "uSv",
                ID = new Guid("7636abf6-7b68-4ae7-ad4a-a2550260f2c4"),
                ConversionFactorFromSIFormula = "1.0/Factors.Micro",
            }
        };

        public DoseEquivalentQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "dose equivalent", "equivalent dose" };
            ID = new Guid("b681bb4a-4350-4a9c-8cf5-344226311185");
            DescriptionMD = "**dose equivalent** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is sievert with unit label $Sv$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
