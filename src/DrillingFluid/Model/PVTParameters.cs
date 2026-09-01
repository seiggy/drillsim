using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.DotnetLibraries.General.DataManagement;
using System;

namespace NORCE.Drilling.DrillingFluid.Model
{
    /// <summary>
    /// a base class other classes may derive from
    /// </summary>
    public class FluidPVTParameters
    {
        /// <summary>
        /// PVT constant 
        /// </summary>
        public GaussianDrillingProperty A0 { get; set; }
        /// <summary>
        /// PVT constant 
        /// </summary>
        public GaussianDrillingProperty B0 { get; set; }
        /// <summary>
        /// PVT constant 
        /// </summary>
        public GaussianDrillingProperty C0 { get; set; }
        /// <summary>
        /// PVT constant 
        /// </summary>
        public GaussianDrillingProperty D0 { get; set; }
        /// <summary>
        /// PVT constant 
        /// </summary>
        public GaussianDrillingProperty E0 { get; set; }
        /// <summary>
        /// PVT constant 
        /// </summary>
        public GaussianDrillingProperty F0 { get; set; }
        /// <summary>
        /// Used for schema generation 
        /// </summary>
        public FluidPVTParameters() : base()
        {
        }
    }
}
