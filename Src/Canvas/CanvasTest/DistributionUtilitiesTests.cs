using System.Collections.Generic;
using System.Linq;
using CanvasCommon;
using Xunit;

namespace CanvasTest
{
    public class DistributionUtilitiesTests
    {
        [Fact]
        public void TestGetGenotypeCombinations()
        {
            int numSamples = 2;
            int altCN = 1;
            var result = DistributionUtilities.GetGenotypeCombinations(numSamples, altCN);

            Assert.Equal(new List<List<int>>
            {
                new []{ 1, 2}.ToList(),
                new []{ 2, 1}.ToList(),
            }, result);
        }
        
        [Fact(Skip = "Single sample SPW is broken")]
        public void TestGetGenotypeCombinationsSingleSample()
        {
            int numSamples = 1;
            int altCN = 1;
            var result = DistributionUtilities.GetGenotypeCombinations(numSamples, altCN);

            Assert.Equal(new[]
            {
                new []{ 2 }.ToList(),
                new []{ 1 }.ToList(),
            }.ToList(), result);
        }
    }
}