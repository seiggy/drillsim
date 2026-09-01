using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.UnitConversion.Conversion
{
    public partial class PressureRateOfChangeQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "p";
        public override string SIUnitName { get; } = "pascal per second";
        public override string SIUnitLabelLatex { get; } = "\\frac{Pa}{s}";
        public override double MassDimension { get; } = 1;
        public override double LengthDimension { get; } = -1;
        public override double TimeDimension { get; } = -3;

        private static PressureRateOfChangeQuantity instance_ = null;

        public static PressureRateOfChangeQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new PressureRateOfChangeQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
                new UnitChoice
                {
                  UnitName = "pascal per second",
                  UnitLabel = "Pa/s",
                  ID = new Guid("146c6da5-9de1-4c41-b6dd-9a7757b14ebf"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                  IsSI = true
                },
                new UnitChoice
                {
                  UnitName = "kilopascal per second",
                  UnitLabel = "KPa/s",
                  ID = new Guid("92faa8ae-3f6f-4bd3-97ef-19709f9b7a43"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Kilo"
                },
                new UnitChoice
                {
                  UnitName = "bar per second",
                  UnitLabel = "bar/s",
                  ID = new Guid("3bd0765c-d3ca-45e9-9818-d70dbd225fdc"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Bar"
                },
                 new UnitChoice
                {
                  UnitName = "millibar per second",
                  UnitLabel = "mbar/s",
                  ID = new Guid("ba3427d8-c516-40c8-8d4f-fdfe162414e3"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Milli*Factors.Bar)"
                },
                 new UnitChoice
                {
                  UnitName = "microbar per second",
                  UnitLabel = "µbar/s",
                  ID = new Guid("65fb7735-63d5-42da-a730-1bdb4bd7f96a"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Micro*Factors.Bar)"
                },
               new UnitChoice
                {
                  UnitName = "pound per square inch per second",
                  UnitLabel = "psi/s",
                  ID = new Guid("6c065cb9-edcc-4093-a81e-dcba0711ab0c"),
                  ConversionFactorFromSIFormula = "1.0/Factors.PSI"
                },
                new UnitChoice
                {
                  UnitName = "pound per 100 square foot per second",
                  UnitLabel = "lbf/100ft²/s",
                  ID = new Guid("c2f71235-2332-42ae-83a3-be1aeea10488"),
                  ConversionFactorFromSIFormula = "100.0*Factors.Foot*Factors.Foot/Factors.PoundForce"
                },
                 new UnitChoice
                {
                  UnitName = "kilopound per square inch per second",
                  UnitLabel = "ksi/s",
                  ID = new Guid("90dd9c87-07f1-4f62-9098-029f78343309"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Kilo*Factors.PSI)"
                 },
                 new UnitChoice
                {
                  UnitName = "standard atmosphere per second",
                  UnitLabel = "atm/s",
                  ID = new Guid("3d6bbda4-a133-4bd4-bf37-afd2c56f5b02"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Atmosphere"
                 },
                 new UnitChoice
                {
                  UnitName = "pound per square foot per second",
                  UnitLabel = "lb/ft²/s",
                  ID = new Guid("c25ea4df-6b1b-4cef-8ece-7565c8ae6739"),
                  ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot/Factors.PoundForce"
                },
                new UnitChoice
                {
                  UnitName = "megapascal per second",
                  UnitLabel = "MPa/s",
                  ID = new Guid("364ded63-6e4f-4d9b-ac57-d3bff57cc36a"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Mega"
                },
                new UnitChoice
                {
                  UnitName = "gigapascal per second",
                  UnitLabel = "GPa/s",
                  ID = new Guid("04567188-9a65-4289-ac76-2b346401ef39"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Giga"
                },
                new UnitChoice
                {
                  UnitName = "newton per square metre per second",
                  UnitLabel = "N/m²/s",
                  ID = new Guid("3efead2c-a99a-4efd-a534-d8221d3dbad4"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Unit"
                },
                  new UnitChoice
                {
                  UnitName = "newton per square centimetre per second",
                  UnitLabel = "N/cm²/s",
                  ID = new Guid("db0bcb17-a1bd-4c68-b4ff-528194f9b766"),
                  ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi"
                },
                 new UnitChoice
                {
                  UnitName = "newton per square millimetre per second",
                  UnitLabel = "N/mm²/s",
                  ID = new Guid("78c99986-523b-4052-b995-64ada11779a0"),
                  ConversionFactorFromSIFormula = "Factors.Milli*Factors.Milli"
                },
                new UnitChoice
                {
                  UnitName = "kilonewton per square metre per second",
                  UnitLabel = "kN/m²/s",
                  ID = new Guid("012d7c45-41a8-45b3-9c40-3ab8333ed624"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Kilo"
                },
                new UnitChoice
                {
                  UnitName = "megapound per square inch per second",
                  UnitLabel = "Mpsi/s",
                  ID = new Guid("cfd1514e-2707-4df5-b963-50390d1e2298"),
                  ConversionFactorFromSIFormula = "Factors.Inch*Factors.Inch/(Factors.Mega*Factors.PoundForce)"
                },
                new UnitChoice
                {
                  UnitName = "torr per second",
                  UnitLabel = "Torr/s",
                  ID = new Guid("334a2e72-7dae-4904-ac5f-5e98dba8f191"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Torr"
                },
                new UnitChoice
                {
                  UnitName = "centimetre mercury at zero degree celsius per second",
                  UnitLabel = "cm Hg 0°C/s",
                  ID = new Guid("29f8c1be-1148-41ed-ad7c-eb7d6fe12800"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Deca*Factors.MillimetreMercury)"
                },
                new UnitChoice
                {
                  UnitName = "millimetre mercury at zero degree celsius per second",
                  UnitLabel = "mm Hg 0°C/s",
                  ID = new Guid("8b0a6a79-0751-4aea-baa8-f685b36b5226"),
                  ConversionFactorFromSIFormula = "1.0/Factors.MillimetreMercury"
                },
                new UnitChoice
                {
                  UnitName = "inch mercury at 32 degree fahrenheit per second",
                  UnitLabel = "in Hg 32°F/s",
                  ID = new Guid("ef059b25-5fdd-481d-bdf2-3785c012b082"),
                  ConversionFactorFromSIFormula = "1.0/Factors.InchMercury32degF"
                },
                new UnitChoice
                {
                  UnitName = "inch mercury at 60 degree fahrenheit per second",
                  UnitLabel = "in Hg 60°F/s",
                  ID = new Guid("23dd84dd-b90a-442e-ad91-a1458fac47f7"),
                  ConversionFactorFromSIFormula = "1.0/Factors.InchMercury60degF"
                },
                new UnitChoice
                {
                  UnitName = "centimetre water at 4 degree celsius per second",
                  UnitLabel = "cm Aq 4°C/s",
                  ID = new Guid("fafbcb83-8425-4e04-8bf6-cb58c64bdcd7"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Deca*Factors.MillimetreWater4degC)"
                },
                new UnitChoice
                {
                  UnitName = "millimetre water at 4 degree celsius per second",
                  UnitLabel = "mm Aq 4°C/s",
                  ID = new Guid("402ce428-d47a-493b-9b90-e3230a79da96"),
                  ConversionFactorFromSIFormula = "1.0/Factors.MillimetreWater4degC"
                },
                new UnitChoice
                {
                  UnitName = "inch water at 4 degree celsius per second",
                  UnitLabel = "in Aq 4°C/s",
                  ID = new Guid("11d5031a-06d5-4950-b877-cae03aff2669"),
                  ConversionFactorFromSIFormula = "1.0/Factors.InchWater4degC"
                },
                new UnitChoice
                {
                  UnitName = "foot water at 4 degree celsius per second",
                  UnitLabel = "ft Aq 4°C/s",
                  ID = new Guid("6c248d57-b406-4b85-b52d-1ccb5fca3c30"),
                  ConversionFactorFromSIFormula = "1.0/Factors.FootWater4degC"
                },
                new UnitChoice
                {
                  UnitName = "dyne per square centimetre per second",
                  UnitLabel = "dyne/cm²/s",
                  ID = new Guid("e003cc76-81e3-4e8e-8e80-aa03ccaec0b5"),
                  ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi/Factors.Dyne"
                },

                 new UnitChoice
                {
                  UnitName = "pascal per minute",
                  UnitLabel = "Pa/min",
                  ID = new Guid("e598bc6c-1858-448e-b6c2-dbefdfe517a7"),
                  ConversionFactorFromSIFormula = "Factors.Minute/Factors.Unit"
                },
                new UnitChoice
                {
                  UnitName = "kilopascal per minute",
                  UnitLabel = "KPa/min",
                  ID = new Guid("1bd828ef-e6a8-4c6b-954e-83d076d81b5b"),
                  ConversionFactorFromSIFormula = "Factors.Minute/Factors.Kilo"
                },
                new UnitChoice
                {
                  UnitName = "bar per minute",
                  UnitLabel = "bar/min",
                  ID = new Guid("d5064ac5-0f02-437a-8d93-004ca9301b88"),
                  ConversionFactorFromSIFormula = "Factors.Minute/Factors.Bar"
                },
                 new UnitChoice
                {
                  UnitName = "millibar per minute",
                  UnitLabel = "mbar/min",
                  ID = new Guid("76136213-1d78-4ac2-8f78-54dc62b815bc"),
                  ConversionFactorFromSIFormula = "Factors.Minute/(Factors.Milli*Factors.Bar)"
                },
                 new UnitChoice
                {
                  UnitName = "microbar per minute",
                  UnitLabel = "µbar/min",
                  ID = new Guid("ee409d37-a87d-4e7c-a595-d01832e66918"),
                  ConversionFactorFromSIFormula = "Factors.Minute/(Factors.Micro*Factors.Bar)"
                },
               new UnitChoice
                {
                  UnitName = "pound per square inch per minute",
                  UnitLabel = "psi/min",
                  ID = new Guid("34e3dd46-c61b-4109-91f9-704a94e4a827"),
                  ConversionFactorFromSIFormula = "Factors.Minute/Factors.PSI"
                },
                new UnitChoice
                {
                  UnitName = "pound per 100 square foot per minute",
                  UnitLabel = "lbf/100ft²/min",
                  ID = new Guid("9220c284-c1d6-41ed-a0c3-ee8e439cc5e2"),
                  ConversionFactorFromSIFormula = "Factors.Minute*100.0*Factors.Foot*Factors.Foot/Factors.PoundForce"
                },
                 new UnitChoice
                {
                  UnitName = "kilopound per square inch per minute",
                  UnitLabel = "ksi/min",
                  ID = new Guid("fd5479cd-6a86-43ba-bbbf-142068a903ee"),
                  ConversionFactorFromSIFormula = "Factors.Minute/(Factors.Kilo*Factors.PSI)"
                 },
                 new UnitChoice
                {
                  UnitName = "standard atmosphere per minute",
                  UnitLabel = "atm/min",
                  ID = new Guid("cfdf265f-5e6f-4b1a-9169-a4a93a232821"),
                  ConversionFactorFromSIFormula = "Factors.Minute/Factors.Atmosphere"
                 },
                 new UnitChoice
                {
                  UnitName = "pound per square foot per minute",
                  UnitLabel = "lb/ft²/min",
                  ID = new Guid("d4aa7c92-885b-4c7e-977b-4ef9908b25a8"),
                  ConversionFactorFromSIFormula = "Factors.Minute*Factors.Foot*Factors.Foot/Factors.PoundForce"
                },
                new UnitChoice
                {
                  UnitName = "megapascal per minute",
                  UnitLabel = "MPa/min",
                  ID = new Guid("a1442a08-69f3-461e-82c5-e53e0322b266"),
                  ConversionFactorFromSIFormula = "Factors.Minute/Factors.Mega"
                },
                new UnitChoice
                {
                  UnitName = "gigapascal per minute",
                  UnitLabel = "GPa/min",
                  ID = new Guid("882534c4-b00b-45e2-a26b-04f9f683a7e6"),
                  ConversionFactorFromSIFormula = "Factors.Minute/Factors.Giga"
                },
                new UnitChoice
                {
                  UnitName = "newton per square metre per minute",
                  UnitLabel = "N/m²/min",
                  ID = new Guid("5dce7d3a-f8b8-4d08-a9d3-1f91b84829b9"),
                  ConversionFactorFromSIFormula = "Factors.Minute/Factors.Unit"
                },
                  new UnitChoice
                {
                  UnitName = "newton per square centimetre per minute",
                  UnitLabel = "N/cm²/min",
                  ID = new Guid("70d9df4b-a360-4b89-b433-cd2d9d0e9fe0"),
                  ConversionFactorFromSIFormula = "Factors.Minute*Factors.Centi*Factors.Centi"
                },
                 new UnitChoice
                {
                  UnitName = "newton per square millimetre per minute",
                  UnitLabel = "N/mm²/min",
                  ID = new Guid("3d587520-1609-4292-8f10-45758b59230d"),
                  ConversionFactorFromSIFormula = "Factors.Minute*Factors.Milli*Factors.Milli"
                },
                new UnitChoice
                {
                  UnitName = "kilonewton per square metre per minute",
                  UnitLabel = "kN/m²/min",
                  ID = new Guid("30466bc7-6978-4c96-a2cf-2158955dbfe7"),
                  ConversionFactorFromSIFormula = "Factors.Minute/Factors.Kilo"
                },
                new UnitChoice
                {
                  UnitName = "megapound per square inch per minute",
                  UnitLabel = "Mpsi/min",
                  ID = new Guid("8ad28da4-f7e5-4dbb-9f70-b06157686aae"),
                  ConversionFactorFromSIFormula = "Factors.Minute*Factors.Inch*Factors.Inch/(Factors.Mega*Factors.PoundForce)"
                },
                new UnitChoice
                {
                  UnitName = "torr per minute",
                  UnitLabel = "Torr/min",
                  ID = new Guid("73b7ed13-4545-4d90-b4a5-83e2f9c8ebb7"),
                  ConversionFactorFromSIFormula = "Factors.Minute/Factors.Torr"
                },
                new UnitChoice
                {
                  UnitName = "centimetre mercury at zero degree celsius per minute",
                  UnitLabel = "cm Hg 0°C/min",
                  ID = new Guid("08b7f12a-ef89-4a13-8b60-dd7cba0f586f"),
                  ConversionFactorFromSIFormula = "Factors.Minute/(Factors.Deca*Factors.MillimetreMercury)"
                },
                new UnitChoice
                {
                  UnitName = "millimetre mercury at zero degree celsius per minute",
                  UnitLabel = "mm Hg 0°C/min",
                  ID = new Guid("fe387842-ca9e-4c35-860a-d377745f6aea"),
                  ConversionFactorFromSIFormula = "Factors.Minute/Factors.MillimetreMercury"
                },
                new UnitChoice
                {
                  UnitName = "inch mercury at 32 degree fahrenheit per minute",
                  UnitLabel = "in Hg 32°F/min",
                  ID = new Guid("6638070e-912d-42d7-b8b3-e4caafc2bb33"),
                  ConversionFactorFromSIFormula = "Factors.Minute/Factors.InchMercury32degF"
                },
                new UnitChoice
                {
                  UnitName = "inch mercury at 60 degree fahrenheit per minute",
                  UnitLabel = "in Hg 60°F/min",
                  ID = new Guid("a84bc253-3a56-494c-80da-d62fe5a3e617"),
                  ConversionFactorFromSIFormula = "Factors.Minute/Factors.InchMercury60degF"
                },
                new UnitChoice
                {
                  UnitName = "centimetre water at 4 degree celsius per minute",
                  UnitLabel = "cm Aq 4°C/min",
                  ID = new Guid("2a18d804-e172-4b17-9f3f-becb8ebbac5f"),
                  ConversionFactorFromSIFormula = "Factors.Minute/(Factors.Deca*Factors.MillimetreWater4degC)"
                },
                new UnitChoice
                {
                  UnitName = "millimetre water at 4 degree celsius per minute",
                  UnitLabel = "mm Aq 4°C/min",
                  ID = new Guid("125bcf03-f190-41bd-a95d-e7bded0bc97e"),
                  ConversionFactorFromSIFormula = "Factors.Minute/Factors.MillimetreWater4degC"
                },
                new UnitChoice
                {
                  UnitName = "inch water at 4 degree celsius per minute",
                  UnitLabel = "in Aq 4°C/min",
                  ID = new Guid("57071e63-13ea-48d2-bfc2-fc82e6cca335"),
                  ConversionFactorFromSIFormula = "Factors.Minute/Factors.InchWater4degC"
                },
                new UnitChoice
                {
                  UnitName = "foot water at 4 degree celsius per minute",
                  UnitLabel = "ft Aq 4°C/min",
                  ID = new Guid("8a06ee9b-f646-4c3a-b087-309a0bd3f844"),
                  ConversionFactorFromSIFormula = "Factors.Minute/Factors.FootWater4degC"
                },
                new UnitChoice
                {
                  UnitName = "dyne per square centimetre per minute",
                  UnitLabel = "dyne/cm²/min",
                  ID = new Guid("ae854308-d419-452a-ba65-9f094ab0c2b5"),
                  ConversionFactorFromSIFormula = "Factors.Minute*Factors.Centi*Factors.Centi/Factors.Dyne"
                },

                new UnitChoice
                {
                  UnitName = "pascal per hour",
                  UnitLabel = "Pa/h",
                  ID = new Guid("bc8c071a-2c0a-4617-90c1-af5e00ec7e94"),
                  ConversionFactorFromSIFormula = "Factors.Hour/Factors.Unit"
                },
                new UnitChoice
                {
                  UnitName = "kilopascal per hour",
                  UnitLabel = "KPa/h",
                  ID = new Guid("618f23ff-90af-416c-8d1a-a90bc421a6de"),
                  ConversionFactorFromSIFormula = "Factors.Hour/Factors.Kilo"
                },
                new UnitChoice
                {
                  UnitName = "bar per hour",
                  UnitLabel = "bar/h",
                  ID = new Guid("387befbe-ad5a-46b5-a1a1-b55bebf96a12"),
                  ConversionFactorFromSIFormula = "Factors.Hour/Factors.Bar"
                },
                 new UnitChoice
                {
                  UnitName = "millibar per hour",
                  UnitLabel = "mbar/h",
                  ID = new Guid("c8b4a1d2-d4aa-4fb7-8b64-fd73b462d924"),
                  ConversionFactorFromSIFormula = "Factors.Hour/(Factors.Milli*Factors.Bar)"
                },
                 new UnitChoice
                {
                  UnitName = "microbar per hour",
                  UnitLabel = "µbar/h",
                  ID = new Guid("1c6a1561-2293-4e37-b6f7-cc01bf3f8e71"),
                  ConversionFactorFromSIFormula = "Factors.Hour/(Factors.Micro*Factors.Bar)"
                },
               new UnitChoice
                {
                  UnitName = "pound per square inch per hour",
                  UnitLabel = "psi/h",
                  ID = new Guid("94ce97e6-e2aa-4240-bb5f-e713abe880ad"),
                  ConversionFactorFromSIFormula = "Factors.Hour/Factors.PSI"
                },
                new UnitChoice
                {
                  UnitName = "pound per 100 square foot per hour",
                  UnitLabel = "lbf/100ft²/h",
                  ID = new Guid("5900f7db-2994-4492-8779-89f54294aaa7"),
                  ConversionFactorFromSIFormula = "Factors.Hour*100.0*Factors.Foot*Factors.Foot/Factors.PoundForce"
                },
                 new UnitChoice
                {
                  UnitName = "kilopound per square inch per hour",
                  UnitLabel = "ksi/h",
                  ID = new Guid("837e7f8f-9e02-4438-9425-9d4aca0c227a"),
                  ConversionFactorFromSIFormula = "Factors.Hour/(Factors.Kilo*Factors.PSI)"
                 },
                 new UnitChoice
                {
                  UnitName = "standard atmosphere per hour",
                  UnitLabel = "atm/h",
                  ID = new Guid("673a6598-498d-4e22-8cd5-972d0d7a52ac"),
                  ConversionFactorFromSIFormula = "Factors.Hour/Factors.Atmosphere"
                 },
                 new UnitChoice
                {
                  UnitName = "pound per square foot per hour",
                  UnitLabel = "lb/ft²/h",
                  ID = new Guid("b7938985-f701-4245-9050-303a9e6d5c9f"),
                  ConversionFactorFromSIFormula = "Factors.Hour*Factors.Foot*Factors.Foot/Factors.PoundForce"
                },
                new UnitChoice
                {
                  UnitName = "megapascal per hour",
                  UnitLabel = "MPa/h",
                  ID = new Guid("a7ed3518-a18f-4edf-b739-0b65961a3b60"),
                  ConversionFactorFromSIFormula = "Factors.Hour/Factors.Mega"
                },
                new UnitChoice
                {
                  UnitName = "gigapascal per hour",
                  UnitLabel = "GPa/h",
                  ID = new Guid("9f24b377-6f28-4c2c-8b64-c7ef2f6b7499"),
                  ConversionFactorFromSIFormula = "Factors.Hour/Factors.Giga"
                },
                new UnitChoice
                {
                  UnitName = "newton per square metre per hour",
                  UnitLabel = "N/m²/h",
                  ID = new Guid("bd39b1da-4c58-4e56-8a3c-e364bce4b38f"),
                  ConversionFactorFromSIFormula = "Factors.Hour/Factors.Unit"
                },
                  new UnitChoice
                {
                  UnitName = "newton per square centimetre per hour",
                  UnitLabel = "N/cm²/h",
                  ID = new Guid("aa3f0b26-d6f3-4feb-bffa-fa48a4a3a1a7"),
                  ConversionFactorFromSIFormula = "Factors.Hour*Factors.Centi*Factors.Centi"
                },
                 new UnitChoice
                {
                  UnitName = "newton per square millimetre per hour",
                  UnitLabel = "N/mm²/h",
                  ID = new Guid("378b7b8b-2d47-4a00-a817-ef9805b09169"),
                  ConversionFactorFromSIFormula = "Factors.Hour*Factors.Milli*Factors.Milli"
                },
                new UnitChoice
                {
                  UnitName = "kilonewton per square metre per hour",
                  UnitLabel = "kN/m²/h",
                  ID = new Guid("42e9c18f-37d4-44b1-8c1d-86f3c524bf8b"),
                  ConversionFactorFromSIFormula = "Factors.Hour/Factors.Kilo"
                },
                new UnitChoice
                {
                  UnitName = "megapound per square inch per hour",
                  UnitLabel = "Mpsi/h",
                  ID = new Guid("f4455f10-dc18-45b6-984f-7fd86a21d26f"),
                  ConversionFactorFromSIFormula = "Factors.Hour*Factors.Inch*Factors.Inch/(Factors.Mega*Factors.PoundForce)"
                },
                new UnitChoice
                {
                  UnitName = "torr per hour",
                  UnitLabel = "Torr/h",
                  ID = new Guid("4e1b41a0-f7cb-4ff2-b691-3a19dbc1d668"),
                  ConversionFactorFromSIFormula = "Factors.Hour/Factors.Torr"
                },
                new UnitChoice
                {
                  UnitName = "centimetre mercury at zero degree celsius per hour",
                  UnitLabel = "cm Hg 0°C/h",
                  ID = new Guid("ce403ddc-7b41-4abf-aea5-78ea2323595b"),
                  ConversionFactorFromSIFormula = "Factors.Hour/(Factors.Deca*Factors.MillimetreMercury)"
                },
                new UnitChoice
                {
                  UnitName = "millimetre mercury at zero degree celsius per hour",
                  UnitLabel = "mm Hg 0°C/h",
                  ID = new Guid("dc27fed5-7e21-4b87-9498-51598af273da"),
                  ConversionFactorFromSIFormula = "Factors.Hour/Factors.MillimetreMercury"
                },
                new UnitChoice
                {
                  UnitName = "inch mercury at 32 degree fahrenheit per hour",
                  UnitLabel = "in Hg 32°F/h",
                  ID = new Guid("eb10ca92-df7f-4ef2-8c95-ac30a3d1a068"),
                  ConversionFactorFromSIFormula = "Factors.Hour/Factors.InchMercury32degF"
                },
                new UnitChoice
                {
                  UnitName = "inch mercury at 60 degree fahrenheit per hour",
                  UnitLabel = "in Hg 60°F/h",
                  ID = new Guid("9f30715a-000c-4de8-8b75-206e9bf87713"),
                  ConversionFactorFromSIFormula = "Factors.Hour/Factors.InchMercury60degF"
                },
                new UnitChoice
                {
                  UnitName = "centimetre water at 4 degree celsius per hour",
                  UnitLabel = "cm Aq 4°C/h",
                  ID = new Guid("ed435708-418d-4a26-9b4c-e0f94ee63509"),
                  ConversionFactorFromSIFormula = "Factors.Hour/(Factors.Deca*Factors.MillimetreWater4degC)"
                },
                new UnitChoice
                {
                  UnitName = "millimetre water at 4 degree celsius per hour",
                  UnitLabel = "mm Aq 4°C/h",
                  ID = new Guid("3dafbaa0-5907-4bf0-8808-b5bc0ec2bf6c"),
                  ConversionFactorFromSIFormula = "Factors.Hour/Factors.MillimetreWater4degC"
                },
                new UnitChoice
                {
                  UnitName = "inch water at 4 degree celsius per hour",
                  UnitLabel = "in Aq 4°C/h",
                  ID = new Guid("084c8268-17a3-4d56-890e-aba0809772bc"),
                  ConversionFactorFromSIFormula = "Factors.Hour/Factors.InchWater4degC"
                },
                new UnitChoice
                {
                  UnitName = "foot water at 4 degree celsius per hour",
                  UnitLabel = "ft Aq 4°C/h",
                  ID = new Guid("2a3ed612-1097-46ad-9b91-63d81c14b943"),
                  ConversionFactorFromSIFormula = "Factors.Hour/Factors.FootWater4degC"
                },
                new UnitChoice
                {
                  UnitName = "dyne per square centimetre per hour",
                  UnitLabel = "dyne/cm²/h",
                  ID = new Guid("0493cff3-1e05-4ea5-8f3d-477f506b13f9"),
                  ConversionFactorFromSIFormula = "Factors.Hour*Factors.Centi*Factors.Centi/Factors.Dyne"
                }
        };
        public PressureRateOfChangeQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "pressure rate of change" };
            ID = new Guid("611830b0-739e-42ef-8215-98e0a4e1df3b");
            DescriptionMD = string.Empty;
            DescriptionMD += @"A pressure rate of change is the time derivative of a pressure." + Environment.NewLine;
            DescriptionMD += @"The dimension of pressure rate of change is:" + Environment.NewLine;
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
