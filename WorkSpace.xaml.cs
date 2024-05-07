using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SRLS_launcher
{
    public partial class WorkSpace : Window
    {
        InitConfigsSys LauncherSys = new InitConfigsSys();
        DispatcherTimer timer;
        public WorkSpace()
        {
            InitializeComponent();
            Start_WorkSpace();
        }
        public void Start_WorkSpace()
        {
            Profile.Visibility = Visibility.Hidden;
            Friends.Visibility = Visibility.Hidden;
            //etc..
        }
        private void OnProfileClicked(object sender, RoutedEventArgs e)
        {
            Profile.Visibility = Visibility.Visible;
            Home.Visibility = Visibility.Hidden;
            Friends.Visibility = Visibility.Hidden;
            //etc..
        }
        private void OnHomeClicked(object sender, RoutedEventArgs e)
        {
            Profile.Visibility = Visibility.Hidden;
            Home.Visibility = Visibility.Visible;
            Friends.Visibility = Visibility.Hidden;
            //etc..
        }
        private void OnFriendsClicked(object sender, RoutedEventArgs e)
        {
            Profile.Visibility = Visibility.Hidden;
            Home.Visibility = Visibility.Hidden;
            Friends.Visibility = Visibility.Visible;
            //etc..
        }

    }
}
