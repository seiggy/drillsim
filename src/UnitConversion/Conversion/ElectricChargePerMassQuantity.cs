using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents electric charge per mass.
    /// </summary>
    public partial class ElectricChargePerMassQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "q_m";
        public override string SIUnitName { get; } = "coulomb per kilogram";
        public override string SIUnitLabelLatex { get; } = "\\frac{C}{kg}";
        public override double TimeDimension { get; } = 1;
        public override double MassDimension { get; } = -1;
        public override double ElectricCurrentDimension { get; } = 1;

        private static ElectricChargePerMassQuantity instance_ = null;
        public static ElectricChargePerMassQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ElectricChargePerMassQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "coulomb per kilogram",
                UnitLabel = "C/kg",
                ID = new Guid("9491def5-2e78-4c5b-957c-c62d5690b263"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "millicoulomb per kilogram",
                UnitLabel = "mC/kg",
                ID = new Guid("6c8eea48-773d-4847-bce2-d46e7c094694"),
                ConversionFactorFromSIFormula = "1.0/Factors.Milli",
            }
        };

        public ElectricChargePerMassQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "electric charge per mass", "specific charge" };
            ID = new Guid("ea5ffd73-5045-4a48-a0a8-8cce3e82d5fb");
            DescriptionMD = "**electric charge per mass** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is coulomb per kilogram with unit label $\\frac{C}{kg}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
