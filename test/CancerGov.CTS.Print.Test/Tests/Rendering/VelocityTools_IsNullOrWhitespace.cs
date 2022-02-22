using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;
using Xunit;


namespace CancerGov.CTS.Print.Rendering.Tests
{
    public class VelocityTools_IsNullOrWhitespace
    {
        /// <summary>
        /// Scenarios for the string overload of IsNullOrWhitespace.
        /// </summary>
        [Theory]
        [InlineData("chicken", false)]
        [InlineData("", true)]
        [InlineData(null, true)]
        public void Strings(string value, bool expected)
        {
            var tools = new VelocityTemplate.VelocityTools();

            bool actual = tools.IsNullOrWhitespace(value);

            Assert.Equal(expected, actual);
        }

        public static IEnumerable<object[]> JValueData = new[]
        {
            new object[] { new JValue("chicken"), false},
            new object[] { new JValue(String.Empty), true},
            new object[] { new JValue((string)null), true},
            new object[] { null, true},
            new object[] { new JValue(5), false},
        };

        /// <summary>
        /// Scenarios for the JValue overload of IsNullOrWhitespace.
        /// </summary>
        [Theory]
        [MemberData(nameof(JValueData))]
        public void JValues(JValue value, bool expected)
        {
            var tools = new VelocityTemplate.VelocityTools();

            bool actual = tools.IsNullOrWhitespace(value);

            Assert.Equal(expected, actual);
        }

    }
}
