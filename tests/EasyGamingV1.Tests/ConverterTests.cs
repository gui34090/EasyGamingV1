using Xunit;
using EasyGamingV1.Core;

public class ConverterTests
{
    [Theory]
    [InlineData(800, 0.5)]
    [InlineData(1600, 1.0)]
    public void CmPer360_IsPositive(double dpi, double sens)
    {
        var cm = Converter.CmPer360(dpi, sens);
        Assert.True(cm > 0 && cm < 2000);
    }
}
