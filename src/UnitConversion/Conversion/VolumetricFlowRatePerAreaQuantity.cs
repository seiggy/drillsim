using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents volumetric flow rate per area.
    /// </summary>
    public partial class VolumetricFlowRatePerAreaQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "v_s";
        public override string SIUnitName { get; } = "cubic metre per square metre second";
        public override string SIUnitLabelLatex { get; } = "\\frac{m^3}{m^2\\cdot s}";
        public override double TimeDimension { get; } = -1;
        public override double LengthDimension { get; } = 1;

        private static VolumetricFlowRatePerAreaQuantity instance_ = null;
        public static VolumetricFlowRatePerAreaQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new VolumetricFlowRatePerAreaQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "cubic metre per square metre second",
                UnitLabel = "m^3/(m^2*s)",
                ID = new Guid("1c3d13e2-4ac3-4835-bf6b-be97a0141f7e"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "litre per square metre second",
                UnitLabel = "L/(m^2*s)",
                ID = new Guid("10974ea8-0a8f-4a48-b828-9c9631af62e1"),
                ConversionFactorFromSIFormula = "1.0/Factors.Litre",
            },
            new UnitChoice
            {
                UnitName = "gallon US per minute square foot",
                UnitLabel = "galUS/(min*ft^2)",
                ID = new Guid("e8feddf4-5fca-44e6-831c-931cedf6390e"),
                ConversionFactorFromSIFormula = "Factors.Minute*Factors.Foot*Factors.Foot/Factors.GallonUS",
            },
            new UnitChoice
            {
                UnitName = "gallon UK per minute square foot",
                UnitLabel = "galUK/(min*ft^2)",
                ID = new Guid("7c5b7b15-9b5a-43be-bd6d-0b980a89d7e2"),
                ConversionFactorFromSIFormula = "Factors.Minute*Factors.Foot*Factors.Foot/Factors.GallonUK",
            }
        };

        public VolumetricFlowRatePerAreaQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "volumetric flow rate per area", "superficial velocity" };
            ID = new Guid("883203d0-f253-4247-85f0-7193e100379c");
            DescriptionMD = "**volumetric flow rate per area** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is " + SIUnitName + " with unit label $" + SIUnitLabelLatex + "$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
