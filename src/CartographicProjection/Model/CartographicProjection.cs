using DotSpatial.Projections.GeographicCategories;
using NJsonSchema.Annotations;
using NORCE.Drilling.CartographicProjection.ModelShared;
using System;
using System.Data.SqlTypes;
using System.Globalization;

namespace NORCE.Drilling.CartographicProjection.Model
{
    public class CartographicProjection
    {
        /// <summary>
        /// a MetaInfo for the CartographicProjection
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
        /// the prototype of the cartographic projection
        /// </summary>
        public ProjectionType ProjectionType { get; set; }
        /// <summary>
        /// the ID of the geodetic datum associated to the cartographic projection
        /// </summary>
        public Guid? GeodeticDatumID { get; set; }
        /// The latitude for the origin of the cartographic projection
        /// </summary>
        public double? LatitudeOrigin { get; set; }
        /// <summary>
        /// the first standard parallel in a Lambert projection
        /// </summary>
        public double? Latitude1 { get; set; }
        /// <summary>
        /// the second standard parallel in a Lambert projection
        /// </summary>
        public double? Latitude2 { get; set; }
        /// <summary>
        /// latitude of true scale
        /// </summary>
        public double? LatitudeTrueScale { get; set; }
        /// <summary>
        /// the longitude for origin of the cartographic projection
        /// </summary>
        public double? LongitudeOrigin { get; set; }
        /// <summary>
        /// a scaling factor
        /// </summary>
        public double? Scaling { get; set; } = 1.0;
        /// <summary>
        /// a false easting (note that UTM has a default false easting)
        /// </summary>
        public double? FalseEasting { get; set; }
        /// <summary>
        /// a false northing (note that UTM has a default false northing)
        /// </summary>
        public double? FalseNorthing { get; set; }
        /// <summary>
        /// the UTM Zone. A value between 1 and 60
        /// </summary>
        public int Zone { get; set; } = 1;
        /// <summary>
        /// true for the south hemisphere
        /// </summary>
        public bool IsSouth { get; set; }
        /// <summary>
        /// set to use the hyperbolic form of the projection
        /// </summary>
        public bool IsHyperbolic { get; set; }
        /// <summary>
        /// Projection height
        /// </summary>
        public double? ProjectionHeight { get; set; }
        /// <summary>
        /// the view point height
        /// </summary>
        public double? HeightViewPoint { get; set; }
        /// <summary>
        /// the sweep angle axis of the viewing instrument
        /// </summary>
        public AxisType Sweep { get; set; }
        /// <summary>
        /// Azimuth of central line
        /// </summary>
        public double? AzimuthCentralLine { get; set; }
        /// <summary>
        /// Weight parameter as for example used in the Lagrange projection
        /// </summary>
        public double? Weight { get; set; }
        /// <summary>
        /// the identifier of the landsat satellite (a value between 1 and 5)
        /// </summary>
        public int? Landsat { get; set; }
        /// <summary>
        /// the code of the path followed by the satellite
        /// </summary>
        public int? Path { get; set; }
        /// <summary>
        /// Azimuth of centerline at the center point of the line
        /// </summary>
        public double? Alpha { get; set; }
        /// <summary>
        /// Azimuth centerline of the rectified bearing of centre line
        /// </summary>
        public double? Gamma { get; set; }
        /// <summary>
        /// Longitude 1
        /// </summary>
        public double? Longitude1 { get; set; }
        /// <summary>
        /// Longitude 1
        /// </summary>
        public double? Longitude2 { get; set; }
        /// <summary>
        /// Longitude 1
        /// </summary>
        public double? LongitudeCentralPoint { get; set; }
        /// <summary>
        /// no offset option
        /// </summary>
        public bool NoOffset { get; set; }
        /// <summary>
        /// no rotation option
        /// </summary>
        public bool NoRotation { get; set; }

