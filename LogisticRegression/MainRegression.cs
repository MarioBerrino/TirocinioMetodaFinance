using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Kmeans_Clustering.Clustering_Kmeans;
using Kmeans_Clustering.LogisticRegression;
using Microsoft.ML;
using Microsoft.ML.Calibrators;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using static Kmeans_Clustering.LogisticRegression.Output;

namespace Kmeans_Clustering.LogisticRegression
{
    internal class MainRegression
    {
        public static void Execute()
        {
            //creo vettore con i numeri di campioni dei dataset
            int[] num_samples = { 14, 50, 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000, 1100, 1246 };

            //sovrascrivo il file ad ogni avvio del programma
            if (File.Exists("C:\\Users\\aless\\OneDrive\\Desktop\\UNI\\TIROCINIO\\LavoroFinale\\RegressionResults\\Results.csv"))
            {
                File.Delete(("C:\\Users\\aless\\OneDrive\\Desktop\\UNI\\TIROCINIO\\LavoroFinale\\RegressionResults\\Results.csv"));
                System.Console.WriteLine("File eliminato con successo");
            }
            foreach (var numSample in num_samples)
            {
                Console.WriteLine($"Esecuzione con {numSample} campioni sintetici:");
                AnomalyDetection(numSample);

                Console.WriteLine();
            }
        }
        static string ConvertExcelToCSV(string excelFilePath)
        {
            try
            {
                // Carica il foglio di lavoro Excel utilizzando ClosedXML
                var workbook = new XLWorkbook(excelFilePath);
                var ws = workbook.Worksheets.Worksheet(1);

                // Nome del file CSV
                var csvFilePath = Path.ChangeExtension(excelFilePath, ".csv");

                // Scrivi i dati in un file CSV
                using (var writer = new StreamWriter(csvFilePath))
                {
                    var firstRow = true;

                    foreach (var row in ws.Rows())
                    {
                        if (firstRow)
                        {
                            // Scrivi l'intestazione nel file CSV
                            writer.WriteLine(string.Join(",", row.CellsUsed().Select(cell => cell.GetString())));
                            firstRow = false;
                        }
                        else
                        {
                            // Scrivi i dati nella riga corrente nel file CSV
                            writer.WriteLine(string.Join(",", row.CellsUsed().Select(cell => cell.GetString())));
                        }
                    }
                }

                Console.WriteLine($"File CSV creato con successo: {csvFilePath}");
                return csvFilePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante la conversione da Excel a CSV: {ex.Message}");
                return null;
            }
        }
        static void PrintResult(RegressionPrediction result)
        {
            Console.WriteLine($"Prediction: {result.IsAnomalous} | Score: {result.Score} | Probability: {result.Probability}");
        }

        static void WriteResultsToCSV(string filePath, int numsamples, double sensitivity, double specificity, double accuracy, double gMean)
        {
            try
            {
                // Dati da scrivere nel file CSV
                var row = $"{numsamples},{sensitivity.ToString("G", CultureInfo.InvariantCulture)},{specificity.ToString("G", CultureInfo.InvariantCulture)},{accuracy.ToString("G", CultureInfo.InvariantCulture)},{gMean.ToString("G", CultureInfo.InvariantCulture)}";

                // Scrivi i dati nel file CSV in modalità append
                using (var writer = File.AppendText(filePath))
                {
                    // Se il file è vuoto, scrivi l'intestazione
                    if (new FileInfo(filePath).Length == 0)
                    {
                        var columns = "NumSamples,Sensitivity,Specificity,Accuracy,G-Mean";
                        writer.WriteLine(columns);
                    }

                    // Scrivi la riga nel file CSV
                    writer.WriteLine(row);

                    // Chiudi il writer
                    writer.Flush();
                    writer.Close();
                }

                Console.WriteLine($"Risultati scritti con successo in: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante la scrittura dei risultati in CSV: {ex.Message}");
            }
        }

