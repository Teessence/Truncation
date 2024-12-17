namespace Truncation
{
    public class Training
    {
        public static void Train()
        {
            string[] files = Directory.GetFiles(@"C:\tessdata_best-main\tessdata_best-main", "*.traineddata");

            foreach (string file in files)
            {
                Console.WriteLine("file: " + file);
                string fileName = Path.GetFileNameWithoutExtension(file);
                Console.WriteLine("filename: " + fileName);
                Database.InsertIntoTesseractLanguages(fileName, 0);
            }


            string[] filesTraining = Directory.GetFiles(@"Training", "*.txt");
            foreach (string file in filesTraining)
            {
                Console.WriteLine("trainingfile: " + file);
                string trainingfilefilename = Path.GetFileNameWithoutExtension(file);
                Console.WriteLine("trainingfilefilename: " + trainingfilefilename);

                int IdOfLanguage = Database.GetIdOfLanguage(trainingfilefilename);

                Console.WriteLine("IdOfLanguage: " + IdOfLanguage);


                string fileContents = File.ReadAllText(file);

                foreach (char c in fileContents)
                {
                    if (char.IsWhiteSpace(c))
                    {
                        continue;  // Skip to the next character
                    }
                    Database.SaveCharacter(c, IdOfLanguage);
                    Console.WriteLine(c);
                }
            }
        }
    }
}