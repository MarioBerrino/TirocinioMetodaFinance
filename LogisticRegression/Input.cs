using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kmeans_Clustering.LogisticRegression
{
    internal class Input
    {
        public class DataPoints
        {
            [LoadColumn(0)]
            public int CNTRCT_ID { get; set; }

            [LoadColumn(1)]
            public int INSTRMNT_ID { get; set; }

            [LoadColumn(2)]
            public float Codice_Controparte { get; set; }

            [LoadColumn(3)]
            public int TypeofInstrument { get; set; }

            [LoadColumn(4)]
            public int ImpairmentStatus { get; set; }

            [LoadColumn(5)]
            public int ONA { get; set; }

            [LoadColumn(6)]
            public int ValoriAnomali { get; set; }


        }
    }
}
