using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents electric conductance.
    /// </summary>
    public partial class ElectricConductanceQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "G";
        public override string SIUnitName { get; } = "siemens";
        public override string SIUnitLabelLatex { get; } = "S";
        public override double LengthDimension { get; } = -2;
        public override double TimeDimension { get; } = 3;
        public override double MassDimension { get; } = -1;
        public override double ElectricCurrentDimension { get; } = 2;

        private static ElectricConductanceQuantity instance_ = null;
        public static ElectricConductanceQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ElectricConductanceQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "siemens",
                UnitLabel = "S",
                ID = new Guid("fcf092e1-03ff-4d1d-8405-39a7b5d35135"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "millisiemens",
                UnitLabel = "mS",
                ID = new Guid("b5dcf8d4-ca7b-4ec2-9529-8d2f092421b2"),
                ConversionFactorFromSIFormula = "1.0/Factors.Milli",
            },
            new UnitChoice
            {
                UnitName = "microsiemens",
                UnitLabel = "uS",
                ID = new Guid("ff5b8ac2-e551-4fa9-8b2b-2488089623d5"),
                ConversionFactorFromSIFormula = "1.0/Factors.Micro",
            }
        };

        public ElectricConductanceQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "electric conductance" };
            ID = new Guid("d787f9a4-7582-46dd-aa4c-52aec757b612");
            DescriptionMD = "**electric conductance** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is siemens with unit label $S$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
