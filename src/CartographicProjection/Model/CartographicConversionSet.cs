using DotSpatial.Projections;
using NORCE.Drilling.CartographicProjection.ModelShared;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NORCE.Drilling.CartographicProjection.Model
{
    /// <summary>
    /// A cartographic conversion set is a series of CartographicCoordinate.
    /// The cartographic data are converted to the target geodetic datum and WGS84 or vice versa.
    /// The grid convergence at the local is also calculated in the geodetic datum and in the WGS84 datum.
    /// The octree code for the WGS84 geodetic position can also calculated at the requested level of details.
    /// it is also possible to pass the octree code (in the WGS84 datum), and then the geodetic coordinates and cartographic coordinates are calculated
    /// </summary>
    public class CartographicConversionSet
    {
        public static readonly double MaxElevation = 34000000.0; // We want the resolution in z to be of the same order of magnitude as for the other directions in the relevant region (circumference of the earth is ca 40 000 km)
        public static readonly double MinElevation = -6000000.0; // The radius of the earth is around 6000 km.
        /// <summary>
        /// a MetaInfo for the CartographicConversionSet
        /// </summary>
        public OSDC.DotnetLibraries.General.DataManagement.MetaInfo? MetaInfo { get; set; }

        /// <summary>
        /// name of the data
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// a description of the data
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// the date when the data was created
        /// </summary>
        public DateTimeOffset? CreationDate { get; set; }

        /// <summary>
        /// the date when the data was last modified
        /// </summary>
        public DateTimeOffset? LastModificationDate { get; set; }

        /// <summary>
        /// the ID of the CartographicProjection
        /// </summary>
        public Guid? CartographicProjectionID { get; set; } = null;

        /// <summary>
        /// an input list of CartographicCoordinate
        /// </summary>
        public List<CartographicCoordinate>? CartographicCoordinateList { get; set; }

        ///// <summary>
        ///// the bounds used in the octree encoding
        ///// </summary>
        //public Bounds? OctreeBounds { get; set; }

        /// <summary>
        /// default constructor required for JSON serialization
        /// </summary>
        public CartographicConversionSet() : base()
        {
        }

        /// <summary>
        /// Main calculation method of the CartographicConversionSet class
        /// if (N, E, TVD) coordinates are provided, deproject them to compute the associated geodetic coordinates associated to the reference geodetic datum
        /// else if (Lat, Long, TVD) geodetic coordinates are provided, project them to compute the associated cartographic coordinates associated to the reference cartographic projection
        /// else return false
        /// This method is supposed to be run in combination with CalculateGeodeticCoordinate() (prior or after) to compute the associated geodetic coordinates in the global geodetic datum (WGS84)
        /// </summary>
        /// <returns></returns>
        public bool CalculateProjection(CartographicProjection? cartographicProjection, GeodeticDatum? geodeticDatum)
        {
            if (cartographicProjection != null && geodeticDatum != null && geodeticDatum.Spheroid != null && CartographicCoordinateList != null)
            {
                try
                {
                    foreach (CartographicCoordinate coordinate in CartographicCoordinateList)
                    {
                        if (coordinate != null)
                        {
                            if (coordinate.Northing != null && coordinate.Easting != null && coordinate.VerticalDepth != null)
                            {
                                FromCarto(cartographicProjection, geodeticDatum, coordinate.Northing.Value, coordinate.Easting.Value, out double? latitudeDatum, out double? longitudeDatum);
                                if (latitudeDatum != null && longitudeDatum != null)
                                {
                                    coordinate.GeodeticCoordinate ??= new GeodeticCoordinate();
                                    coordinate.GeodeticCoordinate.LatitudeDatum = latitudeDatum;
                                    coordinate.GeodeticCoordinate.LongitudeDatum = longitudeDatum;
                                    coordinate.GeodeticCoordinate.VerticalDepthDatum = coordinate.VerticalDepth; // by convention, TVD corresponds to the true vertical depth of the associated geodetic coordinates in the reference geodetic datum
                                    CalculateGridConvergence(cartographicProjection, geodeticDatum, coordinate.Northing.Value, coordinate.Easting.Value, latitudeDatum.Value, longitudeDatum.Value, out double? gridConvergence);
                                    coordinate.GridConvergenceDatum = gridConvergence;
                                }
                            }
                            else if (coordinate.GeodeticCoordinate != null &&
                                coordinate.GeodeticCoordinate.LatitudeDatum != null && coordinate.GeodeticCoordinate.LongitudeDatum != null && coordinate.GeodeticCoordinate.VerticalDepthDatum != null)
                            {
                                ToCarto(cartographicProjection, geodeticDatum, coordinate.GeodeticCoordinate.LatitudeDatum.Value, coordinate.GeodeticCoordinate.LongitudeDatum.Value, out double? northing, out double? easting);
                                if (northing != null && easting != null)
                                {
                                    coordinate.Northing = northing;
                                    coordinate.Easting = easting;
                                    coordinate.VerticalDepth = coordinate.GeodeticCoordinate.VerticalDepthDatum; // by convention, TVD corresponds to the true vertical depth of the associated geodetic coordinates in the reference geodetic datum
                                    CalculateGridConvergence(cartographicProjection, geodeticDatum, coordinate.Northing.Value, coordinate.Easting.Value, coordinate.GeodeticCoordinate.LatitudeDatum.Value, coordinate.GeodeticCoordinate.LongitudeDatum.Value, out double? gridConvergence);
                                    coordinate.GridConvergenceDatum = gridConvergence;
                                }

                            }
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"Exception caught during the computation of the given CartographicConversionSet: {ex.Message}");
                }
            }
            return false;
        }

        /// <summary>
        /// if the 3 coordinates in the reference datum are defined then they are converted to WGS4 and the octree code is calculated
        /// if the 3 coordinates in the WGS84 datum are defined then they are converted to the reference datum and the octree code is calculated
        /// if the octree code is defined, then the geodetic coordinates in the reference and WGS84 datum are calculated
        /// </summary>
        /// <returns></returns>
        public bool CalculateGeodeticCoordinate(CartographicProjection? cartographicProjection, GeodeticDatum? geodeticDatum, GeodeticConversionSet? geodeticConversionSet)
        {
            if (cartographicProjection != null && geodeticDatum != null && geodeticDatum.Spheroid != null && geodeticConversionSet != null && geodeticConversionSet.GeodeticCoordinates != null && CartographicCoordinateList != null)
            {
                try
                {
                    foreach (CartographicCoordinate coordinate in CartographicCoordinateList)
                    {
                        if (coordinate != null && coordinate.GeodeticCoordinate != null)
                        {
                            if (coordinate.GeodeticCoordinate.LatitudeDatum != null && coordinate.GeodeticCoordinate.LongitudeDatum != null && coordinate.GeodeticCoordinate.VerticalDepthDatum != null &&
                                geodeticConversionSet.GeodeticCoordinates != null && geodeticConversionSet.GeodeticCoordinates.Count == 1)
                            {
                                coordinate.GeodeticCoordinate.LatitudeWGS84 = geodeticConversionSet.GeodeticCoordinates.ElementAt(0).LatitudeWGS84;
                                coordinate.GeodeticCoordinate.LongitudeWGS84 = geodeticConversionSet.GeodeticCoordinates.ElementAt(0).LongitudeWGS84;
                                coordinate.GeodeticCoordinate.VerticalDepthWGS84 = geodeticConversionSet.GeodeticCoordinates.ElementAt(0).VerticalDepthWGS84;
                                coordinate.GeodeticCoordinate.OctreeDepth = geodeticConversionSet.GeodeticCoordinates.ElementAt(0).OctreeDepth;
                                coordinate.GeodeticCoordinate.OctreeCode = geodeticConversionSet.GeodeticCoordinates.ElementAt(0).OctreeCode;
                            }
                            else if (coordinate.GeodeticCoordinate.LatitudeWGS84 != null && coordinate.GeodeticCoordinate.LongitudeWGS84 != null && coordinate.GeodeticCoordinate.VerticalDepthWGS84 != null &&
                                geodeticConversionSet.GeodeticCoordinates != null && geodeticConversionSet.GeodeticCoordinates.Count == 1)
                            {
                                coordinate.GeodeticCoordinate.LatitudeDatum = geodeticConversionSet.GeodeticCoordinates.ElementAt(0).LatitudeDatum;
                                coordinate.GeodeticCoordinate.LongitudeDatum = geodeticConversionSet.GeodeticCoordinates.ElementAt(0).LongitudeDatum;
                                coordinate.GeodeticCoordinate.VerticalDepthDatum = geodeticConversionSet.GeodeticCoordinates.ElementAt(0).VerticalDepthDatum;
                                coordinate.GeodeticCoordinate.OctreeDepth = geodeticConversionSet.GeodeticCoordinates.ElementAt(0).OctreeDepth;
                                coordinate.GeodeticCoordinate.OctreeCode = geodeticConversionSet.GeodeticCoordinates.ElementAt(0).OctreeCode;
                            }
                            else if (coordinate.GeodeticCoordinate.OctreeDepth > 0 && coordinate.GeodeticCoordinate.OctreeCode.CodeHigh > 0 &&
                                geodeticConversionSet.GeodeticCoordinates != null && geodeticConversionSet.GeodeticCoordinates.Count == 1)
                            {
                                coordinate.GeodeticCoordinate.LatitudeWGS84 = geodeticConversionSet.GeodeticCoordinates.ElementAt(0).LatitudeWGS84;
                                coordinate.GeodeticCoordinate.LongitudeWGS84 = geodeticConversionSet.GeodeticCoordinates.ElementAt(0).LongitudeWGS84;
                                coordinate.GeodeticCoordinate.VerticalDepthWGS84 = geodeticConversionSet.GeodeticCoordinates.ElementAt(0).VerticalDepthWGS84;
                                coordinate.GeodeticCoordinate.LatitudeDatum = geodeticConversionSet.GeodeticCoordinates.ElementAt(0).LatitudeDatum;
                                coordinate.GeodeticCoordinate.LongitudeDatum = geodeticConversionSet.GeodeticCoordinates.ElementAt(0).LongitudeDatum;
                                coordinate.GeodeticCoordinate.VerticalDepthDatum = geodeticConversionSet.GeodeticCoordinates.ElementAt(0).VerticalDepthDatum;
                            }
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"Exception caught during the computation of the given CartographicConversionSet: {ex.Message}");
                }
            }
            return false;
        }

        public void FromCarto(CartographicProjection? cartographicProjection, GeodeticDatum? geodeticDatum, double northing, double easting, out double? latitudeDatum, out double? longitudeDatum)
        {
            if (cartographicProjection != null && geodeticDatum != null && geodeticDatum.Spheroid != null)
            {
                string projString = cartographicProjection.GetProj4String() + GetProj4String(geodeticDatum.Spheroid);
                ProjectionInfo source = ProjectionInfo.FromProj4String(projString);
                string spheroidString = "+proj=latlong " + GetProj4String(geodeticDatum.Spheroid);
                ProjectionInfo dest = ProjectionInfo.FromProj4String(spheroidString);
                double[] xys = new double[2];
                double[] zs = new double[1];
                zs[0] = 0;
                xys[0] = easting; // easting
                xys[1] = northing; // northing
                Reproject.ReprojectPoints(xys, zs, source, dest, 0, zs.Length);
                longitudeDatum = xys[0] * Math.PI / 180.0;
                latitudeDatum = xys[1] * Math.PI / 180.0;
            }
            else
            {
                latitudeDatum = null;
                longitudeDatum = null;
            }
        }

        public void ToCarto(CartographicProjection? cartographicProjection, GeodeticDatum? geodeticDatum, double latitudeDatum, double longitudeDatum, out double? northing, out double? easting)
        {
            if (cartographicProjection != null && geodeticDatum != null && geodeticDatum.Spheroid != null)
            {
                string projString = cartographicProjection.GetProj4String() + GetProj4String(geodeticDatum.Spheroid);
                ProjectionInfo dest = ProjectionInfo.FromProj4String(projString);
                string spheroidString = "+proj=latlong " + GetProj4String(geodeticDatum.Spheroid);
                ProjectionInfo source = ProjectionInfo.FromProj4String(spheroidString);

                double[] xys = new double[2];
                double[] zs = new double[1];
                zs[0] = 0;
                xys[0] = longitudeDatum * 180.0 / Math.PI; // longitude
                xys[1] = latitudeDatum * 180.0 / Math.PI; // latitude
                Reproject.ReprojectPoints(xys, zs, source, dest, 0, zs.Length);
                easting = xys[0];
                northing = xys[1];
            }
            else
            {
                northing = null;
                easting = null;
            }
        }

        public void FromCarto(CartographicProjection? cartographicProjection, GeodeticDatum? geodeticDatum, List<Tuple<double, double>> inputs, List<Tuple<double, double>> outputs)
        {
            if (cartographicProjection != null && geodeticDatum != null && geodeticDatum.Spheroid != null &&
                inputs != null && outputs != null)
            {
                string projString = cartographicProjection.GetProj4String() + GetProj4String(geodeticDatum.Spheroid);
                ProjectionInfo source = ProjectionInfo.FromProj4String(projString);
                string spheroidString = "+proj=latlong " + GetProj4String(geodeticDatum.Spheroid);
                ProjectionInfo dest = ProjectionInfo.FromProj4String(spheroidString);
                foreach (Tuple<double, double> coord in inputs)
                {
                    double[] xys = new double[2];
                    double[] zs = new double[1];
                    zs[0] = 0;
                    xys[0] = (double)coord.Item2; // easting
                    xys[1] = (double)coord.Item1; // northing
                    Reproject.ReprojectPoints(xys, zs, source, dest, 0, zs.Length);
                    double longitudeDatum = xys[0] * Math.PI / 180.0;
                    double latitudeDatum = xys[1] * Math.PI / 180.0;
                    outputs.Add(new Tuple<double, double>(latitudeDatum, longitudeDatum));
                }
            }
        }

        public void ToCarto(CartographicProjection? cartographicProjection, GeodeticDatum? geodeticDatum, List<Tuple<double, double>> inputs, List<Tuple<double, double>> outputs)
        {
            if (cartographicProjection != null && geodeticDatum != null && geodeticDatum.Spheroid != null &&
                inputs != null && outputs != null)
            {
                string projString = cartographicProjection.GetProj4String() + GetProj4String(geodeticDatum.Spheroid);
                ProjectionInfo dest = ProjectionInfo.FromProj4String(projString);
                string spheroidString = "+proj=latlong " + GetProj4String(geodeticDatum.Spheroid);
                ProjectionInfo source = ProjectionInfo.FromProj4String(spheroidString);
                foreach (Tuple<double, double> latlong in inputs)
                {
                    double[] xys = new double[2];
                    double[] zs = new double[1];
                    zs[0] = 0;
                    xys[0] = latlong.Item2 * 180.0 / Math.PI; // longitude
                    xys[1] = latlong.Item1 * 180.0 / Math.PI; // latitude
                    Reproject.ReprojectPoints(xys, zs, source, dest, 0, zs.Length);
                    double easting = xys[0];
                    double northing = xys[1];
                    outputs.Add(new Tuple<double, double>(northing, easting));
                }
            }
        }

        public void CalculateGridConvergence(CartographicProjection? cartographicProjection, GeodeticDatum? geodeticDatum, double northing, double easting, double latitudeDatum, double longitudeDatum, out double? gridConvergence)
        {
            if (cartographicProjection != null && geodeticDatum != null && geodeticDatum.Spheroid != null)
            {
                double delta = 0.1 * Math.PI / 180.0;
                double latitudeDatum2 = latitudeDatum + delta;
                ToCarto(cartographicProjection, geodeticDatum, latitudeDatum2, longitudeDatum, out double? northing2, out double? easting2);
                if (northing2 != null && easting2 != null)
                {
                    gridConvergence = Math.Atan2((double)(easting2 - easting), (double)(northing2 - northing));
                }
                else
                {
                    gridConvergence = null;
                }
            }
            else
            {
                gridConvergence = null;
            }
        }

        /// <summary>
        /// Build the proj4 query string defining the given spheroid
        /// </summary>
        /// <returns></returns>
        public static string GetProj4String(ModelShared.Spheroid? spheroid)
        {
            if (spheroid != null)
            {
                string sval = "";
                if (spheroid.SemiMajorAxis != null && 
                    spheroid.SemiMajorAxis.DiracDistributionValue != null &&
                    spheroid.SemiMajorAxis.DiracDistributionValue.Value != null)
                {
                    sval += " +a=" + spheroid.SemiMajorAxis.DiracDistributionValue.Value.Value.ToString(CultureInfo.InvariantCulture);
                }
                if (spheroid.SemiMinorAxis != null &&
                    spheroid.SemiMinorAxis.DiracDistributionValue != null &&
                    spheroid.SemiMinorAxis.DiracDistributionValue.Value != null)
                {
                    sval += " +b=" + spheroid.SemiMinorAxis.DiracDistributionValue.Value.Value.ToString(CultureInfo.InvariantCulture);
                }
                if (spheroid.Flattening != null &&
                    spheroid.Flattening.DiracDistributionValue != null &&
                    spheroid.Flattening.DiracDistributionValue.Value != null)
                {
                    sval += " +f=" + spheroid.Flattening.DiracDistributionValue.Value.Value.ToString(CultureInfo.InvariantCulture);
                }
                if (spheroid.InverseFlattening != null &&
                    spheroid.InverseFlattening.DiracDistributionValue != null &&
                    spheroid.InverseFlattening.DiracDistributionValue.Value != null)
                {
                    sval += " +rf=" + spheroid.InverseFlattening.DiracDistributionValue.Value.Value.ToString(CultureInfo.InvariantCulture);
                }
                if (spheroid.Eccentricity != null &&
                    spheroid.Eccentricity.DiracDistributionValue != null &&
                    spheroid.Eccentricity.DiracDistributionValue.Value != null)
                {
                    sval += " +e=" + spheroid.Eccentricity.DiracDistributionValue.Value.Value.ToString(CultureInfo.InvariantCulture);
                }
                if (spheroid.SquaredEccentricity != null &&
                    spheroid.SquaredEccentricity.DiracDistributionValue != null &&
                    spheroid.SquaredEccentricity.DiracDistributionValue.Value != null)
                {
                    sval += " +es=" + spheroid.SquaredEccentricity.DiracDistributionValue.Value.Value.ToString(CultureInfo.InvariantCulture);
                }
                return sval;
            }
            return string.Empty;
        }
    }
}
