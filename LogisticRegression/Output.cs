using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kmeans_Clustering.LogisticRegression
{
    internal class Output
    {
        public class RegressionPrediction
        {
            [ColumnName("PredictedLabel")]
            public bool IsAnomalous;
            [ColumnName("Score")]
            public float Score;
            [ColumnName("Probability")]
            public float Probability;
        }
    }
}
