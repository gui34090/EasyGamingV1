namespace EasyGamingV1.Core;

public static class Converter
{
    // Simple baseline: centimeters per 360° turn given DPI and in-game sensitivity ratio.
    public static double CmPer360(double dpi, double sensitivity)
    {
        // distance (inches) = 360° / (sensitivity * 0.1 * dpiFactor)  -- placeholder simplified model
        // For v1 baseline, assume degrees per count proportional to sensitivity; calibrate later w/ goldens.
        // Here: counts per 360 = 360 / (sensitivity * 0.1); inches = counts / dpi; cm = inches * 2.54
        var countsPer360 = 360.0 / System.Math.Max(0.0001, sensitivity * 0.1);
        var inches = countsPer360 / dpi;
        return inches * 2.54;
    }
}
