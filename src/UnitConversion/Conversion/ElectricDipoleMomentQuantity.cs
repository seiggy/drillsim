using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents electric dipole moment.
    /// </summary>
    public partial class ElectricDipoleMomentQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "p";
        public override string SIUnitName { get; } = "coulomb metre";
        public override string SIUnitLabelLatex { get; } = "C\\cdot m";
        public override double LengthDimension { get; } = 1;
        public override double TimeDimension { get; } = 1;
        public override double ElectricCurrentDimension { get; } = 1;

        private static ElectricDipoleMomentQuantity instance_ = null;
        public static ElectricDipoleMomentQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ElectricDipoleMomentQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "coulomb metre",
                UnitLabel = "C*m",
                ID = new Guid("ebb5ca40-44ed-4a8d-a1f4-afe168ab17e5"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "elementary charge nanometre",
                UnitLabel = "e*nm",
                ID = new Guid("3b2c145b-7a0a-4377-9a72-5a33e25b5a9e"),
                ConversionFactorFromSIFormula = "1.0/(Factors.ElectronCharge*Factors.Nano)",
            }
        };

        public ElectricDipoleMomentQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "electric dipole moment" };
            ID = new Guid("594992c4-b617-4879-8449-d0df6facab57");
            DescriptionMD = "**electric dipole moment** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is coulomb metre with unit label $C\\cdot m$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
