using Tesseract;

namespace Truncation
{
    public class OpticalCharacterRecognition
    {
        //TODO
        // Should return values based on characters in targetstring, so if it cotains japanese characters only shuld terun jap etc...
        // if Polish + Japanese should return pol+jap
        // Write tests for it
        // PERHAPS: We could do it so that run the original langugage + english if it contains english characters.
        public static string GetTesseractEngineLanguageByTargetString(string TargetString)
        {
            return "eng";
        }

        //TODO
        // replace hardcoded path with Settings one
        public static string RunOpticalCharacterRecognition(byte[] Screenshot, string TargetString)
        {
            using (var engine = new TesseractEngine(@"C:\tessdata_best-main\tessdata_best-main", GetTesseractEngineLanguageByTargetString(TargetString), EngineMode.Default))
            {
                using (var pix = Pix.LoadFromMemory(Screenshot))
                {
                    using (var page = engine.Process(pix))
                    {
                        string text = page.GetText();
                        return text;
                    }
                }
            }
        }
    }
}