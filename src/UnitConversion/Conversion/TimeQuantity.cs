using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class TimeQuantity : SymbolizedBasePhysicalQuantity
    {
        public override string DimensionSymbol { get; } = "t";
        public override string SIUnitName { get; } = "second";
        public override string SIUnitLabelLatex { get; } = "s";
        private static TimeQuantity instance_ = null;
        public override double TimeDimension { get; } = 1;

        public static TimeQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new TimeQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
               new UnitChoice
                {
                    UnitName = "second",
                    UnitLabel = "s",
                    ID = new Guid("eac42b09-cc85-4e29-9aaf-05fe73bca2aa"),
                    ConversionFactorFromSIFormula = "1.0",
                    IsSI = true
                },
                new UnitChoice
                {
                    UnitName = "millisecond",
                    UnitLabel = "ms",
                    ID = new Guid("1c1b150f-80a0-47da-836c-a583c4fa4b74"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "microsecond",
                    UnitLabel = "µs",
                    ID = new Guid("18cf5c8f-bdd4-4575-a74b-f8321c567edb"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Micro"
                },
                new UnitChoice
                {
                    UnitName = "shake",
                    UnitLabel = "shake",
                    ID = new Guid("f2b06fdd-ddfe-430b-8107-11597bdfdf2f"),
                    ConversionFactorFromSIFormula = "1.0/(10.0*Factors.Nano)"
                },
                new UnitChoice
                {
                    UnitName = "nanosecond",
                    UnitLabel = "ns",
                    ID = new Guid("6bf1a718-637c-4e86-ae3e-426d2a1a9195"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Nano"
                },
                 new UnitChoice
                {
                    UnitName = "picosecond",
                    UnitLabel = "ps",
                    ID = new Guid("9e10f905-4dc5-4670-9adf-278afd7d4131"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Pico"
                },
                new UnitChoice
                {
                    UnitName = "minute",
                    UnitLabel = "min",
                    ID = new Guid("4b9dc241-388b-4b7a-b862-48db43234c4a"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Minute"
                },
                new UnitChoice
                {
                    UnitName = "hour",
                    UnitLabel = "h",
                    ID = new Guid("f0d815e4-9bef-4422-94e6-1de52216b611"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Hour"
                },
                new UnitChoice
                {
                    UnitName = "day",
                    UnitLabel = "d",
                    ID = new Guid("85442621-bb56-4e2a-8e77-2b72409ff84f"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Day"
                },
                new UnitChoice
                {
                    UnitName = "week",
                    UnitLabel = "week",
                    ID = new Guid("4dd50f01-60b3-4d44-82ea-ff8ededd797d"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Week"
                },
                new UnitChoice
                {
                    UnitName = "fortnight",
                    UnitLabel = "14d",
                    ID = new Guid("bc87f864-3dc1-4f1a-87bc-4123a47c53dc"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Fortnight"
                },
                new UnitChoice
                {
                    UnitName = "month common",
                    UnitLabel = "month common",
                    ID = new Guid("41cceaa2-1a1d-40f1-9195-5183be9770d4"),
                    ConversionFactorFromSIFormula = "1.0/Factors.MonthCommon"
                },
                new UnitChoice
                {
                    UnitName = "month sideral",
                    UnitLabel = "month sideral",
                    ID = new Guid("2e7446c0-5b0e-44e1-9a27-f0bc7d8aeb98"),
                    ConversionFactorFromSIFormula = "1.0/Factors.MonthSideral"
                },
                new UnitChoice
                {
                    UnitName = "month synodic",
                    UnitLabel = "month synodic",
                    ID = new Guid("31edcda9-df8f-4d15-83a9-7dafd8a7e404"),
                    ConversionFactorFromSIFormula = "1.0/Factors.MonthSynodic"
                },
                new UnitChoice
                {
                    UnitName = "quarter common",
                    UnitLabel = "quarter common",
                    ID = new Guid("71f0e01a-c1a2-49ba-a25b-c11854f8867c"),
                    ConversionFactorFromSIFormula = "1.0/Factors.QuarterCommon"
                },
                new UnitChoice
                {
                    UnitName = "year common",
                    UnitLabel = "y",
                    ID = new Guid("38481414-3b9d-472d-ac31-04b00dcc9d5c"),
                    ConversionFactorFromSIFormula = "1.0/Factors.YearCommon"
                },
                new UnitChoice
                {
                    UnitName = "year average gregorian",
                    UnitLabel = "year average gregorian",
                    ID = new Guid("fc33008b-9517-440f-a56b-189c5d80621b"),
                    ConversionFactorFromSIFormula = "1.0/Factors.YearAverageGregorian"
                },
                new UnitChoice
                {
                    UnitName = "year julian",
                    UnitLabel = "year julian",
                    ID = new Guid("281f6c7b-da23-4aab-89e1-994e52280658"),
                    ConversionFactorFromSIFormula = "1.0/Factors.YearJulian"
                },
                new UnitChoice
                {
                    UnitName = "year leap",
                    UnitLabel = "year leap",
                    ID = new Guid("c84c1293-82d4-481b-8f6e-8bb8b81e6273"),
                    ConversionFactorFromSIFormula = "1.0/Factors.YearLeap"
                },
                new UnitChoice
                {
                    UnitName = "year tropical",
                    UnitLabel = "year tropical",
                    ID = new Guid("e93c0b95-68b1-4ecc-9b27-f07a9a53ad49"),
                    ConversionFactorFromSIFormula = "1.0/Factors.YearTropical"
                },
                new UnitChoice
                {
                    UnitName = "decade",
                    UnitLabel = "decade",
                    ID = new Guid("b7d3b041-7119-45a6-a5ea-e05d7cb3c68b"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Decade"
                },
                new UnitChoice
                {
                    UnitName = "century",
                    UnitLabel = "century",
                    ID = new Guid("5cf296b6-48bf-4cc1-bf79-0220100ef1db"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Century"
                },
                new UnitChoice
                {
                    UnitName = "millenia",
                    UnitLabel = "millenia",
                    ID = new Guid("c235c8fc-f15b-45ff-a1b7-aab46ccea159"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Millenia"
                },
                new UnitChoice
                {
                    UnitName = "million year",
                    UnitLabel = "My",
                    ID = new Guid("d0281a3c-86dd-4d05-b856-cb7fa67c0f4d"),
                    ConversionFactorFromSIFormula = "1.0/Factors.MillionYear"
                }
        };
        public TimeQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "time" };
            ID = new Guid("7106f7cb-ddf2-4e2f-9e21-b19bc83eb248");
            DescriptionMD = string.Empty;
            DescriptionMD += @"Time is a continuous, measurable progression in which events occur, from the past through the present to the future." + Environment.NewLine;
            DescriptionMD += @"The dimension of time is:" + Environment.NewLine;
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