        public AreaNormalizationTransformType AreaNormalizationTransform { get; set; }
        /// <summary>
        /// peg latitude
        /// </summary>
        public double? PegLatitude { get; set; }
        /// <summary>
        /// peg Longitude
        /// </summary>
        public double? PegLongitude { get; set; }
        /// <summary>
        /// peg heading
        /// </summary>
        public double? PegHeading { get; set; }
        /// <summary>
        /// the n parameter
        /// </summary>
        public double? N { get; set; }
        /// <summary>
        /// the q parameter
        /// </summary>
        public double? Q { get; set; }
        /// <summary>
        /// default constructor
        /// </summary>
        public CartographicProjection()
        {

        }
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="src"></param>
        public CartographicProjection(CartographicProjection src)
        {
            if (src != null)
            {
                src.Copy(this);
            }
        }
        /// <summary>
        /// copy everything except the ID
        /// </summary>
        /// <param name="dest"></param>
        /// <returns></returns>
        public bool Copy(CartographicProjection dest)
        {
            if (dest != null)
            {
                dest.Name = Name;
                dest.Description = Description;
                dest.ProjectionType = ProjectionType;
                dest.FalseEasting = FalseEasting;
                dest.FalseNorthing = FalseNorthing;
                dest.GeodeticDatumID = GeodeticDatumID;
                dest.LatitudeOrigin = LatitudeOrigin;
                dest.LongitudeOrigin = LongitudeOrigin;
                dest.Zone = Zone;
                dest.IsSouth = IsSouth;
                dest.Latitude1 = Latitude1;
                dest.Latitude2 = Latitude2;
                dest.LatitudeTrueScale = LatitudeTrueScale;
                dest.Scaling = Scaling;
                dest.IsHyperbolic = IsHyperbolic;
                dest.ProjectionHeight = ProjectionHeight;
                dest.HeightViewPoint = HeightViewPoint;
                dest.Sweep = Sweep;
                dest.AzimuthCentralLine = AzimuthCentralLine;
                dest.Weight = Weight;
                dest.Landsat = Landsat;
                dest.Path = Path;
                dest.Alpha = Alpha;
                dest.Gamma = Gamma;
                dest.Longitude1 = Longitude1;
                dest.Longitude2 = Longitude2;
                dest.LongitudeCentralPoint = LongitudeCentralPoint;
                dest.NoOffset = NoOffset;
                dest.NoRotation = NoRotation;
                dest.AreaNormalizationTransform = AreaNormalizationTransform;
                dest.PegLatitude = PegLatitude;
                dest.PegLongitude = PegLongitude;
                dest.PegHeading = PegHeading;
                dest.N = N;
                dest.Q = Q;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// return the proj4 initialization string
        /// </summary>
        /// <returns></returns>
        public string GetProj4String()
        {
            CartographicProjectionType? cartographicProjectionType = CartographicProjectionType.Get(ProjectionType);
            if (cartographicProjectionType != null)
            {
                string sval = cartographicProjectionType.GetProj4String();
                if (cartographicProjectionType.UseFalseEastingNorthing)
                {
                    if (FalseEasting != null)
                    {
                        sval += " +x_0=" + FalseEasting.Value.ToString(CultureInfo.InvariantCulture);
                    }
                    if (FalseNorthing != null)
                    {
                        sval += " +y_0=" + FalseNorthing.Value.ToString(CultureInfo.InvariantCulture);
                    }
                }
                if (cartographicProjectionType.UseLatitudeOrigin)
                {
                    if (LatitudeOrigin != null)
                    {
                        double latOrigin = Utils.ToDegree(LatitudeOrigin);
                        sval += " +lat_0=" + latOrigin.ToString(CultureInfo.InvariantCulture);
                    }
                }
                if (cartographicProjectionType.UseLongitudeOrigin)
                {
                    if (LongitudeOrigin != null)
                    {
                        double longOrigin = Utils.ToDegree(LongitudeOrigin);
                        sval += " +long_0=" + longOrigin.ToString(CultureInfo.InvariantCulture);
                    }
                }
                if (cartographicProjectionType.UseZone)
                {
                    if (Zone > 0 && Zone <= 60)
                    {
                        sval += " +zone=" + Zone.ToString();
                    }
                    else
                    {
                        sval += " +zone=1";
                    }
                }
                if (cartographicProjectionType.UseSouth)
                {
                    if (IsSouth)
                    {
                        sval += " +south";
                    }
                }
                if (cartographicProjectionType.UseLatitude1)
                {
                    if (Latitude1 != null)
                    {
                        double latetude1 = Utils.ToDegree(Latitude1);
                        sval += " +lat_1=" + latetude1.ToString(CultureInfo.InvariantCulture);
                    }
                }
                if (cartographicProjectionType.UseLatitude2)
                {
                    if (Latitude2 != null)
                    {
                        double latetude2 = Utils.ToDegree(Latitude2);
                        sval += " +lat_2=" + latetude2.ToString(CultureInfo.InvariantCulture);
                    }
                }
                if (cartographicProjectionType.UseScaling)
                {
                    if (Scaling != null)
                    {
                        double scaling = Scaling.Value;
                        sval += " +k_0=" + scaling.ToString(CultureInfo.InvariantCulture);
                    }
                }
                if (cartographicProjectionType.UseLatitudeTrueScale)
                {
                    if (LatitudeTrueScale != null)
                    {
                        double latetudeTrueScale = Utils.ToDegree(LatitudeTrueScale);
                        sval += " +lat_ts=" + latetudeTrueScale.ToString(CultureInfo.InvariantCulture);
                    }
                }
                if (cartographicProjectionType.UseHyperbolic)
                {
                    if (IsHyperbolic)
                    {
                        sval += " +hyperbolic";
                    }
                }
                if (cartographicProjectionType.UseProjectionHeight)
                {
                    if (ProjectionHeight != null)
                    {
                        double projectionHeight = ProjectionHeight.Value;
                        sval += " +h_0=" + projectionHeight.ToString(CultureInfo.InvariantCulture);
                    }
                }
                if (cartographicProjectionType.UseHeightViewPoint)
                {
                    if (HeightViewPoint != null)
                    {
                        double heightViewPoint = HeightViewPoint.Value;
                        sval += " +h=" + heightViewPoint.ToString(CultureInfo.InvariantCulture);
                    }
                }
                if (cartographicProjectionType.UseSweep)
                {
                    if (Sweep != AxisType.None)
                    {
                        switch ((AxisType)Sweep)
                        {
                            case AxisType.x:
                                sval += " +sweep=x";
                                break;
                            case AxisType.y:
                                sval += " +sweep=y";
                                break;
                        }
                    }
                }
                if (cartographicProjectionType.UseAzimuthCentralLine)
                {
                    if (AzimuthCentralLine != null)
                    {
                        double azimuthCentralLine = Utils.ToDegree(AzimuthCentralLine);
                        sval += " +azi=" + azimuthCentralLine.ToString(CultureInfo.InvariantCulture);
                    }
                }
                if (cartographicProjectionType.UseWeight)
                {
                    if (Weight != null)
                    {
                        double weight = Weight.Value;
                        sval += " +W=" + weight.ToString(CultureInfo.InvariantCulture);
                    }
                }
                if (cartographicProjectionType.UseLandsat)
                {
                    if (Landsat != null)
                    {
                        int landsat = Landsat.Value;
                        sval += " +lsat=" + landsat.ToString();
                    }
                }
                if (cartographicProjectionType.UsePath)
                {
                    if (Path != null)
                    {
                        int path = Path.Value;
                        sval += " +path=" + path.ToString();
                    }
                }
                if (cartographicProjectionType.UseAlpha)
                {
                    if (Alpha != null)
                    {
                        double alpha = Utils.ToDegree(Alpha);
                        sval += " +alpha=" + alpha.ToString(CultureInfo.InvariantCulture);
                    }
                }
                if (cartographicProjectionType.UseGamma)
                {
                    if (Gamma != null)
                    {
                        double gamma = Utils.ToDegree(Gamma);
                        sval += " +gamma=" + gamma.ToString(CultureInfo.InvariantCulture);
                    }
                }
                if (cartographicProjectionType.UseLongitudeCentralPoint)
                {
                    if (LongitudeCentralPoint != null)
                    {
                        double longitudeCentralPoint = Utils.ToDegree(LongitudeCentralPoint);
                        sval += " +lonc=" + longitudeCentralPoint.ToString(CultureInfo.InvariantCulture);
                    }
                }
                if (cartographicProjectionType.UseLongitude1)
                {
                    if (Longitude1 != null)
                    {
                        double longitude1 = Utils.ToDegree(Longitude1);
                        sval += " +lon_1=" + longitude1.ToString(CultureInfo.InvariantCulture);
                    }
                }
                if (cartographicProjectionType.UseLongitude2)
                {
                    if (Longitude2 != null)
                    {
                        double longitude2 = Utils.ToDegree(Longitude2);
                        sval += " +lon_2=" + longitude2.ToString(CultureInfo.InvariantCulture);
                    }
                }
                if (cartographicProjectionType.UseNoOffset)
                {
                    if (NoOffset)
                    {
                        sval += " +no_off";
                    }
                }
                if (cartographicProjectionType.UseNoRotation)
                {
                    if (NoRotation)
                    {
                        sval += " +no_rot";
                    }
                }
                if (cartographicProjectionType.UseAreaNormalizationTransform)
                {
                    sval += " +UVtoST=";
                    switch (AreaNormalizationTransform)
                    {
                        case AreaNormalizationTransformType.Linear:
                            sval += "linear";
                            break;
                        case AreaNormalizationTransformType.Quadratic:
                            sval += "quadratic";
                            break;
                        case AreaNormalizationTransformType.Tangent:
                            sval += "tangent";
                            break;
                        default:
                            sval += "none";
                            break;
                    }
                }
                if (cartographicProjectionType.UsePegLatitude)
                {
                    if (PegLatitude != null)
                    {
                        double pegLatitude = Utils.ToDegree(PegLatitude);
                        sval += "plat_0=" + pegLatitude.ToString(CultureInfo.InvariantCulture);
                    }
                }
                if (cartographicProjectionType.UsePegLongitude)
                {
                    if (PegLongitude != null)
                    {
                        double pegLongitude = Utils.ToDegree(PegLongitude);
                        sval += "plon_0=" + pegLongitude.ToString(CultureInfo.InvariantCulture);
                    }
                }
                if (cartographicProjectionType.UsePegHeading)
                {
                    if (PegHeading != null)
                    {
                        double pegHeading = Utils.ToDegree(PegHeading);
                        sval += "phdg_0=" + pegHeading.ToString(CultureInfo.InvariantCulture);
                    }
                }
                if (cartographicProjectionType.UseN)
                {
                    if (N != null)
                    {
                        double nd = N.Value;
                        sval += "n=" + nd.ToString(CultureInfo.InvariantCulture);
                    }
                }
                if (cartographicProjectionType.UseQ)
                {
                    if (Q != null)
                    {
                        double qd = Q.Value;
                        sval += "q=" + qd.ToString(CultureInfo.InvariantCulture);
                    }
                }
                return sval;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
