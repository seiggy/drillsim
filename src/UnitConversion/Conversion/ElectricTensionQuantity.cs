using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class ElectricTensionQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "volt";
        public override string SIUnitLabelLatex { get; } = "V";
        public override double LengthDimension { get; } = 2;
        public override double MassDimension { get; } = 1;
        public override double TimeDimension { get; } = -3;
        public override double ElectricCurrentDimension { get; } = -1;
        public override double? MeaningfulPrecisionInSI { get; } = 0.01;
        private static ElectricTensionQuantity instance_ = null;

        public static ElectricTensionQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ElectricTensionQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
             new UnitChoice
        {
          UnitName = "volt",
          UnitLabel = "V",
          ID = new Guid("618fafff-fc9c-4d22-a64d-b7579931aa93"),
          ConversionFactorFromSIFormula = "1.0/Factors.Unit",
          IsSI = true
        },
        new UnitChoice
        {
          UnitName = "millivolt",
          UnitLabel = "mV",
          ID = new Guid("f186f5d4-2b0b-4cfe-b24c-0c02d3155cf8"),
          ConversionFactorFromSIFormula = "1.0/Factors.Milli"
        },
        new UnitChoice
        {
          UnitName = "centivolt",
          UnitLabel = "cV",
          ID = new Guid("ee01194e-dee3-4b50-8312-5fde3c8f774e"),
          ConversionFactorFromSIFormula = "1.0/Factors.Centi"
        },
        new UnitChoice
        {
          UnitName = "microvolt",
          UnitLabel = "µV",
          ID = new Guid("ede7e093-3e7d-429a-8c22-3b35ab5b20f2"),
          ConversionFactorFromSIFormula = "1.0/Factors.Micro"
        },
        new UnitChoice
        {
          UnitName = "nanovolt",
          UnitLabel = "nV",
          ID = new Guid("86dfcbe1-af8c-4081-b6ed-481eb44ab890"),
          ConversionFactorFromSIFormula = "1.0/Factors.Nano"
        },
        new UnitChoice
        {
          UnitName = "picovolt",
          UnitLabel = "pV",
          ID = new Guid("19fb81d7-4991-4902-a1fd-55420789ac59"),
          ConversionFactorFromSIFormula = "1.0/Factors.Pico"
        },
        new UnitChoice
        {
          UnitName = "kilovolt",
          UnitLabel = "kV",
          ID = new Guid("6ffc60bc-ec9f-44d4-961b-79d9e593bf64"),
          ConversionFactorFromSIFormula = "1.0/Factors.Kilo"
        },
        new UnitChoice
        {
          UnitName = "megavolt",
          UnitLabel = "MV",
          ID = new Guid("3342ddbc-b1b2-46f8-addc-216ce94a616a"),
          ConversionFactorFromSIFormula = "1.0/Factors.Mega"
        },
        new UnitChoice
        {
          UnitName = "gigavolt",
          UnitLabel = "GV",
          ID = new Guid("1ce4342f-d4d2-44b2-81e9-73502c7ca10f"),
          ConversionFactorFromSIFormula = "1.0/Factors.Giga"
        }
        };
        public ElectricTensionQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "electric tension", "electric potential", "potential difference" };
            ID = new Guid("da5094a4-7481-4246-9def-1bd3b6f893a1");
            DescriptionMD = string.Empty;
            DescriptionMD += @"Electric tension is the difference in electric potential between two points.In a static electric field, it corresponds to the work needed per unit of charge to move a positive test charge from the first point to the second point." + Environment.NewLine;
            DescriptionMD += @"The dimension of electric tension is:" + Environment.NewLine;
            DescriptionMD += "$" + GetDimensionsEnclosed() + "$." + Environment.NewLine;
            if (!string.IsNullOrEmpty(SIUnitLabelLatex) && !string.IsNullOrEmpty(SIUnitName) && UsualNames != null && UsualNames.Count > 0)
            {
                DescriptionMD += Environment.NewLine;
                DescriptionMD += @"The SI unit for **" + UsualNames.First() + "** is: " + SIUnitName + " with the associated unit label $" + SIUnitLabelLatex + "$" + Environment.NewLine;
            }
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
