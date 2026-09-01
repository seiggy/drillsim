using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class AngleMagneticFluxDensityQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "radian tesla";
        public override string SIUnitLabelLatex { get; } = "rad \\cdot T";
        public override double PlaneAngleDimension { get; } = 1;
        public override double MassDimension { get; } = 1;
        public override double TimeDimension { get; } = -2;
        public override double ElectricCurrentDimension { get; } = -1;
        private static AngleMagneticFluxDensityQuantity instance_ = null;

        public static AngleMagneticFluxDensityQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new AngleMagneticFluxDensityQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
             new UnitChoice
                {
                  UnitName = "radian tesla",
                  UnitLabel = "rad•T",
                  ID = new Guid("242bd328-96ef-4a1b-9a78-1e2eb8366955"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                  IsSI = true
                },
                new UnitChoice
                {
                  UnitName = "radian gauss",
                  UnitLabel = "rad•G",
                  ID = new Guid("aa726b90-bd4b-420c-ae71-6f2f1fde3b58"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Gauss"
                },
                new UnitChoice
                {
                  UnitName = "radian milligauss",
                  UnitLabel = "rad•mG",
                  ID = new Guid("352a5b84-306d-4e38-898a-58705683fdf0"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Milli*Factors.Gauss)"
                },
                new UnitChoice
                {
                  UnitName = "radian millitesla",
                  UnitLabel = "rad•mT",
                  ID = new Guid("663639b3-48b8-4c04-a2eb-6ae2e16daa9b"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Milli"
                },
                new UnitChoice
                {
                  UnitName = "radian microtesla",
                  UnitLabel = "rad•µT",
                  ID = new Guid("b445e592-e0ca-490f-8880-9708e4e96a01"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Micro"
                },
                new UnitChoice
                {
                  UnitName = "radian nanotesla",
                  UnitLabel = "rad•nT",
                  ID = new Guid("b4aeee40-29fa-463a-80a4-e10fa42c743f"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Nano"
                },
                new UnitChoice
                {
                  UnitName = "radian maxwell per square centimetre",
                  UnitLabel = "rad•Mx/cm²",
                  ID = new Guid("02d06899-5d89-4669-a4c2-35adb9ec3924"),
                  ConversionFactorFromSIFormula = "1.0/(Factors.Centi*Factors.Centi)"
                },
                new UnitChoice
                {
                  UnitName = "radian weber per square metre",
                  UnitLabel = "rad•Wb/m²",
                  ID = new Guid("409e8e85-0870-4529-ae0c-95ab6506c445"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Unit"
                },
                 new UnitChoice
                {
                  UnitName = "degree tesla",
                  UnitLabel = "°•T",
                  ID = new Guid("13df770f-6e77-4de4-91c6-137206e53fbb"),
                  ConversionFactorFromSIFormula = "Factors.Degree/Factors.Unit"
                },
                new UnitChoice
                {
                  UnitName = "degree gauss",
                  UnitLabel = "°•G",
                  ID = new Guid("73bb1de0-ccc0-42e6-b88e-44ecfb6fe7e4"),
                  ConversionFactorFromSIFormula = "Factors.Degree/Factors.Gauss"
                },
                new UnitChoice
                {
                  UnitName = "degree milligauss",
                  UnitLabel = "°•mG",
                  ID = new Guid("e74c9ae3-6e17-48e1-896b-e6c1877d68d7"),
                  ConversionFactorFromSIFormula = "Factors.Degree/(Factors.Milli*Factors.Gauss)"
                },
                new UnitChoice
                {
                  UnitName = "degree millitesla",
                  UnitLabel = "°•mT",
                  ID = new Guid("812a3461-ae4a-405b-ae5d-73eb23e8a71f"),
                  ConversionFactorFromSIFormula = "Factors.Degree/Factors.Milli"
                },
                new UnitChoice
                {
                  UnitName = "degree microtesla",
                  UnitLabel = "°•µT",
                  ID = new Guid("50782201-236e-4537-843b-121e8dca28c6"),
                  ConversionFactorFromSIFormula = "Factors.Degree/Factors.Micro"
                },
                new UnitChoice
                {
                  UnitName = "degree nanotesla",
                  UnitLabel = "°•nT",
                  ID = new Guid("0d9bf20d-2b10-4e73-ae8e-3d3e91862ec0"),
                  ConversionFactorFromSIFormula = "Factors.Degree/Factors.Nano"
                },
                new UnitChoice
                {
                  UnitName = "degree maxwell per square centimetre",
                  UnitLabel = "°•Mx/cm²",
                  ID = new Guid("092e8231-a6e6-4b29-bdd6-2ae490aa583a"),
                  ConversionFactorFromSIFormula = "Factors.Degree/(Factors.Centi*Factors.Centi)"
                },
                new UnitChoice
                {
                  UnitName = "degree weber per square metre",
                  UnitLabel = "°•Wb/m²",
                  ID = new Guid("2d7e1d60-6401-41c0-b436-612116be9ad4"),
                  ConversionFactorFromSIFormula = "Factors.Degree/Factors.Unit"
                }
        };
        public AngleMagneticFluxDensityQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "angle magnetic flux density" };
            ID = new Guid("03bb57e6-ca8b-4741-a211-9cf57c8fd177");
            DescriptionMD = string.Empty;
            DescriptionMD += @"The **angle magnetic flux density** is the product of an angle by a magnetic flux density. Let's break it down step by step:" + Environment.NewLine;
            DescriptionMD += @"1. Magnetic Flux Density" + Environment.NewLine;
            DescriptionMD += @"The **magnetic flux density** $\mathbf{B}$ is a measure of the strength and direction of the magnetic field at ";
            DescriptionMD += @"a particular point in space. It represents the amount of magnetic flux passing through a unit area perpendicular to the direction of the magnetic field. ";
            DescriptionMD += @"In simpler terms, it describes how dense or concentrated the magnetic field lines are in a given region.";
            DescriptionMD += @"It can be expressed in terms of the fundamental dimensions as:" + Environment.NewLine;
            DescriptionMD += @"$$[M][T]^{-2}[I]^{-1}$$" + Environment.NewLine;
            DescriptionMD += @"Where:" + Environment.NewLine;
            DescriptionMD += @"- $[M]$ represents mass" + Environment.NewLine;
            DescriptionMD += @"- $[I]$ represents electric current" + Environment.NewLine;
            DescriptionMD += @"- $[T]$ represents time" + Environment.NewLine;
            DescriptionMD += @"2. Plane Angle" + Environment.NewLine;
            DescriptionMD += @"A **plane angle** is one of the nine fundamental dimensions in the International Unit System (SI). It is denoted: ";
            DescriptionMD += @"$$[\theta]$$" + Environment.NewLine;
            DescriptionMD += @"3. Angle Magnetic Flux Density" + Environment.NewLine;
            DescriptionMD += @"So the dimension of **angle magnetic flux density** is:" + Environment.NewLine;
            DescriptionMD += @"$" + GetDimensionsEnclosed() + "$" + Environment.NewLine;
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
