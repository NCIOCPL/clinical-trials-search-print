using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Moq;
using RichardSzalay.MockHttp;
using Xunit;

namespace CancerGov.ClinicalTrialsAPI.Test
{
    /// <summary>
    /// Tests for the ClinicalTrialsAPIClient.Chunks method.
    /// </summary>
    public class ClinicalTrialsAPIClientTests_Chunks
    {
        /// <summary>
        /// Verify chunking works correctly when chunk size is larger than the list's.
        /// </summary>
        [Fact]
        public void ChunkIsLargerThanListSize()
        {
            const int CHUNK_SIZE = 10;

            string[] input = new[] { "one", "two", "three" };
            string[] expected = new[] { "one", "two", "three" };

            IEnumerable<IEnumerable<string>> actual = ClinicalTrialsAPIClient.Chunks(input, CHUNK_SIZE);

            IEnumerable<string>[] actualAsArray = actual.ToArray();

            Assert.Single(actual);
            Assert.Equal(expected, actual.First());
        }

        /// <summary>
        /// Verify chunking works correctly when the chunk size is smaller than the list's.
        /// </summary>
        [Fact]
        public void ChunkIsSmallerThanListSize()
        {
            const int CHUNK_SIZE = 3;

            string[] input = new[] { "one", "two", "three", "four" };
            string[] expected1 = new[] { "one", "two", "three" };
            string[] expected2 = new[] { "four" };


            IEnumerable<IEnumerable<string>> actual = ClinicalTrialsAPIClient.Chunks(input, CHUNK_SIZE);

            IEnumerator<IEnumerable<string>> enumerator = actual.GetEnumerator();

            Assert.True(enumerator.MoveNext());
            Assert.Equal(expected1, enumerator.Current);

            Assert.True(enumerator.MoveNext());
            Assert.Equal(expected2, enumerator.Current);

            Assert.False(enumerator.MoveNext());
        }

        /// <summary>
        /// Verify chunking works correctly when the list's size is exactly divisible
        /// by the chunk size.
        /// </summary>
        [Fact]
        public void ChunkIsExactPortionOfListSize()
        {
            const int CHUNK_SIZE = 2;

            string[] input = new[] { "one", "two", "three", "four", "five", "six" };
            string[] expected1 = new[] { "one", "two" };
            string[] expected2 = new[] { "three", "four" };
            string[] expected3 = new[] { "five", "six" };


            IEnumerable<IEnumerable<string>> actual = ClinicalTrialsAPIClient.Chunks(input, CHUNK_SIZE);

            IEnumerator<IEnumerable<string>> enumerator = actual.GetEnumerator();

            Assert.True(enumerator.MoveNext());
            Assert.Equal(expected1, enumerator.Current);

            Assert.True(enumerator.MoveNext());
            Assert.Equal(expected2, enumerator.Current);

            Assert.True(enumerator.MoveNext());
            Assert.Equal(expected3, enumerator.Current);

            Assert.False(enumerator.MoveNext());
        }

    }
}
