using Voyage.Common.Utilities;
using Xunit;

namespace Voyage.Services.Tests.Utilities
{
    public class TextNormalizerTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\r\n")]
        public void SingleLine_EmptyOrWhitespace_ReturnsEmptyString(string? value)
        {
            Assert.Equal(string.Empty, TextNormalizer.SingleLine(value));
        }

        [Theory]
        [InlineData("Rome", "Rome")]
        [InlineData("  Rome  ", "Rome")]
        [InlineData("Seven   Rila    Lakes", "Seven Rila Lakes")]
        [InlineData("  Train   Prague -  Budapest ", "Train Prague - Budapest")]
        public void SingleLine_ExtraSpaces_AreCollapsed(string value, string expected)
        {
            Assert.Equal(expected, TextNormalizer.SingleLine(value));
        }

        [Fact]
        public void SingleLine_TabsAndNewLines_BecomeSingleSpaces()
        {
            Assert.Equal("Hotel 3 nights", TextNormalizer.SingleLine("Hotel\t3\r\nnights"));
        }

        [Fact]
        public void SingleLine_MixedWhitespaceAroundText_IsTrimmed()
        {
            Assert.Equal("Old Town", TextNormalizer.SingleLine("\r\n \t Old \n\n Town \t\r\n"));
        }

        [Fact]
        public void SingleLine_PunctuationAndUnicode_AreKept()
        {
            Assert.Equal("Café & Bar – Plovdiv, 2026!", TextNormalizer.SingleLine(" Café  &  Bar – Plovdiv,  2026! "));
        }

        [Fact]
        public void SingleLine_CyrillicText_IsNormalized()
        {
            Assert.Equal("Велико Търново", TextNormalizer.SingleLine("  Велико    Търново "));
        }
    }
}
