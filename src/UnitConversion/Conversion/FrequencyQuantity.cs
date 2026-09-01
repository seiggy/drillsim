using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class FrequencyQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "hertz";
        public override string SIUnitLabelLatex { get; } = "Hz";
        public override double TimeDimension { get; } = -1;
        private static FrequencyQuantity instance_ = null;

        public static FrequencyQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new FrequencyQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
                {
                    UnitName = "hertz",
                    UnitLabel = "Hz",
                    ID = new Guid("7c572c06-0699-4187-9d0c-397f479fe93d"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                    IsSI = true
                },
                new UnitChoice
                {
                    UnitName = "reciprocal second",
                    UnitLabel = "1/s",
                    ID = new Guid("39240f8f-8c82-4026-9db7-f72ec60cb4c9"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Unit"
                },
                new UnitChoice
                {
                    UnitName = "kilohertz",
                    UnitLabel = "kHz",
                    ID = new Guid("acf483c1-5d7a-4914-afa2-de7abed9be3e"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Kilo"
                },
                new UnitChoice
                {
                    UnitName = "megahertz",
                    UnitLabel = "MHz",
                    ID = new Guid("6dea9f29-d4f4-49a7-86fe-0205d4bab45e"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Mega"
                },
                new UnitChoice
                {
                    UnitName = "gigahertz",
                    UnitLabel = "GHz",
                    ID = new Guid("655ee4f9-1782-4ec0-894a-afff9b75cac7"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Giga"
                },
                new UnitChoice
                {
                    UnitName = "terahertz",
                    UnitLabel = "THz",
                    ID = new Guid("9ca52ae4-2fc5-4e60-b774-79c73442de13"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Tera"
                },
            new UnitChoice
            {
                UnitName = "rpm",
                UnitLabel = "rpm",
                ID = new Guid("30880b5f-803d-412e-9736-62dca3ba5bbd"),
                ConversionFactorFromSIFormula = "Factors.Minute"
            },
            new UnitChoice
            {
                UnitName = "spm",
                UnitLabel = "spm",
                ID = new Guid("426b000b-235f-41d5-8806-b2e47fbfb53b"),
                ConversionFactorFromSIFormula = "Factors.Minute"
            },
            new UnitChoice
            {
                UnitName = "rotation per second",
                UnitLabel = "rps",
                ID = new Guid("6e0ff63e-ef67-440d-a0f7-a17ff73cfff2"),
                ConversionFactorFromSIFormula = "Factors.Unit"
            },
            new UnitChoice
            {
                UnitName = "stroke per second",
                UnitLabel = "sps",
                ID = new Guid("fe114eaf-117a-480e-9dbc-2db244b6d210"),
                ConversionFactorFromSIFormula = "Factors.Unit"
            },            new UnitChoice
            {
                UnitName = "stroke per hour",
                UnitLabel = "sph",
                ID = new Guid("b0f63a0c-9a53-4bdc-9166-03eb4254d3d8"),
                ConversionFactorFromSIFormula = "Factors.Hour"
            },
             new UnitChoice
            {
                UnitName = "rotation per hour",
                UnitLabel = "rph",
                ID = new Guid("cdc5dd34-dc2d-4bd8-85ac-13f6d71ea188"),
                ConversionFactorFromSIFormula = "Factors.Hour"
            },
            new UnitChoice
            {
                UnitName = "shock per second",
                UnitLabel = "sps",
                ID = new Guid("b5318133-64e9-43c7-b7bf-3c86140fe7aa"),
                ConversionFactorFromSIFormula = "Factors.Unit"
            },
            new UnitChoice
            {
                UnitName = "shock per minute",
                UnitLabel = "spm",
                ID = new Guid("6ccbee46-cb8a-4777-b1d2-e88eedd24f73"),
                ConversionFactorFromSIFormula = "Factors.Minute"
            },
            new UnitChoice
            {
                UnitName = "shock per hour",
                UnitLabel = "spm",
                ID = new Guid("0c0d4ecb-ee11-4b57-9bc7-70860637232e"),
                ConversionFactorFromSIFormula = "Factors.Hour"
            },
            new UnitChoice
                {
                  UnitName = "radian per second",
                  UnitLabel = "rad/s",
                  ID = new Guid("1da89c19-8dba-44c4-bff5-d1a7bcf97269"),
                  ConversionFactorFromSIFormula = "Factors.Revolution/Factors.Unit"
                },
                new UnitChoice
                {
                  UnitName = "degree per second",
                  UnitLabel = "°/s",
                  ID = new Guid("dd8dc22c-5dd3-494e-8ff8-fed249a354bb"),
                  ConversionFactorFromSIFormula = "Factors.Revolution*Factors.Degree"
                },
                new UnitChoice
                {
                  UnitName = "radian per day",
                  UnitLabel = "rad/d",
                  ID = new Guid("9d9be8e7-a4ff-4540-9e83-ecde31c24d6b"),
                  ConversionFactorFromSIFormula = "Factors.Revolution*Factors.Day"
                },
                new UnitChoice
                {
                  UnitName = "radian per hour",
                  UnitLabel = "rad/h",
                  ID = new Guid("aaab4534-864c-4864-91de-e094f7c332b9"),
                  ConversionFactorFromSIFormula = "Factors.Revolution*Factors.Hour"
                },
                new UnitChoice
                {
                  UnitName = "radian per minute",
                  UnitLabel = "rad/min",
                  ID = new Guid("e2d71725-cebb-4c24-8cb8-4685390bdead"),
                  ConversionFactorFromSIFormula = "Factors.Revolution*Factors.Minute"
                },
                new UnitChoice
                {
                  UnitName = "degree per day",
                  UnitLabel = "°/d",
                  ID = new Guid("a53ece85-ca3b-421a-aa71-6661edd30fa2"),
                  ConversionFactorFromSIFormula = "Factors.Revolution*Factors.Degree*Factors.Day"
                },
                new UnitChoice
                {
                  UnitName = "degree per hour",
                  UnitLabel = "°/h",
                  ID = new Guid("9f885257-5f15-4a4d-b5be-7d2f0bc0538e"),
                  ConversionFactorFromSIFormula = "Factors.Revolution*Factors.Degree*Factors.Hour"
                },
                new UnitChoice
                {
                  UnitName = "degree per minute",
                  UnitLabel = "°/min",
                  ID = new Guid("61bc0b74-78f4-486f-b725-03ae520a6e8c"),
                  ConversionFactorFromSIFormula = "Factors.Revolution*Factors.Degree*Factors.Minute"
                }
        };
        public FrequencyQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "frequency" };
            ID = new Guid("8a1ff3d9-95c9-43e1-abb4-4ae9b8df861e");
            DescriptionMD = string.Empty;
            DescriptionMD += @"Frequency is the number of occurrences of a repeating event per unit of time." + Environment.NewLine;
            DescriptionMD += @"The dimension of frequency is:" + Environment.NewLine;
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
