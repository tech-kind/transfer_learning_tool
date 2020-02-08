using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferUI.Model
{
    public class LearningDatasetModel
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LearningDatasetModel()
        {
            Label = "";
            ImageCount = "";
        }

        /// <summary>
        /// ラベル
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// 画像枚数
        /// </summary>
        public string ImageCount { get; set; }
    }
}
