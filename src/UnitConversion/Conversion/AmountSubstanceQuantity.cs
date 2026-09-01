using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class AmountSubstanceQuantity : SymbolizedBasePhysicalQuantity
    {
        public override string DimensionSymbol { get; } = "N";
        public override string SIUnitName { get; } = "mole";

        public override string SIUnitLabelLatex { get; } = "mol";
        public override double AmountSubstanceDimension { get; } = 1;

        private static AmountSubstanceQuantity instance_ = null;

        public static AmountSubstanceQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new AmountSubstanceQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
                {
                    UnitName = "mole",
                    UnitLabel = "mol",
                    ID = new Guid("d1072bb3-265e-400a-91b2-9c49616dc3de"),
                    ConversionFactorFromSIFormula = "Factors.Unit",
                    IsSI = true
                },
                new UnitChoice
                {
                    UnitName = "decimole",
                    UnitLabel ="dmol",
                    ID = new Guid("b458e56b-edc3-48c3-8858-75d507f1f1f2"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Deci"
                },
                new UnitChoice
                {
                    UnitName = "centimole",
                    UnitLabel ="cmol",
                    ID = new Guid("4d83c0db-ddc4-4087-ae50-076148976cad"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Centi"
                },
                new UnitChoice
                {
                    UnitName = "millimole",
                    UnitLabel ="mmol",
                    ID = new Guid("0a83a1b6-cef4-4899-bcd4-4aa1e10d232a"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "micromole",
                    UnitLabel ="µmol",
                    ID = new Guid("ec2669ad-9742-4667-8a14-76e00c47a41e"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Micro"
                },
                new UnitChoice
                {
                    UnitName = "nanomole",
                    UnitLabel ="nmol",
                    ID = new Guid("dc8d0034-ecfa-4dbf-a24c-f1851c4aaf9c"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Nano"
                },
                new UnitChoice
                {
                    UnitName = "picomole",
                    UnitLabel ="pmol",
                    ID = new Guid("642555ea-37f0-49b9-9f21-8dd616d477c4"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Pico"
                },
                new UnitChoice
                {
                    UnitName = "kilomole",
                    UnitLabel ="kmol",
                    ID = new Guid("01157606-070e-41a2-8c78-84a7ae950bd6"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Kilo"
                }
        };
        public AmountSubstanceQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "amount substance" };
            ID = new Guid("200be1eb-c278-447c-9b15-32d20fc778b9");
            DescriptionMD = string.Empty;
            DescriptionMD += "The **amount of substance** refers to the quantity of entities (such as atoms, molecules, ions, or other particles) in a system.";
            DescriptionMD += "The standard unit used to measure the amount of substance is the **mole** (mol). One mole corresponds to $6.022×10^{23}$ entities (Avogadro's number) of the given substance." + Environment.NewLine;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "This is one of the nine fundamental dimensions in the International System of Units (SI).";
            DescriptionMD += "A fundamental quantity does not depend on any combinations of other fundamental dimensions.";
            DescriptionMD += "It is denoted " + GetDimensionsEnclosed() + ".";
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
