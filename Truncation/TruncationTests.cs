using System.Text.Json;
using NUnit.Framework;

namespace Truncation.Tests
{
    public class TruncationTests
    {
        public static IEnumerable<TestCaseData> GetTestCases()
        {
            string[] pngFiles = Directory.GetFiles("TestCases", "*.png");

            foreach (string filePath in pngFiles)
            {
                var CoordinatesFile = filePath.Replace(".png", ".json");
                var TruncationsFile = filePath.Replace(".png", ".txt");
                string TruncationsContent = File.ReadAllText(TruncationsFile);
                List<int> numbers = TruncationsContent.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                string CoordinatesContent = File.ReadAllText(CoordinatesFile);
                List<TextSegment> segments = JsonSerializer.Deserialize<List<TextSegment>>(CoordinatesContent);
                yield return new TestCaseData(filePath, segments, numbers);
            }
        }

        [TestCaseSource(nameof(GetTestCases))]
        public void TestScreenshotProcessing(string screenshotPath, List<TextSegment> coordinates, List<int> expectedOutput)
        {
            TestContext.WriteLine($"Running test with screenshotPath: {screenshotPath}");
            TestContext.WriteLine($"Coordinates:");

            foreach (var Coordinate in coordinates)
            {
                TestContext.WriteLine("\t " + Coordinate.ToString());
            }

            TestContext.WriteLine($"Expected Output: {string.Join(", ", expectedOutput)}");

            List<int> actualOutput = Truncation.Analyze(screenshotPath, coordinates);
            Assert.AreEqual(expectedOutput, actualOutput);
        }
    }
}