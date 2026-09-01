using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents volume per pressure.
    /// </summary>
    public partial class VolumePerPressureQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "V/p";
        public override string SIUnitName { get; } = "cubic metre per pascal";
        public override string SIUnitLabelLatex { get; } = "\\frac{m^3}{Pa}";
        public override double LengthDimension { get; } = 4;
        public override double TimeDimension { get; } = 2;
        public override double MassDimension { get; } = -1;

        private static VolumePerPressureQuantity instance_ = null;
        public static VolumePerPressureQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new VolumePerPressureQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "cubic metre per pascal",
                UnitLabel = "m^3/Pa",
                ID = new Guid("8fa70f02-90dd-4add-a13f-89e99bae4310"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "litre per bar",
                UnitLabel = "L/bar",
                ID = new Guid("3c027d5f-dbd3-4ca3-94fe-0ad937cc3f17"),
                ConversionFactorFromSIFormula = "Factors.Bar/Factors.Litre",
            },
            new UnitChoice
            {
                UnitName = "gallon US per psi",
                UnitLabel = "galUS/psi",
                ID = new Guid("99df103e-3761-4931-8436-1b3b7c5e9df7"),
                ConversionFactorFromSIFormula = "Factors.PSI/Factors.GallonUS",
            },
            new UnitChoice
            {
                UnitName = "gallon UK per psi",
                UnitLabel = "galUK/psi",
                ID = new Guid("7e526423-2bc0-4a90-b30a-86798f2bb4dc"),
                ConversionFactorFromSIFormula = "Factors.PSI/Factors.GallonUK",
            }
        };

        public VolumePerPressureQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "volume per pressure", "volumetric compliance" };
            ID = new Guid("1b1a2335-7755-4e54-b931-adb70ae48c2d");
            DescriptionMD = "**volume per pressure** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is cubic metre per pascal with unit label $\\frac{m^3}{Pa}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
