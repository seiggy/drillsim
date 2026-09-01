using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents volumetric thermal expansion coefficient.
    /// </summary>
    public partial class VolumetricThermalExpansionCoefficientQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "alpha_V";
        public override string SIUnitName { get; } = "reciprocal kelvin";
        public override string SIUnitLabelLatex { get; } = "\\frac{1}{K}";
        public override double TemperatureDimension { get; } = -1;

        private static VolumetricThermalExpansionCoefficientQuantity instance_ = null;
        public static VolumetricThermalExpansionCoefficientQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new VolumetricThermalExpansionCoefficientQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "reciprocal kelvin",
                UnitLabel = "1/K",
                ID = new Guid("db8a9f66-0d53-4e94-a600-724e963857fc"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "percent per kelvin",
                UnitLabel = "%/K",
                ID = new Guid("611e3bae-862f-4dfc-93b7-7f62780b6eb1"),
                ConversionFactorFromSIFormula = "1.0/Factors.Centi",
            },
            new UnitChoice
            {
                UnitName = "reciprocal degree fahrenheit",
                UnitLabel = "1/degF",
                ID = new Guid("90afffe4-4f86-4f0f-a01b-bbe4fe28a382"),
                ConversionFactorFromSIFormula = "Factors.FahrenheitSlope",
            }
        };

        public VolumetricThermalExpansionCoefficientQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "volumetric thermal expansion coefficient" };
            ID = new Guid("6ff516c8-6817-4355-bf29-408fc20d7320");
            DescriptionMD = "**volumetric thermal expansion coefficient** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is reciprocal kelvin with unit label $\\frac{1}{K}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
