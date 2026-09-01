using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents radioactivity.
    /// </summary>
    public partial class RadioactivityQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "A";
        public override string SIUnitName { get; } = "becquerel";
        public override string SIUnitLabelLatex { get; } = "Bq";
        public override double TimeDimension { get; } = -1;
        public override double? MeaningfulPrecisionInSI { get; } = 1;

        private static RadioactivityQuantity instance_ = null;
        public static RadioactivityQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new RadioactivityQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "becquerel",
                UnitLabel = "Bq",
                ID = new Guid("9ac2ee01-e74f-440c-b927-f165b8eb7a76"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "kilobecquerel",
                UnitLabel = "kBq",
                ID = new Guid("b0f25c9c-26fa-468c-8934-c389c00fc7a0"),
                ConversionFactorFromSIFormula = "1.0/Factors.Kilo",
            },
            new UnitChoice
            {
                UnitName = "megabecquerel",
                UnitLabel = "MBq",
                ID = new Guid("7e873097-2723-44e7-a147-a5781121ed1b"),
                ConversionFactorFromSIFormula = "1.0/Factors.Mega",
            }
        };

        public RadioactivityQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "radioactivity", "activity" };
            ID = new Guid("c1792f77-6c52-44ab-b811-9ceacb6dc5de");
            DescriptionMD = "**radioactivity** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is becquerel with unit label $Bq$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
