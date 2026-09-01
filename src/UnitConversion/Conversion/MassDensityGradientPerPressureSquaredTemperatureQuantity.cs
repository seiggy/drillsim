using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class MassDensityGradientPerPressureSquaredTemperatureQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "kilogram per cubic metre per pascal squared kelvin";
        public override string SIUnitLabelLatex { get; } = "\\frac{\\frac{kg}{m^{3}}}{Pa^{2} \\cdot K}";
        public override double TimeDimension { get; } = 4;
        public override double MassDimension { get; } = -1;
        public override double LengthDimension { get; } = -1;
        public override double TemperatureDimension { get; } = -1;

        private static MassDensityGradientPerPressureSquaredTemperatureQuantity instance_ = null;

        public static MassDensityGradientPerPressureSquaredTemperatureQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MassDensityGradientPerPressureSquaredTemperatureQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
                {
                    UnitName = "kilogram per cubic metre per pascal squared kelvin",
                    UnitLabel = "kg/m³/(Pa²•K)",
                    ID = new Guid("e00b25c4-cf11-43f8-b87f-e7e482729d18"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                    IsSI = true
                },
                new UnitChoice
                {
                    UnitName = "specific gravity per pascal squared kelvin",
                    UnitLabel = "sg/(Pa²•K)",
                    ID = new Guid("111b012c-3e70-4e3b-8102-5fb0ecd0b1cd"),
                    ConversionFactorFromSIFormula = "Factors.SpecificGavity4degC"
                },
                new UnitChoice
                {
                    UnitName = "gram per cubic centimetre per pascal squared kelvin",
                    UnitLabel = "g/cm³/(Pa²•K)",
                    ID = new Guid("4497271d-cfe7-402e-a20a-7d4aa644d569"),
                    ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi*Factors.Centi/Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (UK) per pascal squared kelvin",
                    UnitLabel = "ppgUK/(Pa²•K)",
                    ID = new Guid("2a2cc741-54d7-4b54-bee7-616e48d9bec6"),
                    ConversionFactorFromSIFormula = "Factors.GallonUK/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (US) per pascal squared kelvin",
                    UnitLabel = "ppgUS/(Pa²•K)",
                    ID = new Guid("f09f13b4-a260-4192-9ca6-3a3b1466f6bf"),
                    ConversionFactorFromSIFormula = "Factors.GallonUS/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic foot per pascal squared kelvin",
                    UnitLabel = "lb/ft³/(Pa²•K)",
                    ID = new Guid("2cbb0b80-789a-4b11-8fa4-62d8f5e7c92f"),
                    ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot*Factors.Foot/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic inch per pascal squared kelvin",
                    UnitLabel = "lb/in³/(Pa²•K)",
                    ID = new Guid("da10a140-ae39-4b34-83b1-223e456a3865"),
                    ConversionFactorFromSIFormula = "Factors.Inch*Factors.Inch*Factors.Inch/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic yard per pascal squared kelvin",
                    UnitLabel = "lb/yd³/(Pa²•K)",
                    ID = new Guid("63e7f7d8-a38b-4a45-937c-8567e97d71a9"),
                    ConversionFactorFromSIFormula = "Factors.Yard*Factors.Yard*Factors.Yard/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "kilogram per cubic metre per bar squared kelvin",
                    UnitLabel = "kg/m³/(bar²•K)",
                    ID = new Guid("cbc5f940-560b-44cf-b652-3f0b953abe5e"),
                    ConversionFactorFromSIFormula = "Factors.Bar*Factors.Bar/Factors.Unit"
                },
                new UnitChoice
                {
                    UnitName = "specific gravity per bar squared kelvin",
                    UnitLabel = "sg/(bar²•K)",
                    ID = new Guid("3e25dd0e-2726-4c26-922b-81c7ec8b416d"),
                    ConversionFactorFromSIFormula = "Factors.SpecificGavity4degC*Factors.Bar*Factors.Bar"
                },
                new UnitChoice
                {
                    UnitName = "gram per cubic centimetre per bar squared kelvin",
                    UnitLabel = "g/cm³/(bar²•K)",
                    ID = new Guid("26b11134-95aa-49f4-8959-7475ca97626b"),
                    ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi*Factors.Centi*Factors.Bar*Factors.Bar/Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (UK) per bar squared kelvin",
                    UnitLabel = "ppgUK/(bar²•K)",
                    ID = new Guid("5c136f82-a39a-4973-bb95-ca19b256bfb8"),
                    ConversionFactorFromSIFormula = "Factors.GallonUK*Factors.Bar*Factors.Bar/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (US) per bar squared kelvin",
                    UnitLabel = "ppgUS/(bar²•K)",
                    ID = new Guid("14a06bea-ee90-4a76-92b4-7186c5b1b43f"),
                    ConversionFactorFromSIFormula = "Factors.GallonUS*Factors.Bar*Factors.Bar/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic foot per bar squared kelvin",
                    UnitLabel = "lb/ft³/(bar²•K)",
                    ID = new Guid("f96cf481-f2bc-4f18-9119-c2d88a705cf1"),
                    ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot*Factors.Foot*Factors.Bar*Factors.Bar/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic inch per bar squared kelvin",
                    UnitLabel = "lb/in³/(bar²•K)",
                    ID = new Guid("dc4112ed-2c78-4f94-9be2-880fb9271578"),
                    ConversionFactorFromSIFormula = "Factors.Inch*Factors.Inch*Factors.Inch*Factors.Bar*Factors.Bar/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic yard per bar squared kelvin",
                    UnitLabel = "lb/yd³/(bar²•K)",
                    ID = new Guid("8623f335-7ff1-4bed-8ada-5635c6c32b18"),
                    ConversionFactorFromSIFormula = "Factors.Yard*Factors.Yard*Factors.Yard*Factors.Bar*Factors.Bar/Factors.Pound"
                },

                new UnitChoice
                {
                    UnitName = "kilogram per cubic metre per megapascal squared kelvin",
                    UnitLabel = "kg/m³/(MPa²•K)",
                    ID = new Guid("d45bc21a-7b78-4d93-969a-95ac6aa7a3b1"),
                    ConversionFactorFromSIFormula = "Factors.Mega*Factors.Mega/Factors.Unit"
                },
                new UnitChoice
                {
                    UnitName = "specific gravity per megapascal squared kelvin",
                    UnitLabel = "sg/(MPa²•K)",
                    ID = new Guid("e2f290d5-1ead-46d5-9ae6-da068e022447"),
                    ConversionFactorFromSIFormula = "Factors.SpecificGavity4degC*Factors.Mega*Factors.Mega"
                },
                new UnitChoice
                {
                    UnitName = "gram per cubic centimetre per megapascal squared kelvin",
                    UnitLabel = "g/cm³/(MPa²•K)",
                    ID = new Guid("6dab06b6-83da-4283-9504-9e05cd881aa4"),
                    ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi*Factors.Centi*Factors.Mega*Factors.Mega/Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (UK) per megapascal squared kelvin",
                    UnitLabel = "ppgUK/(MPa²•K)",
                    ID = new Guid("56b48f9d-7725-465e-b8f7-fb60b56305a4"),
                    ConversionFactorFromSIFormula = "Factors.GallonUK*Factors.Mega*Factors.Mega/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (US) per megapascal squared kelvin",
                    UnitLabel = "ppgUS/(MPa²•K)",
                    ID = new Guid("28f33e02-ccf4-4b73-bfcb-f7bd567e1129"),
                    ConversionFactorFromSIFormula = "Factors.GallonUS*Factors.Mega*Factors.Mega/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic foot per megapascal squared kelvin",
                    UnitLabel = "lb/ft³/(MPa²•K)",
                    ID = new Guid("bb8afc52-e0d4-45e5-b300-7dc6140f72a1"),
                    ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot*Factors.Foot*Factors.Mega*Factors.Mega/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic inch per megapascal squared kelvin",
                    UnitLabel = "lb/in³/(MPa²•K)",
                    ID = new Guid("a89bf491-7a64-4637-8ae3-091e1d088bbd"),
                    ConversionFactorFromSIFormula = "Factors.Inch*Factors.Inch*Factors.Inch*Factors.Mega*Factors.Mega/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic yard per megapascal squared kelvin",
                    UnitLabel = "lb/yd³/(MPa²•K)",
                    ID = new Guid("0f74bd5d-5f14-4b76-b2c1-827a85fb5670"),
                    ConversionFactorFromSIFormula = "Factors.Yard*Factors.Yard*Factors.Yard*Factors.Mega*Factors.Mega/Factors.Pound"
                },

                new UnitChoice
                {
                    UnitName = "kilogram per cubic metre per gigapascal squared kelvin",
                    UnitLabel = "kg/m³/(GPa²•K)",
                    ID = new Guid("e7410220-85c2-4a2e-95de-7701cfc9fc44"),
                    ConversionFactorFromSIFormula = "Factors.Giga*Factors.Giga/Factors.Unit"
                },
                new UnitChoice
                {
                    UnitName = "specific gravity per gigapascal squared kelvin",
                    UnitLabel = "sg/(GPa²•K)",
                    ID = new Guid("2a482430-a155-4c91-a981-633f12d115d0"),
                    ConversionFactorFromSIFormula = "Factors.SpecificGavity4degC*Factors.Giga*Factors.Giga"
                },
                new UnitChoice
                {
                    UnitName = "gram per cubic centimetre per gigapascal squared kelvin",
                    UnitLabel = "g/cm³/(GPa²•K)",
                    ID = new Guid("0046231f-e430-444f-bd2e-7fa79fe20aab"),
                    ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi*Factors.Centi*Factors.Giga*Factors.Giga/Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (UK) per gigapascal squared kelvin",
                    UnitLabel = "ppgUK/(GPa²•K)",
                    ID = new Guid("6e2bf814-a090-415b-9043-c43d02c469d7"),
                    ConversionFactorFromSIFormula = "Factors.GallonUK*Factors.Giga*Factors.Giga/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (US) per gigapascal squared kelvin",
                    UnitLabel = "ppgUS/(GPa²•K)",
                    ID = new Guid("efbd074e-1aeb-4941-8339-837f7c9e8abe"),
                    ConversionFactorFromSIFormula = "Factors.GallonUS*Factors.Giga*Factors.Giga/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic foot per gigapascal squared kelvin",
                    UnitLabel = "lb/ft³/(GPa²•K)",
                    ID = new Guid("cd9f0127-ad45-4842-8b64-fc5d2eb4ec15"),
                    ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot*Factors.Foot*Factors.Giga*Factors.Giga/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic inch per gigapascal squared kelvin",
                    UnitLabel = "lb/in³/(GPa²•K)",
                    ID = new Guid("f898dd9c-cbda-478d-bff7-b24c1887e9e1"),
                    ConversionFactorFromSIFormula = "Factors.Inch*Factors.Inch*Factors.Inch*Factors.Giga*Factors.Giga/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic yard per gigapascal squared kelvin",
                    UnitLabel = "lb/yd³/(GPa²•K)",
                    ID = new Guid("dc9cf37d-6036-4151-8ea4-2a8c40efb3c8"),
                    ConversionFactorFromSIFormula = "Factors.Yard*Factors.Yard*Factors.Yard*Factors.Giga*Factors.Giga/Factors.Pound"
                },

                new UnitChoice
                {
                    UnitName = "kilogram per cubic metre per pound per square inch squared kelvin",
                    UnitLabel = "kg/m³/(psi²•K)",
                    ID = new Guid("7179c84a-113f-44c1-97c2-ccda465876bd"),
                    ConversionFactorFromSIFormula = "Factors.PSI*Factors.PSI/Factors.Unit"
                },
                new UnitChoice
                {
                    UnitName = "specific gravity per pound per square inch squared kelvin",
                    UnitLabel = "sg/(psi²•K)",
                    ID = new Guid("1700ead8-a6fa-455c-9389-6526cb099af1"),
                    ConversionFactorFromSIFormula = "Factors.SpecificGavity4degC*Factors.PSI*Factors.PSI"
                },
                new UnitChoice
                {
                    UnitName = "gram per cubic centimetre per pound per square inch squared kelvin",
                    UnitLabel = "g/cm³/(psi²•K)",
                    ID = new Guid("9c5f6e94-c1ca-4107-8656-29a00c575f34"),
                    ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi*Factors.Centi*Factors.PSI*Factors.PSI/Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (UK) per pound per square inch squared kelvin",
                    UnitLabel = "ppgUK/(psi²•K)",
                    ID = new Guid("5a8b846c-1fb3-4c53-b181-14d2b5806919"),
                    ConversionFactorFromSIFormula = "Factors.GallonUK*Factors.PSI*Factors.PSI/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (US) per pound per square inch squared kelvin",
                    UnitLabel = "ppgUS/(psi²•K)",
                    ID = new Guid("6f009bc6-d776-4bb5-9f34-1870f6492c45"),
                    ConversionFactorFromSIFormula = "Factors.GallonUS*Factors.PSI*Factors.PSI/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic foot per pound per square inch squared kelvin",
                    UnitLabel = "lb/ft³/(psi²•K)",
                    ID = new Guid("61df2832-d446-4875-b000-660e89ff1510"),
                    ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot*Factors.Foot*Factors.PSI*Factors.PSI/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic inch per pound per square inch squared kelvin",
                    UnitLabel = "lb/in³/(psi²•K)",
                    ID = new Guid("c68060f8-20b1-46ad-aed8-6612ebdbc7d2"),
                    ConversionFactorFromSIFormula = "Factors.Inch*Factors.Inch*Factors.Inch*Factors.PSI*Factors.PSI/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic yard per pound per square inch squared kelvin",
                    UnitLabel = "lb/yd³/(psi²•K)",
                    ID = new Guid("2170eff8-21a6-4682-81ab-2d2834ba03d9"),
                    ConversionFactorFromSIFormula = "Factors.Yard*Factors.Yard*Factors.Yard*Factors.PSI*Factors.PSI/Factors.Pound"
                },

                new UnitChoice
                {
                    UnitName = "kilogram per cubic metre per pascal squared celsius",
                    UnitLabel = "kg/m³/(Pa²•°C)",
                    ID = new Guid("0d5a8548-b83e-4a96-9550-57725a3f3e9c"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                },
                new UnitChoice
                {
                    UnitName = "specific gravity per pascal squared celsius",
                    UnitLabel = "sg/(Pa²•°C)",
                    ID = new Guid("667e0395-0678-458f-9a27-3ee726df2ea6"),
                    ConversionFactorFromSIFormula = "Factors.SpecificGavity4degC"
                },
                new UnitChoice
                {
                    UnitName = "gram per cubic centimetre per pascal squared celsius",
                    UnitLabel = "g/cm³/(Pa²•°C)",
                    ID = new Guid("3454083c-dfe0-454d-a048-8ed9e6835fe9"),
                    ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi*Factors.Centi/Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (UK) per pascal squared celsius",
                    UnitLabel = "ppgUK/(Pa²•°C)",
                    ID = new Guid("b0761965-0273-497b-843f-54faf816ad0f"),
                    ConversionFactorFromSIFormula = "Factors.GallonUK/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (US) per pascal squared celsius",
                    UnitLabel = "ppgUS/(Pa²•°C)",
                    ID = new Guid("c28837e9-5bfb-4154-a33e-04f775cf71c3"),
                    ConversionFactorFromSIFormula = "Factors.GallonUS/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic foot per pascal squared celsius",
                    UnitLabel = "lb/ft³/(Pa²•°C)",
                    ID = new Guid("7367c345-b965-43ea-94d0-7068d8eab3a3"),
                    ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot*Factors.Foot/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic inch per pascal squared celsius",
                    UnitLabel = "lb/in³/(Pa²•°C)",
                    ID = new Guid("1c6f0c40-80ef-4fb3-a0b7-892ba89c35e7"),
                    ConversionFactorFromSIFormula = "Factors.Inch*Factors.Inch*Factors.Inch/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic yard per pascal squared celsius",
                    UnitLabel = "lb/yd³/(Pa²•°C)",
                    ID = new Guid("623d2ac0-cb0e-485e-8996-bed2b943c6fa"),
                    ConversionFactorFromSIFormula = "Factors.Yard*Factors.Yard*Factors.Yard/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "kilogram per cubic metre per bar squared celsius",
                    UnitLabel = "kg/m³/(bar²•°C)",
                    ID = new Guid("a7346d6e-bd4a-4f93-8c4d-39d2e70d70a1"),
                    ConversionFactorFromSIFormula = "Factors.Bar*Factors.Bar/Factors.Unit"
                },
                new UnitChoice
                {
                    UnitName = "specific gravity per bar squared celsius",
                    UnitLabel = "sg/(bar²•°C)",
                    ID = new Guid("04e257cd-c95b-46d3-ace6-3a7de12dba45"),
                    ConversionFactorFromSIFormula = "Factors.SpecificGavity4degC*Factors.Bar*Factors.Bar"
                },
                new UnitChoice
                {
                    UnitName = "gram per cubic centimetre per bar squared celsius",
                    UnitLabel = "g/cm³/(bar²•°C)",
                    ID = new Guid("da92c1ef-6ce0-4ee8-bdf1-aa6111aac4c7"),
                    ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi*Factors.Centi*Factors.Bar*Factors.Bar/Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (UK) per bar squared celsius",
                    UnitLabel = "ppgUK/(bar²•°C)",
                    ID = new Guid("bcd39cc2-603f-4663-a896-3a7d5556b51d"),
                    ConversionFactorFromSIFormula = "Factors.GallonUK*Factors.Bar*Factors.Bar/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (US) per bar squared celsius",
                    UnitLabel = "ppgUS/(bar²•°C)",
                    ID = new Guid("80757949-116e-45ee-98ec-cee61c7e6175"),
                    ConversionFactorFromSIFormula = "Factors.GallonUS*Factors.Bar*Factors.Bar/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic foot per bar squared celsius",
                    UnitLabel = "lb/ft³/(bar²•°C)",
                    ID = new Guid("731cb76b-17c1-4a7d-9a84-2bd3b90ec32a"),
                    ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot*Factors.Foot*Factors.Bar*Factors.Bar/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic inch per bar squared celsius",
                    UnitLabel = "lb/in³/(bar²•°C)",
                    ID = new Guid("2d1eb56c-e8f5-46ce-bafd-0696f0b1956b"),
                    ConversionFactorFromSIFormula = "Factors.Inch*Factors.Inch*Factors.Inch*Factors.Bar*Factors.Bar/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic yard per bar squared celsius",
                    UnitLabel = "lb/yd³/(bar²•°C)",
                    ID = new Guid("f9ffcd7c-4479-4122-9906-4f115712a799"),
                    ConversionFactorFromSIFormula = "Factors.Yard*Factors.Yard*Factors.Yard*Factors.Bar*Factors.Bar/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "kilogram per cubic metre per pound per square inch squared celsius",
                    UnitLabel = "kg/m³/(psi²•°C)",
                    ID = new Guid("1ce45b46-493e-4e43-9a01-da7f137822c7"),
                    ConversionFactorFromSIFormula = "Factors.PSI*Factors.PSI/Factors.Unit"
                },
                new UnitChoice
                {
                    UnitName = "specific gravity per pound per square inch squared celsius",
                    UnitLabel = "sg/(psi²•°C)",
                    ID = new Guid("b5642483-4968-41a8-addf-55f5844f24ec"),
                    ConversionFactorFromSIFormula = "Factors.SpecificGavity4degC*Factors.PSI*Factors.PSI"
                },
                new UnitChoice
                {
                    UnitName = "gram per cubic centimetre per pound per square inch squared celsius",
                    UnitLabel = "g/cm³/(psi²•°C)",
                    ID = new Guid("bd600761-8fe6-4fc2-b3d8-f0b27e8a414c"),
                    ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi*Factors.Centi*Factors.PSI*Factors.PSI/Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (UK) per pound per square inch squared celsius",
                    UnitLabel = "ppgUK/(psi²•°C)",
                    ID = new Guid("c7d1f1ba-6574-4271-927c-0983ece07f8b"),
                    ConversionFactorFromSIFormula = "Factors.GallonUK*Factors.PSI*Factors.PSI/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (US) per pound per square inch squared celsius",
                    UnitLabel = "ppgUS/(psi²•°C)",
                    ID = new Guid("f3b574d7-be5d-4c41-ad56-188a175e823d"),
                    ConversionFactorFromSIFormula = "Factors.GallonUS*Factors.PSI*Factors.PSI/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic foot per pound per square inch squared celsius",
                    UnitLabel = "lb/ft³/(psi²•°C)",
                    ID = new Guid("ca74b7e6-a4dc-419d-bdfd-75b927aaa380"),
                    ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot*Factors.Foot*Factors.PSI*Factors.PSI/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic inch per pound per square inch squared celsius",
                    UnitLabel = "lb/in³/(psi²•°C)",
                    ID = new Guid("48ae377a-0eea-4b9c-b1a9-0156a2d0ff35"),
                    ConversionFactorFromSIFormula = "Factors.Inch*Factors.Inch*Factors.Inch*Factors.PSI*Factors.PSI/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic yard per pound per square inch squared celsius",
                    UnitLabel = "lb/yd³/(psi²•°C)",
                    ID = new Guid("eaaf6fee-8773-4ec3-a0ea-de1d8672066f"),
                    ConversionFactorFromSIFormula = "Factors.Yard*Factors.Yard*Factors.Yard*Factors.PSI*Factors.PSI/Factors.Pound"
                },

                new UnitChoice
                {
                    UnitName = "kilogram per cubic metre per pascal squared fahrenheit",
                    UnitLabel = "kg/m³/(Pa²•°F)",
                    ID = new Guid("489deba9-0377-46ea-9577-68efb2aedbf6"),
                    ConversionFactorFromSIFormula = "Factors.FahrenheitSlope/Factors.Unit",
                },
                new UnitChoice
                {
                    UnitName = "specific gravity per pascal squared fahrenheit",
                    UnitLabel = "sg/(Pa²•°F)",
                    ID = new Guid("b57c2f62-a6b6-4a8d-a9f7-2edfb0b1651c"),
                    ConversionFactorFromSIFormula = "Factors.SpecificGavity4degC*Factors.FahrenheitSlope"
                },
                new UnitChoice
                {
                    UnitName = "gram per cubic centimetre per pascal squared fahrenheit",
                    UnitLabel = "g/cm³/(Pa²•°F)",
                    ID = new Guid("546e2ac5-4937-433f-906f-077f010e7964"),
                    ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi*Factors.Centi*Factors.FahrenheitSlope/Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (UK) per pascal squared fahrenheit",
                    UnitLabel = "ppgUK/(Pa²•°F)",
                    ID = new Guid("ec7d57e4-b2fa-40b6-9d81-743a0610baf9"),
                    ConversionFactorFromSIFormula = "Factors.GallonUK*Factors.FahrenheitSlope/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (US) per pascal squared fahrenheit",
                    UnitLabel = "ppgUS/(Pa²•°F)",
                    ID = new Guid("4d8dc1f2-032f-4792-b59a-2bf25dd5990f"),
                    ConversionFactorFromSIFormula = "Factors.GallonUS*Factors.FahrenheitSlope/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic foot per pascal squared fahrenheit",
                    UnitLabel = "lb/ft³/(Pa²•°F)",
                    ID = new Guid("a1517641-82b9-4270-aa74-b0b2ece8c693"),
                    ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot*Factors.Foot*Factors.FahrenheitSlope/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic inch per pascal squared fahrenheit",
                    UnitLabel = "lb/in³/(Pa²•°F)",
                    ID = new Guid("8def68a4-6327-4457-9bd1-7703c405bd83"),
                    ConversionFactorFromSIFormula = "Factors.Inch*Factors.Inch*Factors.Inch*Factors.FahrenheitSlope/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic yard per pascal squared fahrenheit",
                    UnitLabel = "lb/yd³/(Pa²•°F)",
                    ID = new Guid("6eec2c4d-5b2f-4aa7-ac68-18d57c81928d"),
                    ConversionFactorFromSIFormula = "Factors.Yard*Factors.Yard*Factors.Yard*Factors.FahrenheitSlope/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "kilogram per cubic metre per bar squared fahrenheit",
                    UnitLabel = "kg/m³/(bar²•°F)",
                    ID = new Guid("9fcb07f9-bd0c-46da-bd97-8cfd7ddaa24f"),
                    ConversionFactorFromSIFormula = "Factors.Bar*Factors.Bar*Factors.FahrenheitSlope/Factors.Unit"
                },
                new UnitChoice
                {
                    UnitName = "specific gravity per bar squared fahrenheit",
                    UnitLabel = "sg/(bar²•°F)",
                    ID = new Guid("496be92b-9b7f-447e-8bd4-ccdbfb7e5d45"),
                    ConversionFactorFromSIFormula = "Factors.SpecificGavity4degC*Factors.Bar*Factors.Bar*Factors.FahrenheitSlope"
                },
                new UnitChoice
                {
                    UnitName = "gram per cubic centimetre per bar squared fahrenheit",
                    UnitLabel = "g/cm³/(bar²•°F)",
                    ID = new Guid("a295e2a1-736b-455f-9a1a-26b93fc3ce37"),
                    ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi*Factors.Centi*Factors.Bar*Factors.Bar*Factors.FahrenheitSlope/Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (UK) per bar squared fahrenheit",
                    UnitLabel = "ppgUK/(bar²•°F)",
                    ID = new Guid("a2eab10e-27ef-409e-85dc-41770fe3bcb3"),
                    ConversionFactorFromSIFormula = "Factors.GallonUK*Factors.Bar*Factors.Bar*Factors.FahrenheitSlope/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (US) per bar squared fahrenheit",
                    UnitLabel = "ppgUS/(bar²•°F)",
                    ID = new Guid("f8a5b28b-a219-4045-9c72-a65086b7c459"),
                    ConversionFactorFromSIFormula = "Factors.GallonUS*Factors.Bar*Factors.Bar*Factors.FahrenheitSlope/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic foot per bar squared fahrenheit",
                    UnitLabel = "lb/ft³/(bar²•°F)",
                    ID = new Guid("fac1aaa4-f129-4f0b-80d8-9706223f27af"),
                    ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot*Factors.Foot*Factors.Bar*Factors.Bar*Factors.FahrenheitSlope/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic inch per bar squared fahrenheit",
                    UnitLabel = "lb/in³/(bar²•°F)",
                    ID = new Guid("512f5c6c-717b-4532-8294-47883c881377"),
                    ConversionFactorFromSIFormula = "Factors.Inch*Factors.Inch*Factors.Inch*Factors.Bar*Factors.Bar*Factors.FahrenheitSlope/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic yard per bar squared fahrenheit",
                    UnitLabel = "lb/yd³/(bar²•°F)",
                    ID = new Guid("af3daad4-a444-46a1-b046-4bb36141b5bd"),
                    ConversionFactorFromSIFormula = "Factors.Yard*Factors.Yard*Factors.Yard*Factors.Bar*Factors.Bar*Factors.FahrenheitSlope/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "kilogram per cubic metre per pound per square inch squared fahrenheit",
                    UnitLabel = "kg/m³/(psi²•°F)",
                    ID = new Guid("7733b2c6-da6b-42d8-84d0-abf1c8dd8259"),
                    ConversionFactorFromSIFormula = "Factors.PSI*Factors.PSI*Factors.FahrenheitSlope/Factors.Unit"
                },
                new UnitChoice
                {
                    UnitName = "specific gravity per pound per square inch squared fahrenheit",
                    UnitLabel = "sg/(psi²•°F)",
                    ID = new Guid("0cb8518e-1e1b-4e3c-9f34-5e0042c9e6d7"),
                    ConversionFactorFromSIFormula = "Factors.SpecificGavity4degC*Factors.PSI*Factors.PSI*Factors.FahrenheitSlope"
                },
                new UnitChoice
                {
                    UnitName = "gram per cubic centimetre per pound per square inch squared fahrenheit",
                    UnitLabel = "g/cm³/(psi²•°F)",
                    ID = new Guid("cd1cab93-09b5-4793-afd4-d04d01cbae69"),
                    ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi*Factors.Centi*Factors.PSI*Factors.PSI*Factors.FahrenheitSlope/Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (UK) per pound per square inch squared fahrenheit",
                    UnitLabel = "ppgUK/(psi²•°F)",
                    ID = new Guid("c958922f-4a1c-4186-b08a-e642df71dde3"),
                    ConversionFactorFromSIFormula = "Factors.GallonUK*Factors.PSI*Factors.PSI*Factors.FahrenheitSlope/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (US) per pound per square inch squared fahrenheit",
                    UnitLabel = "ppgUS/(psi²•°F)",
                    ID = new Guid("bcb65f47-4365-4143-a517-8db45650aa11"),
                    ConversionFactorFromSIFormula = "Factors.GallonUS*Factors.PSI*Factors.PSI*Factors.FahrenheitSlope/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic foot per pound per square inch squared fahrenheit",
                    UnitLabel = "lb/ft³/(psi²•°F)",
                    ID = new Guid("f3548d52-8d7d-4f67-9029-118edb2c13b9"),
                    ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot*Factors.Foot*Factors.PSI*Factors.PSI*Factors.FahrenheitSlope/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic inch per pound per square inch squared fahrenheit",
                    UnitLabel = "lb/in³/(psi²•°F)",
                    ID = new Guid("8d02dc32-3c91-4323-b840-ebea2a6cbf7b"),
                    ConversionFactorFromSIFormula = "Factors.Inch*Factors.Inch*Factors.Inch*Factors.PSI*Factors.PSI*Factors.FahrenheitSlope/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic yard per pound per square inch squared fahrenheit",
                    UnitLabel = "lb/yd³/(psi²•°F)",
                    ID = new Guid("5e07f626-35e9-494b-a4e1-ba7f746c627d"),
                    ConversionFactorFromSIFormula = "Factors.Yard*Factors.Yard*Factors.Yard*Factors.PSI*Factors.PSI*Factors.FahrenheitSlope/Factors.Pound"
                },
        };
        public MassDensityGradientPerPressureSquaredTemperatureQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "mass density gradient per pressure squared and temperature" };
            ID = new Guid("2d4b23e0-01ea-472f-85c1-1ced4d6507a6");
            DescriptionMD = string.Empty;
            DescriptionMD += @"A mass density gradient per pressure squared is the second derivative of a mass density compared to pressure: $\frac{d^{2}\rho}{dp^{2}}$, where $\rho$ is a mass density and $p$ is pressure." + Environment.NewLine;
            DescriptionMD += @"The dimension of mass density gradient per pressure squared is:" + Environment.NewLine;
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
