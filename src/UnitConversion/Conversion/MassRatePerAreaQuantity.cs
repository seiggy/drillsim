using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents mass rate per area.
    /// </summary>
    public partial class MassRatePerAreaQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "G";
        public override string SIUnitName { get; } = "kilogram per square metre second";
        public override string SIUnitLabelLatex { get; } = "\\frac{kg}{m^2\\cdot s}";
        public override double LengthDimension { get; } = -2;
        public override double TimeDimension { get; } = -1;
        public override double MassDimension { get; } = 1;

        private static MassRatePerAreaQuantity instance_ = null;
        public static MassRatePerAreaQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MassRatePerAreaQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "kilogram per square metre second",
                UnitLabel = "kg/(m^2*s)",
                ID = new Guid("8a29a29b-8cb1-41a6-8649-2fc4997831d5"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "kilogram per square metre hour",
                UnitLabel = "kg/(m^2*h)",
                ID = new Guid("84cf7f23-5d7b-44c4-ba0c-638138a3e3a2"),
                ConversionFactorFromSIFormula = "Factors.Hour",
            },
            new UnitChoice
            {
                UnitName = "pound per square foot second",
                UnitLabel = "lb/(ft^2*s)",
                ID = new Guid("7b66712d-8c22-495b-8e89-eafbf83fb7e7"),
                ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot/Factors.Pound",
            }
        };

        public MassRatePerAreaQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "mass rate per area", "mass flux" };
            ID = new Guid("36e93f6c-3812-403c-b3c6-fa2999b47980");
            DescriptionMD = "**mass rate per area** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is kilogram per square metre second with unit label $\\frac{kg}{m^2\\cdot s}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
