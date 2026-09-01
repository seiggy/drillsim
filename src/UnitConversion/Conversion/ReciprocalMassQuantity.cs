using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents reciprocal mass.
    /// </summary>
    public partial class ReciprocalMassQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "m^-1";
        public override string SIUnitName { get; } = "reciprocal kilogram";
        public override string SIUnitLabelLatex { get; } = "\\frac{1}{kg}";
        public override double MassDimension { get; } = -1;

        private static ReciprocalMassQuantity instance_ = null;
        public static ReciprocalMassQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ReciprocalMassQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "reciprocal kilogram",
                UnitLabel = "1/kg",
                ID = new Guid("0a9c46ca-6fe0-40e0-97cb-e3ec61e1b4de"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "reciprocal gram",
                UnitLabel = "1/g",
                ID = new Guid("55fca621-68b2-448a-97eb-d8637bc4f34c"),
                ConversionFactorFromSIFormula = "Factors.Milli",
            },
            new UnitChoice
            {
                UnitName = "reciprocal pound",
                UnitLabel = "1/lb",
                ID = new Guid("40672be3-695b-445e-b466-9b63feb8687c"),
                ConversionFactorFromSIFormula = "Factors.Pound",
            }
        };

        public ReciprocalMassQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "reciprocal mass" };
            ID = new Guid("87278911-3f4b-4a76-9d72-01adecda370f");
            DescriptionMD = "**reciprocal mass** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is reciprocal kilogram with unit label $\\frac{1}{kg}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
