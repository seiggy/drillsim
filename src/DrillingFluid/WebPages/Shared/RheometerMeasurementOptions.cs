namespace NORCE.Drilling.DrillingFluid.WebPages.Shared;

public class RheometerMeasurementOptions
{
    public bool showRPM { get; set; } = true;
    public bool showShearRate { get; set; } = false;
    public bool showShearRateBob { get; set; } = false;
    public bool showShearStress { get; set; } = true;
    public bool showShearStressBob { get; set; } = false;
    public bool showTorque { get; set; } = false;
}
