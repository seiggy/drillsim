using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents mass length.
    /// </summary>
    public partial class MassLengthQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "mL";
        public override string SIUnitName { get; } = "kilogram metre";
        public override string SIUnitLabelLatex { get; } = "kg\\cdot m";
        public override double MassDimension { get; } = 1;
        public override double LengthDimension { get; } = 1;

        private static MassLengthQuantity instance_ = null;
        public static MassLengthQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MassLengthQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "kilogram metre",
                UnitLabel = "kg*m",
                ID = new Guid("99577a2c-c4a7-4a9e-bea4-2f06687cdaf5"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "gram centimetre",
                UnitLabel = "g*cm",
                ID = new Guid("784940f3-a2f0-46a3-8023-db9e0ce0ff3b"),
                ConversionFactorFromSIFormula = "1.0/(Factors.Milli*Factors.Centi)",
            },
            new UnitChoice
            {
                UnitName = "pound foot",
                UnitLabel = "lb*ft",
                ID = new Guid("f85d0d50-d8f3-45b7-9ea2-d1170c4b4a28"),
                ConversionFactorFromSIFormula = "1.0/(Factors.Pound*Factors.Foot)",
            }
        };

        public MassLengthQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "mass length" };
            ID = new Guid("1b197346-779c-4d77-839b-0e28bcb96a81");
            DescriptionMD = "**mass length** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is kilogram metre with unit label $kg\\cdot m$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
