using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMSkin.Core;
using System.Windows.Input;
using TransferUI.Model;
using TransferUI.View;

namespace TransferUI.ViewModel
{
    class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {

        }

        /// <summary>
        /// ページ遷移コマンド
        /// </summary>    
        public ICommand NavigationCommand => new DelegateCommand(obj =>
        {
            Menu menu = (Menu)Enum.Parse(typeof(Menu), obj.ToString());
            switch (menu)
            {
                case Menu.Null:
                    break;
                case Menu.Learning:
                    Broadcast.PushBroadcast("Navigation", new PageLearning());
                    break;
                case Menu.Inference:
                    // Broadcast.PushBroadcast("Navigation", new PageBroadcast());
                    break;
            }
        });
    }
}
