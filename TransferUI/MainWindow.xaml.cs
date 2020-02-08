using System.Windows.Controls;
using DMSkin.Core;

namespace TransferUI
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            Broadcast.RegisterBroadcast<Page>("Navigation", (obj) =>
            {
                Frame.Navigate(obj);
            });
        }
    }
}
