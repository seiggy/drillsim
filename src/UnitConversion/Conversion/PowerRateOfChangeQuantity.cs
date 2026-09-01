using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// The rate at which power changes with time.
    /// </summary>
    public partial class PowerRateOfChangeQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "dP/dt";
        public override string SIUnitName { get; } = "watt per second";
        public override string SIUnitLabelLatex { get; } = "\\frac{W}{s}";
        public override double LengthDimension { get; } = 2;
        public override double MassDimension { get; } = 1;
        public override double TimeDimension { get; } = -4;

        private static PowerRateOfChangeQuantity instance_ = null;
        public static PowerRateOfChangeQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new PowerRateOfChangeQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "watt per second",
                UnitLabel = "W/s",
                ID = new Guid("f97ebd0e-aa46-4d24-add4-d49380360e4e"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true
            },
            new UnitChoice
            {
                UnitName = "kilowatt per second",
                UnitLabel = "kW/s",
                ID = new Guid("b2cb43a2-dd5b-4682-99d5-4085579d852b"),
                ConversionFactorFromSIFormula = "1.0/Factors.Kilo"
            },
            new UnitChoice
            {
                UnitName = "megawatt per second",
                UnitLabel = "MW/s",
                ID = new Guid("2b37872e-a376-4ccd-aace-ca04b413954c"),
                ConversionFactorFromSIFormula = "1.0/Factors.Mega"
            },
            new UnitChoice
            {
                UnitName = "watt per minute",
                UnitLabel = "W/min",
                ID = new Guid("b311cfde-8c1e-4b68-a5d5-95aa0330bbb1"),
                ConversionFactorFromSIFormula = "Factors.Minute/Factors.Unit"
            },
            new UnitChoice
            {
                UnitName = "kilowatt per minute",
                UnitLabel = "kW/min",
                ID = new Guid("45f3cda8-e81a-48fc-9ffd-0c3d6756bbcb"),
                ConversionFactorFromSIFormula = "Factors.Minute/Factors.Kilo"
            }
        };

        public PowerRateOfChangeQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "power rate of change", "power rate", "rate of change of power" };
            ID = new Guid("32774627-7eb2-4cab-8270-ae075ea5f335");
            DescriptionMD = "**Power rate of change** is the change in power per unit time." + Environment.NewLine;
            DescriptionMD += "Its physical dimension is " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is watt per second with unit label $" + SIUnitLabelLatex + "$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
