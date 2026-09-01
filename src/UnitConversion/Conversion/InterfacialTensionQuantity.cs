using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class InterfacialTensionQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "newton per metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{N}{m}";
        public override double MassDimension { get; } = 1;
        public override double TimeDimension { get; } = -2;
        private static InterfacialTensionQuantity instance_ = null;

        public static InterfacialTensionQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new InterfacialTensionQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
        {
          UnitName = "newton per metre",
          UnitLabel = "N/m",
          ID = new Guid("7ee9eca6-2704-442a-bd50-c8a0826da932"),
          ConversionFactorFromSIFormula = "1.0/Factors.Unit",
          IsSI = true
        },
        new UnitChoice
        {
          UnitName = "millinewton per metre",
          UnitLabel = "mN/m",
          ID = new Guid("7b1b363c-cbb0-4499-9d7c-762adc43e690"),
          ConversionFactorFromSIFormula = "1.0/Factors.Milli"
        },
        new UnitChoice
        {
          UnitName = "dyne per centimetre",
          UnitLabel = "dyne/cm",
          ID = new Guid("a3c12fb9-6936-44bf-ad66-f4139163d11b"),
          ConversionFactorFromSIFormula = "Factors.Centi/Factors.Dyne"
        },
        new UnitChoice
        {
          UnitName = "pound per second squared",
          UnitLabel = "lb/s²",
          ID = new Guid("03db472b-b8e8-4ad0-b2b1-b8970686210c"),
          ConversionFactorFromSIFormula = "1.0/Factors.Pound" 
        }
        };
        public InterfacialTensionQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "interfacial tension", "surface tension" };
            ID = new Guid("6c2da24b-fa92-415d-9161-150de87dad4c");
            DescriptionMD = string.Empty;
            DescriptionMD += @"Interfacial tension is the force per unit length acting along the boundary between two immiscible liquids, resisting their mixing." + Environment.NewLine;
            DescriptionMD += @"The dimension of interfacial tension is:" + Environment.NewLine;
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
