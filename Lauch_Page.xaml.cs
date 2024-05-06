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
using System.Net.Http;
using System.IO;
using Path = System.IO.Path;

using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Auth.Repository;
using Firebase.Storage;

using FireSharp.Interfaces;
using FireSharp;
using FireSharp.Config;
using FireSharp.Response;
using System.Security.RightsManagement;
using System.Xml.Linq;

namespace SRLS_launcher
{
    public partial class Lauch_Page : Window
    {
        InitConfigsSys LauncherSys = new InitConfigsSys();
        public Lauch_Page()
        {
            InitializeComponent();
            AppStarted();
        }
        public void Enter_to_System()
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
        private void Login1_button_Click(object sender, RoutedEventArgs e)
        {
            Login_window.Visibility = Visibility.Visible;
            Create_window.Visibility = Visibility.Hidden;
        }
        private void Exit1_Click(object sender, RoutedEventArgs e)
        {
            System.Environment.Exit(0);
        }
        public void AppStarted()
        {
            Login_window.Visibility = Visibility.Hidden;
            Create_window.Visibility= Visibility.Hidden;
        }
        private void Login_Page_Close(object sender, RoutedEventArgs e)
        {
            Login_window.Visibility = Visibility.Hidden;
            login_box.Text = "";
            password_box.Password = "";
        }
        private async void Login2_Button_Click(object sender, RoutedEventArgs e)
        {
            if (login_box.Text != "" && password_box.Password != "")
            {
                bool isLoggedIn = await LauncherSys.Login(login_box.Text, password_box.Password);
                if (isLoggedIn)
                {
                    Enter_to_System();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль!");
                }
            }
            else
            {
                MessageBox.Show("Введите логин и пароль!");
            }
        }
        private void Create_Page_Close(object sender, RoutedEventArgs e)
        {
            Create_window.Visibility = Visibility.Hidden;
            login_create_box.Text = "";
            password_create_box.Password = "";
        }
        private void Create1_Button_Click(object sender, RoutedEventArgs e)
        {
            Login_window.Visibility = Visibility.Hidden;
            Create_window.Visibility = Visibility.Visible;
        }
        private async void Create2_Button_Click(object sender, RoutedEventArgs e)
        {
            if (login_create_box.Text != "" && password_create_box.Password != "")
            {
                bool isRegisterIn = await LauncherSys.Create(login_create_box.Text, password_create_box.Password);
                if (isRegisterIn)
                {
                    //var userInfo = new
                    //{
                    //    Name = name_.Text,
                    //    Address = address_.Text,
                    //    Age = age_.Text,
                    //    PhoneNumber = number_.Text
                    //};
                    //await LauncherSys.GetFirebaseClient().SetAsync($"Information/{LauncherSys.GetUserCredential().User.Uid}", userInfo);
                    //ShowWorkspace();
                    MessageBox.Show("Успешная регистрация!");
                    Enter_to_System();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль!");
                }
            }
            else
            {
                MessageBox.Show("Неверное заполены поля!");
            }
        }
    }
    public class InitConfigsSys
    {
        static FirebaseAuthClient client;
        static FireSharp.FirebaseClient firebaseClient;
        static UserCredential userCredential;
        static FirebaseStorage storage;
        public InitConfigsSys()
        {
            InitConfigs();
        }
        public FireSharp.FirebaseClient GetFirebaseClient()
        {
            return firebaseClient;
        }
        public UserCredential GetUserCredential()
        {
            return userCredential;
        }
        public FirebaseStorage GetStorage()
        {
            return storage;
        }
        private void InitConfigs()
        {
            FirebaseAuthConfig config = new FirebaseAuthConfig
            {
                ApiKey = "AIzaSyCgXCL5GHSGmVJgDC1SDooeDubU3yCFz6I",
                AuthDomain = "srls-launcher.firebaseapp.com",
                Providers = new FirebaseAuthProvider[]
                {
                    new EmailProvider()
                },
                UserRepository = new FileUserRepository("test")
            };
            client = new FirebaseAuthClient(config);
        }
        private void InitConfigFirebase()
        {
            string _authSecret = null;

            if (userCredential != null && userCredential.User != null && userCredential.User.Credential != null)
            {
                _authSecret = userCredential.User.Credential.IdToken;
            }

            IFirebaseConfig firebaseConfig = new FireSharp.Config.FirebaseConfig
            {
                RequestTimeout = TimeSpan.FromDays(1),
                BasePath = "https://srls-launcher-default-rtdb.firebaseio.com",
                AuthSecret = _authSecret
            };
            firebaseClient = new FirebaseClient(firebaseConfig);
        }
        private void InitConfigFirebaseStorage()
        {
            storage = new FirebaseStorage("screw-launcher.appspot.com", new FirebaseStorageOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(userCredential.User.Credential.IdToken)
            });
        }
        public async Task<bool> Login(string username, string password)
        {
            try
            {
                userCredential = await client.SignInWithEmailAndPasswordAsync(username, password);
                InitConfigFirebaseStorage();
                InitConfigFirebase();

                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> Create(string username, string password)
        {
            try
            {
                userCredential = await client.CreateUserWithEmailAndPasswordAsync(username, password);
                InitConfigFirebaseStorage();
                InitConfigFirebase();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}