using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.UnitConversion.Conversion
{
    public partial class TorqueRateOfChangeQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "newton metre per second";
        public override string SIUnitLabelLatex { get; } = "\\frac{N \\cdot m}{s}";
        public override double LengthDimension { get; } = 2;
        public override double MassDimension { get; } = 1;
        public override double TimeDimension { get; } = -3;
        private static TorqueRateOfChangeQuantity instance_ = null;

        public static TorqueRateOfChangeQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new TorqueRateOfChangeQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
               new UnitChoice
                {
                  UnitName = "newton metre per second",
                  UnitLabel = "N•m/s",
                  ID = new Guid("0af9bebb-adde-49b9-bf0b-0d5002e454a2"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                  IsSI = true
                },
                new UnitChoice
                {
                  UnitName = "decanewton metre per second",
                  UnitLabel = "daN•m/s",
                  ID = new Guid("a6672d76-f845-47ce-9d67-7f1242ba9f60"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Deca)"
                },
                new UnitChoice
                {
                  UnitName = "kilogram force metre per second",
                  UnitLabel = "kgf•m/s",
                  ID = new Guid("6150a99d-34f0-438b-a8d3-038b8864c19f"),
                  ConversionFactorFromSIFormula = "1.0/ Factors.KilogramForce"
                },
                new UnitChoice
                {
                  UnitName = "kilonewton metre per second",
                  UnitLabel = "kN•m/s",
                  ID = new Guid("0875163d-a610-4e0d-8e05-5fe56896e44f"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Kilo"
                },
                new UnitChoice
                {
                  UnitName = "foot pound per second",
                  UnitLabel = "ft•lbf/s",
                  ID = new Guid("e53f2ab8-0883-4a6f-ae67-9f660eb20368"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Foot*Factors.PoundForce)"
                },
                new UnitChoice
                {
                  UnitName = "kilofoot pound per second",
                  UnitLabel = "kft•lbf/s",
                  ID = new Guid("6166acf4-c3bd-439c-b4a9-6f0282c18731"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Kilo*Factors.Foot*Factors.PoundForce)"
                },
                new UnitChoice
                {
                  UnitName = "newton decimetre per second",
                  UnitLabel = "N•dm/s",
                  ID = new Guid("7898b31c-821a-414d-9dcc-19822f3aef28"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Deci"
                },
                new UnitChoice
                {
                  UnitName = "newton centimetre per second",
                  UnitLabel = "N•cm/s",
                  ID = new Guid("13d51bb3-8f11-4fb5-a89a-0eac4fd26fe7"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Centi"
                },
                new UnitChoice
                {
                  UnitName = "newton millimetre per second",
                  UnitLabel = "N•mm/s",
                  ID = new Guid("173f9d03-f9c3-4d44-a889-52055887f8da"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Milli"
                },
                new UnitChoice
                {
                  UnitName = "inch pound per second",
                  UnitLabel = "in•lbf/s",
                  ID = new Guid("914875df-9234-40be-a5eb-f02e29e0457b"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.PoundForce*Factors.Inch)"
                },

                new UnitChoice
                {
                  UnitName = "newton metre per minute",
                  UnitLabel = "N•m/min",
                  ID = new Guid("8c3ab891-e5bc-4fa1-9f14-a3250e062ef4"),
                  ConversionFactorFromSIFormula = "Factors.Minute/Factors.Unit"
                },
                new UnitChoice
                {
                  UnitName = "decanewton metre per minute",
                  UnitLabel = "daN•m/min",
                  ID = new Guid("a1b76c0a-7ef2-46df-8d5b-0253a5dd42e8"),
                  ConversionFactorFromSIFormula = "Factors.Minute/(Factors.Deca)"
                },
                new UnitChoice
                {
                  UnitName = "kilogram force metre per minute",
                  UnitLabel = "kgf•m/min",
                  ID = new Guid("91fa2fa9-d0dc-4d26-b694-2eac6ee7ad92"),
                  ConversionFactorFromSIFormula = "Factors.Minute/ Factors.KilogramForce"
                },
                new UnitChoice
                {
                  UnitName = "kilonewton metre per minute",
                  UnitLabel = "kN•m/min",
                  ID = new Guid("746fdf99-afde-483d-88d4-e512b46efe3e"),
                  ConversionFactorFromSIFormula = "Factors.Minute/Factors.Kilo"
                },
                new UnitChoice
                {
                  UnitName = "foot pound per minute",
                  UnitLabel = "ft•lbf/min",
                  ID = new Guid("66567700-9838-48f0-aa21-672c21380f57"),
                  ConversionFactorFromSIFormula = "Factors.Minute/(Factors.Foot*Factors.PoundForce)"
                },
                new UnitChoice
                {
                  UnitName = "kilofoot pound per minute",
                  UnitLabel = "kft•lbf/min",
                  ID = new Guid("dfa752eb-792f-4eb1-9eaa-e32f497a1ea2"),
                  ConversionFactorFromSIFormula = "Factors.Minute/(Factors.Kilo*Factors.Foot*Factors.PoundForce)"
                },
                new UnitChoice
                {
                  UnitName = "newton decimetre per minute",
                  UnitLabel = "N•dm/min",
                  ID = new Guid("d4109f79-1724-4bba-8251-5847b5689037"),
                  ConversionFactorFromSIFormula = "Factors.Minute/Factors.Deci"
                },
                new UnitChoice
                {
                  UnitName = "newton centimetre per minute",
                  UnitLabel = "N•cm/min",
                  ID = new Guid("5d526e83-6a91-4230-97cd-054167a7a3d7"),
                  ConversionFactorFromSIFormula = "Factors.Minute/Factors.Centi"
                },
                new UnitChoice
                {
                  UnitName = "newton millimetre per minute",
                  UnitLabel = "N•mm/min",
                  ID = new Guid("5f1dbc46-8b88-4879-826f-f07836f018b8"),
                  ConversionFactorFromSIFormula = "Factors.Minute/Factors.Milli"
                },
                new UnitChoice
                {
                  UnitName = "inch pound per minute",
                  UnitLabel = "in•lbf/min",
                  ID = new Guid("72d76192-c058-44f6-b074-48291be96f5b"),
                  ConversionFactorFromSIFormula = "Factors.Minute/(Factors.PoundForce*Factors.Inch)"
                },

                new UnitChoice
                {
                  UnitName = "newton metre per hour",
                  UnitLabel = "N•m/h",
                  ID = new Guid("c17eae23-0496-4fa1-a389-aa24828fa243"),
                  ConversionFactorFromSIFormula = "Factors.Hour/Factors.Unit"
                },
                new UnitChoice
                {
                  UnitName = "decanewton metre per hour",
                  UnitLabel = "daN•m/h",
                  ID = new Guid("3907090e-c10e-41dc-95cf-634e1a28bb11"),
                  ConversionFactorFromSIFormula = "Factors.Hour/(Factors.Deca)"
                },
                new UnitChoice
                {
                  UnitName = "kilogram force metre per hour",
                  UnitLabel = "kgf•m/h",
                  ID = new Guid("6295ae90-c198-499e-a90d-8dba22c77584"),
                  ConversionFactorFromSIFormula = "Factors.Hour/ Factors.KilogramForce"
                },
                new UnitChoice
                {
                  UnitName = "kilonewton metre per hour",
                  UnitLabel = "kN•m/h",
                  ID = new Guid("e35e63cc-09dc-4d8a-bb5d-b0a0c24998f8"),
                  ConversionFactorFromSIFormula = "Factors.Hour/Factors.Kilo"
                },
                new UnitChoice
                {
                  UnitName = "foot pound per hour",
                  UnitLabel = "ft•lbf/h",
                  ID = new Guid("b55029ec-fadc-46da-a3b8-a48a08f4d92f"),
                  ConversionFactorFromSIFormula = "Factors.Hour/(Factors.Foot*Factors.PoundForce)"
                },
                new UnitChoice
                {
                  UnitName = "kilofoot pound per hour",
                  UnitLabel = "kft•lbf/h",
                  ID = new Guid("18156d8f-dd9f-4b63-a138-18827cafc14d"),
                  ConversionFactorFromSIFormula = "Factors.Hour/(Factors.Kilo*Factors.Foot*Factors.PoundForce)"
                },
                new UnitChoice
                {
                  UnitName = "newton decimetre per hour",
                  UnitLabel = "N•dm/h",
                  ID = new Guid("f19ae752-df39-48ae-bda3-c955116e5a01"),
                  ConversionFactorFromSIFormula = "Factors.Hour/Factors.Deci"
                },
                new UnitChoice
                {
                  UnitName = "newton centimetre per hour",
                  UnitLabel = "N•cm/h",
                  ID = new Guid("c7d197cf-831e-454d-aaf2-73399ff9afa2"),
                  ConversionFactorFromSIFormula = "Factors.Hour/Factors.Centi"
                },
                new UnitChoice
                {
                  UnitName = "newton millimetre per hour",
                  UnitLabel = "N•mm/h",
                  ID = new Guid("66c884c5-7596-4af7-919e-724b243336ae"),
                  ConversionFactorFromSIFormula = "Factors.Hour/Factors.Milli"
                },
                new UnitChoice
                {
                  UnitName = "inch pound per hour",
                  UnitLabel = "in•lbf/h",
                  ID = new Guid("64f6b76d-4a99-4379-bdf3-59b96e468e84"),
                  ConversionFactorFromSIFormula = "Factors.Hour/(Factors.PoundForce*Factors.Inch)"
                }
        };
        public TorqueRateOfChangeQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "torque rate of change", "bending moment rate of change" };
            ID = new Guid("e94ee582-62bd-472b-9188-1f423729e99e");
            DescriptionMD = string.Empty;
            DescriptionMD += @"A torque rate of change is the time derivative of a torque." + Environment.NewLine;
            DescriptionMD += @"The dimension of torque rate of change is:" + Environment.NewLine;
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
