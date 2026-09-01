using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents illuminance.
    /// </summary>
    public partial class IlluminanceQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "E_v";
        public override string SIUnitName { get; } = "lux";
        public override string SIUnitLabelLatex { get; } = "lx";
        public override double SolidAngleDimension { get; } = 1;
        public override double LengthDimension { get; } = -2;
        public override double LuminousIntensityDimension { get; } = 1;

        private static IlluminanceQuantity instance_ = null;
        public static IlluminanceQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new IlluminanceQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "lux",
                UnitLabel = "lx",
                ID = new Guid("4fd99f29-efe9-407b-9cf2-b3a03f878453"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "kilolux",
                UnitLabel = "klx",
                ID = new Guid("98e85663-a416-48f8-bb15-8d63541498a4"),
                ConversionFactorFromSIFormula = "1.0/Factors.Kilo",
            },
            new UnitChoice
            {
                UnitName = "foot candle",
                UnitLabel = "fc",
                ID = new Guid("fdfd69d5-aa3f-45d6-ac90-0eec5ca15a7a"),
                ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot",
            }
        };

        public IlluminanceQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "illuminance" };
            ID = new Guid("f0bdfefc-c4dd-4bf5-84a6-1abeee605b1a");
            DescriptionMD = "**illuminance** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is lux with unit label $lx$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
