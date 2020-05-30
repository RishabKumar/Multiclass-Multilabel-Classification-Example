using Microsoft.ML.Data;

namespace NewsCategoryPrediction.Models
{
    class NewsArticle
    {
        [LoadColumn(0)]
        public string Category { get; set; }
        [LoadColumn(1)]
        public string Headline { get; set; }
    }
}