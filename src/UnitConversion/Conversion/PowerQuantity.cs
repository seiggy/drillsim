using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.UnitConversion.Conversion
{
    public partial class PowerQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "watt";
        public override string SIUnitLabelLatex { get; } = "W";
        public override double LengthDimension { get; } = 2;
        public override double MassDimension { get; } = 1;
        public override double TimeDimension { get; } = -3;
        private static PowerQuantity instance_ = null;

        public static PowerQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new PowerQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>() {
                   new UnitChoice
                   {
                       UnitName = "watt",
                       UnitLabel = "W",
                       ID = new Guid("9d986a0c-700f-4448-a48c-a028bbd22049"),
                       ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                       IsSI = true
                   },
                   new UnitChoice
                   {
                       UnitName = "decawatt",
                       UnitLabel = "daW",
                       ID = new Guid("fa888306-f2ef-420a-9ce2-8c56fe64ea3c"),
                       ConversionFactorFromSIFormula = "1.0/Factors.Deca"
                   },
                   new UnitChoice
                   {
                       UnitName = "hectowatt",
                       UnitLabel = "hW",
                       ID = new Guid("1f159f0d-635a-4bc8-9020-6c09d72b3f63"),
                       ConversionFactorFromSIFormula = "1.0/Factors.Hecto"
                   },
                   new UnitChoice
                   {
                       UnitName = "kilowatt",
                       UnitLabel = "kW",
                       ID = new Guid("016b23c7-0231-45bb-8723-5d3d4cc5c054"),
                       ConversionFactorFromSIFormula = "1.0/Factors.Kilo"
                   },
                   new UnitChoice
                   {
                       UnitName = "megawatt",
                       UnitLabel = "MW",
                       ID = new Guid("5719b5f3-5c24-46d2-8ccd-0a06cc6b49ae"),
                       ConversionFactorFromSIFormula = "1.0/Factors.Mega"
                   },
                   new UnitChoice
                   {
                       UnitName = "gigawatt",
                       UnitLabel = "GW",
                       ID = new Guid("ba67ba92-cdf5-46a8-a5f5-56c1ad102417"),
                       ConversionFactorFromSIFormula = "1.0/Factors.Giga"
                   },
                   new UnitChoice
                   {
                       UnitName = "terawatt",
                       UnitLabel = "TW",
                       ID = new Guid("b3e60a20-9e0f-479b-903b-16b22d86a515"),
                       ConversionFactorFromSIFormula = "1.0/Factors.Tera"
                   },
                   new UnitChoice
                   {
                       UnitName = "petawatt",
                       UnitLabel = "PW",
                       ID = new Guid("bafba6b7-8a58-46b0-b4c7-c9a008c5e8f4"),
                       ConversionFactorFromSIFormula = "1.0/Factors.Peta"
                   },
                   new UnitChoice
                   {
                       UnitName = "exawatt",
                       UnitLabel = "EW",
                       ID = new Guid("457950e4-0d4c-4f18-87ae-c35a7d2f512a"),
                       ConversionFactorFromSIFormula = "1.0/Factors.Exa"
                   },
                   new UnitChoice
                   {
                       UnitName = "deciwatt",
                       UnitLabel = "dW",
                       ID = new Guid("6a3cd886-1c2c-41c8-8214-b21aff588b1e"),
                       ConversionFactorFromSIFormula = "1.0/Factors.Deci"
                   },
                   new UnitChoice
                   {
                       UnitName = "centiwatt",
                       UnitLabel = "cW",
                       ID = new Guid("ac6c67e1-0912-44f2-9496-ed82aca2b925"),
                       ConversionFactorFromSIFormula = "1.0/Factors.Centi"
                   },
                   new UnitChoice
                   {
                       UnitName = "milliwatt",
                       UnitLabel = "mW",
                       ID = new Guid("4b9e8b24-6c84-423e-8f79-b2bec161f219"),
                       ConversionFactorFromSIFormula = "1.0/Factors.Milli"
                   },
                   new UnitChoice
                   {
                       UnitName = "microwatt",
                       UnitLabel = "µW",
                       ID = new Guid("f0345b17-3e67-4c27-a787-69cd6feb7b1b"),
                       ConversionFactorFromSIFormula = "1.0/Factors.Micro"
                   },
                   new UnitChoice
                   {
                       UnitName = "nanowatt",
                       UnitLabel = "nW",
                       ID = new Guid("622ee208-1b04-42c4-ba6e-552e6e328e02"),
                       ConversionFactorFromSIFormula = "1.0/Factors.Nano"
                   },
                   new UnitChoice
                   {
                       UnitName = "picowatt",
                       UnitLabel = "pW",
                       ID = new Guid("5b46567b-0571-4ca7-90d5-6304a0b7f938"),
                       ConversionFactorFromSIFormula = "1.0/Factors.Pico"
                   },
                   new UnitChoice
                   {
                       UnitName = "femtowatt",
                       UnitLabel = "fW",
                       ID = new Guid("325622ea-c161-4f4f-9ee4-86d9e802f21c"),
                       ConversionFactorFromSIFormula = "1.0/Factors.Femto"
                   },
                   new UnitChoice
                   {
                       UnitName = "attowatt",
                       UnitLabel = "aW",
                       ID = new Guid("7bc1807f-90ac-41b0-a15f-9d1c81101f6d"),
                       ConversionFactorFromSIFormula = "1.0/Factors.Atto"
                   }
        };
        public PowerQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "power" };
            ID = new Guid("6fd69f30-a219-4d56-a1dd-000d8175e2ed");
            DescriptionMD = string.Empty;
            DescriptionMD += @"Power is the rate at which work is done or energy is transferred over time." + Environment.NewLine;
            DescriptionMD += @"The dimension of power is:" + Environment.NewLine;
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
