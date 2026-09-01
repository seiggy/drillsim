using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.DotnetLibraries.General.DataManagement;
using System;
using System.Collections.Generic;

namespace NORCE.Drilling.DrillingFluid.Model
{
    /// <summary>
    /// a base class other classes may derive from
    /// </summary>
    public class RheologicalModelFitting
    {
        public RheologicalModelsEnum RheologicalModel { get; set; } 
        public double p1 { get; set; }
        public double p2 { get; set; }
        public double p3 { get; set; }
        public double p4 { get; set; }
        /// <summary>
        /// default constructor required for JSON serialization
        /// </summary>
        public RheologicalModelFitting() : base()
        {
        }
    }
}
