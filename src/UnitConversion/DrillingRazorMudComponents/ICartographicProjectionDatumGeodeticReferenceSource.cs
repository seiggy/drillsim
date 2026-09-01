using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.UnitConversion.DrillingRazorMudComponents
{
    public interface ICartographicProjectionDatumGeodeticReferenceSource
    {
        public double? CartographicProjectionDatumLatitudeReference { get; set; }
        public double? CartographicProjectionDatumLongitudeReference { get; set; }
    }
}
