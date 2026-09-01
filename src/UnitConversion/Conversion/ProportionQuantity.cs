using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class ProportionQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "proportion";
        public override string SIUnitLabelLatex { get; } = "";
        private static ProportionQuantity instance_ = null;

        public static ProportionQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ProportionQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
        new UnitChoice
        {
          UnitName = "proportion",
          UnitLabel = "",
          ID = new Guid("03eb339b-61aa-4b42-aa35-4a20c547fdb9"),
          ConversionFactorFromSIFormula = "1.0/Factors.Unit",
          IsSI = true
        },
        new UnitChoice
        {
          UnitName = "percent",
          UnitLabel = "%",
          ID = new Guid("1a825e84-bc53-4da8-a089-118fdf40b8f7"),
          ConversionFactorFromSIFormula = "1.0/Factors.Centi"
        },
        new UnitChoice
        {
          UnitName = "per thousand",
          UnitLabel = "‰",
          ID = new Guid("141465a2-9c3c-4dda-82ec-eb35e72250c2"),
          ConversionFactorFromSIFormula = "1.0/Factors.Milli"
        },
        new UnitChoice
        {
          UnitName = "part per million",
          UnitLabel = "ppm",
          ID = new Guid("af33bf27-c3b8-4746-8b08-826ed1d21792"),
          ConversionFactorFromSIFormula = "1.0/Factors.Micro"
        }
        };
        public ProportionQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "proportion" };
            ID = new Guid("10d2d588-19b8-4822-9240-e1d278d99e32");
            DescriptionMD = string.Empty;
            DescriptionMD += @"A proportion is the ratio of two quantities with the same dimension. It represents how one quantity compares to another in relative terms." + Environment.NewLine;
            DescriptionMD += @"A proportion is dimensionless:" + Environment.NewLine;
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
