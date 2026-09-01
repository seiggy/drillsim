using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class MassDensityGradientPerPressureTemperatureQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "kilogram per cubic metre per pascal kelvin";
        public override string SIUnitLabelLatex { get; } = "\\frac{\\frac{kg}{m^{3}}}{Pa \\cdot K}";
        public override double TimeDimension { get; } = 2;
        public override double LengthDimension { get; } = -2;
        public override double TemperatureDimension { get; } = -1;

        private static MassDensityGradientPerPressureTemperatureQuantity instance_ = null;

        public static MassDensityGradientPerPressureTemperatureQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MassDensityGradientPerPressureTemperatureQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
                {
                    UnitName = "kilogram per cubic metre per pascal kelvin",
                    UnitLabel = "kg/m³/(Pa•K)",
                    ID = new Guid("ab41e361-7ef5-488f-b121-e51afa56fcfa"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                    IsSI = true
                },
                new UnitChoice
                {
                    UnitName = "specific gravity per pascal kelvin",
                    UnitLabel = "sg/(Pa•K)",
                    ID = new Guid("bd96fb03-de24-4171-abf8-eccfd6fcd2e8"),
                    ConversionFactorFromSIFormula = "Factors.SpecificGavity4degC"
                },
                new UnitChoice
                {
                    UnitName = "gram per cubic centimetre per pascal kelvin",
                    UnitLabel = "g/cm³/(Pa•K)",
                    ID = new Guid("c2cae333-be7c-4567-a42e-00d2fa95d1aa"),
                    ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi*Factors.Centi/Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (UK) per pascal kelvin",
                    UnitLabel = "ppgUK/(Pa•K)",
                    ID = new Guid("3ae6d54a-e365-4dc5-b5c8-083b88297719"),
                    ConversionFactorFromSIFormula = "Factors.GallonUK/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (US) per pascal kelvin",
                    UnitLabel = "ppgUS/(Pa•K)",
                    ID = new Guid("8b5d3eec-f8ec-4233-8d99-6a99fee53ae8"),
                    ConversionFactorFromSIFormula = "Factors.GallonUS/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic foot per pascal kelvin",
                    UnitLabel = "lb/ft³/(Pa•K)",
                    ID = new Guid("d6be025a-14b5-4c0e-8971-4fe5468237d5"),
                    ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot*Factors.Foot/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic inch per pascal kelvin",
                    UnitLabel = "lb/in³/(Pa•K)",
                    ID = new Guid("e1dc7833-9369-4901-ae8c-c4c528f1d11d"),
                    ConversionFactorFromSIFormula = "Factors.Inch*Factors.Inch*Factors.Inch/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic yard per pascal kelvin",
                    UnitLabel = "lb/yd³/(Pa•K)",
                    ID = new Guid("fbb4e022-e102-436f-93c1-c9ea2c30f9e0"),
                    ConversionFactorFromSIFormula = "Factors.Yard*Factors.Yard*Factors.Yard/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "kilogram per cubic metre per bar kelvin",
                    UnitLabel = "kg/m³/(bar•K)",
                    ID = new Guid("8cbe6cb2-2a0f-400d-a659-0695eb16d508"),
                    ConversionFactorFromSIFormula = "Factors.Bar/Factors.Unit"
                },
                new UnitChoice
                {
                    UnitName = "specific gravity per bar kelvin",
                    UnitLabel = "sg/(bar•K)",
                    ID = new Guid("9aa8835d-e374-4527-bdd6-636ec7148e66"),
                    ConversionFactorFromSIFormula = "Factors.SpecificGavity4degC*Factors.Bar"
                },
                new UnitChoice
                {
                    UnitName = "gram per cubic centimetre per bar kelvin",
                    UnitLabel = "g/cm³/(bar•K)",
                    ID = new Guid("bbb10e84-4dee-459d-b389-5127606db8cc"),
                    ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi*Factors.Centi*Factors.Bar/Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (UK) per bar kelvin",
                    UnitLabel = "ppgUK/(bar•K)",
                    ID = new Guid("07e8b9de-be10-4750-836a-929a4b16588d"),
                    ConversionFactorFromSIFormula = "Factors.GallonUK*Factors.Bar/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (US) per bar kelvin",
                    UnitLabel = "ppgUS/(bar•K)",
                    ID = new Guid("19804fa6-8dc6-4043-8b0a-d7cabaf98c13"),
                    ConversionFactorFromSIFormula = "Factors.GallonUS*Factors.Bar/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic foot per bar kelvin",
                    UnitLabel = "lb/ft³/(bar•K)",
                    ID = new Guid("fb6f3791-8de0-42d7-a870-2b87b6a391bf"),
                    ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot*Factors.Foot*Factors.Bar/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic inch per bar kelvin",
                    UnitLabel = "lb/in³/(bar•K)",
                    ID = new Guid("3390b64d-7959-4c92-9188-f842b1a7b4d6"),
                    ConversionFactorFromSIFormula = "Factors.Inch*Factors.Inch*Factors.Inch*Factors.Bar/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic yard per bar kelvin",
                    UnitLabel = "lb/yd³/(bar•K)",
                    ID = new Guid("2caa723f-4107-4632-ad12-3b72c3eb1174"),
                    ConversionFactorFromSIFormula = "Factors.Yard*Factors.Yard*Factors.Yard*Factors.Bar/Factors.Pound"
                },

                new UnitChoice
                {
                    UnitName = "kilogram per cubic metre per megapascal kelvin",
                    UnitLabel = "kg/m³/(MPa•K)",
                    ID = new Guid("4fc27c0f-f9a6-47be-9f76-61f3a00a78f1"),
                    ConversionFactorFromSIFormula = "Factors.Mega/Factors.Unit"
                },
                new UnitChoice
                {
                    UnitName = "specific gravity per megapascal kelvin",
                    UnitLabel = "sg/(MPa•K)",
                    ID = new Guid("6eca416d-d590-475d-877a-3e5d9b74ecbc"),
                    ConversionFactorFromSIFormula = "Factors.SpecificGavity4degC*Factors.Mega"
                },
                new UnitChoice
                {
                    UnitName = "gram per cubic centimetre per megapascal kelvin",
                    UnitLabel = "g/cm³/(MPa•K)",
                    ID = new Guid("6b3c00ae-499a-4a48-8400-b1b5704c9904"),
                    ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi*Factors.Centi*Factors.Mega/Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (UK) per megapascal kelvin",
                    UnitLabel = "ppgUK/(MPa•K)",
                    ID = new Guid("35523ce7-c1a8-4742-9e80-a253d62d16dc"),
                    ConversionFactorFromSIFormula = "Factors.GallonUK*Factors.Mega/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (US) per megapascal kelvin",
                    UnitLabel = "ppgUS/(MPa•K)",
                    ID = new Guid("3d5692e3-0baa-473a-abbd-b61fb3184c95"),
                    ConversionFactorFromSIFormula = "Factors.GallonUS*Factors.Mega/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic foot per megapascal kelvin",
                    UnitLabel = "lb/ft³/(MPa•K)",
                    ID = new Guid("403a4c46-5990-47be-97d9-ef59505f6338"),
                    ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot*Factors.Foot*Factors.Mega/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic inch per megapascal kelvin",
                    UnitLabel = "lb/in³/(MPa•K)",
                    ID = new Guid("b238105f-64e8-4d20-9cc4-09847041b60b"),
                    ConversionFactorFromSIFormula = "Factors.Inch*Factors.Inch*Factors.Inch*Factors.Mega/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic yard per megapascal kelvin",
                    UnitLabel = "lb/yd³/(MPa•K)",
                    ID = new Guid("b4959d44-0010-4853-9219-5696882ed448"),
                    ConversionFactorFromSIFormula = "Factors.Yard*Factors.Yard*Factors.Yard*Factors.Mega/Factors.Pound"
                },

                new UnitChoice
                {
                    UnitName = "kilogram per cubic metre per gigapascal kelvin",
                    UnitLabel = "kg/m³/(GPa•K)",
                    ID = new Guid("c53d4c83-4dc4-4084-9cbb-bb91ea9b704b"),
                    ConversionFactorFromSIFormula = "Factors.Giga/Factors.Unit"
                },
                new UnitChoice
                {
                    UnitName = "specific gravity per gigapascal kelvin",
                    UnitLabel = "sg/(GPa•K)",
                    ID = new Guid("23de1690-a6d7-438c-b96b-9dfc670b428d"),
                    ConversionFactorFromSIFormula = "Factors.SpecificGavity4degC*Factors.Giga"
                },
                new UnitChoice
                {
                    UnitName = "gram per cubic centimetre per gigapascal kelvin",
                    UnitLabel = "g/cm³/(GPa•K)",
                    ID = new Guid("b262ca47-7eea-44fa-8e04-8a8d80fb9b34"),
                    ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi*Factors.Centi*Factors.Giga/Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (UK) per gigapascal kelvin",
                    UnitLabel = "ppgUK/(GPa•K)",
                    ID = new Guid("67464a81-a4fb-4534-b44f-8f400528dd3e"),
                    ConversionFactorFromSIFormula = "Factors.GallonUK*Factors.Giga/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (US) per gigapascal kelvin",
                    UnitLabel = "ppgUS/(GPa•K)",
                    ID = new Guid("028368a8-be80-4ce3-ad13-e6e4d2a17486"),
                    ConversionFactorFromSIFormula = "Factors.GallonUS*Factors.Giga/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic foot per gigapascal kelvin",
                    UnitLabel = "lb/ft³/(GPa•K)",
                    ID = new Guid("612dc2f8-a5bb-44e9-a469-ba7bf15a2c79"),
                    ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot*Factors.Foot*Factors.Giga/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic inch per gigapascal kelvin",
                    UnitLabel = "lb/in³/(GPa•K)",
                    ID = new Guid("2f5739b2-c7d4-406f-9596-508b3c2a1041"),
                    ConversionFactorFromSIFormula = "Factors.Inch*Factors.Inch*Factors.Inch*Factors.Giga/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic yard per gigapascal kelvin",
                    UnitLabel = "lb/yd³/(GPa•K)",
                    ID = new Guid("15be077d-791a-4b91-90c1-15373fc27017"),
                    ConversionFactorFromSIFormula = "Factors.Yard*Factors.Yard*Factors.Yard*Factors.Giga/Factors.Pound"
                },

                new UnitChoice
                {
                    UnitName = "kilogram per cubic metre per pound per square inch kelvin",
                    UnitLabel = "kg/m³/(psi•K)",
                    ID = new Guid("f0a415eb-f811-49bb-8a08-640a976fe1cf"),
                    ConversionFactorFromSIFormula = "Factors.PSI/Factors.Unit"
                },
                new UnitChoice
                {
                    UnitName = "specific gravity per pound per square inch kelvin",
                    UnitLabel = "sg/(psi•K)",
                    ID = new Guid("3a2b82e9-6f53-4149-9530-b202abee4a22"),
                    ConversionFactorFromSIFormula = "Factors.SpecificGavity4degC*Factors.PSI"
                },
                new UnitChoice
                {
                    UnitName = "gram per cubic centimetre per pound per square inch kelvin",
                    UnitLabel = "g/cm³/(psi•K)",
                    ID = new Guid("4a4df929-c2df-4da5-aad4-601e82945f2c"),
                    ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi*Factors.Centi*Factors.PSI/Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (UK) per pound per square inch kelvin",
                    UnitLabel = "ppgUK/(psi•K)",
                    ID = new Guid("dd330e7b-b47c-43c9-a4fe-4259a6a74b40"),
                    ConversionFactorFromSIFormula = "Factors.GallonUK*Factors.PSI/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (US) per pound per square inch kelvin",
                    UnitLabel = "ppgUS/(psi•K)",
                    ID = new Guid("e351b849-3d09-44b7-ac67-4380d7a6908a"),
                    ConversionFactorFromSIFormula = "Factors.GallonUS*Factors.PSI/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic foot per pound per square inch kelvin",
                    UnitLabel = "lb/ft³/(psi•K)",
                    ID = new Guid("11c5c48f-a880-479f-9f8a-952433aa1105"),
                    ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot*Factors.Foot*Factors.PSI/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic inch per pound per square inch kelvin",
                    UnitLabel = "lb/in³/(psi•K)",
                    ID = new Guid("1b9b4c68-a0f7-43e3-bc6c-65a13672e340"),
                    ConversionFactorFromSIFormula = "Factors.Inch*Factors.Inch*Factors.Inch*Factors.PSI/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic yard per pound per square inch kelvin",
                    UnitLabel = "lb/yd³/(psi•K)",
                    ID = new Guid("5d80643e-871a-41d2-b215-6bf62585a408"),
                    ConversionFactorFromSIFormula = "Factors.Yard*Factors.Yard*Factors.Yard*Factors.PSI/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "kilogram per cubic metre per pascal celsius",
                    UnitLabel = "kg/m³/(Pa•°C)",
                    ID = new Guid("c0602be3-4094-40af-a85f-fab2378a5d83"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Unit"
                },
                new UnitChoice
                {
                    UnitName = "specific gravity per pascal celsius",
                    UnitLabel = "sg/(Pa•°C)",
                    ID = new Guid("03cc0ee6-3c31-417a-b8e6-c40cd5b377d3"),
                    ConversionFactorFromSIFormula = "Factors.SpecificGavity4degC"
                },
                new UnitChoice
                {
                    UnitName = "gram per cubic centimetre per pascal celsius",
                    UnitLabel = "g/cm³/(Pa•°C)",
                    ID = new Guid("ddcc5558-7bcc-424f-8a16-42a8e3cb4a52"),
                    ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi*Factors.Centi/Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (UK) per pascal celsius",
                    UnitLabel = "ppgUK/(Pa•°C)",
                    ID = new Guid("8022319e-61bc-42aa-b7d9-1b7e33771224"),
                    ConversionFactorFromSIFormula = "Factors.GallonUK/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (US) per pascal celsius",
                    UnitLabel = "ppgUS/(Pa•°C)",
                    ID = new Guid("31baed8a-9474-414b-adf7-74df7d244e40"),
                    ConversionFactorFromSIFormula = "Factors.GallonUS/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic foot per pascal celsius",
                    UnitLabel = "lb/ft³/(Pa•°C)",
                    ID = new Guid("684db3d1-6610-4774-8d80-ae212241991a"),
                    ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot*Factors.Foot/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic inch per pascal celsius",
                    UnitLabel = "lb/in³/(Pa•°C)",
                    ID = new Guid("85efde7e-0ab7-44c3-adeb-1934753aea1a"),
                    ConversionFactorFromSIFormula = "Factors.Inch*Factors.Inch*Factors.Inch/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic yard per pascal celsius",
                    UnitLabel = "lb/yd³/(Pa•°C)",
                    ID = new Guid("dc662923-2863-494a-a1b0-90ad30b812e9"),
                    ConversionFactorFromSIFormula = "Factors.Yard*Factors.Yard*Factors.Yard/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "kilogram per cubic metre per bar celsius",
                    UnitLabel = "kg/m³/(bar•°C)",
                    ID = new Guid("e70b203c-cae0-4494-bf5d-fac95d99cb8f"),
                    ConversionFactorFromSIFormula = "Factors.Bar/Factors.Unit"
                },
                new UnitChoice
                {
                    UnitName = "specific gravity per bar celsius",
                    UnitLabel = "sg/(bar•°C)",
                    ID = new Guid("891b9f2e-94cc-41f2-8c36-d4315d923da2"),
                    ConversionFactorFromSIFormula = "Factors.SpecificGavity4degC*Factors.Bar"
                },
                new UnitChoice
                {
                    UnitName = "gram per cubic centimetre per bar celsius",
                    UnitLabel = "g/cm³/(bar•°C)",
                    ID = new Guid("d59d2a23-67f3-4224-8960-3732ca7e3c19"),
                    ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi*Factors.Centi*Factors.Bar/Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (UK) per bar celsius",
                    UnitLabel = "ppgUK/(bar•°C)",
                    ID = new Guid("55531b71-6a17-41bc-ac69-def74092a04b"),
                    ConversionFactorFromSIFormula = "Factors.GallonUK*Factors.Bar/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (US) per bar celsius",
                    UnitLabel = "ppgUS/(bar•°C)",
                    ID = new Guid("c04f57ed-37d8-4f32-bedf-48616e977cf3"),
                    ConversionFactorFromSIFormula = "Factors.GallonUS*Factors.Bar/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic foot per bar celsius",
                    UnitLabel = "lb/ft³/(bar•°C)",
                    ID = new Guid("cd1b76ec-a7e4-4963-8a3c-cbf8d7c1a858"),
                    ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot*Factors.Foot*Factors.Bar/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic inch per bar celsius",
                    UnitLabel = "lb/in³/(bar•°C)",
                    ID = new Guid("45c0e47f-4844-49a8-bb92-a564c99eb6df"),
                    ConversionFactorFromSIFormula = "Factors.Inch*Factors.Inch*Factors.Inch*Factors.Bar/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic yard per bar celsius",
                    UnitLabel = "lb/yd³/(bar•°C)",
                    ID = new Guid("82092268-2d5f-4318-9812-556f63b68089"),
                    ConversionFactorFromSIFormula = "Factors.Yard*Factors.Yard*Factors.Yard*Factors.Bar/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "kilogram per cubic metre per pound per square inch celsius",
                    UnitLabel = "kg/m³/(psi•°C)",
                    ID = new Guid("fd02625d-d171-4ea2-8349-488ddbf9f6c8"),
                    ConversionFactorFromSIFormula = "Factors.PSI/Factors.Unit"
                },
                new UnitChoice
                {
                    UnitName = "specific gravity per pound per square inch celsius",
                    UnitLabel = "sg/(psi•°C)",
                    ID = new Guid("d0e994dc-68fb-41e9-84fb-a62544d16674"),
                    ConversionFactorFromSIFormula = "Factors.SpecificGavity4degC*Factors.PSI"
                },
                new UnitChoice
                {
                    UnitName = "gram per cubic centimetre per pound per square inch celsius",
                    UnitLabel = "g/cm³/(psi•°C)",
                    ID = new Guid("261a18c2-5f0b-4e6f-9988-e1715de29c6f"),
                    ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi*Factors.Centi*Factors.PSI/Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (UK) per pound per square inch celsius",
                    UnitLabel = "ppgUK/(psi•°C)",
                    ID = new Guid("edf9f94f-2cd9-4b76-b0f9-38576be98feb"),
                    ConversionFactorFromSIFormula = "Factors.GallonUK*Factors.PSI/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (US) per pound per square inch celsius",
                    UnitLabel = "ppgUS/(psi•°C)",
                    ID = new Guid("a3c8fa89-a1fd-46a7-bef8-7a4cde7ad585"),
                    ConversionFactorFromSIFormula = "Factors.GallonUS*Factors.PSI/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic foot per pound per square inch celsius",
                    UnitLabel = "lb/ft³/(psi•°C)",
                    ID = new Guid("a0e2f8d9-cf7e-454e-8722-b4777f3ecd1b"),
                    ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot*Factors.Foot*Factors.PSI/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic inch per pound per square inch celsius",
                    UnitLabel = "lb/in³/(psi•°C)",
                    ID = new Guid("33eb59dd-07fe-4826-a1a3-108246bf98e8"),
                    ConversionFactorFromSIFormula = "Factors.Inch*Factors.Inch*Factors.Inch*Factors.PSI/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic yard per pound per square inch celsius",
                    UnitLabel = "lb/yd³/(psi•°C)",
                    ID = new Guid("a10eaf12-acd4-42e6-8bff-f0fe223dfcc2"),
                    ConversionFactorFromSIFormula = "Factors.Yard*Factors.Yard*Factors.Yard*Factors.PSI/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "kilogram per cubic metre per pascal fahrenheit",
                    UnitLabel = "kg/m³/(Pa•°F)",
                    ID = new Guid("81baad5f-7be5-4db4-9e64-b7df06c3babd"),
                    ConversionFactorFromSIFormula = "Factors.FahrenheitSlope/Factors.Unit"
                },
                new UnitChoice
                {
                    UnitName = "specific gravity per pascal fahrenheit",
                    UnitLabel = "sg/(Pa•°F)",
                    ID = new Guid("1c7199e1-89e6-479e-a048-179bfbce39d8"),
                    ConversionFactorFromSIFormula = "Factors.SpecificGavity4degC*Factors.FahrenheitSlope"
                },
                new UnitChoice
                {
                    UnitName = "gram per cubic centimetre per pascal fahrenheit",
                    UnitLabel = "g/cm³/(Pa•°F)",
                    ID = new Guid("1a1fc347-5829-4ed7-bb2c-cf2a0a08cd3b"),
                    ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi*Factors.Centi*Factors.FahrenheitSlope/Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (UK) per pascal fahrenheit",
                    UnitLabel = "ppgUK/(Pa•°F)",
                    ID = new Guid("6af15113-8196-4dc0-8688-543e4ab5c050"),
                    ConversionFactorFromSIFormula = "Factors.GallonUK*Factors.FahrenheitSlope/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (US) per pascal fahrenheit",
                    UnitLabel = "ppgUS/(Pa•°F)",
                    ID = new Guid("06c4a7f8-e43c-4a58-be79-c0fadc2416ec"),
                    ConversionFactorFromSIFormula = "Factors.GallonUS*Factors.FahrenheitSlope/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic foot per pascal fahrenheit",
                    UnitLabel = "lb/ft³/(Pa•°F)",
                    ID = new Guid("240f2cae-25fc-47cf-8757-fe51552189b7"),
                    ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot*Factors.Foot*Factors.FahrenheitSlope/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic inch per pascal fahrenheit",
                    UnitLabel = "lb/in³/(Pa•°F)",
                    ID = new Guid("3f059a64-a660-4c5f-a4e6-68844da8b1c4"),
                    ConversionFactorFromSIFormula = "Factors.Inch*Factors.Inch*Factors.Inch*Factors.FahrenheitSlope/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic yard per pascal fahrenheit",
                    UnitLabel = "lb/yd³/(Pa•°F)",
                    ID = new Guid("19e48858-c3a7-44f7-b9e9-c326f0fafa33"),
                    ConversionFactorFromSIFormula = "Factors.Yard*Factors.Yard*Factors.Yard*Factors.FahrenheitSlope/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "kilogram per cubic metre per bar fahrenheit",
                    UnitLabel = "kg/m³/(bar•°F)",
                    ID = new Guid("83606dd6-7f81-45ea-994c-743581cd62d3"),
                    ConversionFactorFromSIFormula = "Factors.Bar*Factors.FahrenheitSlope/Factors.Unit"
                },
                new UnitChoice
                {
                    UnitName = "specific gravity per bar fahrenheit",
                    UnitLabel = "sg/(bar•°F)",
                    ID = new Guid("070c9187-3d1c-4c1d-a26c-8370d9ac6d8f"),
                    ConversionFactorFromSIFormula = "Factors.SpecificGavity4degC*Factors.Bar*Factors.FahrenheitSlope"
                },
                new UnitChoice
                {
                    UnitName = "gram per cubic centimetre per bar fahrenheit",
                    UnitLabel = "g/cm³/(bar•°F)",
                    ID = new Guid("7dd71c46-d10b-407f-abc9-6597b7dec4bf"),
                    ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi*Factors.Centi*Factors.Bar*Factors.FahrenheitSlope/Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (UK) per bar fahrenheit",
                    UnitLabel = "ppgUK/(bar•°F)",
                    ID = new Guid("349ffb7c-caa0-4ee1-b8a7-8a00ec028b6f"),
                    ConversionFactorFromSIFormula = "Factors.GallonUK*Factors.Bar*Factors.FahrenheitSlope/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (US) per bar fahrenheit",
                    UnitLabel = "ppgUS/(bar•°F)",
                    ID = new Guid("cbbc44bb-a924-4e10-8c7d-9388f248722d"),
                    ConversionFactorFromSIFormula = "Factors.GallonUS*Factors.Bar*Factors.FahrenheitSlope/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic foot per bar fahrenheit",
                    UnitLabel = "lb/ft³/(bar•°F)",
                    ID = new Guid("2de087ee-34df-4e54-b6d2-cfe5393cda8d"),
                    ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot*Factors.Foot*Factors.Bar*Factors.FahrenheitSlope/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic inch per bar fahrenheit",
                    UnitLabel = "lb/in³/(bar•°F)",
                    ID = new Guid("c2033261-a7fe-4fe4-a99e-641db13c276e"),
                    ConversionFactorFromSIFormula = "Factors.Inch*Factors.Inch*Factors.Inch*Factors.Bar*Factors.FahrenheitSlope/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic yard per bar fahrenheit",
                    UnitLabel = "lb/yd³/(bar•°F)",
                    ID = new Guid("32a9db15-caf4-40f8-9a78-e4444d1cc687"),
                    ConversionFactorFromSIFormula = "Factors.Yard*Factors.Yard*Factors.Yard*Factors.Bar*Factors.FahrenheitSlope/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "kilogram per cubic metre per pound per square inch fahrenheit",
                    UnitLabel = "kg/m³/(psi•°F)",
                    ID = new Guid("8bac278b-fb5c-4ab2-b58e-bda28e6b9f65"),
                    ConversionFactorFromSIFormula = "Factors.PSI*Factors.FahrenheitSlope/Factors.Unit"
                },
                new UnitChoice
                {
                    UnitName = "specific gravity per pound per square inch fahrenheit",
                    UnitLabel = "sg/(psi•°F)",
                    ID = new Guid("9d2e9844-cf55-43ca-806b-12147ca5d981"),
                    ConversionFactorFromSIFormula = "Factors.SpecificGavity4degC*Factors.PSI*Factors.FahrenheitSlope"
                },
                new UnitChoice
                {
                    UnitName = "gram per cubic centimetre per pound per square inch fahrenheit",
                    UnitLabel = "g/cm³/(psi•°F)",
                    ID = new Guid("49a3a66b-c08f-48bc-b2cd-207a8a31150d"),
                    ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi*Factors.Centi*Factors.PSI*Factors.FahrenheitSlope/Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (UK) per pound per square inch fahrenheit",
                    UnitLabel = "ppgUK/(psi•°F)",
                    ID = new Guid("65be0323-b39c-4406-9a1e-91eb36a7c963"),
                    ConversionFactorFromSIFormula = "Factors.GallonUK*Factors.PSI*Factors.FahrenheitSlope/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per gallon (US) per pound per square inch fahrenheit",
                    UnitLabel = "ppgUS/(psi•°F)",
                    ID = new Guid("8b1a8cf9-ce8c-40b2-872d-ae3112b69da1"),
                    ConversionFactorFromSIFormula = "Factors.GallonUS*Factors.PSI*Factors.FahrenheitSlope/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic foot per pound per square inch fahrenheit",
                    UnitLabel = "lb/ft³/(psi•°F)",
                    ID = new Guid("07ba1470-7fda-43a1-81bb-8e1e0075021a"),
                    ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot*Factors.Foot*Factors.PSI*Factors.FahrenheitSlope/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic inch per pound per square inch fahrenheit",
                    UnitLabel = "lb/in³/(psi•°F)",
                    ID = new Guid("af345846-e411-416e-b97c-0119f2cb4c3b"),
                    ConversionFactorFromSIFormula = "Factors.Inch*Factors.Inch*Factors.Inch*Factors.PSI*Factors.FahrenheitSlope/Factors.Pound"
                },
                new UnitChoice
                {
                    UnitName = "pound per cubic yard per pound per square inch fahrenheit",
                    UnitLabel = "lb/yd³/(psi•°F)",
                    ID = new Guid("846a356a-3ce8-4f4d-b2c1-641d2236e151"),
                    ConversionFactorFromSIFormula = "Factors.Yard*Factors.Yard*Factors.Yard*Factors.PSI*Factors.FahrenheitSlope/Factors.Pound"
                },
        };
        public MassDensityGradientPerPressureTemperatureQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "mass density gradient per pressure and temperature" };
            ID = new Guid("1f5a6169-f514-4d86-a030-956efc8cb4f1");
            DescriptionMD = string.Empty;
            DescriptionMD += @"A mass density gradient per pressure and temperature is the second derivative of a mass density compared to pressure and temperature: $\frac{d^{2}\rho}{dpdT}$, where $\rho$ is a mass density, $p$ is pressure and $T$ is the temperature." + Environment.NewLine;
            DescriptionMD += @"The dimension of mass density gradient per pressure and temperature is:" + Environment.NewLine;
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