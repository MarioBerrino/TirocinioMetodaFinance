using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kmeans_Clustering.Clustering_Kmeans
{
    internal class Output
    {
        public class ClusterPrediction
        {
            [ColumnName("PredictedLabel")]
            public uint ClusterId;

            [ColumnName("Score")]
            public float[] Score;
        }
    }
}
