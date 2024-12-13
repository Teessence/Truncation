using NUnit.Framework;
using Truncation;

namespace Truncation.Tests
{
    public class TruncationTests
    {
        [Test]
        public void Add_TwoNumbers_ReturnsCorrectSum()
        {
            Assert.AreEqual(4, Truncation.Add(1, 3));
        }
    }
}