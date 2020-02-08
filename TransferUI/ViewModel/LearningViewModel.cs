using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DMSkin.Core;
using Microsoft.WindowsAPICodePack.Dialogs;
using TransferUI.Model;
using LearningProcessor;
using LiveCharts;
using LiveCharts.Wpf;

namespace TransferUI.ViewModel
{
    class LearningViewModel : ViewModelBase
    {
        public LearningModel Model { get; private set; }
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        private ObservableCollection<LearningDatasetModel> _Dataset;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LearningViewModel()
        {
            Model = new LearningModel();
            _Dataset = new ObservableCollection<LearningDatasetModel>();

            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "accuracy",
                    Values = new ChartValues<float>()
                }
            };
        }

        /// <summary>
        /// 学習ステータス
        /// </summary>
        private string _state = "";
        public string State
        {
            get { return _state; }
            set
            {
                if (_state != value)
                {
                    _state = value;
                    OnPropertyChanged("State");
                }
            }
        }

        /// <summary>
        /// データセットのパス
        /// </summary>
        public string DatasetPath
        {
            get { return Model.DatasetPath; }
            set
            {
                if (Model.DatasetPath != value)
                {
                    Model.DatasetPath = value;
                    OnPropertyChanged("DatasetPath");
                }
            }
        }

        /// <summary>
        /// データセット情報
        /// </summary>
        public ObservableCollection<LearningDatasetModel> Dataset
        {
            get { return _Dataset; }
            set
            {
                if (_Dataset != value)
                {
                    _Dataset = value;
                    OnPropertyChanged(nameof(Dataset));
                }
            }
        }

        /// <summary>
        /// モデルリスト
        /// </summary>
        public IList<string> ModelNames
        {
            get
            {
                IList<string> res = new List<string>() { "Resnet100", "Incepstion", "MobileNet", "Resnet50" };
                return res;
            }
        }

        /// <summary>
        /// 使用モデル
        /// </summary>
        public string UseModel
        {
            get { return Model.UseModel; }
            set
            {
                if (Model.UseModel != value)
                {
                    Model.UseModel = value;
                    OnPropertyChanged("UseModel");
                }
            }
        }

        /// <summary>
        /// 検証用データセット比率
        /// </summary>
        public string ValidationRate
        {
            get { return Model.ValidationRate.ToString(); }
            set
            {
                try
                {
                    int ivalue = int.Parse(value);

                    if (ivalue < 0) ivalue = 0;
                    if (ivalue > 50) ivalue = 50;

                    if (Model.ValidationRate != ivalue)
                    {
                        Model.ValidationRate = ivalue;
                        OnPropertyChanged("ValidationRate");
                    }
                }
                catch
                {
                    ValidationRate = Model.ValidationRate.ToString();
                }
            }
        }

        /// <summary>
        /// バッチサイズ
        /// </summary>
        public string BatchSize
        {
            get { return Model.BatchSize.ToString(); }
            set
            {
                try
                {
                    int ivalue = int.Parse(value);

                    if (ivalue < 1) ivalue = 1;
                    if (ivalue > 1000) ivalue = 1000;

                    if(Model.BatchSize != ivalue)
                    {
                        Model.BatchSize = ivalue;
                        OnPropertyChanged("BatchSize");
                    }
                }
                catch
                {
                    BatchSize = Model.BatchSize.ToString();
                }
            }
        }

        /// <summary>
        /// エポック数
        /// </summary>
        public string Epoch
        {
            get { return Model.Epoch.ToString(); }
            set
            {
                try
                {
                    int ivalue = int.Parse(value);

                    if (ivalue < 1) ivalue = 1;
                    if (ivalue > 200) ivalue = 200;

                    if (Model.Epoch != ivalue)
                    {
                        Model.Epoch = ivalue;
                        OnPropertyChanged("Epoch");
                    }
                }
                catch
                {
                    Epoch = Model.Epoch.ToString();
                }
            }
        }

        /// <summary>
        /// データセット選択
        /// </summary>    
        public ICommand SelectCommand => new DelegateCommand(obj =>
        {
            var dlg = new CommonOpenFileDialog("フォルダ選択");
            dlg.IsFolderPicker = true;
            if(dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Dataset.Clear();
                DatasetPath = dlg.FileName;
                // サブディレクトリを取得し、それぞれのディレクトリごとに
                // 何枚の画像が格納されているか確認する
                string[] labels = Directory.GetDirectories(dlg.FileName);
                foreach (var label in labels)
                {
                    LearningDatasetModel tmp = new LearningDatasetModel();
                    tmp.Label = Path.GetFileName(label);
                    string[] files = Directory.GetFiles(label, "*.jpg", SearchOption.TopDirectoryOnly);
                    tmp.ImageCount = files.Count().ToString();
                    Dataset.Add(tmp);
                }
            }
        });

        /// <summary>
        /// 学習開始
        /// </summary>
        public ICommand LearningStartCommand => new DelegateCommand(obj =>
        {
            State = "学習中...";
            Processor processor = new Processor();
            processor.LearningCallBackEvent += LearningCallBack;
            processor.Initialize(DatasetPath, ValidationRate);
            var task = processor.Run(BatchSize, Epoch, UseModel);
            Task.Factory.StartNew(() =>
            {
                task.Wait();
                State = "学習完了!!";
            });
        });

        /// <summary>
        /// 学習中コールバック
        /// </summary>
        /// <param name="accuracy"></param>
        private void LearningCallBack(float accuracy)
        {
            SeriesCollection[0].Values.Add(accuracy * 100);
        }
    }
}
