using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class ElectricCurrentQuantity : SymbolizedBasePhysicalQuantity
    {
        public override string DimensionSymbol { get; } = "I";
        public override string SIUnitName { get; } = "ampere";
        public override string SIUnitLabelLatex { get; } = "A";
        private static ElectricCurrentQuantity instance_ = null;
        public override double ElectricCurrentDimension { get; } = 1;

        public static ElectricCurrentQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ElectricCurrentQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
                {
                    UnitName = "ampere",
                    UnitLabel = "A",
                    ID = new Guid("1cd2ef0e-baf8-43fb-9fac-64f84560d0a9"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                    IsSI = true
                },
                new UnitChoice
                {
                    UnitName = "coulomb per second",
                    UnitLabel = "C/s",
                    ID = new Guid("0a9cc349-3bac-4f44-9a9b-3940ae595f03"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Unit"
                },
                new UnitChoice
                {
                    UnitName = "siemens volt",
                    UnitLabel = "S•V",
                    ID = new Guid("52bc6bd9-f25d-470b-9068-b6e87f1f2ed0"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Unit"
                },
                new UnitChoice
                {
                    UnitName = "volt per ohm",
                    UnitLabel = "V/Ω",
                    ID = new Guid("100dd38e-19ad-465c-a995-0fb1174e506b"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Unit"
                },
                new UnitChoice
                {
                    UnitName = "watt per volt",
                    UnitLabel = "W/V",
                    ID = new Guid("29464509-67a2-4062-b78a-8156e54cfa88"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Unit"
                },
                new UnitChoice
                {
                    UnitName = "weber per henry",
                    UnitLabel = "Wb/H",
                    ID = new Guid("bf3285f5-34be-4592-822d-f6ffc3ce4858"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Unit"
                },
                new UnitChoice
                {
                    UnitName = "deciampere",
                    UnitLabel = "dA",
                    ID = new Guid("fdca0a23-f088-4a93-8bfa-4776d19dc26e"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Deci"
                },
                new UnitChoice
                {
                    UnitName = "centiampere",
                    UnitLabel = "cA",
                    ID = new Guid("f057be23-0f56-4a5f-bb39-3ec032b66ff8"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Centi"
                },
                new UnitChoice
                {
                    UnitName = "milliampere",
                    UnitLabel = "mA",
                    ID = new Guid("a2b3179e-3003-48eb-82aa-80fbfb2cfe39"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "microampere",
                    UnitLabel = "µA",
                    ID = new Guid("fb4dfa70-2011-468a-9c4a-06aba3754fc2"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Micro"
                },
                new UnitChoice
                {
                    UnitName = "nanoampere",
                    UnitLabel = "nA",
                    ID = new Guid("f5f75652-d393-4328-87f0-6132fd8dc786"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Nano"
                },
                new UnitChoice
                {
                    UnitName = "picoampere",
                    UnitLabel = "pA",
                    ID = new Guid("9c5c1ea2-89bc-48f8-83ab-fbde7b1f3721"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Pico"
                },
                new UnitChoice
                {
                    UnitName = "biot",
                    UnitLabel = "Bi",
                    ID = new Guid("4648ec96-2c82-4aa2-8de3-d6eb105f470e"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Deca"
                },
                 new UnitChoice
                {
                    UnitName = "abampere",
                    UnitLabel = "abA",
                    ID = new Guid("d589a05d-d6a4-4d2d-9ec7-3a02d0de2ef0"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Deca"
                },
               new UnitChoice
                {
                    UnitName = "kiloampere",
                    UnitLabel = "kA",
                    ID = new Guid("5a500f57-a5d1-41c2-9b8d-b08757420bb8"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Kilo"
                },
                new UnitChoice
                {
                    UnitName = "megaampere",
                    UnitLabel = "MA",
                    ID = new Guid("978af2aa-e776-43d6-bfe6-4055b6d602e8"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Mega"
                },
                new UnitChoice
                {
                    UnitName = "gigaampere",
                    UnitLabel = "GA",
                    ID = new Guid("eb1b76cd-4863-4cf3-b421-1cd80d2fb0b4"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Giga"
                },
                new UnitChoice
                {
                    UnitName = "teraampere",
                    UnitLabel = "TA",
                    ID = new Guid("4bf45d0a-e177-45b9-8a40-f8f51d5b15c6"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Tera"
                },
                new UnitChoice
                {
                    UnitName = "statampere",
                    UnitLabel = "stA",
                    ID = new Guid("7d9a22ac-62d8-476d-8429-bc41febbe707"),
                    ConversionFactorFromSIFormula = "Factors.C_cgs/10.0"
                }
        };

        public ElectricCurrentQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "electric current" };
            ID = new Guid("a322deae-e965-41bf-b4fe-a7530d33c9a0");
            DescriptionMD = string.Empty;
            DescriptionMD += @"Electric current is the flow of electric charge through a conductor or circuit." + Environment.NewLine;
            DescriptionMD += @"The dimension of electric current is:" + Environment.NewLine;
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
