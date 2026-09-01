using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class DimensionlessQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "dimensionless";
        public override string SIUnitLabelLatex { get; } = "";
        private static DimensionlessQuantity instance_ = null;

        public static DimensionlessQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new DimensionlessQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
        {
          UnitName = "dimensionless",
          UnitLabel = "",
          ID = new Guid("8744b0f7-2866-42d8-bf6c-b619ac87b945"),
          ConversionFactorFromSIFormula = "1.0/Factors.Unit",
          IsSI = true
        }
        };
        public DimensionlessQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "dimensionless" };
            ID = new Guid("790ae2cd-170c-4908-b2b9-163ba95f5b43");
            DescriptionMD = string.Empty;
            DescriptionMD += @"As its name indicates, a dimensionless quantity has no dimension:" + Environment.NewLine;
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
