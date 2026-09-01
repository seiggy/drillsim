using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class PressureQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "p";
        public override string SIUnitName { get; } = "pascal";
        public override string SIUnitLabelLatex { get; } = "Pa";
        public override double MassDimension { get; } = 1;
        public override double LengthDimension { get; } = -1;
        public override double TimeDimension { get; } = -2;

        private static PressureQuantity instance_ = null;

        public static PressureQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new PressureQuantity();
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
                  ID = new Guid("4f8ebaf4-cd1b-4714-a609-0a9fbe44cafb"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                  IsSI = true
                },
                new UnitChoice
                {
                  UnitName = "kilopascal",
                  UnitLabel = "KPa",
                  ID = new Guid("a41c04f5-198b-4a04-b90a-5700412a2a29"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Kilo"
                },
                new UnitChoice
                {
                  UnitName = "bar",
                  UnitLabel = "bar",
                  ID = new Guid("0d182739-f8f6-47a6-afcb-71feac973307"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Bar"
                },
                 new UnitChoice
                {
                  UnitName = "millibar",
                  UnitLabel = "mbar",
                  ID = new Guid("43e4fe86-948d-4765-a69d-513ce6dc2b5b"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Milli*Factors.Bar)"
                },
                 new UnitChoice
                {
                  UnitName = "microbar",
                  UnitLabel = "µbar",
                  ID = new Guid("7fb9e41f-4748-4457-b8b9-efb73da52d94"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Micro*Factors.Bar)"
                },
               new UnitChoice
                {
                  UnitName = "pound per square inch",
                  UnitLabel = "psi",
                  ID = new Guid("afce482e-a8cf-47f8-85c1-22595d5b5485"),
                  ConversionFactorFromSIFormula = "1.0/Factors.PSI"
                },
                new UnitChoice
                {
                  UnitName = "pound per 100 square foot",
                  UnitLabel = "lbf/100ft²",
                  ID = new Guid("e3b95821-d782-4f12-a492-489cbcd6d2a1"),
                  ConversionFactorFromSIFormula = "100.0*Factors.Foot*Factors.Foot/Factors.PoundForce"
                },
                 new UnitChoice
                {
                  UnitName = "kilopound per square inch",
                  UnitLabel = "ksi",
                  ID = new Guid("a07b5fe5-87e3-4422-afe1-f54de24deeb8"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Kilo*Factors.PSI)"
                 },
                 new UnitChoice
                {
                  UnitName = "standard atmosphere",
                  UnitLabel = "atm",
                  ID = new Guid("93839971-33f2-43e9-82eb-9f869846f999"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Atmosphere"
                 },
                 new UnitChoice
                {
                  UnitName = "pound per square foot",
                  UnitLabel = "lb/ft²",
                  ID = new Guid("35b28889-c076-4274-b200-cf7732b17aa3"),
                  ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot/Factors.PoundForce"
                },
                new UnitChoice
                {
                  UnitName = "megapascal",
                  UnitLabel = "MPa",
                  ID = new Guid("4ef28797-f416-4d97-b36a-711ea848bcc0"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Mega"
                },
                new UnitChoice
                {
                  UnitName = "gigapascal",
                  UnitLabel = "GPa",
                  ID = new Guid("5c81fb9b-36ad-47b7-9a8e-c999f7fdbfe3"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Giga"
                },
                new UnitChoice
                {
                  UnitName = "newton per square metre",
                  UnitLabel = "N/m²",
                  ID = new Guid("101e92c3-47ab-4d55-8982-93061bc82dea"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Unit"
                },
                  new UnitChoice
                {
                  UnitName = "newton per square centimetre",
                  UnitLabel = "N/cm²",
                  ID = new Guid("2aa59deb-84d9-41c5-969f-8c8bb9d0c369"),
                  ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi"
                },
                 new UnitChoice
                {
                  UnitName = "newton per square millimetre",
                  UnitLabel = "N/mm²",
                  ID = new Guid("e5e9cb06-38a8-4ac2-a8a5-8b74689a31a8"),
                  ConversionFactorFromSIFormula = "Factors.Milli*Factors.Milli"
                },
                new UnitChoice
                {
                  UnitName = "kilonewton per square metre",
                  UnitLabel = "kN/m²",
                  ID = new Guid("eaa46677-af1c-4922-bf61-d82f2925534b"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Kilo"
                },
                new UnitChoice
                {
                  UnitName = "megapound per square inch",
                  UnitLabel = "Mpsi",
                  ID = new Guid("bb49de0a-fbf3-4914-b6b9-fc60ab502522"),
                  ConversionFactorFromSIFormula = "Factors.Inch*Factors.Inch/(Factors.Mega*Factors.PoundForce)"
                },
                new UnitChoice
                {
                  UnitName = "torr",
                  UnitLabel = "Torr",
                  ID = new Guid("f5afdfee-624e-46fa-b798-0ab1b04d2181"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Torr"
                },
                new UnitChoice
                {
                  UnitName = "centimetre mercury at zero degree celsius",
                  UnitLabel = "cm Hg 0°C",
                  ID = new Guid("412602dc-837b-4fab-afc9-3bf4798a9bed"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Deca*Factors.MillimetreMercury)"
                },
                new UnitChoice
                {
                  UnitName = "millimetre mercury at zero degree celsius",
                  UnitLabel = "mm Hg 0°C",
                  ID = new Guid("d91f64fe-4df4-4ddd-943c-d985fbd1659b"),
                  ConversionFactorFromSIFormula = "1.0/Factors.MillimetreMercury"
                },
                new UnitChoice
                {
                  UnitName = "inch mercury at 32 degree fahrenheit",
                  UnitLabel = "in Hg 32°F",
                  ID = new Guid("ab729585-0716-4f24-9502-fcd07ba051bc"),
                  ConversionFactorFromSIFormula = "1.0/Factors.InchMercury32degF"
                },
                new UnitChoice
                {
                  UnitName = "inch mercury at 60 degree fahrenheit",
                  UnitLabel = "in Hg 60°F",
                  ID = new Guid("83ed97cc-526c-41cc-be78-ea0c86412080"),
                  ConversionFactorFromSIFormula = "1.0/Factors.InchMercury60degF"
                },
                new UnitChoice
                {
                  UnitName = "centimetre water at 4 degree celsius",
                  UnitLabel = "cm Aq 4°C",
                  ID = new Guid("a1bac4cc-f37c-4aa5-aec6-ede0b4c52f09"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Deca*Factors.MillimetreWater4degC)"
                },
                new UnitChoice
                {
                  UnitName = "millimetre water at 4 degree celsius",
                  UnitLabel = "mm Aq 4°C",
                  ID = new Guid("a46b3ef6-fe2a-4ff3-bc2d-7a26661ce45e"),
                  ConversionFactorFromSIFormula = "1.0/Factors.MillimetreWater4degC"
                },
                new UnitChoice
                {
                  UnitName = "inch water at 4 degree celsius",
                  UnitLabel = "in Aq 4°C",
                  ID = new Guid("3015f436-b35d-455c-af23-b9bc4dd857da"),
                  ConversionFactorFromSIFormula = "1.0/Factors.InchWater4degC"
                },
                new UnitChoice
                {
                  UnitName = "foot water at 4 degree celsius",
                  UnitLabel = "ft Aq 4°C",
                  ID = new Guid("52de6721-dfec-4a54-861c-e74da72c8470"),
                  ConversionFactorFromSIFormula = "1.0/Factors.FootWater4degC"
                },
                new UnitChoice
                {
                  UnitName = "dyne per square centimetre",
                  UnitLabel = "dyne/cm²",
                  ID = new Guid("04ca59b8-90e1-4903-ac82-ee95cac0ca38"),
                  ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi/Factors.Dyne"
                }
        };
        public PressureQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "pressure" };
            ID = new Guid("0f282508-9223-489d-86e6-36307f987045");
            DescriptionMD = string.Empty;
            DescriptionMD += @"Pressure is the force applied per unit area on a surface." + Environment.NewLine;
            DescriptionMD += @"The dimension of pressure is:" + Environment.NewLine;
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
