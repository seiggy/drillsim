using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class ElectricResistivityQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "ohm metre";
        public override string SIUnitLabelLatex { get; } = "\\Omega \\cdot m";
        public override double LengthDimension { get; } = 3;
        public override double MassDimension { get; } = 1;
        public override double TimeDimension { get; } = -3;
        public override double ElectricCurrentDimension { get; } = -2;
        private static ElectricResistivityQuantity instance_ = null;

        public static ElectricResistivityQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ElectricResistivityQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
                new UnitChoice
                {
                  UnitName = "ohm metre",
                  UnitLabel = "Ω•m",
                  ID = new Guid("fb07d86d-d69f-46ca-892c-17ec45adffcb"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                  IsSI = true
                },
                new UnitChoice
                {
                  UnitName = "kilo ohm metre",
                  UnitLabel = "kΩ•m",
                  ID = new Guid("c58ce3f0-7389-4c36-b291-55fa5ceb9962"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Kilo"
                },
                new UnitChoice
                {
                  UnitName = "mega ohm metre",
                  UnitLabel = "MΩ•m",
                  ID = new Guid("cf90cab7-e973-469a-9727-08bfa7f708e6"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Mega"
                },
                new UnitChoice
                {
                  UnitName = "giga ohm metre",
                  UnitLabel = "GΩ•m",
                  ID = new Guid("eecfdf24-7a8e-4783-a627-d4387831767d"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Giga"
                }
        };
        public ElectricResistivityQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "electric resistivity" };
            ID = new Guid("c6c87a27-c04d-4658-8a71-1e46eb3bfd80");
            DescriptionMD = string.Empty;
            DescriptionMD += @"Electric resistivity is a material's inherent property that measures how strongly it resists the flow of electric current." + Environment.NewLine;
            DescriptionMD += @"The dimension of resistivity is:" + Environment.NewLine;
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
