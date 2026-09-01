using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents electromagnetic moment.
    /// </summary>
    public partial class ElectromagneticMomentQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "m";
        public override string SIUnitName { get; } = "ampere square metre";
        public override string SIUnitLabelLatex { get; } = "A\\cdot m^2";
        public override double ElectricCurrentDimension { get; } = 1;
        public override double LengthDimension { get; } = 2;

        private static ElectromagneticMomentQuantity instance_ = null;
        public static ElectromagneticMomentQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ElectromagneticMomentQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "ampere square metre",
                UnitLabel = "A*m^2",
                ID = new Guid("2783341f-c473-4440-91fb-fb8192ebea70"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "ampere square centimetre",
                UnitLabel = "A*cm^2",
                ID = new Guid("5f37f86b-d5c9-43bf-be31-7ff262071d97"),
                ConversionFactorFromSIFormula = "1.0/(Factors.Centi*Factors.Centi)",
            }
        };

        public ElectromagneticMomentQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "electromagnetic moment", "magnetic dipole moment" };
            ID = new Guid("4435dc43-c681-4e1f-ac4d-fe6bbf3f9c4f");
            DescriptionMD = "**electromagnetic moment** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is ampere square metre with unit label $A\\cdot m^2$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
