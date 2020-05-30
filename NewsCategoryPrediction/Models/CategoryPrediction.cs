using Microsoft.ML.Data;
using System.Collections.Generic;
using System.IO;

namespace NewsCategoryPrediction.Models
{
    public class CategoryPrediction
    {
        [ColumnName("Classification")]
        public string Category { get; set; }

        [ColumnName("Score")]
        [VectorType]
        public float[] Score { get; set; }
    }
}