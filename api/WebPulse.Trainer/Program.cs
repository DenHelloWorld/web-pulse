using Microsoft.ML;
using WebPulse.Api.Models;

var mlContext = new MLContext(seed: 1);

Console.WriteLine("🚀 Загрузка данных...");
var dataView = mlContext.Data.LoadFromTextFile<SentimentData>(
    path: "train.tsv",
    hasHeader: false,
    separatorChar: '\t');

Console.WriteLine("🧠 Обучение модели...");
var pipeline = mlContext.Transforms.Text.FeaturizeText("Features", nameof(SentimentData.Text))
    .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "Label"));

var model = pipeline.Fit(dataView);

Console.WriteLine("✅ Модель обучена! Сохраняем в ZIP...");

// СОХРАНЯЕМ В ZIP - ЭТО НЕ МОЖЕТ ВЫДАТЬ ОШИБКУ
mlContext.Model.Save(model, dataView.Schema, "sentiment.zip");

Console.WriteLine($"\n⭐ УСПЕХ! Файл sentiment.zip создан.");
