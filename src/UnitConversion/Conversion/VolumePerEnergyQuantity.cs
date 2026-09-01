using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class VolumePerEnergyQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "V/E";
        public override string SIUnitName { get; } = "cubic metre per joule";
        public override string SIUnitLabelLatex { get; } = "\\frac{m^3}{J}";
        public override double LengthDimension { get; } = 1;
        public override double MassDimension { get; } = -1;
        public override double TimeDimension { get; } = 2;

        private static VolumePerEnergyQuantity instance_ = null;
        public static VolumePerEnergyQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new VolumePerEnergyQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new()
        {
            new UnitChoice { UnitName = "cubic metre per joule", UnitLabel = "m³/J", ID = new Guid("a00aaeea-d574-458f-a411-0e9784c230db"), ConversionFactorFromSIFormula = "1.0/Factors.Unit", IsSI = true },
            new UnitChoice { UnitName = "litre per megajoule", UnitLabel = "L/MJ", ID = new Guid("5d48e5b3-edf6-4e77-bace-3572c69765df"), ConversionFactorFromSIFormula = "Factors.Mega/Factors.Litre" },
            new UnitChoice { UnitName = "US gallon per british thermal unit", UnitLabel = "USGal/BTU", ID = new Guid("b4efec52-58a4-4cb0-ba79-5d26ce82ebe6"), ConversionFactorFromSIFormula = "Factors.BTU/Factors.GallonUS" },
            new UnitChoice { UnitName = "UK gallon per british thermal unit", UnitLabel = "UKGal/BTU", ID = new Guid("0782d4b9-2588-47e3-a8b3-596135d549f0"), ConversionFactorFromSIFormula = "Factors.BTU/Factors.GallonUK" }
        };

        public VolumePerEnergyQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").First();
            UsualNames = new HashSet<string>() { "volume per energy", "specific fuel consumption volume basis", "SpecificFuelConsumptionVolumeBasis" };
            ID = new Guid("d1bdac66-5afb-4514-b019-bd99ef87166e");
            DescriptionMD = "**Volume per energy** is volume consumption divided by delivered energy." + Environment.NewLine;
            DescriptionMD += "Its coherent SI unit is cubic metre per joule with unit label $" + SIUnitLabelLatex + "$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
