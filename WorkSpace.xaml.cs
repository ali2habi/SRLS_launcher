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
using System.Runtime.InteropServices.ComTypes;
using System.Xml.Linq;
using System.Windows.Forms;
using Orientation = System.Windows.Controls.Orientation;
using Label = System.Windows.Controls.Label;
using Border = System.Windows.Controls.Border;
using System.Security.RightsManagement;
using System.Security.Cryptography;
using System.Net.Mail;
using System.Windows.Media.Animation;
using System.Windows.Controls.Primitives;

namespace SRLS_launcher
{
    public partial class WorkSpace : Window
    {
        InitConfigsSys LauncherSys = new InitConfigsSys();
        DispatcherTimer timer;
        private DispatcherTimer UpdateTimer;
        private string Receiver;
        public WorkSpace()
        {
            InitializeComponent();
            Start_WorkSpace();
        }
        public async void Start_WorkSpace()
        {
            Profile.Visibility = Visibility.Hidden;
            People.Visibility = Visibility.Hidden;
            Messanger.Visibility = Visibility.Hidden;
            Type_and_SendMessage.Visibility = Visibility.Hidden;
            List_of_messages.Visibility = Visibility.Hidden;
            //etc..

            var _username = await LauncherSys.GetFirebaseClient().GetAsync($"Information/{LauncherSys.GetUserCredential().User.Uid}/Name");
            var _user_dateofbirth = await LauncherSys.GetFirebaseClient().GetAsync($"Information/{LauncherSys.GetUserCredential().User.Uid}/Date_Of_Birth");
            var _user_email = await LauncherSys.GetFirebaseClient().GetAsync($"Information/{LauncherSys.GetUserCredential().User.Uid}/Login");
            var _user_date_regist = await LauncherSys.GetFirebaseClient().GetAsync($"Information/{LauncherSys.GetUserCredential().User.Uid}/Date_Of_Registration");
            var _avatar = await LauncherSys.GetFirebaseClient().GetAsync($"Information/{LauncherSys.GetUserCredential().User.Uid}/Avatar");

            _userlogin.Text = _user_email.ResultAs<string>();
            _user_name.Content = _username.ResultAs<string>();
            _user_date_of_birth.Content = _user_dateofbirth.ResultAs<string>();
            _user_date_of_registration.Content = _user_date_regist.ResultAs<string>();
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
            mainGrid.Background = new SolidColorBrush(Color.FromRgb(173, 216, 230));
            mainGrid.Margin = new Thickness(0, 0, 0, 6);
            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;

            Image image = new Image();
            image.MaxWidth = 50;
            image.MaxHeight = 50;
            image.Margin = new Thickness(6);
            image.Stretch = Stretch.Fill;
            DownloadAvatar(avatarUrl, image);

            Label label = new Label();
            label.VerticalContentAlignment = VerticalAlignment.Center;
            label.FontFamily = new FontFamily("Comic Sans MS");
            label.FontSize = 22;
            label.Margin = new Thickness(10, 0, 0, 0);
            label.Content = username;
            
            Border border = new Border();
            border.BorderThickness = new Thickness(1);
            border.CornerRadius = new CornerRadius(300);
            border.HorizontalAlignment = HorizontalAlignment;
            border.VerticalAlignment = VerticalAlignment.Center;
            border.Width = 10;
            border.Height = 10;
            border.Background = Brushes.Red;
            isOnline(uid, border);
            stackPanel.Children.Add(image);
            stackPanel.Children.Add(label);
            stackPanel.Children.Add(border);

            mainGrid.MouseLeftButtonUp += async (sender, e) =>
            {
                Empty_Receiver.Visibility = Visibility.Hidden;
                List_of_messages.Visibility = Visibility.Visible;
                Type_and_SendMessage.Visibility = Visibility.Visible;
                Receiver = uid;
                DisplayMessages();
            };

            mainGrid.MouseEnter += (sender, e) =>
            {
                mainGrid.Background = new SolidColorBrush(Color.FromRgb(176, 224, 230));
            };

            mainGrid.MouseLeave += (sender, e) =>
            {
                mainGrid.Background = new SolidColorBrush(Color.FromRgb(173, 216, 230));
            };

            mainGrid.Children.Add(stackPanel);
            friends_list.Children.Add(mainGrid);

            this.IsEnabled = true;
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
            this.IsEnabled = false;
            try
            {
                var stream = System.IO.File.Open(path, FileMode.Open);
                string name = $"{new Random().Next(0, 100000)}{System.IO.Path.GetExtension(path)}";
                var userIDFolder = GetUserIDFolder();
                var task = new FirebaseStorage(
                    "srls-launcher.appspot.com",

                     new FirebaseStorageOptions
                     {
                         AuthTokenAsyncFactory = () => Task.FromResult(LauncherSys.GetUserCredential().User.Credential.IdToken),
                         ThrowOnCancel = true,
                     })
                    .Child(userIDFolder)
                    .Child(name)
                    .PutAsync(stream);

                var downloadUrl = await task;
                //DeletePreviousAvatar();
                FirebaseResponse avatar = await LauncherSys.GetFirebaseClient().SetAsync("Information/" + LauncherSys.GetUserCredential().User.Uid + "/Avatar", $"{userIDFolder}/{name}");
                await DownloadAvatar($"{userIDFolder}/{name}", _useravatar);
            }
            catch
            {
            }
            this.IsEnabled = true;
        }
        //public async void DeletePreviousAvatar()
        //{
        //    try
        //    {
        //        var userIDFolder = GetUserIDFolder();
        //        var task = new FirebaseStorage(
        //            "srls-launcher.appspot.com",
        //            new FirebaseStorageOptions
        //            {
        //                AuthTokenAsyncFactory = () => Task.FromResult(LauncherSys.GetUserCredential().User.Credential.IdToken),
        //                ThrowOnCancel = true,
        //            })
        //            .Child(userIDFolder)
        //            .DeleteAsync();

