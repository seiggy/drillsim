using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents electric charge.
    /// </summary>
    public partial class ElectricChargeQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "Q";
        public override string SIUnitName { get; } = "coulomb";
        public override string SIUnitLabelLatex { get; } = "C";
        public override double ElectricCurrentDimension { get; } = 1;
        public override double TimeDimension { get; } = 1;

        private static ElectricChargeQuantity instance_ = null;
        public static ElectricChargeQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ElectricChargeQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "coulomb",
                UnitLabel = "C",
                ID = new Guid("fc4741ce-7704-439c-95df-6c24df9551c5"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "ampere hour",
                UnitLabel = "A*h",
                ID = new Guid("5087c5ba-6a37-4e5d-9b7e-f8dbcd2a4eda"),
                ConversionFactorFromSIFormula = "1.0/Factors.Hour",
            },
            new UnitChoice
            {
                UnitName = "elementary charge",
                UnitLabel = "e",
                ID = new Guid("be19407f-276d-457e-afe0-00e2bed34e2b"),
                ConversionFactorFromSIFormula = "1.0/Factors.ElectronCharge",
            }
        };

        public ElectricChargeQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "electric charge" };
            ID = new Guid("a41a4b57-58ef-481f-b959-29dcf6b69dd7");
            DescriptionMD = "**electric charge** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is coulomb with unit label $C$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
