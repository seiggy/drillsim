using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class AreaQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "A";
        public override string SIUnitName { get; } = "square metre";
        public override string SIUnitLabelLatex { get; } = "m^{2}";
        public override double LengthDimension { get; } = 2;
        private static AreaQuantity instance_ = null;

        public static AreaQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new AreaQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
                new UnitChoice
                {
                  UnitName = "square metre",
                  UnitLabel = "m²",
                  ID = new Guid("6225a0d7-d2f1-4bb1-9721-5b260bac26ee"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Unit*Factors.Unit)",
                  IsSI = true
                },
                new UnitChoice
                {
                  UnitName = "square kilometre",
                  UnitLabel = "km²",
                  ID = new Guid("6353513a-6e38-4a58-b20c-d3e8d7b70fb8"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Kilo*Factors.Kilo)"
                },
                new UnitChoice
                {
                  UnitName = "hectare",
                  UnitLabel = "ha",
                  ID = new Guid("14313265-7985-4010-a19a-5ffaebe05092"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Hecto*Factors.Hecto)"
                },
                new UnitChoice
                {
                  UnitName = "square decametre",
                  UnitLabel = "dam²",
                  ID = new Guid("df9417fc-1c08-4c76-a177-e8ea803b2e2f"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Deca*Factors.Deca)"
                },
                new UnitChoice
                {
                  UnitName = "square decimetre",
                  UnitLabel = "dm²",
                  ID = new Guid("125fd8d6-d1eb-4826-a952-5219603409ab"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Deci*Factors.Deci)"
                },
                new UnitChoice
                {
                  UnitName = "square centimetre",
                  UnitLabel = "cm²",
                  ID = new Guid("d74bb2bc-9c86-4be4-bff1-88cac7b1049b"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Centi*Factors.Centi)"
                },
                new UnitChoice
                {
                  UnitName = "square millimetre",
                  UnitLabel = "mm²",
                  ID = new Guid("0b87d221-284a-4e8c-8a60-50c522f9ade4"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Milli*Factors.Milli)"
                },
                new UnitChoice
                {
                  UnitName = "square micrometre",
                  UnitLabel = "µm²",
                  ID = new Guid("bec98c97-72c7-4485-9138-058ed14e7fbe"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Micro*Factors.Micro)"
                },
                new UnitChoice
                {
                  UnitName = "square foot",
                  UnitLabel = "ft²",
                  ID = new Guid("5a59332e-17b3-4fa2-9527-12d06a2b4248"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Foot*Factors.Foot)"
                },
                new UnitChoice
                {
                  UnitName = "square inch",
                  UnitLabel = "in²",
                  ID = new Guid("294bc6d0-5be7-4c70-95f3-ad9dc50f02cf"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Inch*Factors.Inch)"
                },
                new UnitChoice
                {
                  UnitName = "square yard",
                  UnitLabel = "yd²",
                  ID = new Guid("ae3df24c-e5db-4b88-9e81-228f29855f1b"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Yard*Factors.Yard)"
                },
                new UnitChoice
                {
                  UnitName = "acre",
                  UnitLabel = "ac",
                  ID = new Guid("bc94456a-b8b9-49ac-b349-eaded6c984c6"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Acre"
                },
                new UnitChoice
                {
                  UnitName = "square mile",
                  UnitLabel = "mi²",
                  ID = new Guid("5bbe8c59-cce9-47c8-b357-c5a15610af72"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Mile*Factors.Mile)"
                }
        };
        protected AreaQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "area" };
            ID = new Guid("2a892bab-1b39-4ae4-b2d2-989621b09557");
            DescriptionMD = string.Empty;
            DescriptionMD += @"Area is the measure of the extent of a surface or the size of a two-dimensional shape." + Environment.NewLine;
            DescriptionMD += @"The dimension of area is:" + Environment.NewLine;
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
