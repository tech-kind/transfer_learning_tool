using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML;
using static Microsoft.ML.DataOperationsCatalog;
using Microsoft.ML.Vision;

namespace LearningProcessor
{
    public class Processor
    {
        private string projectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory));
        private string workspaceRelativePath = "";
        private MLContext mlContext = new MLContext();
        private IDataView trainSet;
        private IDataView validationSet;

        public delegate void LearningCallBackDelegate(float accuracy);
        public LearningCallBackDelegate LearningCallBackEvent = null;        

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="imageFolderPath"></param>
        /// <param name="validationRate"></param>
        public void Initialize(string imageFolderPath, string validationRate)
        {
            DateTime current = DateTime.Now;
            workspaceRelativePath = Path.Combine(projectDirectory, "result", current.ToString("yyyyMMdd-HHmmss"));

            IEnumerable<ImageData> images = LoadImagesFromDirectory(imageFolderPath, true);

            IDataView imageData = mlContext.Data.LoadFromEnumerable(images);

            IDataView shuffledData = mlContext.Data.ShuffleRows(imageData);

            var preprocessingPipeline = mlContext.Transforms.Conversion.MapValueToKey(
                    inputColumnName: "Label",
                    outputColumnName: "LabelAsKey")
                .Append(mlContext.Transforms.LoadRawImageBytes(
                    outputColumnName: "Image",
                    imageFolder: imageFolderPath,
                    inputColumnName: "ImagePath"));

            IDataView preProcessedData = preprocessingPipeline
                                .Fit(shuffledData)
                                .Transform(shuffledData);

            double fraction = double.Parse(validationRate) / 100;
            TrainTestData trainSplit = mlContext.Data.TrainTestSplit(data: preProcessedData, testFraction: fraction);
            TrainTestData validationTestSplit = mlContext.Data.TrainTestSplit(trainSplit.TestSet);

            trainSet = trainSplit.TrainSet;
            validationSet = validationTestSplit.TrainSet;
        }

        /// <summary>
        /// 学習開始
        /// </summary>
        /// <param name="batchSize"></param>
        /// <param name="epoch"></param>
        /// <param name="architecture"></param>
        /// <returns></returns>
        public Task Run(string batchSize, string epoch, string architecture)
        {
             var task = Task.Factory.StartNew(() =>
            {
                ImageClassificationTrainer.Architecture arch = GetArchitecture(architecture);
                var classifierOptions = new ImageClassificationTrainer.Options()
                {
                    FeatureColumnName = "Image",
                    LabelColumnName = "LabelAsKey",
                    ValidationSet = validationSet,
                    Arch = arch,
                    MetricsCallback = (metrics) => LearningCallBack(metrics),
                    TestOnTrainSet = false,
                    WorkspacePath = workspaceRelativePath
                };

                var trainingPipeline = mlContext.MulticlassClassification.Trainers.ImageClassification(classifierOptions)
                    .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

                ITransformer trainedModel = trainingPipeline.Fit(trainSet);
            });

            return task;
        }

        /// <summary>
        /// 学習用画像ロード
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="useFolderNameAsLabel"></param>
        /// <returns></returns>
        private IEnumerable<ImageData> LoadImagesFromDirectory(string folder, bool useFolderNameAsLabel = true)
        {
            var files = Directory.GetFiles(folder, "*",
                searchOption: SearchOption.AllDirectories);

            foreach (var file in files)
            {
                if ((Path.GetExtension(file) != ".jpg") && (Path.GetExtension(file) != ".png"))
                    continue;

                var label = Path.GetFileName(file);

                if (useFolderNameAsLabel)
                    label = Directory.GetParent(file).Name;
                else
                {
                    for (int index = 0; index < label.Length; index++)
                    {
                        if (!char.IsLetter(label[index]))
                        {
                            label = label.Substring(0, index);
                            break;
                        }
                    }
                }

                yield return new ImageData()
                {
                    ImagePath = file,
                    Label = label
                };
            }
        }

        /// <summary>
        /// アーキテクチャ取得
        /// </summary>
        /// <param name="architecture"></param>
        /// <returns></returns>
        private ImageClassificationTrainer.Architecture GetArchitecture(string architecture)
        {
            switch(architecture)
            {
                case "Resnet100":
                    return ImageClassificationTrainer.Architecture.ResnetV2101;
                case "Inception":
                    return ImageClassificationTrainer.Architecture.InceptionV3;
                case "MobileNet":
                    return ImageClassificationTrainer.Architecture.MobilenetV2;
                case "Resnet50":
                    return ImageClassificationTrainer.Architecture.ResnetV250;
                default:
                    return ImageClassificationTrainer.Architecture.ResnetV2101;
            }
        }

        /// <summary>
        /// 学習中コールバック
        /// </summary>
        /// <param name="metrics"></param>
        private void LearningCallBack(ImageClassificationTrainer.ImageClassificationMetrics metrics)
        {
            if(LearningCallBackEvent != null && metrics.Train != null)
            {
                LearningCallBackEvent(metrics.Train.Accuracy);
            }
        }
    }

    public class ImageData
    {
        public string ImagePath { get; set; }

        public string Label { get; set; }
    }
}
