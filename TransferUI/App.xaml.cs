using System.Windows;
using DMSkin.Core;

namespace TransferUI
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // UI初期化
            UIExecute.Initialize();

            Broadcast.Initialize();
        }
    }
}
