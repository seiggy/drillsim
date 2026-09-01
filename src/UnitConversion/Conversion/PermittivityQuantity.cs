using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents permittivity.
    /// </summary>
    public partial class PermittivityQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "epsilon";
        public override string SIUnitName { get; } = "farad per metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{F}{m}";
        public override double LengthDimension { get; } = -3;
        public override double TimeDimension { get; } = 4;
        public override double MassDimension { get; } = -1;
        public override double ElectricCurrentDimension { get; } = 2;

        private static PermittivityQuantity instance_ = null;
        public static PermittivityQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new PermittivityQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "farad per metre",
                UnitLabel = "F/m",
                ID = new Guid("b0ac1f42-633d-4d26-8f74-4e9a274f39b9"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "picofarad per metre",
                UnitLabel = "pF/m",
                ID = new Guid("d254d268-0c9d-420c-a2ce-da7ffadc20d6"),
                ConversionFactorFromSIFormula = "1.0/Factors.Pico",
            },
            new UnitChoice
            {
                UnitName = "nanofarad per metre",
                UnitLabel = "nF/m",
                ID = new Guid("2e1e5ad7-99b8-4b41-bf80-52bead473e8b"),
                ConversionFactorFromSIFormula = "1.0/Factors.Nano",
            }
        };

        public PermittivityQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "permittivity", "electric permittivity" };
            ID = new Guid("efe69117-c302-419f-acc1-836b295e845d");
            DescriptionMD = "**permittivity** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is farad per metre with unit label $\\frac{F}{m}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
