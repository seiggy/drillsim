using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NORCE.Drilling.CartographicProjection.Model
{
    internal static class Utils
    {
        public static double ToDegree(double? value)
        {
            return value!.Value * 180.0 / Math.PI;
        }
    }
}
