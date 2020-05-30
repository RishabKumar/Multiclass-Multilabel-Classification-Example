using Microsoft.ML;
using Microsoft.ML.Data;
using NewsCategoryPrediction.Models;
using NewsCategoryPrediction.Utilities;
using System;
using System.Linq;

namespace NewsCategoryPrediction
{
    /// <summary>
    /// Multiclass and Multilabel classification example
    /// </summary>
    class Program
    {
        private static MLContext _mlContext = new MLContext(0);
        private const string categoryFilePath = "category.txt";
        private const string trainingFilePath = "D://newsdata.csv";
        private const string trainedModelPath = "newsmodel.data";

        static void Main()
        {
            // No need to train everytime
            Train(trainingFilePath, trainedModelPath, '|');

            var model = LoadModel(trainedModelPath);
            PredictCategory(model, 0.1f);
        }

        /// <summary>
        /// Trains the model
        /// </summary>
        /// <param name="trainingDataPath">source of dataset</param>
        /// <param name="trainedModelPath">the path of model where it will be saved after training</param>
        /// <param name="separator">separator in dataset. Example , or |</param>
        /// <param name="hasHeader">if dataset has header column</param>
        public static void Train(string trainingDataPath, string trainedModelPath, char separator, bool hasHeader = true)
        {
            Console.WriteLine("Starting training...");
            // load training data
            var data = _mlContext.Data.LoadFromTextFile<NewsArticle>(trainingDataPath, separator, hasHeader, trimWhitespace: true);

            // collect all categories in the training data
            var categories = data.GetColumn<string>("Category");
            Category.SaveCategories(categories, categoryFilePath);

            // create pipeline
            var pipelineCategory = _mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "Label", inputColumnName: "Category") // Map the desired output column in dataset to a hypothetical column
               .Append(_mlContext.Transforms.Text.FeaturizeText(inputColumnName: "Headline", outputColumnName: "HeadlineFeaturized"))  // Map the input column(s) in dataset to a hypothetical column
               .Append(_mlContext.Transforms.Concatenate("Features", "HeadlineFeaturized"))  // Map all the hypothetical input columns into 1 hypothetical input column
               .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features"))  // Define what algorithm to be used
               .Append(_mlContext.Transforms.Conversion.MapKeyToValue("Classification", "PredictedLabel"));  // Map the output to the property in the NewsCategoryPrediction Model

            // train and create model
            var model = pipelineCategory.Fit(data);

            // save model
            _mlContext.Model.Save(model, data.Schema, trainedModelPath);
            Console.WriteLine("Training complete");
        }

        /// <summary>
        /// Loads the trained model
        /// </summary>
        /// <param name="trainedModelPath">Path of trainedModel</param>
        /// <returns></returns>
        public static ITransformer LoadModel(string trainedModelPath)
        {
            Console.WriteLine("Loading Model");
            return _mlContext.Model.Load(trainedModelPath, out var schema);
        }

        /// <summary>
        /// Predicts category with multi-classification and multi-label
        /// </summary>
        /// <param name="model">Trained Model</param>
        /// <param name="scoreDelta">Permitted delta of 'score of best label' and the 'score of other labels'</param>
        public static void PredictCategory(ITransformer model, float scoreDelta = 0.1f)
        {
            // create prediction engine
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<NewsArticle, CategoryPrediction>(model);
            var categories = Category.GetCategories(categoryFilePath);

            while (true)
            {
                var news = predictionEngine.Predict(new NewsArticle { Headline = GetInput() });
                Console.WriteLine("Best Match: " + news.Category);
                Console.Write("All possible categories:");
                var maxScore = news.Score.Max();
                for (var index = 0; index < news.Score.Count(); index++)
                {
                    if (maxScore - news.Score[ index ] <= scoreDelta)
                    {
                        Console.Write($"{categories.ElementAt(index)},");
                    }
                }
                Console.WriteLine("");
            }
        }

        private static string GetInput()
        {
            Console.Write("Enter news: ");
            return Console.ReadLine();
        }
    }
}