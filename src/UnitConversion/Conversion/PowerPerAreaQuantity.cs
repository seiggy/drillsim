using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents power per area.
    /// </summary>
    public partial class PowerPerAreaQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "q_double_prime";
        public override string SIUnitName { get; } = "watt per square metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{W}{m^2}";
        public override double TimeDimension { get; } = -3;
        public override double MassDimension { get; } = 1;

        private static PowerPerAreaQuantity instance_ = null;
        public static PowerPerAreaQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new PowerPerAreaQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "watt per square metre",
                UnitLabel = "W/m^2",
                ID = new Guid("1b1af17b-4896-4797-beb8-34d44fa22756"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "kilowatt per square metre",
                UnitLabel = "kW/m^2",
                ID = new Guid("7b3dd2fe-39d4-4cfd-9b87-246a179efbf6"),
                ConversionFactorFromSIFormula = "1.0/Factors.Kilo",
            },
            new UnitChoice
            {
                UnitName = "british thermal unit per hour square foot",
                UnitLabel = "BTU/(h*ft^2)",
                ID = new Guid("c2c4d8a2-fc19-4d18-91b6-ebdfc8009606"),
                ConversionFactorFromSIFormula = "Factors.Hour*Factors.Foot*Factors.Foot/Factors.BTU",
            }
        };

        public PowerPerAreaQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "power per area", "heat flux density" };
            ID = new Guid("c528cf86-98de-45b9-86db-5e4a8215db8d");
            DescriptionMD = "**power per area** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is watt per square metre with unit label $\\frac{W}{m^2}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
