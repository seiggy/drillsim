using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class MagneticFluxDensityQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "tesla";
        public override string SIUnitLabelLatex { get; } = "T";
        public override double MassDimension { get; } = 1;
        public override double TimeDimension { get; } = -2;
        public override double ElectricCurrentDimension { get; } = -1;
        private static MagneticFluxDensityQuantity instance_ = null;

        public static MagneticFluxDensityQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MagneticFluxDensityQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
        new UnitChoice
                {
                    UnitName = "tesla",
                    UnitLabel = "T",
                    ID = new Guid("33c3b59d-9876-4918-9f31-f22de88d7bde"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                    IsSI = true
                },
                new UnitChoice
                {
                    UnitName = "gauss",
                    UnitLabel = "G",
                    ID = new Guid("c09cd87d-8a84-45d0-88d3-20bb5cc48559"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Gauss"
                },
                new UnitChoice
                {
                    UnitName = "milligauss",
                    UnitLabel = "mG",
                    ID = new Guid("41ace729-a2ff-4047-adc3-375829de64c6"),
                    ConversionFactorFromSIFormula = "1.0/(Factors.Milli*Factors.Gauss)"
                },
                new UnitChoice
                {
                    UnitName = "millitesla",
                    UnitLabel = "mT",
                    ID = new Guid("9b6d864e-6775-4668-a59d-e1ab432f8960"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "microtesla",
                    UnitLabel = "µT",
                    ID = new Guid("c6b30197-be6b-41b7-803d-a8de61338612"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Micro"
                },
                new UnitChoice
                {
                    UnitName = "nanotesla",
                    UnitLabel = "nT",
                    ID = new Guid("9bef9def-8cd3-4f7b-b991-290d3441b3d4"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Nano"
                },
                new UnitChoice
                {
                    UnitName = "maxwell per square centimetre",
                    UnitLabel = "Mx/cm²",
                    ID = new Guid("d1b202cb-87c6-417a-947c-5247e5cdfe82"),
                    ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi/Factors.Unit"
                },
                new UnitChoice
                {
                    UnitName = "weber per square metre",
                    UnitLabel = "Wb/m²",
                    ID = new Guid("fefe997a-f3a6-4663-a1de-32889ee0cf15"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Unit"
                }
        };
        public MagneticFluxDensityQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "magnetic flux density" };
            ID = new Guid("b9a3f96b-8861-4b03-9c8a-3c0d7d6ec139");
            DescriptionMD = string.Empty;
            DescriptionMD += @"Magnetic flux density is the measure of the strength of a magnetic field per unit area through which the magnetic flux passes. It indicates how concentrated the magnetic field is." + Environment.NewLine;
            DescriptionMD += @"The dimension of magnetic flux density is:" + Environment.NewLine;
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