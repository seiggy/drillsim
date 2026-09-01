using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class PressurePerVolumeQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "p/V";
        public override string SIUnitName { get; } = "pascal per cubic metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{Pa}{m^3}";
        public override double LengthDimension { get; } = -4;
        public override double MassDimension { get; } = 1;
        public override double TimeDimension { get; } = -2;

        private static PressurePerVolumeQuantity instance_ = null;
        public static PressurePerVolumeQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new PressurePerVolumeQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new()
        {
            new UnitChoice { UnitName = "pascal per cubic metre", UnitLabel = "Pa/m³", ID = new Guid("628b9120-dab8-430c-bec0-9d9ad2aa55fb"), ConversionFactorFromSIFormula = "1.0/Factors.Unit", IsSI = true },
            new UnitChoice { UnitName = "bar per cubic metre", UnitLabel = "bar/m³", ID = new Guid("0727709b-bbb7-440e-8020-131a342ea217"), ConversionFactorFromSIFormula = "1.0/Factors.Bar" },
            new UnitChoice { UnitName = "bar per litre", UnitLabel = "bar/L", ID = new Guid("02f9fa89-9916-4fe6-9ba6-d51771e11f85"), ConversionFactorFromSIFormula = "Factors.Litre/Factors.Bar" },
            new UnitChoice { UnitName = "psi per barrel", UnitLabel = "psi/bbl", ID = new Guid("8eb47e61-a01c-42d0-81df-e5adae17c0ed"), ConversionFactorFromSIFormula = "Factors.Barrel/Factors.PSI" },
            new UnitChoice { UnitName = "psi per US gallon", UnitLabel = "psi/USGal", ID = new Guid("eeea969b-fb3e-4e41-8061-98a6acbdfc81"), ConversionFactorFromSIFormula = "Factors.GallonUS/Factors.PSI" },
            new UnitChoice { UnitName = "psi per UK gallon", UnitLabel = "psi/UKGal", ID = new Guid("0aabb0c8-c1fe-4f51-8727-893170947d26"), ConversionFactorFromSIFormula = "Factors.GallonUK/Factors.PSI" }
        };

        public PressurePerVolumeQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").First();
            UsualNames = new HashSet<string>() { "pressure per volume", "PressureperVolume" };
            ID = new Guid("b4f048cf-90f2-4ef2-b4fa-c0cf57eb6044");
            DescriptionMD = "**Pressure per volume** is pressure divided by volume." + Environment.NewLine;
            DescriptionMD += "Its coherent SI unit is pascal per cubic metre with unit label $" + SIUnitLabelLatex + "$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
