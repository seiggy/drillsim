using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents area per amount of substance.
    /// </summary>
    public partial class AreaPerAmountSubstanceQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "A/n";
        public override string SIUnitName { get; } = "square metre per mole";
        public override string SIUnitLabelLatex { get; } = "\\frac{m^2}{mol}";
        public override double AmountSubstanceDimension { get; } = -1;
        public override double LengthDimension { get; } = 2;

        private static AreaPerAmountSubstanceQuantity instance_ = null;
        public static AreaPerAmountSubstanceQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new AreaPerAmountSubstanceQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "square metre per mole",
                UnitLabel = "m^2/mol",
                ID = new Guid("db0b80ff-86c9-4b33-997c-0228fe1e9d4f"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "square centimetre per mole",
                UnitLabel = "cm^2/mol",
                ID = new Guid("2432bd35-7c69-415c-a7ab-cb48d61f1380"),
                ConversionFactorFromSIFormula = "1.0/(Factors.Centi*Factors.Centi)",
            }
        };

        public AreaPerAmountSubstanceQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "area per amount of substance", "molar area" };
            ID = new Guid("96f11a30-1115-47a7-8b59-6c4c94d0e2d0");
            DescriptionMD = "**area per amount of substance** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is square metre per mole with unit label $\\frac{m^2}{mol}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
