using NORCE.Drilling.CartographicProjection.ModelShared;
using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using System;

namespace NORCE.Drilling.CartographicProjection.Model
{
    /// <summary>
    /// a class for handling cartographic coordinates
    /// </summary>
    public class CartographicCoordinate
    {
        /// <summary>
        /// the cartographic north coordinate to run conversions from or to
        /// </summary>
        public double? Northing { get; set; }
        /// <summary>
        /// the cartographic east coordinate to run conversions from or to
        /// </summary>
        public double? Easting { get; set; }
        /// <summary>
        /// the cartographic vertical depth coordinate to run conversions from or to
        /// by convention, VerticalDepth corresponds to the vertical depth of the associated geodetic coordinates in the reference geodetic datum
        /// therefore: VerticalDepth = GeodeticCoordinate.VerticalDepth
        /// </summary>
        public double? VerticalDepth { get; set; }
        /// <summary>
        /// the geodetic coordinate to run conversions from or to
        /// </summary>
        public GeodeticCoordinate? GeodeticCoordinate { get; set; }
        /// <summary>
        /// the grid convergence in the geodetic datum
        /// </summary>
        public double? GridConvergenceDatum { get; set; }
        /// <summary>
        /// default constructor required for JSON serialization
        /// </summary>
        public CartographicCoordinate() : base()
        {
        }

        //private void GetGeoCoordinate(byte[] octree, out double latitude, out double longitude, out double elevation)
        //{
        //    if (OctreeBounds == null)
        //    {
        //        OctreeBounds = new Bounds(-Numeric.PI / 2.0, Numeric.PI / 2.0, -Numeric.PI, Numeric.PI, MinElevation, MaxElevation);
        //    }
        //    Bounds topBound = new Bounds(OctreeBounds);
        //    foreach (byte b in octree)
        //    {
        //        topBound = topBound.CalculateBounds(b);
        //    }
        //    latitude = topBound.MiddleX;
        //    longitude = topBound.MiddleY;
        //    elevation = topBound.MiddleZ;
        //}
    }
}
