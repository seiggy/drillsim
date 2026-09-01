using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.UnitConversion.Conversion
{
    public partial class MomentOfAreaQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "I";
        public override string SIUnitName { get; } = "metres to the fourth power";
        public override string SIUnitLabelLatex { get; } = "m^{4}";
        public override double LengthDimension { get; } = 4;
        private static MomentOfAreaQuantity instance_ = null;

        public static MomentOfAreaQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MomentOfAreaQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
           new UnitChoice
           {
              UnitName = "metres to the fourth power",
              UnitLabel = "m⁴",
              ID = new Guid("f479bbab-bdd0-4c33-b13c-6dac1d57539b"),
              ConversionFactorFromSIFormula = "1.0/Factors.Unit",
              IsSI = true
           },
           new UnitChoice
           {
              UnitName = "centimetres to the fourth power",
              UnitLabel = "cm⁴",
              ID = new Guid("1e94dc47-7563-4fb0-9749-e4c88523e1f4"),
              ConversionFactorFromSIFormula = "1.0/(Factors.Centi*Factors.Centi*Factors.Centi*Factors.Centi)"
           },
           new UnitChoice
           {
              UnitName = "inches to the fourth power",
              UnitLabel = "in⁴",
              ID = new Guid("86914b8d-6a5d-43ee-ad7c-e69fbf6d5087"),
              ConversionFactorFromSIFormula = "1.0/(Factors.Inch*Factors.Inch*Factors.Inch*Factors.Inch)"
           },
           new UnitChoice
           {
              UnitName = "feet to the fourth power",
              UnitLabel = "ft⁴",
              ID = new Guid("d35362a3-1352-4dcd-b425-c376afcf4d36"),
              ConversionFactorFromSIFormula = "1.0/(Factors.Foot*Factors.Foot*Factors.Foot*Factors.Foot)"
           }
        };

        public MomentOfAreaQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "moment of area", "second moment of area", "area moment of inertia" };
            ID = new Guid("669f44f8-ed5f-43e0-9b63-adf1b6b9e865");
            DescriptionMD = string.Empty;
            DescriptionMD += @"A moment of area is a geometrical property of an area which reflects how its points are distributed with regard to an arbitrary axis. It is used in the study of beam bending and deflection in structural engineering." + Environment.NewLine;
            DescriptionMD += @"The dimension of moment of area is:" + Environment.NewLine;
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
