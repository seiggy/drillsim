using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents inductance.
    /// </summary>
    public partial class InductanceQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "L";
        public override string SIUnitName { get; } = "henry";
        public override string SIUnitLabelLatex { get; } = "H";
        public override double LengthDimension { get; } = 2;
        public override double TimeDimension { get; } = -2;
        public override double MassDimension { get; } = 1;
        public override double ElectricCurrentDimension { get; } = -2;

        private static InductanceQuantity instance_ = null;
        public static InductanceQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new InductanceQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "henry",
                UnitLabel = "H",
                ID = new Guid("2ed09fce-b549-4e80-9340-3ff2b642f2d4"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "millihenry",
                UnitLabel = "mH",
                ID = new Guid("082a2617-a363-4931-8791-a05dbf560c23"),
                ConversionFactorFromSIFormula = "1.0/Factors.Milli",
            },
            new UnitChoice
            {
                UnitName = "microhenry",
                UnitLabel = "uH",
                ID = new Guid("05d37c8e-1b8b-47d1-aabb-2ea45a32499f"),
                ConversionFactorFromSIFormula = "1.0/Factors.Micro",
            }
        };

        public InductanceQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "inductance" };
            ID = new Guid("4b300ae8-d2b2-41cd-bcfa-6e023ee46de6");
            DescriptionMD = "**inductance** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is henry with unit label $H$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
