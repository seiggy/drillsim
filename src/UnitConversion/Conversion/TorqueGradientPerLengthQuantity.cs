using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class TorqueGradientPerLengthQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "newton metre per metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{N \\cdot m}{m}";
        public override double LengthDimension { get; } = 1;
        public override double MassDimension { get; } = 1;
        public override double TimeDimension { get; } = -2;
        private static TorqueGradientPerLengthQuantity instance_ = null;

        public static TorqueGradientPerLengthQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new TorqueGradientPerLengthQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
                new UnitChoice
                {
                    UnitName = "newton metre per metre",
                    UnitLabel = "N•m/m",
                    ID = new Guid("33baa8d7-6987-4217-959b-1e3aa5b04752"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                    IsSI = true
                },
                new UnitChoice
                {
                    UnitName = "decanewton metre per metre",
                    UnitLabel = "daN•m/m",
                    ID = new Guid("50a1ea8d-9a46-4e24-9e9f-dad66e8bb9ca"),
                    ConversionFactorFromSIFormula = "Factors.Unit/Factors.Deca"
                },
                new UnitChoice
                {
                  UnitName = "kilogram force metre per metre",
                  UnitLabel = "kgf•m/m",
                  ID = new Guid("66f7449d-5a06-4dd0-bf27-0bed2d2e4bed"),
                  ConversionFactorFromSIFormula = "Factors.Unit/(Factors.KilogramForce)"
                },
                new UnitChoice
                {
                  UnitName = "kilonewton metre per metre",
                  UnitLabel = "kN•m/m",
                  ID = new Guid("d07e4c6c-fea3-4545-b020-9cc2402e1ca5"),
                  ConversionFactorFromSIFormula = "Factors.Unit/Factors.Kilo"
                },
                new UnitChoice
                {
                  UnitName = "foot pound per metre",
                  UnitLabel = "ft•lbf/m",
                  ID = new Guid("e3b4fe22-6590-4b2d-b2fa-0250f1ca8b26"),
                  ConversionFactorFromSIFormula = "Factors.Unit/(Factors.PoundForce*Factors.Foot)"
                },
                new UnitChoice
                {
                  UnitName = "kilofoot pound per metre",
                  UnitLabel = "kft•lbf/m",
                  ID = new Guid("e9bff76e-5388-4ea0-85af-62c772d919c5"),
                  ConversionFactorFromSIFormula = "Factors.Unit/(Factors.Kilo*Factors.PoundForce*Factors.Foot)"
                },
                new UnitChoice
                {
                  UnitName = "newton decimetre per metre",
                  UnitLabel = "N•dm/m",
                  ID = new Guid("87ef9e2b-7e3b-4bda-a406-9f0b7f06e8fa"),
                  ConversionFactorFromSIFormula = "Factors.Unit/Factors.Deci"
                },
                new UnitChoice
                {
                  UnitName = "newton centimetre per metre",
                  UnitLabel = "N•cm/m",
                  ID = new Guid("c03b845d-f2e5-4a16-afca-efab2591c526"),
                  ConversionFactorFromSIFormula = "Factors.Unit/Factors.Centi"
                },
                new UnitChoice
                {
                  UnitName = "newton millimetre per metre",
                  UnitLabel = "N•mm/m",
                  ID = new Guid("fb1bb6bb-9c4a-4ecd-99fc-4af502271614"),
                  ConversionFactorFromSIFormula = "Factors.Unit/Factors.Milli"
                },
                new UnitChoice
                {
                  UnitName = "inch pound per metre",
                  UnitLabel = "in•lbf/m",
                  ID = new Guid("18259cf1-1f96-4ace-b7ad-657a78254baf"),
                  ConversionFactorFromSIFormula = "Factors.Unit/(Factors.Inch*Factors.PoundForce)"
                },
                new UnitChoice
                {
                    UnitName = "Newton metre per decimetre",
                    UnitLabel = "N•m/dm",
                    ID = new Guid("5ceaa09f-0de4-4025-ba23-d3b76f55a8b1"),
                    ConversionFactorFromSIFormula = "Factors.Deci/Factors.Unit"
                },
                new UnitChoice
                {
                    UnitName = "decanewton metre per decimetre",
                    UnitLabel = "daN•m/dm",
                    ID = new Guid("902dd7b3-0b4f-40a3-a089-02bb39367219"),
                    ConversionFactorFromSIFormula = "Factors.Deci/Factors.Deca"
                },
                new UnitChoice
                {
                  UnitName = "kilogram force metre per decimetre",
                  UnitLabel = "kgf•m/dm",
                  ID = new Guid("2c83dced-5b36-49f1-bd4a-c95d558fb868"),
                  ConversionFactorFromSIFormula = "Factors.Deci/(Factors.KilogramForce)"
                },
                new UnitChoice
                {
                  UnitName = "kilonewton metre per decimetre",
                  UnitLabel = "kN•m/dm",
                  ID = new Guid("a3949720-a023-4c5d-9a5e-4194af30005f"),
                  ConversionFactorFromSIFormula = "Factors.Deci/(Factors.Kilo*Factors.Unit)"
                },
                new UnitChoice
                {
                  UnitName = "foot pound per decimetre",
                  UnitLabel = "ft•lbf/dm",
                  ID = new Guid("a6b11edb-e9bc-4a7e-9af9-ace7ca62b93b"),
                  ConversionFactorFromSIFormula = "Factors.Deci/(Factors.Foot*Factors.PoundForce)"
                },
                new UnitChoice
                {
                  UnitName = "kilofoot pound per decimetre",
                  UnitLabel = "kft•lbf/dm",
                  ID = new Guid("92cb8e61-c58d-461a-a69c-ad9fe7324e2a"),
                  ConversionFactorFromSIFormula = "Factors.Deci/(Factors.Kilo*Factors.Foot*Factors.PoundForce)"
                },
                new UnitChoice
                {
                  UnitName = "newton decimetre per decimetre",
                  UnitLabel = "N•dm/dm",
                  ID = new Guid("61261367-5cb0-4038-bcfb-6c8395758a21"),
                  ConversionFactorFromSIFormula = "Factors.Deci/(Factors.Deci*Factors.Unit)"
                },
                new UnitChoice
                {
                  UnitName = "newton centimetre per decimetre",
                  UnitLabel = "N•cm/dm",
                  ID = new Guid("1eee8ef4-cb7b-451a-9658-d1704ccf81d2"),
                  ConversionFactorFromSIFormula = "Factors.Deci/(Factors.Centi*Factors.Unit)"
                },
                new UnitChoice
                {
                  UnitName = "newton millimetre per decimetre",
                  UnitLabel = "N•mm/dm",
                  ID = new Guid("6c49e6aa-7d4c-4c92-96e5-5e3f26c3a367"),
                  ConversionFactorFromSIFormula = "Factors.Deci/(Factors.Milli*Factors.Unit)"
                },
                new UnitChoice
                {
                  UnitName = "inch pound per decimetre",
                  UnitLabel = "in•lbf/dm",
                  ID = new Guid("e638f5ee-bbf9-4e7b-ae6a-9613eb9792cc"),
                  ConversionFactorFromSIFormula ="Factors.Deci/(Factors.Inch*Factors.PoundForce)"
                },
                new UnitChoice
                {
                    UnitName = "Newton metre per centimetre",
                    UnitLabel = "N•m/cm",
                    ID = new Guid("d8c63694-bf19-48be-b9bd-0f5b462ce2ec"),
                    ConversionFactorFromSIFormula = "Factors.Centi/(Factors.Unit*Factors.Unit)"
                },
                new UnitChoice
                {
                    UnitName = "decanewton metre per centimetre",
                    UnitLabel = "daN•m/cm",
                    ID = new Guid("e87d21e6-2191-4cee-aea8-1929df1d8bd0"),
                    ConversionFactorFromSIFormula = "Factors.Centi/(Factors.Deca)"
                },
                new UnitChoice
                {
                  UnitName = "kilogram force metre per centimetre",
                  UnitLabel = "kgf•m/cm",
                  ID = new Guid("6ee38a6b-907d-4de9-94f1-0d979ef58340"),
                  ConversionFactorFromSIFormula = "Factors.Centi/(Factors.KilogramForce)"
                },
                new UnitChoice
                {
                  UnitName = "kilonewton metre per centimetre",
                  UnitLabel = "kN•m/cm",
                  ID = new Guid("2149c60b-d6a8-4056-9818-f7fe6d10c409"),
                  ConversionFactorFromSIFormula = "Factors.Centi/(Factors.Kilo)"
                },
                new UnitChoice
                {
                  UnitName = "foot pound per centimetre",
                  UnitLabel = "ft•lbf/cm",
                  ID = new Guid("730e5c03-816e-4f88-b7bf-632a8a30c3ca"),
                  ConversionFactorFromSIFormula = "Factors.Centi/(Factors.Foot*Factors.PoundForce)"
                },
                new UnitChoice
                {
                  UnitName = "kilofoot pound per centimetre",
                  UnitLabel = "kft•lbf/cm",
                  ID = new Guid("6249a894-93d9-45b6-a188-eb2d4bef800e"),
                  ConversionFactorFromSIFormula = "Factors.Centi/(Factors.Kilo*Factors.Foot*Factors.PoundForce)"
                },
                new UnitChoice
                {
                  UnitName = "newton decimetre per centimetre",
                  UnitLabel = "N•dm/cm",
                  ID = new Guid("0dbdd140-66d3-4f47-bbea-f5025b804b20"),
                  ConversionFactorFromSIFormula = "Factors.Centi/(Factors.Deci)"
                },
                new UnitChoice
                {
                  UnitName = "newton centimetre per centimetre",
                  UnitLabel = "N•cm/cm",
                  ID = new Guid("2f7c8e32-f865-4b68-8a3c-4d8c862fd5f2"),
                  ConversionFactorFromSIFormula = "Factors.Centi/Factors.Centi"
                },
                new UnitChoice
                {
                  UnitName = "newton millimetre per centimetre",
                  UnitLabel = "N•mm/cm",
                  ID = new Guid("830ae4b8-76d5-404a-b0c7-90db357a68ec"),
                  ConversionFactorFromSIFormula = "Factors.Centi/Factors.Milli"
                },
                new UnitChoice
                {
                  UnitName = "inch pound per centimetre",
                  UnitLabel = "in•lbf/cm",
                  ID = new Guid("6822172d-adf0-4a71-b883-f4bc825ee9ea"),
                  ConversionFactorFromSIFormula = "Factors.Centi/(Factors.Inch*Factors.PoundForce)"
                },
                new UnitChoice
                {
                    UnitName = "Newton metre per millimetre",
                    UnitLabel = "N•m/mm",
                    ID = new Guid("a6416087-d525-4d98-aa45-2006ceb4a474"),
                    ConversionFactorFromSIFormula = "Factors.Milli/(Factors.Unit*Factors.Unit)"
                },
                new UnitChoice
                {
                    UnitName = "decanewton metre per millimetre",
                    UnitLabel = "daN•m/mm",
                    ID = new Guid("6acab23d-3952-4eed-b1a0-7c38f03109b0"),
                    ConversionFactorFromSIFormula = "Factors.Milli/(Factors.Deca)"
                },
                new UnitChoice
                {
                  UnitName = "kilogram force metre per millimetre",
                  UnitLabel = "kgf•m/mm",
                  ID = new Guid("cdd4e6aa-ee0b-4679-97be-82553960efd1"),
                  ConversionFactorFromSIFormula = "Factors.Milli/(Factors.KilogramForce)"
                },
                new UnitChoice
                {
                  UnitName = "kilonewton metre per millimetre",
                  UnitLabel = "kN•m/mm",
                  ID = new Guid("1335bebf-fcda-4a39-afa8-7de3ed24fa0c"),
                  ConversionFactorFromSIFormula = "Factors.Milli/(Factors.Kilo)"
                },
                new UnitChoice
                {
                  UnitName = "foot pound per millimetre",
                  UnitLabel = "ft•lbf/mm",
                  ID = new Guid("73a284d2-8900-44bb-96ab-897416a525e1"),
                  ConversionFactorFromSIFormula = "Factors.Milli/(Factors.Foot*Factors.PoundForce)"
                },
                new UnitChoice
                {
                  UnitName = "kilofoot pound per millimetre",
                  UnitLabel = "kft•lbf/mm",
                  ID = new Guid("bad7b651-25f9-4687-875f-7624388228d6"),
                  ConversionFactorFromSIFormula = "Factors.Milli/(Factors.Kilo*Factors.Foot*Factors.PoundForce)"
                },
                new UnitChoice
                {
                  UnitName = "newton decimetre per millimetre",
                  UnitLabel = "N•dm/mm",
                  ID = new Guid("ecfa262e-1242-4c36-8325-b98de1ef4ffd"),
                  ConversionFactorFromSIFormula = "Factors.Milli/(Factors.Deci)"
                },
                new UnitChoice
                {
                  UnitName = "newton centimetre per millimetre",
                  UnitLabel = "N•cm/mm",
                  ID = new Guid("0f0cd3a8-84ec-4b58-b100-3f413bea1e05"),
                  ConversionFactorFromSIFormula = "Factors.Milli/Factors.Centi"
                },
                new UnitChoice
                {
                  UnitName = "newton millimetre per millimetre",
                  UnitLabel = "N•mm/mm",
                  ID = new Guid("b20d7cec-c1f6-4f64-9b60-e77ea699d940"),
                  ConversionFactorFromSIFormula = "Factors.Milli/Factors.Milli"
                },
                new UnitChoice
                {
                  UnitName = "inch pound per millimetre",
                  UnitLabel = "in•lbf/mm",
                  ID = new Guid("cd28eba2-c7ea-40d0-ac32-fb67a8f581bc"),
                  ConversionFactorFromSIFormula = "Factors.Milli/(Factors.Inch*Factors.PoundForce)"
                },
                new UnitChoice
                {
                    UnitName = "Newton metre per foot",
                    UnitLabel = "N•m/ft",
                    ID = new Guid("2ce8e697-3a8a-4a73-ac23-4730790b4812"),
                    ConversionFactorFromSIFormula = "Factors.Foot/Factors.Unit"
                },
                new UnitChoice
                {
                    UnitName = "decanewton metre per foot",
                    UnitLabel = "daN•m/ft",
                    ID = new Guid("c6e8f7e7-0239-47bc-b230-0f5870c94b82"),
                    ConversionFactorFromSIFormula = "Factors.Foot/(Factors.Deca)"
                },
                new UnitChoice
                {
                  UnitName = "kilogram force metre per foot",
                  UnitLabel = "kgf•m/ft",
                  ID = new Guid("7b2d82cc-0ac5-4945-9914-c91e62ac61dd"),
                  ConversionFactorFromSIFormula = "Factors.Foot/Factors.KilogramForce"
                },
                new UnitChoice
                {
                  UnitName = "kilonewton metre per foot",
                  UnitLabel = "kN•m/ft",
                  ID = new Guid("8b5be3db-bc7a-4107-b751-53d3d2772eb8"),
                  ConversionFactorFromSIFormula = "Factors.Foot/Factors.Kilo"
                },
                new UnitChoice
                {
                  UnitName = "foot pound per foot",
                  UnitLabel = "ft•lbf/ft",
                  ID = new Guid("85a75741-c967-4e10-b195-01e5e7297eda"),
                  ConversionFactorFromSIFormula = "Factors.Foot/(Factors.Foot*Factors.PoundForce)"
                },
                new UnitChoice
                {
                  UnitName = "kilofoot pound per foot",
                  UnitLabel = "kft•lbf/ft",
                  ID = new Guid("65606e85-199d-4fee-8cd4-97715431d868"),
                  ConversionFactorFromSIFormula ="Factors.Foot/(Factors.Kilo*Factors.Foot*Factors.PoundForce)"
                },
                new UnitChoice
                {
                  UnitName = "newton decimetre per foot",
                  UnitLabel = "N•dm/ft",
                  ID = new Guid("cdd6f7a0-954c-4826-8263-bb2d7fb06764"),
                  ConversionFactorFromSIFormula = "Factors.Foot/(Factors.Deci)"
                },
                new UnitChoice
                {
                  UnitName = "newton centimetre per foot",
                  UnitLabel = "N•cm/ft",
                  ID = new Guid("1767c385-e868-457a-b0be-aac0a4db42ff"),
                  ConversionFactorFromSIFormula = "Factors.Foot/Factors.Centi"
                },
                new UnitChoice
                {
                  UnitName = "newton millimetre per foot",
                  UnitLabel = "N•mm/ft",
                  ID = new Guid("ba4691ad-7575-4b9d-a6c6-9c98dde239ca"),
                  ConversionFactorFromSIFormula = "Factors.Foot/Factors.Milli"
                },
                new UnitChoice
                {
                  UnitName = "inch pound per foot",
                  UnitLabel = "in•lbf/ft",
                  ID = new Guid("d5f1dfc6-80ec-4780-a4ad-8df68c179eee"),
                  ConversionFactorFromSIFormula = "Factors.Foot/(Factors.Inch*Factors.PoundForce)"
                },
                new UnitChoice
                {
                    UnitName = "Newton metre per inch",
                    UnitLabel = "N•m/in",
                    ID = new Guid("394cc997-ae71-47d1-91be-8aa69fdb71d7"),
                    ConversionFactorFromSIFormula = "Factors.Inch/Factors.Unit"
                },
                new UnitChoice
                {
                    UnitName = "decanewton metre per inch",
                    UnitLabel = "daN•m/in",
                    ID = new Guid("1f8786b1-4aae-45b9-9875-7fe63bddb6cb"),
                    ConversionFactorFromSIFormula = "Factors.Inch/Factors.Deca"
                },
                new UnitChoice
                {
                  UnitName = "kilogram force metre per inch",
                  UnitLabel = "kgf•m/in",
                  ID = new Guid("df62fa6d-b983-4960-ad7b-853e00ccf45c"),
                  ConversionFactorFromSIFormula = "Factors.Inch/Factors.KilogramForce"
                },
                new UnitChoice
                {
                  UnitName = "kilonewton metre per inch",
                  UnitLabel = "kN•m/in",
                  ID = new Guid("cbc02a54-98e8-4b10-a028-152a5c92f2ce"),
                  ConversionFactorFromSIFormula = "Factors.Inch/Factors.Kilo"
                },
                new UnitChoice
                {
                  UnitName = "foot pound per inch",
                  UnitLabel = "ft•lbf/in",
                  ID = new Guid("9c9771ff-d194-4e9f-b663-7f817da4d207"),
                  ConversionFactorFromSIFormula = "Factors.Inch/(Factors.Foot*Factors.PoundForce)"
                },
                new UnitChoice
                {
                  UnitName = "kilofoot pound per inch",
                  UnitLabel = "kft•lbf/in",
                  ID = new Guid("70978437-cf05-4138-8389-ec633fdc1fce"),
                  ConversionFactorFromSIFormula = "Factors.Inch/(Factors.Kilo*Factors.Foot*Factors.PoundForce)"
                },
                new UnitChoice
                {
                  UnitName = "newton decimetre per inch",
                  UnitLabel = "N•dm/in",
                  ID = new Guid("0f34a9e2-dc04-4ce7-ac42-6cc12e2c98b5"),
                  ConversionFactorFromSIFormula = "Factors.Inch/Factors.Deci"
                },
                new UnitChoice
                {
                  UnitName = "newton centimetre per inch",
                  UnitLabel = "N•cm/in",
                  ID = new Guid("622d36f4-6fed-4d16-a246-14477f724bca"),
                  ConversionFactorFromSIFormula = "Factors.Inch/Factors.Centi"
                },
                new UnitChoice
                {
                  UnitName = "newton millimetre per inch",
                  UnitLabel = "N•mm/in",
                  ID = new Guid("31169945-1b67-4225-a2c6-6e850c39cb2f"),
                  ConversionFactorFromSIFormula = "Factors.Inch/Factors.Milli"
                },
                new UnitChoice
                {
                  UnitName = "inch pound per inch",
                  UnitLabel = "in•lbf/in",
                  ID = new Guid("2e16e0be-2037-413d-9f43-6316a24d1fca"),
                  ConversionFactorFromSIFormula = "Factors.Inch/(Factors.Inch*Factors.PoundForce)"
                }
        };
        public TorqueGradientPerLengthQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "torque gradient per length" };
            ID = new Guid("002104cd-25d6-438d-afe3-065ff392b294");
            DescriptionMD = string.Empty;
            DescriptionMD += @"A torque gradient per length is the first derivative of torque compared to a distance: $\frac{d\tau}{ds}$, where $\tau$ is a torque and $s$ is a distance." + Environment.NewLine;
            DescriptionMD += @"The dimension of torque gradient per length is:" + Environment.NewLine;
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