using System.Collections.Generic;
using System.IO;

namespace NewsCategoryPrediction.Utilities
{
    public static class Category
    {
        public static void SaveCategories(IEnumerable<string> categories, string categoryFilePath)
        {
            using (var streamWriter = new StreamWriter(File.OpenWrite(categoryFilePath)))
            {
                foreach (var category in categories)
                {
                    streamWriter.WriteLine(category);
                }
            }
        }

        public static HashSet<string> GetCategories(string categoryFilePath)
        {
            var allCategories = new HashSet<string>();
            foreach (var category in File.ReadAllLines(categoryFilePath))
            {
                allCategories.Add(category);
            }
            return allCategories;
        }
    }
}