using Tesseract;

namespace Truncation
{
    public class OpticalCharacterRecognition
    {
        public static string SQLiteDatabaseConnection = Global.config["ConnectionStrings:SQLiteDatabaseConnection"];

        public static List<string> GetTesseractEngineLanguagesByTargetString(string TargetString)
        {
            HashSet<char> uniqueCharacters = StringOperations.GetUniqueCharacters(TargetString);

            List<string> TesseractLanguages = [];

            foreach (char character in uniqueCharacters)
            {
                string Language = Database.GetCharacterLanguage(character);

                if (Language.Length > 0)
                {
                    TesseractLanguages.Add(Language);
                }
            }

            TesseractLanguages = TesseractLanguages.Distinct().ToList();
            TesseractLanguages = StringOperations.GetAllCombinations(TesseractLanguages);
            return TesseractLanguages;
        }

        // Returns whether strings is truncated or not (bool).
        public static bool RunOpticalCharacterRecognition(byte[] Screenshot, string TargetString)
        {
            bool returnable = true;

            List<string> TesseractLanguages = GetTesseractEngineLanguagesByTargetString(TargetString);

            if (TesseractLanguages.Count == 0)
            {
                Console.WriteLine("Tesseract languages found = 0 for: " + TargetString);
                return false;
            }

            foreach (string TesseractLanguage in TesseractLanguages)
            {
                Console.WriteLine("Running tesseractLanguages: " + TesseractLanguage);
                Console.WriteLine(TargetString);

                using (var engine = new TesseractEngine(Global.TessdataFolderPath, TesseractLanguage, EngineMode.Default))
                {
                    using (var pix = Pix.LoadFromMemory(Screenshot))
                    {
                        using (var page = engine.Process(pix))
                        {
                            string text = page.GetText();

                            if (!StringOperations.IsTruncated(TargetString, text))
                            {
                                returnable = false;
                                break;
                            }
                        }
                    }
                }
            }

            return returnable;
        }
    }
}