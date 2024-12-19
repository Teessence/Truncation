namespace Truncation
{
    public class StringOperations
    {
        //(new[] { "" }, ""),

        public static List<(string[] targets, string replacement)> replacements = new List<(string[] targets, string replacement)>
            {
                (new[] { "أ", "إ", "آ" }, "ا"),
                (new[] { "ة" }, "ه"),
                (new[] { "ب", "ى", "ي", "ئ" }, "ي"),
                (new[] { "ف", "ق" }, "ف"),
                (new[] { "ح", "ج", "خ" }, "ح"),
                (new[] { "س", "ش" }, "س"),
                (new[] { "ص", "ض" }, "ص"),
                (new[] { "ط", "ظ" }, "ط"),
                (new[] { "ع", "غ" }, "ع"),
                (new[] { "و", "ؤ" }, "و"),
                (new[] { "ه", "ح" }, "ه"),
                (new[] { "ż", "ź", "ž" }, "z"),
                (new[] { "в" }, "b"),
                (new[] { "а́", "а", "á", "à", "ą", "â", "ä" }, "a"),
                (new[] { "е́", "ё", "е", "é", "è", "ę", "ê", "ë" }, "e"),
                (new[] { "ö", "ó", "ô", "о́" }, "o"),
                (new[] { "ć", "ç", "č" }, "c"),
                (new[] { "ń", "ň" }, "n"),
                (new[] { "ś", "$", "š" }, "s"),
                (new[] { "і́", "ł", "1", "l", "î", "ï", "ì", "í", "ľ" }, "i"),
                (new[] { "ù", "ú", "û", "ü" }, "u"),
                (new[] { "ÿ", "ý", "у́" }, "y"),
                (new[] { "ď" }, "d"),
                (new[] { "ř", "г" }, "r"),
                (new[] { "ť" }, "t")
            };

        // TargetString: Text from Screenshot, SourceString: Text from OCR 
        public static bool IsTruncated(string TargetString, string SourceString)
        {
            TargetString = ReplaceSimilar(TargetString);
            SourceString = ReplaceSimilar(SourceString);

            TargetString = TargetString.ToLowerInvariant();
            SourceString = SourceString.ToLowerInvariant();

            Dictionary<char, int> TargetCharacterCounts = GetCharacterCounts(TargetString);
            Dictionary<char, int> SourceCharacterCounts = GetCharacterCounts(SourceString);

            var returnable = !IsDictionarySubset(TargetCharacterCounts, SourceCharacterCounts);

            Console.WriteLine("Automation: " + TargetString);
            Console.WriteLine("OCR: " + SourceString);

            return returnable;
        }

        //TODO
        // Do it in some better way instead of one by one.
        public static string ReplaceSimilar(string String)
        {
            foreach (var (targets, replacement) in replacements)
            {
                foreach (var target in targets)
                {
                    String = String.Replace(target, replacement, StringComparison.InvariantCultureIgnoreCase);
                }
            }

            return String;
        }

        public static bool IsDictionarySubset(Dictionary<char, int> smaller, Dictionary<char, int> larger)
        {
            foreach (var pair in smaller)
            {
                char key = pair.Key;
                int count = pair.Value;

                if (!larger.ContainsKey(key) || larger[key] < count)
                {
                    return false;
                }
            }

            return true;
        }

        public static Dictionary<char, int> GetCharacterCounts(string input)
        {
            Dictionary<char, int> characterCounts = new Dictionary<char, int>();

            foreach (char c in input)
            {
                if (char.IsWhiteSpace(c))
                {
                    continue;
                }

                if (characterCounts.ContainsKey(c))
                {
                    characterCounts[c]++;
                }
                else
                {
                    characterCounts[c] = 1;
                }
            }

            return characterCounts;
        }

        public static HashSet<char> GetUniqueCharacters(string input)
        {
            var uniqueChars = new HashSet<char>();

            foreach (char c in input)
            {
                char lowerChar = char.ToLowerInvariant(c);
                uniqueChars.Add(lowerChar);
            }

            return uniqueChars;
        }
    }
}