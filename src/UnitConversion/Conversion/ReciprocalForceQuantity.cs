using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents reciprocal force.
    /// </summary>
    public partial class ReciprocalForceQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "F^-1";
        public override string SIUnitName { get; } = "reciprocal newton";
        public override string SIUnitLabelLatex { get; } = "\\frac{1}{N}";
        public override double LengthDimension { get; } = -1;
        public override double TimeDimension { get; } = 2;
        public override double MassDimension { get; } = -1;

        private static ReciprocalForceQuantity instance_ = null;
        public static ReciprocalForceQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ReciprocalForceQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "reciprocal newton",
                UnitLabel = "1/N",
                ID = new Guid("32c97575-6497-4f87-819c-fb733de5de76"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "reciprocal kilonewton",
                UnitLabel = "1/kN",
                ID = new Guid("eccd1de9-d0eb-4fac-beff-f08d53d5e8c6"),
                ConversionFactorFromSIFormula = "Factors.Kilo",
            },
            new UnitChoice
            {
                UnitName = "reciprocal pound force",
                UnitLabel = "1/lbf",
                ID = new Guid("f2b6f1b1-56ab-49a6-ae7a-1331c49e58ee"),
                ConversionFactorFromSIFormula = "Factors.PoundForce",
            }
        };

        public ReciprocalForceQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "reciprocal force" };
            ID = new Guid("98977886-d1de-499e-b365-e0dba1c48bf0");
            DescriptionMD = "**reciprocal force** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is reciprocal newton with unit label $\\frac{1}{N}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
