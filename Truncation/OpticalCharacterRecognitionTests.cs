using NUnit.Framework;

namespace Truncation.Tests
{
    public class OpticalCharacterRecognitionTests
    {
        [Test]
        public static void GetTesseractEngineLanguageByTargetString()
        {
            Assert.AreEqual(new List<string> { "eng" }, OpticalCharacterRecognition.GetTesseractEngineLanguagesByTargetString("Welcome, John"));
            Assert.AreEqual(new List<string> { "eng+pol", "eng", "pol" }, OpticalCharacterRecognition.GetTesseractEngineLanguagesByTargetString("Welcome, Zdzisław"));
            Assert.AreEqual(new List<string> { "pol" }, OpticalCharacterRecognition.GetTesseractEngineLanguagesByTargetString("ł"));
        }
    }
}