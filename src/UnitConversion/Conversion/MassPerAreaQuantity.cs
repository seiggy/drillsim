using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents mass per area.
    /// </summary>
    public partial class MassPerAreaQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "m/A";
        public override string SIUnitName { get; } = "kilogram per square metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{kg}{m^2}";
        public override double MassDimension { get; } = 1;
        public override double LengthDimension { get; } = -2;

        private static MassPerAreaQuantity instance_ = null;
        public static MassPerAreaQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MassPerAreaQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "kilogram per square metre",
                UnitLabel = "kg/m^2",
                ID = new Guid("91ccf59f-2ea7-4e3d-9b20-09f365f40083"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "gram per square centimetre",
                UnitLabel = "g/cm^2",
                ID = new Guid("d85eab07-847e-4dc5-a2f2-6d7e32cb35b5"),
                ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi/Factors.Milli",
            },
            new UnitChoice
            {
                UnitName = "pound per square foot",
                UnitLabel = "lb/ft^2",
                ID = new Guid("f6de3be8-ae7b-4920-bd4a-b6ace09f02c7"),
                ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot/Factors.Pound",
            }
        };

        public MassPerAreaQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "mass per area", "areal density" };
            ID = new Guid("79400a63-c7dd-475e-8114-dbc09b1c8d41");
            DescriptionMD = "**mass per area** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is kilogram per square metre with unit label $\\frac{kg}{m^2}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
