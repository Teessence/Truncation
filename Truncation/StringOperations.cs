namespace Truncation
{
    public class StringOperations
    {
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
        // Replace similar such as i, 1 with l
        // $ with s
        // Do it in some better way instead of one by one.
        public static string ReplaceSimilar(string String)
        {
            String = String.Replace("1", "i", StringComparison.InvariantCultureIgnoreCase);
            String = String.Replace("l", "i", StringComparison.InvariantCultureIgnoreCase);

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
    }
}