        //        FirebaseStorageReference imageRef = LauncherSys.GetStorage().Child(userIDFolder);
        //        await imageRef.DeleteAsync();
        //    }
        //    catch
        //    {
        //    }
        //}
        public string GetUserIDFolder()
        {
            return $"user_avatars/{LauncherSys.GetUserCredential().User.Uid}";
        }
        private void OnProfileClicked(object sender, RoutedEventArgs e)
        {
            Profile.Visibility = Visibility.Visible;
            Home.Visibility = Visibility.Hidden;
            People.Visibility = Visibility.Hidden;
            Messanger.Visibility = Visibility.Hidden;
        }
        private void OnHomeClicked(object sender, RoutedEventArgs e)
        {
            Profile.Visibility = Visibility.Hidden;
            Home.Visibility = Visibility.Visible;
            People.Visibility = Visibility.Hidden;
            Messanger.Visibility = Visibility.Hidden;
        }
        private void OnFriendsClicked(object sender, RoutedEventArgs e)
        {
            Profile.Visibility = Visibility.Hidden;
            Home.Visibility = Visibility.Hidden;
            People.Visibility = Visibility.Visible;
            Messanger.Visibility = Visibility.Hidden;
        }
        private void OnMessagesClicked(object sender, RoutedEventArgs e)
        {
            Profile.Visibility = Visibility.Hidden;
            Home.Visibility = Visibility.Hidden;
            People.Visibility = Visibility.Hidden;
            Messanger.Visibility = Visibility.Visible;
            //etc..
            GetAllUsers();
        }
        public string GetMessagePath()
        {
            return "Messages/";
        }
        private async void Send_Message(object sender, RoutedEventArgs e)
        {
            if (TextMessage.Text != string.Empty && Receiver != string.Empty)
            {   
                SendMessage(TextMessage.Text, LauncherSys.GetUserCredential().User.Uid, Receiver);
                TextMessage.Text = string.Empty;
                DisplayMessages();
            }
            else
            {
                System.Windows.MessageBox.Show("Текст сообщения пустой!");
            }
        }
        private async void SendMessage(string text, string sender, string receiver)
        {          
            string name = $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}" + $"{new Random().Next(0, 100000)}";
            var message = new
            {
                Text = text,
                Sender = sender,
                Receiver = receiver,
                Timestamp = DateTime.Now
            };
            await LauncherSys.GetFirebaseClient().SetAsync($"{GetMessagePath()}/{name}", message);
        }
        private void UpdatingEvents()
        {
            UpdateTimer = new DispatcherTimer();
            //UpdateTimer.Tick += new EventHandler(DisplayMessages);
            UpdateTimer.Interval = new TimeSpan(0, 0, 5);
            UpdateTimer.Start();
        }
        private async void DisplayMessages()
        {
            try
            {
                messagesList.Items.Clear();
                var user_messages = await LauncherSys.GetFirebaseClient().GetAsync($"{GetMessagePath()}");
                string json = user_messages.Body;

                JObject jsonObject = JObject.Parse(json);

                foreach (var user in jsonObject)
                {
                    string userId = user.Key;
                    JObject userData = (JObject)user.Value;
                    string sender_ = (string)userData["Sender"];
                    string receiver = (string)userData["Receiver"];
                    string message = (string)userData["Text"];

                    if ( (receiver == Receiver && sender_ == LauncherSys.GetUserCredential().User.Uid) || (receiver == LauncherSys.GetUserCredential().User.Uid && sender_ == Receiver) )
                    {
                        var textBlock = new TextBlock
                        {
                            Text = $"{message}",
                            TextWrapping = TextWrapping.Wrap,
                            Background = sender_ == LauncherSys.GetUserCredential().User.Uid ? Brushes.LightGreen : Brushes.LightBlue,
                            Padding = new Thickness(5)
                        };

                        var messageItem = new ListBoxItem();
                        messageItem.Content = textBlock;
                        messageItem.HorizontalAlignment = sender_ == LauncherSys.GetUserCredential().User.Uid ? System.Windows.HorizontalAlignment.Right : System.Windows.HorizontalAlignment.Left;
                        messageItem.MaxWidth = 300;
                        messageItem.MinWidth = 0;
                        #region Анимация
                        messageItem.Opacity = 0;
                        DoubleAnimation fadeInAnimation = new DoubleAnimation
                        {
                            From = 0,
                            To = 1,
                            Duration = TimeSpan.FromSeconds(0.2)
                        };
                        Storyboard.SetTarget(fadeInAnimation, messageItem);
                        Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath(System.Windows.Controls.Button.OpacityProperty));
                        Storyboard storyboard = new Storyboard();
                        storyboard.Children.Add(fadeInAnimation);
                        storyboard.Begin();
                        #endregion
                        messagesList.Items.Add(messageItem);
                    }
                    //else
                    //{
                    //    List_of_messages.Visibility = Visibility.Hidden;
                    //    Empty_Receiver.Text = "У вас нет сообщений с этим пользователем.";
                    //    Empty_Receiver.Visibility = Visibility.Visible;
                    //    //System.Windows.MessageBox.Show("ошибочка какая-то.");
                    //}
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Нет сообщений.\n" + ex);
            }
        }
    }
}