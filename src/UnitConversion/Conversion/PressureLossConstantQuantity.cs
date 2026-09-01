using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class PressureLossConstantQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "pressure loss constant SI";
        public override string SIUnitLabelLatex { get; } = "(\\frac{m^{3}}{s})^{2} \\cdot \\frac{(\\frac{kg}{m^{3}})}{Pa}";
        public override double LengthDimension { get; } = 4;
        private static PressureLossConstantQuantity instance_ = null;

        public static PressureLossConstantQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new PressureLossConstantQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
                {
                    UnitName = "pressure loss constant SI",
                    UnitLabel = "(m³/s)²•(kg/m³)/Pa",
                    ID = new Guid("e0b334c4-2e44-4b1b-891f-9deae86a4d17"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                    IsSI = true
                },
                new UnitChoice
                {
                    UnitName = "pressure loss constant metric",
                    UnitLabel = "(l/min)²•sg/bar",
                    ID = new Guid("043fbd34-1e4f-45bc-9935-b1797b606fd6"),
                    ConversionFactorFromSIFormula = "Factors.Minute*Factors.Minute*Factors.Bar*Factors.SpecificGavity4degC/(Factors.Milli*Factors.Milli)" 
                },
                new UnitChoice
                {
                    UnitName = "pressure loss constant UK",
                    UnitLabel = "gpmuk²•ppguk/psi",
                    ID = new Guid("d5a97f2d-cb2f-449f-8f60-0ad292a01b87"),
                    ConversionFactorFromSIFormula = "Factors.Minute*Factors.Minute*Factors.PSI/(Factors.PPGUK*Factors.GallonUK*Factors.GallonUK)" 
                },
                new UnitChoice
                {
                    UnitName = "pressure loss constant US",
                    UnitLabel = "gpmus²•ppgus/psi",
                    ID = new Guid("b5cb21d1-0e71-4ab2-8d9d-42de21753edc"),
                    ConversionFactorFromSIFormula = "Factors.Minute*Factors.Minute*Factors.PSI/(Factors.PPGUS*Factors.GallonUS*Factors.GallonUS)" 
                }
        };
        public PressureLossConstantQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "pressure loss constant" };
            ID = new Guid("6417f6e0-969d-43f2-bee6-249199ec1697");
            DescriptionMD = string.Empty;
            DescriptionMD += @"The pressure loss constant is a parameter used to quantify the resistance to flow in a system, such as a pipe or more complex tubulars, which leads to a reduction in pressure. It helps in calculating the pressure drop due to friction or other factors in fluid dynamics." + Environment.NewLine;
            DescriptionMD += @"The dimension of pressure loss constant is:" + Environment.NewLine;
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
