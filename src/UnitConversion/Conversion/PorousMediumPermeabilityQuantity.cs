using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Reference for conversions from darcy to area units: https://www.spe.org/authors/docs/metric_standard.pdf
    /// </summary>
    public partial class PorousMediumPermeabilityQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "k";
        public override string SIUnitName { get; } = "square metre";
        public override string SIUnitLabelLatex { get; } = "m^{2}";
        public override double LengthDimension { get; } = 2;

        private static PorousMediumPermeabilityQuantity instance_ = null;

        public static new PorousMediumPermeabilityQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new PorousMediumPermeabilityQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "square metre",
                UnitLabel = "m²",
                ID = new Guid("5e27ad4a-b541-4807-9a36-4bd159b33f52"),
                ConversionFactorFromSIFormula = "1.0 / Factors.Unit",
                IsSI = true
            },
            new UnitChoice
            {
                UnitName = "darcy",
                UnitLabel = "D",
                ID = new Guid("9a89bcc3-dc77-4e3a-a492-fcdabc24ec41"),
                ConversionFactorFromSIFormula = "1.0/Factors.Darcy" //1013249965828.14
            },
            new UnitChoice
            {
                UnitName = "millidarcy",
                UnitLabel = "mD",
                ID = new Guid("8d7a6767-6c6b-4daf-8617-d35e4055d457"),
                ConversionFactorFromSIFormula = "1.0/(Factors.Milli*Factors.Darcy)" //1013249965828140.0
            },
            new UnitChoice
            {
                UnitName = "microdarcy",
                UnitLabel = "µD",
                ID = new Guid("b552f28d-c68a-4c59-853c-fe6e03dd5f4c"),
                ConversionFactorFromSIFormula = "1.0/(Factors.Micro*Factors.Darcy)" //1013249965828140000.0
            },
            new UnitChoice
            {
                UnitName = "nanodarcy",
                UnitLabel = "nD",
                ID = new Guid("f35b05c7-8b2f-4194-9336-d42cec5f3ba5"),
                ConversionFactorFromSIFormula = "1.0/(Factors.Nano*Factors.Darcy)" //1013249965828140000000.0
            }

        };
        public PorousMediumPermeabilityQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "porous medium permeability" };
            ID = new Guid("413da2c1-ebad-454a-ae14-1a8620f8f59c");
            DescriptionMD = string.Empty;
            DescriptionMD += @"The permeability is a property of porous medium that quantifies its ability to allow fluids to pass through it." + Environment.NewLine;
            DescriptionMD += @"The dimension of porous medium permeability is:" + Environment.NewLine;
            DescriptionMD += "$" + GetDimensionsEnclosed() + "$." + Environment.NewLine;
            if (!string.IsNullOrEmpty(SIUnitLabelLatex) && !string.IsNullOrEmpty(SIUnitName) && UsualNames != null && UsualNames.Count > 0)
            {
                DescriptionMD += Environment.NewLine;
                DescriptionMD += @"The SI unit for **" + UsualNames.First() + "** is: " + SIUnitName + " with the associated unit label $" + SIUnitLabelLatex + "$" + Environment.NewLine;
            }
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
