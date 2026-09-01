using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.UnitConversion.Conversion
{
    public partial class BendingMomentQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "newton metre";
        public override string SIUnitLabelLatex { get; } = "N \\cdot m";
        public override double LengthDimension { get; } = 2;
        public override double MassDimension { get; } = 1;
        public override double TimeDimension { get; } = -2;
        private static BendingMomentQuantity instance_ = null;

        public static BendingMomentQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new BendingMomentQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
               new UnitChoice
                {
                  UnitName = "newton metre",
                  UnitLabel = "N•m",
                  ID = new Guid("bd43cd1d-23db-4137-8f87-f00c686b1609"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                  IsSI = true
                },
                new UnitChoice
                {
                  UnitName = "decanewton metre",
                  UnitLabel = "daN•m",
                  ID = new Guid("e63f5c69-880e-4ed2-985d-a6c1316a8ff5"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Deca)"
                },
                new UnitChoice
                {
                  UnitName = "kilogram force metre",
                  UnitLabel = "kgf•m",
                  ID = new Guid("ac7a836f-daf8-4373-b926-dde5b65ef005"),
                  ConversionFactorFromSIFormula = "1.0/ Factors.KilogramForce"
                },
                new UnitChoice
                {
                  UnitName = "kilonewton metre",
                  UnitLabel = "kN•m",
                  ID = new Guid("62a7ba7e-e9e4-4ad4-8faa-3e80ffd79c76"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Kilo"
                },
                new UnitChoice
                {
                  UnitName = "foot pound",
                  UnitLabel = "ft•lbf",
                  ID = new Guid("ac12e954-14db-4a0d-8d40-7196b5a819b5"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Foot*Factors.PoundForce)"
                },
                new UnitChoice
                {
                  UnitName = "kilofoot pound",
                  UnitLabel = "kft•lbf",
                  ID = new Guid("fd9c2e34-fcd9-4fb0-9b2f-995dc2d15467"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Kilo*Factors.Foot*Factors.PoundForce)"
                },
                new UnitChoice
                {
                  UnitName = "newton decimetre",
                  UnitLabel = "N•dm",
                  ID = new Guid("6da5c57f-34ae-4b7f-9511-cf08812e6092"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Deci"
                },
                new UnitChoice
                {
                  UnitName = "newton centimetre",
                  UnitLabel = "N•cm",
                  ID = new Guid("bf09633d-103f-4ca2-8200-b27f6200df01"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Centi"
                },
                new UnitChoice
                {
                  UnitName = "newton millimetre",
                  UnitLabel = "N•mm",
                  ID = new Guid("a31d03fe-2397-40d0-a647-8e81faa4298f"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Milli"
                },
                new UnitChoice
                {
                  UnitName = "inch pound",
                  UnitLabel = "in•lbf",
                  ID = new Guid("cfbb3f04-27e9-42b8-ae7d-074ced09831e"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.PoundForce*Factors.Inch)"
                }
        };
        public BendingMomentQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "bending moment" };
            ID = new Guid("82b03224-e2af-47e9-bcf5-810d2506f4e2");
            DescriptionMD = string.Empty;
            DescriptionMD += @"A bending moment is the internal mechanical moment at a given cross-section of a structure that causes it to bend, " + Environment.NewLine;
            DescriptionMD += @"equal to the resultant of forces acting at a distance from that section and producing curvature rather than rotation." + Environment.NewLine;
            DescriptionMD += @"The dimension of bending moment is:" + Environment.NewLine;
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
