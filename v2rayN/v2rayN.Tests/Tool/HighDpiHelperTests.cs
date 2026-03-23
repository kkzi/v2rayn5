using System.Drawing;

using v2rayN.Tool;

using Xunit;

namespace v2rayN.Tests.Tool
{
    public class HighDpiHelperTests
    {
        [Theory]
        [InlineData(16, 96, 16)]
        [InlineData(16, 144, 24)]
        [InlineData(16, 192, 32)]
        [InlineData(80, 120, 100)]
        public void ScaleLogicalValue_ReturnsScaledPixels(int logicalValue, int deviceDpi, int expected)
        {
            int actual = HighDpiHelper.ScaleLogicalValue(logicalValue, deviceDpi);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(320, 160, 96, 320, 160)]
        [InlineData(320, 160, 144, 480, 240)]
        [InlineData(80, 32, 192, 160, 64)]
        public void ScaleLogicalSize_ReturnsScaledSize(int width, int height, int deviceDpi, int expectedWidth, int expectedHeight)
        {
            Size actual = HighDpiHelper.ScaleLogicalSize(new Size(width, height), deviceDpi);

            Assert.Equal(new Size(expectedWidth, expectedHeight), actual);
        }

        [Fact]
        public void ScaleLogicalValue_UsesLogicalValueWhenDpiIsInvalid()
        {
            int actual = HighDpiHelper.ScaleLogicalValue(32, 0);

            Assert.Equal(32, actual);
        }

        [Fact]
        public void NormalizeFontToPoints_ConvertsPixelFontToEquivalentPointFont()
        {
            using var source = new Font("Microsoft YaHei UI", 12f, FontStyle.Regular, GraphicsUnit.Pixel);
            using Font actual = HighDpiHelper.NormalizeFontToPoints(source);

            Assert.Equal(GraphicsUnit.Point, actual.Unit);
            Assert.Equal(9f, actual.SizeInPoints, 3);
            Assert.Equal(source.Style, actual.Style);
            Assert.Equal(source.FontFamily.Name, actual.FontFamily.Name);
        }

        [Fact]
        public void NormalizeFontToPoints_KeepsPointFontMetrics()
        {
            using var source = new Font("Microsoft YaHei UI", 9f, FontStyle.Bold, GraphicsUnit.Point);
            using Font actual = HighDpiHelper.NormalizeFontToPoints(source);

            Assert.Equal(GraphicsUnit.Point, actual.Unit);
            Assert.Equal(source.SizeInPoints, actual.SizeInPoints, 3);
            Assert.Equal(source.Style, actual.Style);
            Assert.Equal(source.FontFamily.Name, actual.FontFamily.Name);
        }

        [Theory]
        [InlineData(96, 144, 96, 144)]
        [InlineData(144, 96, 96, 144)]
        [InlineData(96, 0, 120, 120)]
        [InlineData(0, 0, 0, 96)]
        public void ResolveEffectiveDpi_PrefersBestAvailableDpi(int controlDpi, int windowDpi, int graphicsDpi, int expected)
        {
            int actual = HighDpiHelper.ResolveEffectiveDpi(controlDpi, windowDpi, graphicsDpi);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(11f, 96, 8.25f)]
        [InlineData(11f, 144, 5.5f)]
        [InlineData(12f, 96, 9f)]
        public void PixelsToPoints_UsesSpecifiedDpi(float pixelSize, int dpi, float expected)
        {
            float actual = HighDpiHelper.PixelsToPoints(pixelSize, dpi);

            Assert.Equal(expected, actual, 3);
        }

        [Fact]
        public void GetLogicalUiFont_UsesSystemUiFontForLegacyPixelFont()
        {
            using var source = new Font("Microsoft Sans Serif", 11f, FontStyle.Regular, GraphicsUnit.Pixel);
            using Font actual = HighDpiHelper.GetLogicalUiFont(source);

            Assert.Equal(GraphicsUnit.Point, actual.Unit);
            Assert.Equal(SystemFonts.MessageBoxFont.FontFamily.Name, actual.FontFamily.Name);
            Assert.Equal(SystemFonts.MessageBoxFont.SizeInPoints, actual.SizeInPoints, 3);
        }

        [Fact]
        public void GetLogicalUiFont_UsesSystemUiFontForLegacyPointFont()
        {
            using var source = new Font("Microsoft Sans Serif", 5.5f, FontStyle.Regular, GraphicsUnit.Point);
            using Font actual = HighDpiHelper.GetLogicalUiFont(source);

            Assert.Equal(GraphicsUnit.Point, actual.Unit);
            Assert.Equal(SystemFonts.MessageBoxFont.FontFamily.Name, actual.FontFamily.Name);
            Assert.Equal(SystemFonts.MessageBoxFont.SizeInPoints, actual.SizeInPoints, 3);
        }

        [Fact]
        public void AreFontsEquivalent_ReturnsTrueForEquivalentFonts()
        {
            using var left = new Font("Segoe UI", 9f, FontStyle.Regular, GraphicsUnit.Point);
            using var right = new Font("Segoe UI", 9f, FontStyle.Regular, GraphicsUnit.Point);

            Assert.True(HighDpiHelper.AreFontsEquivalent(left, right));
        }

        [Fact]
        public void AreFontsEquivalent_ReturnsFalseForDifferentFonts()
        {
            using var left = new Font("Segoe UI", 9f, FontStyle.Regular, GraphicsUnit.Point);
            using var right = new Font("Segoe UI", 10f, FontStyle.Regular, GraphicsUnit.Point);

            Assert.False(HighDpiHelper.AreFontsEquivalent(left, right));
        }

    }
}
