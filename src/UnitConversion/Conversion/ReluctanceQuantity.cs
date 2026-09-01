using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents reluctance.
    /// </summary>
    public partial class ReluctanceQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "R_m";
        public override string SIUnitName { get; } = "reciprocal henry";
        public override string SIUnitLabelLatex { get; } = "\\frac{1}{H}";
        public override double LengthDimension { get; } = -2;
        public override double TimeDimension { get; } = 2;
        public override double MassDimension { get; } = -1;
        public override double ElectricCurrentDimension { get; } = 2;

        private static ReluctanceQuantity instance_ = null;
        public static ReluctanceQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ReluctanceQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "reciprocal henry",
                UnitLabel = "1/H",
                ID = new Guid("574c6277-85f5-49cd-bd12-1d2500cf236b"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "reciprocal millihenry",
                UnitLabel = "1/mH",
                ID = new Guid("5ed33523-5dd4-4427-874b-a29b593e237f"),
                ConversionFactorFromSIFormula = "Factors.Milli",
            }
        };

        public ReluctanceQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "reluctance", "magnetic reluctance" };
            ID = new Guid("fd9fb2d9-98da-4cd4-9ee4-7ae5cb5eaf1e");
            DescriptionMD = "**reluctance** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is reciprocal henry with unit label $\\frac{1}{H}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
