using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class PressureGradientPerLengthQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "pascal per metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{Pa}{m}";
        public override double LengthDimension { get; } = -2;
        public override double MassDimension { get; } = 1;
        public override double TimeDimension { get; } = -2;
        private static PressureGradientPerLengthQuantity instance_ = null;

        public static PressureGradientPerLengthQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new PressureGradientPerLengthQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
                new UnitChoice
                {
                    UnitName = "pascal per metre",
                    UnitLabel = "Pa/m",
                    ID = new Guid("f5a37831-4a70-44de-af34-4f6ce1a54af3"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                    IsSI = true
                },
                new UnitChoice
                {
                    UnitName = "bar per metre",
                    UnitLabel = "bar/m",
                    ID = new Guid("73a70891-87cf-44fc-8437-94938f034eec"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Bar"
                },
                new UnitChoice
                {
                    UnitName = "psi per metre",
                    UnitLabel = "psi/m",
                    ID = new Guid("2235a51b-cdf2-4f53-9664-b7a968dbbba3"),
                    ConversionFactorFromSIFormula = "1.0/Factors.PSI"
                },
                new UnitChoice
                {
                    UnitName = "psi per foot",
                    UnitLabel = "psi/ft",
                    ID = new Guid("b99cef5c-d6df-4803-b52b-6050cf7e7ff8"),
                    ConversionFactorFromSIFormula = "Factors.Foot/Factors.PSI"
                }
        };
        public PressureGradientPerLengthQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "pressure gradient per length" };
            ID = new Guid("62eb6afe-bd7d-48dd-b4fd-de40e9f3c632");
            DescriptionMD = string.Empty;
            DescriptionMD += @"A pressure gradient per length is the first derivative of a pressure compared to a distance: $\frac{dp}{ds}$, where $p$ is a pressure and $s$ is a distance." + Environment.NewLine;
            DescriptionMD += @"The dimension of pressure gradient per length is:" + Environment.NewLine;
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