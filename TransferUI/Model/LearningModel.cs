using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferUI.Model
{
    public class LearningModel
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LearningModel()
        {
            DatasetPath = "";
            UseModel = "Resnet100";
            ValidationRate = 20;
            BatchSize = 64;
            Epoch = 40;
        }

        /// <summary>
        /// データセットのパス
        /// </summary>
        public string DatasetPath { get; set; }

        /// <summary>
        /// 使用モデル
        /// </summary>
        public string UseModel { get; set; }

        /// <summary>
        /// 検証用データセット比率
        /// </summary>
        public int ValidationRate { get; set; }

        /// <summary>
        /// バッチサイズ
        /// </summary>
        public int BatchSize { get; set; }

        /// <summary>
        /// エポック数
        /// </summary>
        public int Epoch { get; set; }
        
    }
}
