using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class CompressibilityQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "β";
        public override string SIUnitName { get; } = "inverse pascal";
        public override string SIUnitLabelLatex { get; } = "\\frac{1}{Pa}";
        public override double MassDimension { get; } = -1;
        public override double LengthDimension { get; } = 1;
        public override double TimeDimension { get; } = 2;
        private static CompressibilityQuantity instance_ = null;

        public static CompressibilityQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new CompressibilityQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
    {
      UnitName = "inverse pascal",
      UnitLabel = "1/Pa",
      ID = new Guid("0e259998-c8bb-4a4d-b281-afb8008b2693"),
      ConversionFactorFromSIFormula = "1.0/Factors.Unit",
      IsSI = true
    },
    new UnitChoice
    {
      UnitName = "inverse bar",
      UnitLabel = "1/bar",
      ID = new Guid("4a0f6df4-0d2d-489b-80f1-511be7713101"),
      ConversionFactorFromSIFormula = "Factors.Bar"
    },
    new UnitChoice
    {
      UnitName = "inverse pound per square inch",
      UnitLabel = "1/psi",
      ID = new Guid("282ab24c-6c43-4710-8e52-433bdf90cc2e"),
      ConversionFactorFromSIFormula = "Factors.PSI"
    },
        new UnitChoice
    {
      UnitName = "inverse atmosphere",
      UnitLabel = "1/atm",
      ID = new Guid("92c19398-ac0f-41fc-8a22-516c3dc59c82"),
      ConversionFactorFromSIFormula = "Factors.Atmosphere"
    }
        };
        public CompressibilityQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "compressibility" };
            ID = new Guid("1e7af8b8-0267-4d5d-a162-59123a8fde14");
            DescriptionMD = string.Empty;
            DescriptionMD += @"Compressibility is the measure of how much a substance's volume decreases under pressure. It indicates how easily a material or fluid can be compressed and is typically expressed as a change in volume per unit change in pressure." + Environment.NewLine;
            DescriptionMD += @"The dimension of compressibility is:" + Environment.NewLine;
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
