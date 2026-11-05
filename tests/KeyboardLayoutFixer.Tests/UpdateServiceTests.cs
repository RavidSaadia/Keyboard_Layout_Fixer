using KeyboardLayoutFixer.Services;
using Xunit;

namespace KeyboardLayoutFixer.Tests
{
    public class UpdateServiceTests
    {
        [Theory]
        [InlineData("https://github.com/RavidSaadia/Keyboard_Layout_Fixer/releases/tag/v1.3.0", "1.3.0")]
        [InlineData("https://github.com/RavidSaadia/Keyboard_Layout_Fixer/releases/tag/1.3.0", "1.3.0")]
        [InlineData("https://github.com/RavidSaadia/Keyboard_Layout_Fixer/releases/tag/v1.3", "1.3")]
        [InlineData("https://github.com/RavidSaadia/Keyboard_Layout_Fixer/releases/tag/v2.0.1/", "2.0.1")]
        public void ParseTagFromLocation_ValidTag_ReturnsVersion(string location, string expected)
        {
            Assert.Equal(Version.Parse(expected), UpdateService.ParseTagFromLocation(location));
        }

        [Theory]
        [InlineData("")]
        [InlineData("https://github.com/RavidSaadia/Keyboard_Layout_Fixer/releases")]
        [InlineData("https://github.com/RavidSaadia/Keyboard_Layout_Fixer/releases/tag/latest")]
        [InlineData("https://github.com/RavidSaadia/Keyboard_Layout_Fixer/releases/tag/v1.3.0-beta")]
        public void ParseTagFromLocation_InvalidTag_ReturnsNull(string location)
        {
            Assert.Null(UpdateService.ParseTagFromLocation(location));
        }

        [Theory]
        [InlineData("1.3.0", "1.2.9", true)]
        [InlineData("1.2.10", "1.2.9", true)]
        [InlineData("2.0", "1.9.9", true)]
        [InlineData("1.3.0", "1.3.0", false)]
        [InlineData("1.3", "1.3.0", false)]
        [InlineData("1.2.9", "1.3.0", false)]
        [InlineData("1.3.0", "1.3.0.0", false)]
        [InlineData("1.3.0.0", "1.3", false)]
        [InlineData("1.3.1", "1.3.0.0", true)]
        public void IsNewer_ComparesVersions(string remote, string current, bool expected)
        {
            Assert.Equal(expected, UpdateService.IsNewer(Version.Parse(remote), Version.Parse(current)));
        }
    }
}
