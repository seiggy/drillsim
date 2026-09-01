using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.UnitConversion.DrillingRazorMudComponents
{
    public interface IClusterPositionReferenceSource
    {
        public double? ClusterNorthPositionReference { get; set; }
        public double? ClusterEastPositionReference { get; set; }
    }
}
