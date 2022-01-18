using System;

using Xunit;

namespace CancerGov.CTS.Print.Rendering.Tests
{
    public class PrintRenderer_Instantiation
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("     ")]
        public void InvalidTemplatePath(string path)
        {
            Assert.Throws<ArgumentNullException>(
                () => new PrintRenderer(path)
            );
        }

        [Fact]
        public void ValidPath()
        {
            var renderer = new PrintRenderer("c:\\somepath\\sometemplate.vm");

            Assert.NotNull(renderer);
        }
    }
}
