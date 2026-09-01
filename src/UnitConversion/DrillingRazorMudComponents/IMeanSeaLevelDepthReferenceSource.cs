using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.UnitConversion.DrillingRazorMudComponents
{
    public interface IMeanSeaLevelDepthReferenceSource
    {
        public double? MeanSeaLevelDepthReference { get; set; }
    }
}
