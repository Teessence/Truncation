using NUnit.Framework;

namespace Truncation.Tests
{
    public class OpticalCharacterRecognitionTests
    {
        [Test]
        public static void GetTesseractEngineLanguageByTargetString()
        {
            Assert.AreEqual("eng", OpticalCharacterRecognition.GetTesseractEngineLanguageByTargetString("Welcome, John"));
            Assert.AreEqual("eng", OpticalCharacterRecognition.GetTesseractEngineLanguageByTargetString("Welcome, Zdzisław"));
            Assert.AreEqual("eng", OpticalCharacterRecognition.GetTesseractEngineLanguageByTargetString("ł"));
        }
    }
}