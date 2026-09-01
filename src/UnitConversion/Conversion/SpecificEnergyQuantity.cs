using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents specific energy.
    /// </summary>
    public partial class SpecificEnergyQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "e";
        public override string SIUnitName { get; } = "joule per kilogram";
        public override string SIUnitLabelLatex { get; } = "\\frac{J}{kg}";
        public override double TimeDimension { get; } = -2;
        public override double LengthDimension { get; } = 2;

        private static SpecificEnergyQuantity instance_ = null;
        public static SpecificEnergyQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new SpecificEnergyQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "joule per kilogram",
                UnitLabel = "J/kg",
                ID = new Guid("d000f7cb-1e7a-4d8b-8806-a97dfbbf7941"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "kilojoule per kilogram",
                UnitLabel = "kJ/kg",
                ID = new Guid("34b7d64d-5ec5-4016-98fe-c96244ba531b"),
                ConversionFactorFromSIFormula = "1.0/Factors.Kilo",
            },
            new UnitChoice
            {
                UnitName = "british thermal unit per pound",
                UnitLabel = "BTU/lb",
                ID = new Guid("68b33f90-312d-4eb5-a094-5a5008d8ed81"),
                ConversionFactorFromSIFormula = "Factors.Pound/Factors.BTU",
            }
        };

        public SpecificEnergyQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "specific energy", "energy per mass" };
            ID = new Guid("596254b1-4a99-43a8-b21d-b8b3ebca6335");
            DescriptionMD = "**specific energy** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is joule per kilogram with unit label $\\frac{J}{kg}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
