using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents molar volume.
    /// </summary>
    public partial class MolarVolumeQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "V_m";
        public override string SIUnitName { get; } = "cubic metre per mole";
        public override string SIUnitLabelLatex { get; } = "\\frac{m^3}{mol}";
        public override double AmountSubstanceDimension { get; } = -1;
        public override double LengthDimension { get; } = 3;

        private static MolarVolumeQuantity instance_ = null;
        public static MolarVolumeQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MolarVolumeQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "cubic metre per mole",
                UnitLabel = "m^3/mol",
                ID = new Guid("c5983410-59e5-4bd5-825f-60b4b878a5ee"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "litre per mole",
                UnitLabel = "L/mol",
                ID = new Guid("b42bb32a-e007-4103-aa46-ad6161242ee6"),
                ConversionFactorFromSIFormula = "1.0/Factors.Litre",
            }
        };

        public MolarVolumeQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "molar volume" };
            ID = new Guid("2ba239f8-e96c-4a63-9fb2-a0dc2a045e08");
            DescriptionMD = "**molar volume** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is cubic metre per mole with unit label $\\frac{m^3}{mol}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
