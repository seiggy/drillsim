using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class MassGradientPerLengthQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "kilogram per metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{kg}{m}";
        public override double LengthDimension { get; } = -1;
        public override double MassDimension { get; } = 1;
        private static MassGradientPerLengthQuantity instance_ = null;

        public static MassGradientPerLengthQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MassGradientPerLengthQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
                new UnitChoice
                {
                    UnitName = "kilogram per metre",
                    UnitLabel = "kg/m",
                    ID = new Guid("c8b4a6ea-29bf-4e5a-b7ce-9142cefc0752"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                    IsSI = true
                },
                new UnitChoice
                {
                    UnitName = "pound per foot",
                    UnitLabel = "lb/ft",
                    ID = new Guid("6fdf4cb6-a43b-482d-9bc8-d4ad49770f9e"),
                    ConversionFactorFromSIFormula = "Factors.Foot/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "gram per metre",
                    UnitLabel = "g/m",
                    ID = new Guid("0cea1e32-9adb-4882-9070-d027cd0eef8e"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Milli"
                }
        };
        public MassGradientPerLengthQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "mass gradient per length" };
            ID = new Guid("b8694592-8f8d-4684-b0ba-c88de50c8486");
            DescriptionMD = string.Empty;
            DescriptionMD += @"A mass gradient per length is the first derivative of a mass compared to a distance: $\frac{dm}{ds}$, where $m$ is a mass and $s$ is a distance." + Environment.NewLine;
            DescriptionMD += @"The dimension of mass gradient per length is:" + Environment.NewLine;
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
