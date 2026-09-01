using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class LengthQuantity : SymbolizedBasePhysicalQuantity
    {
        public override string DimensionSymbol { get; } = "L";
        public override string SIUnitName { get; } = "metre";

        public override string SIUnitLabelLatex { get; } = "m";
        private static LengthQuantity instance_ = null;

        public override double LengthDimension { get; } = 1;

        public static LengthQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new LengthQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
                {
                    UnitName = "metre",
                    UnitLabel = "m",
                    ID = new Guid("cc442e11-bb28-4e51-9074-87df66050d8a"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                    IsSI = true
                },
                new UnitChoice
                {
                    UnitName = "decimetre",
                    UnitLabel = "dm",
                    ID = new Guid("e84c1968-cc63-412e-82c1-93ed39a43c01"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Deci"
                },
                new UnitChoice
                {
                    UnitName = "centimetre",
                    UnitLabel = "cm",
                    ID = new Guid("96a3d4b4-c321-4528-92c0-7a52646b6461"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Centi"
                },
                new UnitChoice
                {
                    UnitName = "millimetre",
                    UnitLabel = "mm",
                    ID = new Guid("0b2094f1-ba22-4b7b-888a-7a6b5da2ba25"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "micrometre",
                    UnitLabel = "µm",
                    ID = new Guid("60820c6d-d721-49b8-ba40-a75343aa0f2f"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Micro"
                },
                new UnitChoice
                {
                    UnitName = "nanometre",
                    UnitLabel = "nm",
                    ID = new Guid("0d181caf-8121-46a8-bfa7-2cb7457d9db9"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Nano"
                },
                new UnitChoice
                {
                    UnitName = "ångstrøm",
                    UnitLabel = "Å",
                    ID = new Guid("0db6a73f-c58f-415c-ac0d-e56137b28d5a"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Angstrom"
                },
                new UnitChoice
                {
                    UnitName = "picometre",
                    UnitLabel = "pm",
                    ID = new Guid("f305ce05-7bd1-4d67-a834-e9b932ca586e"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Pico"
                },
                new UnitChoice
                {
                    UnitName = "decametre",
                    UnitLabel = "dam",
                    ID = new Guid("0ff9e118-e7d5-4ace-b140-eb3383812b21"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Deca"
                },
                new UnitChoice
                {
                    UnitName = "hectometre",
                    UnitLabel = "hm",
                    ID = new Guid("cb5c81a3-b9da-42b5-a2ea-0509df445d01"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Hecto"
                },
                new UnitChoice
                {
                    UnitName = "kilometre",
                    UnitLabel = "km",
                    ID = new Guid("93aee1b8-653d-4841-b948-10460cb84334"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Kilo"
                },
                new UnitChoice
                {
                    UnitName = "astronomical unit",
                    UnitLabel = "au",
                    ID = new Guid("0471d74c-cc44-45bd-be0a-aaad6c05f0d0"),
                    ConversionFactorFromSIFormula = "1.0/Factors.AstronomicalUnit"
                },
                new UnitChoice
                {
                    UnitName = "light year",
                    UnitLabel = "ly",
                    ID = new Guid("fc43d439-576f-430c-855e-60b28f70856e"),
                    ConversionFactorFromSIFormula = "1.0/Factors.LightYear"
                },
                new UnitChoice
                {
                    UnitName = "parsec",
                    UnitLabel = "pc",
                    ID = new Guid("0565c7e8-11cb-48de-8d7a-d58c89955d0f"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Parsec"
                },
                new UnitChoice
                {
                    UnitName = "foot",
                    UnitLabel = "ft",
                    ID = new Guid("b4adebce-d0cd-417a-b38c-ab4a2e38233a"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Foot"
                },
                new UnitChoice
                {
                    UnitName = "US survey foot",
                    UnitLabel = "ft",
                    ID = new Guid("eaf5909f-c68e-4346-9517-1dafad48b161"),
                    ConversionFactorFromSIFormula = "1.0/Factors.USSurveyFoot"
                },
                new UnitChoice
                {
                    UnitName = "inch",
                    UnitLabel = "in",
                    ID = new Guid("0a6e2349-6f90-4ac5-baed-ccdaf5e5b919"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Inch"
                },
                new UnitChoice
                {
                    UnitName = "yard",
                    UnitLabel = "yd",
                    ID = new Guid("7b9156e4-7cce-41cf-a251-95412f4d91a5"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Yard"
                },
                new UnitChoice
                {
                    UnitName = "fathom",
                    UnitLabel = "fathom",
                    ID = new Guid("6be602af-ea19-41cc-af7f-8263564c3c3b"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Fathom"
                },
                new UnitChoice
                {
                    UnitName = "surveyor's chain",
                    UnitLabel = "chain",
                    ID = new Guid("f101708b-ab63-4f21-ac87-4b5b3615eb30"),
                    ConversionFactorFromSIFormula = "1.0/Factors.SurveyorChain"
                },
                new UnitChoice
                {
                    UnitName = "mile",
                    UnitLabel = "mi",
                    ID = new Guid("95736fd3-878b-4d93-9a78-ee6f20619628"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Mile"
                },
                new UnitChoice
                {
                    UnitName = "international nautical mile",
                    UnitLabel = "nmi",
                    ID = new Guid("4c297035-e0cf-4bfe-aa63-d835170e8e25"),
                    ConversionFactorFromSIFormula = "1.0/Factors.InternationalNauticalMile"
                },
                new UnitChoice
                {
                    UnitName = "UK nautical mile",
                    UnitLabel = "UK nmi",
                    ID = new Guid("3b7a50d6-dc58-4cd7-9a5b-96dba59eceaa"),
                    ConversionFactorFromSIFormula = "1.0/Factors.UKNauticalMile"
                },
                new UnitChoice
                {
                    UnitName = "scandinavian mile",
                    UnitLabel = "mil",
                    ID = new Guid("22e6763c-4105-4a4c-9dfe-044256a107d1"),
                    ConversionFactorFromSIFormula = "1.0/Factors.ScandinavianMile"
                },
                new UnitChoice
                {
                    UnitName = "inch per 32",
                    UnitLabel = "in/32",
                    ID = new Guid("758ae28a-7484-4e0c-91af-aa105bcd744f"),
                    ConversionFactorFromSIFormula = "1.0/Factors.InchPer32"
                },
                new UnitChoice
                {
                    UnitName = "mil",
                    UnitLabel = "mil",
                    ID = new Guid("648502a7-47e0-42dc-aacc-2e1789b0f1ce"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Mil"
                },
                new UnitChoice
                {
                    UnitName = "thou",
                    UnitLabel = "thou",
                    ID = new Guid("2ce1b2d1-8157-4ad5-ae85-5d6c634f5c68"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Thou"
                },
                new UnitChoice
                {
                    UnitName = "hand",
                    UnitLabel = "hand",
                    ID = new Guid("608205f3-8c52-40f2-9796-a586d1cd62da"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Hand"
                },
                new UnitChoice
                {
                    UnitName = "furlong",
                    UnitLabel = "furlong",
                    ID = new Guid("7ce130d1-8147-409f-99b3-bf22b0aae4cc"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Furlong"
                }
        };
        public LengthQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "length" };
            ID = new Guid("96058475-80c4-4394-b21a-afd2fb1594c8");
            DescriptionMD = string.Empty;
            DescriptionMD += @"Length is a measure of distance." + Environment.NewLine;
            DescriptionMD += @"The dimension of length is:" + Environment.NewLine;
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
