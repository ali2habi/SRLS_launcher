using Firebase.Storage;
using FireSharp.Response;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        public async void Start_WorkSpace()
        {
            Profile.Visibility = Visibility.Hidden;
            Friends.Visibility = Visibility.Hidden;
            //etc..

            var _username = await LauncherSys.GetFirebaseClient().GetAsync($"Information/{LauncherSys.GetUserCredential().User.Uid}/Name");
            var _user_dateofbirth = await LauncherSys.GetFirebaseClient().GetAsync($"Information/{LauncherSys.GetUserCredential().User.Uid}/Date_Of_Birth");
            var _user_email = await LauncherSys.GetFirebaseClient().GetAsync($"Information/{LauncherSys.GetUserCredential().User.Uid}/Login");
            //var _user_date_regist = await LauncherSys.GetFirebaseClient().GetAsync($"Information/{LauncherSys.GetUserCredential().User.Uid}/Name");
            var _avatar = await LauncherSys.GetFirebaseClient().GetAsync($"Information/{LauncherSys.GetUserCredential().User.Uid}/Avatar");

            _userlogin.Text = _user_email.ResultAs<string>();
            _user_name.Content = _username.ResultAs<string>();
            _user_date_of_birth.Content = _user_dateofbirth.ResultAs<string>();
            _user_date_of_birth.Content = _user_dateofbirth.ResultAs<string>();
            //_user_date_of_registration.Content = _user_date_regist.ResultAs<string>();
            InitTimer();

            try
            {
                string path = _avatar.ResultAs<string>();
                await DownloadAvatar(path, _useravatar);
            }
            catch
            {
                await DownloadAvatar("anonimus.png", _useravatar);
            }          
        }
        public async Task DownloadAvatar(string key, Image avatar)
        {
            if (String.IsNullOrEmpty(key))
            {
                key = "anonimus.png";
            }
            FirebaseStorageReference imageRef = LauncherSys.GetStorage().Child(key);

            var downloadUrl = await imageRef.GetDownloadUrlAsync();
            using (var httpClient = new HttpClient())
            {
                var stream = await httpClient.GetStreamAsync(downloadUrl);
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
                avatar.Source = bitmapImage;
            }
        }
        public async void timer_Tick(object sender, EventArgs e)
        {
            SetResponse online = await LauncherSys.GetFirebaseClient().SetAsync($"Information/{LauncherSys.GetUserCredential().User.Uid}/Online", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            isOnline(LauncherSys.GetUserCredential().User.Uid, _userOnline);
        }
        public void InitTimer()
        {
            timer_Tick(null, null);
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(20);
            timer.Tick += timer_Tick;
            timer.Start();
        }
        public async void isOnline(string uid, Border border)
        {
            try
            {
                var online = await LauncherSys.GetFirebaseClient().GetAsync($"Information/{uid}/Online");
                if (online.ResultAs<long>() + 100 >= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00ff0a"));
                }
                else
                {
                    border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff0000"));
                }
            }
            catch
            {
                border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff0000"));
            }
        }
        public async void GetAllUsers()
        {
            friends_list.Children.Clear();
            var users = await LauncherSys.GetFirebaseClient().GetAsync("Information");
            string json = users.Body;

            JObject jsonObject = JObject.Parse(json);

            foreach (var user in jsonObject)
            {
                string userId = user.Key;
                JObject userData = (JObject)user.Value;
                string name = (string)userData["Name"];
                string avatar = null;

                if (userData.ContainsKey("Avatar"))
                {
                    avatar = (string)userData["Avatar"];
                }
                InitializeUserInList(userId, avatar, name);
            }
        }
        private async void InitializeUserInList(string uid, string avatarUrl, string username)
        {
            if (uid == LauncherSys.GetUserCredential().User.Uid)
                return;

            Grid mainGrid = new Grid();
            mainGrid.Background = new SolidColorBrush(Color.FromRgb(215, 241, 247));
            mainGrid.Margin = new Thickness(0, 0, 0, 6);

            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;

            Image image = new Image();
            image.Width = 100;
            image.Height = 100;
            image.Margin = new Thickness(6);
            image.Stretch = Stretch.Fill;
            DownloadAvatar(avatarUrl, image);

            Label label = new Label();
            label.VerticalContentAlignment = VerticalAlignment.Center;
            label.FontFamily = new FontFamily("Comic Sans MS");
            label.FontSize = 30;
            label.Margin = new Thickness(10, 0, 0, 0);
            label.Content = username;

            Border border = new Border();
            border.BorderThickness = new Thickness(1);
            border.CornerRadius = new CornerRadius(300);
            border.HorizontalAlignment = HorizontalAlignment.Left;
            border.VerticalAlignment = VerticalAlignment.Center;
            border.Width = 22;
            border.Height = 22;
            border.Background = Brushes.Red;
            isOnline(uid, border);
            stackPanel.Children.Add(image);
            stackPanel.Children.Add(label);
            stackPanel.Children.Add(border);

            mainGrid.Children.Add(stackPanel);
            friends_list.Children.Add(mainGrid);

            this.IsEnabled = true;
        }
        private void OnAvatarUpload(object sender, MouseButtonEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Images (*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                UploadAvatarFile(openFileDialog.FileName);
            }
        }
        public async void UploadAvatarFile(string path)
        {
            try
            {
                var stream = System.IO.File.Open(path, FileMode.Open);
                var name = $"{new Random().Next(0, 100000)}{System.IO.Path.GetExtension(path)}";
                var task = new FirebaseStorage(
                    "srls-launcher.appspot.com",

                     new FirebaseStorageOptions
                     {
                         AuthTokenAsyncFactory = () => Task.FromResult(LauncherSys.GetUserCredential().User.Credential.IdToken),
                         ThrowOnCancel = true,
                     })
                    .Child("user_avatars")
                    .Child(name)
                    .PutAsync(stream);

                var downloadUrl = await task;
                FirebaseResponse avatar = await LauncherSys.GetFirebaseClient().SetAsync("Information/" + LauncherSys.GetUserCredential().User.Uid + "/Avatar", $"user_avatars/{name}");
                await DownloadAvatar($"user_avatars/{name}", _useravatar);
            }
            catch
            {

            }
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
            this.IsEnabled = false;
            Profile.Visibility = Visibility.Hidden;
            Home.Visibility = Visibility.Hidden;
            Friends.Visibility = Visibility.Visible;
            //etc..
            GetAllUsers();
        }
    }
}