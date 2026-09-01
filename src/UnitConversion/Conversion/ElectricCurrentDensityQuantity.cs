using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents electric current density.
    /// </summary>
    public partial class ElectricCurrentDensityQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "J";
        public override string SIUnitName { get; } = "ampere per square metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{A}{m^2}";
        public override double ElectricCurrentDimension { get; } = 1;
        public override double LengthDimension { get; } = -2;

        private static ElectricCurrentDensityQuantity instance_ = null;
        public static ElectricCurrentDensityQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ElectricCurrentDensityQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "ampere per square metre",
                UnitLabel = "A/m^2",
                ID = new Guid("52df7588-2f3d-4a96-b74a-cef1dac5b69b"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "milliampere per square centimetre",
                UnitLabel = "mA/cm^2",
                ID = new Guid("e571a9c5-e485-4b09-a00d-1642c10baff8"),
                ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi/Factors.Milli",
            }
        };

        public ElectricCurrentDensityQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "electric current density" };
            ID = new Guid("96bc308d-e056-4aec-bc7d-8df207a0eddd");
            DescriptionMD = "**electric current density** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is ampere per square metre with unit label $\\frac{A}{m^2}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
