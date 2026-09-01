using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents angular velocity per volumetric flow rate.
    /// </summary>
    public partial class AngularVelocityPerVolumetricFlowRateQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "omega/Q";
        public override string SIUnitName { get; } = "radian per second per cubic metre per second";
        public override string SIUnitLabelLatex { get; } = "\\frac{rad/s}{m^3/s}";
        public override double PlaneAngleDimension { get; } = 1;
        public override double LengthDimension { get; } = -3;

        private static AngularVelocityPerVolumetricFlowRateQuantity instance_ = null;
        public static AngularVelocityPerVolumetricFlowRateQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new AngularVelocityPerVolumetricFlowRateQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "radian per second per cubic metre per second",
                UnitLabel = "rad/m^3",
                ID = new Guid("9b2339aa-e707-4db5-9654-31caa9eecc17"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "revolution per minute per litre per minute",
                UnitLabel = "rev/L",
                ID = new Guid("b730c2a5-964d-4275-8a73-ecf594739c1a"),
                ConversionFactorFromSIFormula = "Factors.Litre/Factors.Revolution",
            },
            new UnitChoice
            {
                UnitName = "revolution per minute per gallon US per minute",
                UnitLabel = "rev/galUS",
                ID = new Guid("6534bd66-5155-4a88-a3c7-9b3e8081d11d"),
                ConversionFactorFromSIFormula = "Factors.GallonUS/Factors.Revolution",
            },
            new UnitChoice
            {
                UnitName = "revolution per minute per gallon UK per minute",
                UnitLabel = "rev/galUK",
                ID = new Guid("c4034096-6ae6-48da-8bcc-ceedb876b5c5"),
                ConversionFactorFromSIFormula = "Factors.GallonUK/Factors.Revolution",
            }
        };

        public AngularVelocityPerVolumetricFlowRateQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "angular velocity per volumetric flow rate" };
            ID = new Guid("0a2e0354-d0ed-4963-b902-14e653f9a956");
            DescriptionMD = "**angular velocity per volumetric flow rate** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is " + SIUnitName + " with unit label $" + SIUnitLabelLatex + "$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
