using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class PressureTimePerVolumeQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "pt/V";
        public override string SIUnitName { get; } = "pascal second per cubic metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{Pa\\cdot s}{m^3}";
        public override double LengthDimension { get; } = -4;
        public override double MassDimension { get; } = 1;
        public override double TimeDimension { get; } = -1;

        private static PressureTimePerVolumeQuantity instance_ = null;
        public static PressureTimePerVolumeQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new PressureTimePerVolumeQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new()
        {
            new UnitChoice { UnitName = "pascal second per cubic metre", UnitLabel = "Pa•s/m³", ID = new Guid("8e991492-6e5d-4de6-8abb-19a8c3387a7b"), ConversionFactorFromSIFormula = "1.0/Factors.Unit", IsSI = true },
            new UnitChoice { UnitName = "bar second per cubic metre", UnitLabel = "bar•s/m³", ID = new Guid("9fe0b498-7b10-4180-8750-29e84b500c1f"), ConversionFactorFromSIFormula = "1.0/Factors.Bar" },
            new UnitChoice { UnitName = "bar second per litre", UnitLabel = "bar•s/L", ID = new Guid("b34a6394-c440-47bb-8be0-df723ab547a8"), ConversionFactorFromSIFormula = "Factors.Litre/Factors.Bar" },
            new UnitChoice { UnitName = "psi second per barrel", UnitLabel = "psi•s/bbl", ID = new Guid("4708a257-d19e-496a-aa82-4473f250f4b6"), ConversionFactorFromSIFormula = "Factors.Barrel/Factors.PSI" },
            new UnitChoice { UnitName = "psi second per US gallon", UnitLabel = "psi•s/USGal", ID = new Guid("bd57d35f-f3d9-4a00-b22a-82490af6f54a"), ConversionFactorFromSIFormula = "Factors.GallonUS/Factors.PSI" },
            new UnitChoice { UnitName = "psi second per UK gallon", UnitLabel = "psi•s/UKGal", ID = new Guid("2812ea08-5e50-46f5-b9a7-4b1f956c05ef"), ConversionFactorFromSIFormula = "Factors.GallonUK/Factors.PSI" }
        };

        public PressureTimePerVolumeQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").First();
            UsualNames = new HashSet<string>() { "pressure time per volume", "PressureTimePerVolume" };
            ID = new Guid("bcd53761-f5f0-4e95-927d-55003466b0eb");
            DescriptionMD = "**Pressure time per volume** is the product of pressure and time divided by volume." + Environment.NewLine;
            DescriptionMD += "Its coherent SI unit is pascal second per cubic metre with unit label $" + SIUnitLabelLatex + "$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
