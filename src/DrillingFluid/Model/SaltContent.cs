using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.DotnetLibraries.General.DataManagement;
using System;

namespace NORCE.Drilling.DrillingFluid.Model
{
    /// <summary>
    /// a base class other classes may derive from
    /// </summary>
    public class SaltContent
    
    {
        /// <summary>
        /// Salt content for the brine composition
        /// </summary>
        public SaltOptions Salt { get; set; }
        /// <summary>
        /// Mass of salt added for a cubic meter of brine
        /// </summary>    
        public GaussianDrillingProperty WeightPerCubicMeter { get; set; }

        public SaltContent() : base()
        {
        }
    }
}
