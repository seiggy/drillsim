using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class StressQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "p";
        public override string SIUnitName { get; } = "pascal";
        public override string SIUnitLabelLatex { get; } = "Pa";
        public override double MassDimension { get; } = 1;
        public override double LengthDimension { get; } = -1;
        public override double TimeDimension { get; } = -2;

        private static StressQuantity instance_ = null;

        public static StressQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new StressQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
                new UnitChoice
                {
                  UnitName = "pascal",
                  UnitLabel = "Pa",
                  ID = new Guid("7a4c7d2e-62f1-43c7-9c9d-8ff8664b0d98"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                  IsSI = true
                },
                new UnitChoice
                {
                  UnitName = "kilopascal",
                  UnitLabel = "KPa",
                  ID = new Guid("8f070021-4cc7-424d-a325-e2e57fc82874"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Kilo"
                },
                new UnitChoice
                {
                  UnitName = "bar",
                  UnitLabel = "bar",
                  ID = new Guid("69864a1c-bb6b-400e-be3b-527bc94a9a96"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Bar"
                },
                 new UnitChoice
                {
                  UnitName = "millibar",
                  UnitLabel = "mbar",
                  ID = new Guid("cf58a57a-381b-4864-9ab3-bbe42589d871"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Milli*Factors.Bar)"
                },
                 new UnitChoice
                {
                  UnitName = "microbar",
                  UnitLabel = "µbar",
                  ID = new Guid("b3ae1880-5d17-4f4b-b837-c6dc13c44cae"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Micro*Factors.Bar)"
                },
               new UnitChoice
                {
                  UnitName = "pound per square inch",
                  UnitLabel = "psi",
                  ID = new Guid("0e385d5b-5d3a-4360-8695-a934f0152a09"),
                  ConversionFactorFromSIFormula = "1.0/Factors.PSI"
                },
                new UnitChoice
                {
                  UnitName = "pound per 100 square foot",
                  UnitLabel = "lbf/100ft²",
                  ID = new Guid("d1aade96-1038-4902-9c4a-95f96933d54d"),
                  ConversionFactorFromSIFormula = "100.0*Factors.Foot*Factors.Foot/Factors.PoundForce"
                },
                 new UnitChoice
                {
                  UnitName = "kilopound per square inch",
                  UnitLabel = "ksi",
                  ID = new Guid("02b3acd6-0715-4ef6-b8e4-6134a3cdc3a6"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Kilo*Factors.PSI)"
                 },
                 new UnitChoice
                {
                  UnitName = "pound per square foot",
                  UnitLabel = "lb/ft²",
                  ID = new Guid("2d835d44-2ffd-4239-b0dd-c9c36a763d4a"),
                  ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot/Factors.PoundForce"
                },
                new UnitChoice
                {
                  UnitName = "megapascal",
                  UnitLabel = "MPa",
                  ID = new Guid("b6de095b-2800-4faf-931b-e8b2b9b2e35f"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Mega"
                },
                new UnitChoice
                {
                  UnitName = "gigapascal",
                  UnitLabel = "GPa",
                  ID = new Guid("213a896e-47e4-4745-baed-c28861f938bb"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Giga"
                },
                new UnitChoice
                {
                  UnitName = "newton per square metre",
                  UnitLabel = "N/m²",
                  ID = new Guid("23d4c68e-a606-4fc0-a2b8-74998f6c2862"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Unit"
                },
                  new UnitChoice
                {
                  UnitName = "newton per square centimetre",
                  UnitLabel = "N/cm²",
                  ID = new Guid("b42eccea-4c35-42af-ba98-5101a3c10b6b"),
                  ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi/Factors.Unit"
                },
                 new UnitChoice
                {
                  UnitName = "newton per square millimetre",
                  UnitLabel = "N/mm²",
                  ID = new Guid("9f96d22c-9021-4ed6-9904-344d6cd2417a"),
                  ConversionFactorFromSIFormula = "Factors.Milli*Factors.Milli/Factors.Unit"
                },
                new UnitChoice
                {
                  UnitName = "kilonewton per square metre",
                  UnitLabel = "kN/m²",
                  ID = new Guid("ff05dc39-74f1-4bc8-b2d4-e9ee518d3e43"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Kilo*Factors.Unit)"
                },
                new UnitChoice
                {
                  UnitName = "megapound per square inch",
                  UnitLabel = "Mpsi",
                  ID = new Guid("c1b8c4db-7a1e-4201-b0aa-e23d1df40871"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Mega*Factors.PSI)"
                },
                new UnitChoice
                {
                  UnitName = "dyne per square centimetre",
                  UnitLabel = "dyne/cm²",
                  ID = new Guid("eee0197b-0fbd-4a21-8023-61403c9417fe"),
                  ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi/Factors.Dyne"
                }
        };
        public StressQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "stress" };
            ID = new Guid("e4aa819b-a385-418b-bbca-cfb1421093f5");
            DescriptionMD = string.Empty;
            DescriptionMD += @"Stress in a material is the internal force per unit area that arises when the material is subjected to external forces or loads. It reflects how much the material is being compressed, stretched, or sheared." + Environment.NewLine;
            DescriptionMD += @"The dimension of stress is:" + Environment.NewLine;
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
