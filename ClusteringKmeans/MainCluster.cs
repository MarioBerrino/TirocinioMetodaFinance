using System;
using System.IO;
using Microsoft.ML;
using Microsoft.ML.Data;
using ClosedXML.Excel;
using System.Linq;
using Microsoft.ML.Trainers;
using static Kmeans_Clustering.Clustering_Kmeans.Inputs;
using static Kmeans_Clustering.Clustering_Kmeans.Output;
using System.Collections.Generic;
using System.Globalization;
using DocumentFormat.OpenXml.Drawing;
namespace Kmeans_Clustering.Clustering_Kmeans
{
    internal class MainCluster
    {
        public static void Execute()
        {
            //creo vettore con i numeri di campioni dei dataset
            int[] num_samples = { 14, 50, 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000, 1100, 1246 };

            //sovrascrivo il file ad ogni avvio del programma
            if (File.Exists($"C:\\Users\\aless\\OneDrive\\Desktop\\UNI\\TIROCINIO\\LavoroFinale\\ClusteringResults\\Results.csv"))
            {
                File.Delete(($"C:\\Users\\aless\\OneDrive\\Desktop\\UNI\\TIROCINIO\\LavoroFinale\\ClusteringResults\\Results.csv"));
                System.Console.WriteLine("File eliminato con successo");
            }
            foreach (var numSample in num_samples)
            {
                Console.WriteLine($"Esecuzione con {numSample} campioni sintetici:");
                ClusteringAnomalyDetection(numSample);

                Console.WriteLine();
            }
        }

        static void IntestazioneCSV(string filePath)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    // Scrivi l'intestazione nel file CSV
                    writer.WriteLine("RowCount,Distance from Anomalous Cluster");
                }

                Console.WriteLine($"Risultati del clustering salvati con successo: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante il salvataggio dei risultati del clustering in CSV: {ex.Message}");
            }
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
        static string ConvertExcelToCSV(string excelFilePath)
        {
            try
            {
                // Carica il foglio di lavoro Excel utilizzando ClosedXML
                var workbook = new XLWorkbook(excelFilePath);
                var ws = workbook.Worksheets.Worksheet(1);

                // Nome del file CSV
                var csvFilePath = System.IO.Path.ChangeExtension(excelFilePath, ".csv");

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
        static void ClusteringAnomalyDetection(int numsamples)
        {
            // Imposta il percorso del file Excel contenente i dati
            var excelFilePath = $"C:\\Users\\aless\\OneDrive\\Desktop\\UNI\\TIROCINIO\\LavoroFinale\\PreparedDatasets\\{numsamples}SamplesDataset.xlsx";

            // Converte il file Excel in formato CSV
            var csvFilePath = ConvertExcelToCSV(excelFilePath);

            // Se la conversione ha avuto successo, continua con il resto del codice
            if (!string.IsNullOrEmpty(csvFilePath))
            {
                // Inizializza il contesto ML.NET
                var mlContext = new MLContext(seed: 1);

                // Carica i dati dal file CSV
                var dataView = mlContext.Data.LoadFromTextFile<Inputs.DataPoints>(csvFilePath, hasHeader: true, separatorChar: ',');

                // Configura l'algoritmo KMeans
                var featuresColumnName = "Features";
                var options = new KMeansTrainer.Options
                {
                    NumberOfClusters = 4,
                    MaximumNumberOfIterations = 1000,
                    OptimizationTolerance = 1e-6f,
                    NumberOfThreads = 1,
                };

                // Costruzione della pipeline di trasformazioni e addestramento
                var pipeline =
                    mlContext.Transforms.Concatenate(featuresColumnName, "TypeofInstrument", "ImpairmentStatus", "ONA")
                    .Append(mlContext.Transforms.Conversion.ConvertType(featuresColumnName, outputKind: DataKind.Single))
                    .Append(mlContext.Clustering.Trainers.KMeans(options));

                // Addestramento del modello
                var model = pipeline.Fit(dataView);

                // Esecuzione della predizione
                var predictions = model.Transform(dataView);

                // Estrai i risultati delle predizioni
                var clusters = mlContext.Data.CreateEnumerable<ClusterPrediction>(predictions, reuseRowObject: false);

                // Salva i risultati del clustering in un file CSV
                Console.WriteLine("Clustering Predictions: ");

                int anomalyCount = 0;


                // Visualizza il numero totale di anomalie rilevate
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Total Anomalies Detected = {anomalyCount}.");
                Console.ResetColor();

                //Veri e Falsi Positivi e Negativi per ottenere sensibilità,accuratezza,specificità del modello
                var truePositives = 0;
                var falsePositives = 0;
                var trueNegatives = 0;
                var falseNegatives = 0;

                //Accedo a InputModel per ottenere l'informazione necessaria a capire se la riga è anomala (col. ValoriAnomali)
                var inputModels = mlContext.Data.CreateEnumerable<Inputs.DataPoints>(dataView, reuseRowObject: false);

                //combino i risultati del cluster con inputModel e itero per calcolare Veri e Falsi Positivi e Negativi 
                foreach (var (cluster, inputs) in clusters.Zip(inputModels, (c, i) => (c, i)))
                {
                    if (cluster.ClusterId == 2 && inputs.ValoriAnomali == 1)
                    {
                        truePositives++;
                    }
                    else if (cluster.ClusterId == 2 && inputs.ValoriAnomali == 0)
                    {
                        falsePositives++;
                    }
                    else if (cluster.ClusterId != 2 && inputs.ValoriAnomali == 0)
                    {
                        trueNegatives++;
                    }
                    else if (cluster.ClusterId != 2 && inputs.ValoriAnomali == 1)
                    {
                        falseNegatives++;
                    }
                }

                double sensitivity = (double)truePositives / (truePositives + falseNegatives);
                double specificity = (double)trueNegatives / (trueNegatives + falsePositives);
                double accuracy = (double)(truePositives + trueNegatives) / (truePositives + trueNegatives + falsePositives + falseNegatives);
                double gMean = Math.Sqrt(sensitivity * specificity);

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Sensitivity: {sensitivity}");
                Console.WriteLine($"Specificity: {specificity}");
                Console.WriteLine($"Accuracy: {accuracy}");
                Console.WriteLine($"G-Mean:{gMean}");
                Console.ResetColor();

                // File dove salvare i risultati:
                var resultsFilePath = ($"C:\\Users\\aless\\OneDrive\\Desktop\\UNI\\TIROCINIO\\LavoroFinale\\ClusteringResults\\Results.csv");
                WriteResultsToCSV(resultsFilePath, numsamples, sensitivity, specificity, accuracy, gMean);

                Console.ReadLine();
            }
        }
    }
}
