using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class MassPerEnergyQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "m/E";
        public override string SIUnitName { get; } = "kilogram per joule";
        public override string SIUnitLabelLatex { get; } = "\\frac{kg}{J}";
        public override double LengthDimension { get; } = -2;
        public override double TimeDimension { get; } = 2;

        private static MassPerEnergyQuantity instance_ = null;
        public static MassPerEnergyQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new MassPerEnergyQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new()
        {
            new UnitChoice { UnitName = "kilogram per joule", UnitLabel = "kg/J", ID = new Guid("8dd8f768-9029-47ae-b17e-bb469013456f"), ConversionFactorFromSIFormula = "1.0/Factors.Unit", IsSI = true },
            new UnitChoice { UnitName = "kilogram per megajoule", UnitLabel = "kg/MJ", ID = new Guid("428dfc44-76a3-4139-b091-9ffd573c2b8a"), ConversionFactorFromSIFormula = "Factors.Mega" },
            new UnitChoice { UnitName = "gram per kilojoule", UnitLabel = "g/kJ", ID = new Guid("4921b33b-348a-45fc-ac2d-e222839c17fa"), ConversionFactorFromSIFormula = "Factors.Kilo/Factors.Milli" },
            new UnitChoice { UnitName = "pound per british thermal unit", UnitLabel = "lb/BTU", ID = new Guid("e5313428-2943-4ca7-9c93-4b4df0b9c45e"), ConversionFactorFromSIFormula = "Factors.BTU/Factors.Pound" }
        };

        public MassPerEnergyQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").First();
            UsualNames = new HashSet<string>() { "mass per energy", "specific fuel consumption mass basis", "SpecificFuelConsumptionMassBasis" };
            ID = new Guid("cefe77c3-88c2-491e-aadf-a62218e1329b");
            DescriptionMD = "**Mass per energy** is mass consumption divided by delivered energy." + Environment.NewLine;
            DescriptionMD += "Its coherent SI unit is kilogram per joule with unit label $" + SIUnitLabelLatex + "$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
