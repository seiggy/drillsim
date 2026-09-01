using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// The permeability of a porous medium divided by a fluid's dynamic viscosity.
    /// </summary>
    public partial class MobilityQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "lambda";
        public override string SIUnitName { get; } = "square metre per pascal second";
        public override string SIUnitLabelLatex { get; } = "\\frac{m^2}{Pa\\cdot s}";
        public override double LengthDimension { get; } = 3;
        public override double MassDimension { get; } = -1;
        public override double TimeDimension { get; } = 1;

        private static MobilityQuantity instance_ = null;
        public static MobilityQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MobilityQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "square metre per pascal second",
                UnitLabel = "m^2/(Pa*s)",
                ID = new Guid("b72eaf40-0787-4e94-a3d0-705e076af436"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "darcy per pascal second",
                UnitLabel = "D/(Pa*s)",
                ID = new Guid("130fc453-45ba-4c58-bb35-7b79aab66ce6"),
                ConversionFactorFromSIFormula = "1.0/Factors.Darcy",
            },
            new UnitChoice
            {
                UnitName = "millidarcy per centipoise",
                UnitLabel = "mD/cP",
                ID = new Guid("b271dc26-2870-4577-9198-8341a9aa7014"),
                ConversionFactorFromSIFormula = "1.0/Factors.Darcy",
            },
            new UnitChoice
            {
                UnitName = "square micrometre per millipascal second",
                UnitLabel = "um^2/(mPa*s)",
                ID = new Guid("37dff8de-a3c1-499f-bd7b-9b5a9f7482a5"),
                ConversionFactorFromSIFormula = "1.0/Factors.Nano",
            }
        };

        public MobilityQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "mobility", "hydraulic mobility", "permeability per dynamic viscosity" };
            ID = new Guid("0dc5157f-9f12-4ef7-b4d4-5f7a3616183c");
            DescriptionMD = "**Mobility** is porous-medium permeability divided by dynamic viscosity." + Environment.NewLine;
            DescriptionMD += "Its physical dimension is " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is square metre per pascal second with unit label $" + SIUnitLabelLatex + "$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
