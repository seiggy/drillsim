using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion
{
    public partial class EarthMagneticFluxDensityQuantity : MagneticFluxDensityQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-9;

        private static EarthMagneticFluxDensityQuantity instance_ = null;

        public static new EarthMagneticFluxDensityQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new EarthMagneticFluxDensityQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public EarthMagneticFluxDensityQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "earth magnetic flux density" };
            ID = new Guid("ed95aca5-aaf9-4822-b045-342ffcd06ca7");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += @"Earth's magnetic flux density refers to the strength and direction of the Earth's magnetic field at a specific location." + Environment.NewLine;
            DescriptionMD += @"The meaningful precision of earth magnetic flux density is: " + MeaningfulPrecisionInSI.ToString() + " " + MagneticFluxDensityQuantity.Instance.GetUnitChoice(MagneticFluxDensityQuantity.UnitChoicesEnum.Tesla).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(MagneticFluxDensityQuantity.Instance.GetUnitChoice(MagneticFluxDensityQuantity.UnitChoicesEnum.Tesla));
            this.UnitChoices.Add(MagneticFluxDensityQuantity.Instance.GetUnitChoice(MagneticFluxDensityQuantity.UnitChoicesEnum.Gauss));
            this.UnitChoices.Add(MagneticFluxDensityQuantity.Instance.GetUnitChoice(MagneticFluxDensityQuantity.UnitChoicesEnum.Milligauss));
            this.UnitChoices.Add(MagneticFluxDensityQuantity.Instance.GetUnitChoice(MagneticFluxDensityQuantity.UnitChoicesEnum.Microtesla));
            this.UnitChoices.Add(MagneticFluxDensityQuantity.Instance.GetUnitChoice(MagneticFluxDensityQuantity.UnitChoicesEnum.Nanotesla));
            SemanticExample = GetSemanticExample();
        }
    }
}
