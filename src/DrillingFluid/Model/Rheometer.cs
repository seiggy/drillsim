using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.DotnetLibraries.General.DataManagement;
using System;

namespace NORCE.Drilling.DrillingFluid.Model
{
    /// <summary>
    /// a base class other classes may derive from
    /// </summary>
    public class Rheometer
    {
        public string Name { get; set; }
        public double BobSize { get; set; }
        public double GapSize { get; set; }
        public double Length { get; set; }
        public RheometerConfiguration RheometerConfiguration { get; set; }        
        public Rheometer() : base()
        {
        }
    }
}
