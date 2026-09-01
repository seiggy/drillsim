using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents volume per area.
    /// </summary>
    public partial class VolumePerAreaQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "V/A";
        public override string SIUnitName { get; } = "cubic metre per square metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{m^3}{m^2}";
        public override double LengthDimension { get; } = 1;

        private static VolumePerAreaQuantity instance_ = null;
        public static VolumePerAreaQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new VolumePerAreaQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "cubic metre per square metre",
                UnitLabel = "m^3/m^2",
                ID = new Guid("8bfc267b-0302-4811-bf00-9034372ae2e7"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "litre per square metre",
                UnitLabel = "L/m^2",
                ID = new Guid("e729798b-fa80-4004-a648-7376aaf64078"),
                ConversionFactorFromSIFormula = "1.0/Factors.Litre",
            },
            new UnitChoice
            {
                UnitName = "gallon US per square foot",
                UnitLabel = "galUS/ft^2",
                ID = new Guid("ef990147-20ec-4898-ab0e-77cd04eb7a1f"),
                ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot/Factors.GallonUS",
            },
            new UnitChoice
            {
                UnitName = "gallon UK per square foot",
                UnitLabel = "galUK/ft^2",
                ID = new Guid("666401b1-68c4-4f43-b7b9-9ac067354a36"),
                ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot/Factors.GallonUK",
            }
        };

        public VolumePerAreaQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "volume per area" };
            ID = new Guid("4304481e-e6c7-48e0-93f7-d3b7c077edc6");
            DescriptionMD = "**volume per area** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is " + SIUnitName + " with unit label $" + SIUnitLabelLatex + "$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
