using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents molar mass.
    /// </summary>
    public partial class MolarMassQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "M";
        public override string SIUnitName { get; } = "kilogram per mole";
        public override string SIUnitLabelLatex { get; } = "\\frac{kg}{mol}";
        public override double AmountSubstanceDimension { get; } = -1;
        public override double MassDimension { get; } = 1;

        private static MolarMassQuantity instance_ = null;
        public static MolarMassQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MolarMassQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "kilogram per mole",
                UnitLabel = "kg/mol",
                ID = new Guid("915ae218-7794-4d09-acb7-3be121224712"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "gram per mole",
                UnitLabel = "g/mol",
                ID = new Guid("11ef8810-213d-4767-9916-8dd4cc690e34"),
                ConversionFactorFromSIFormula = "1.0/Factors.Milli",
            },
            new UnitChoice
            {
                UnitName = "pound per pound mole",
                UnitLabel = "lb/lbmol",
                ID = new Guid("2dfc62db-f8b4-4f5c-89a1-61a0d67db36c"),
                ConversionFactorFromSIFormula = "1.0/Factors.Milli",
            }
        };

        public MolarMassQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "molar mass" };
            ID = new Guid("92841dda-643c-4c0e-8f7a-46aec8183496");
            DescriptionMD = "**molar mass** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is kilogram per mole with unit label $\\frac{kg}{mol}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