        static void AnomalyDetection(int numsamples)
        {
            MLContext mlContext = new MLContext(seed: 1);

            // Imposta il percorso del file Excel contenente i dati
            var excelFilePath = $"C:\\Users\\aless\\OneDrive\\Desktop\\UNI\\TIROCINIO\\LavoroFinale\\PreparedDatasets\\{numsamples}SamplesDataset.xlsx";
            var excelFilePathTest = "C:\\Users\\aless\\OneDrive\\Desktop\\UNI\\TIROCINIO\\LavoroFinale\\Test Dataset Anacredit.xlsx";

            // Converte il file Excel in formato CSV
            var csvFilePath = ConvertExcelToCSV(excelFilePath);

            // Converte il file Excel in formato CSV
            var csvFilePathTest = ConvertExcelToCSV(excelFilePathTest);

            // Carica i dati dal file CSV
            var dataView = mlContext.Data.LoadFromTextFile<Input.DataPoints>(csvFilePath, hasHeader: true, separatorChar: ',');

            // Carica i dati dal file CSV
            var dataViewTest = mlContext.Data.LoadFromTextFile<Input.DataPoints>(csvFilePathTest, hasHeader: true, separatorChar: ',');

            // Costruzione Pipeline di trasformazioni
            var pipeline =
            mlContext.Transforms.Concatenate(outputColumnName: "Features", nameof(Input.DataPoints.TypeofInstrument), nameof(Input.DataPoints.ONA), nameof(Input.DataPoints.ValoriAnomali))
            .Append(mlContext.Transforms.Conversion.ConvertType("Features", outputKind: DataKind.Single))
            .Append(mlContext.Transforms.Conversion.ConvertType("Label", "ValoriAnomali", Microsoft.ML.Data.DataKind.Boolean))
            .Append(mlContext.BinaryClassification.Trainers.LbfgsLogisticRegression());


            //Addestramento del modello
            var model = pipeline.Fit(dataView);

            // Esegui la predizione
            var predictions = model.Transform(dataViewTest);

            // Estrai i risultati delle predizioni
            var results = mlContext.Data.CreateEnumerable<RegressionPrediction>(predictions, reuseRowObject: false).ToList();

            int truePositives = 0;
            int falsePositives = 0;
            int trueNegatives = 0;
            int falseNegatives = 0;

            // Accedi a InputModel per ottenere l'informazione necessaria
            var inputModels = mlContext.Data.CreateEnumerable<Inputs.DataPoints>(dataView, reuseRowObject: false).ToList();

            for (int i = 0; i < results.Count; i++)
            {
                var inputs = inputModels[i];
                var result = results[i];

                // Converte la previsione continua in una previsione binaria utilizzando la soglia di decisione
                // bool predictedLabel = result.Score >= decisionThreshold;

                // Valuta le prestazioni del modello
                if (result.IsAnomalous && inputs.ValoriAnomali == 1)
                {
                    truePositives++;
                }
                else if (result.IsAnomalous && inputs.ValoriAnomali == 0)
                {
                    falsePositives++;
                }
                else if (!result.IsAnomalous && inputs.ValoriAnomali == 0)
                {
                    trueNegatives++;
                }
                else if (!result.IsAnomalous && inputs.ValoriAnomali == 1)
                {
                    falseNegatives++;
                }
            }

            // Stampa i valori di TP, TN, FP e FN
            Console.WriteLine($"True Positives (TP): {truePositives}");
            Console.WriteLine($"True Negatives (TN): {trueNegatives}");
            Console.WriteLine($"False Positives (FP): {falsePositives}");
            Console.WriteLine($"False Negatives (FN): {falseNegatives}");

            double sensitivity = (double)truePositives / (truePositives + falseNegatives);
            double specificity = (double)trueNegatives / (trueNegatives + falsePositives);
            double accuracy = (double)(truePositives + trueNegatives) / (truePositives + trueNegatives + falsePositives + falseNegatives);
            double gMean = Math.Sqrt(sensitivity * specificity);




            int nCampioni = 0;
            int b = 0;
            foreach (var result in results)
            {

                if (result.IsAnomalous == true)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    //Console.WriteLine($"TypeofInstrument:{inputModels[b].TypeofInstrument}, ImpairmentStatus:{inputModels[b].ImpairmentStatus}, ONA:{inputModels[b].ONA}");
                    Console.ResetColor();
                    //PrintResult(result);
                    nCampioni++;
                }
                b++;

            }
            Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"numero totale di anomalie : {nCampioni}");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Sensitivity (Recall): {sensitivity}");
            Console.WriteLine($"Specificity: {specificity}");
            Console.WriteLine($"Accuracy: {accuracy}");
            Console.WriteLine($"G-Mean:{gMean}");
            Console.ResetColor();

            var resultsFilePath = $"C:\\Users\\aless\\OneDrive\\Desktop\\UNI\\TIROCINIO\\LavoroFinale\\RegressionResults\\Results.csv";
            WriteResultsToCSV(resultsFilePath, numsamples, sensitivity, specificity, accuracy, gMean);

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"Campioni sintetici utilizzati: {numsamples} ");
            Console.ResetColor();
            Console.ReadLine();
        }

    }
}


