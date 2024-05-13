using System;
using System.Windows;

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
            WorkSpace mainWindow = new WorkSpace();
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
            Create_window.Visibility = Visibility.Hidden;
            Create_DataInsert_window.Visibility = Visibility.Hidden;
        }
        private void Login_Page_Close(object sender, RoutedEventArgs e)
        {
            Login_window.Visibility = Visibility.Hidden;
            login_box.Text = "";
            password_box.Password = "";
        }
        private async void Login2_Button_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            if (login_box.Text.Length > 0 && password_box.Password.Length > 0)
            {
                bool isLoggedIn = await LauncherSys.Login(login_box.Text, password_box.Password);
                if (isLoggedIn)
                {
                    Enter_to_System();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль!");
                    this.IsEnabled = true;
                }
            }
            else
            {
                MessageBox.Show("Введите логин и пароль!");
                this.IsEnabled = true;
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
        private void Create2_Button_Click(object sender, RoutedEventArgs e)
        {
            Create_window.Visibility= Visibility.Hidden;
            Create_DataInsert_window.Visibility = Visibility.Visible;
        }
        private async void TryToCreate_Button_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            if (login_create_box.Text.Length > 0 && password_create_box.Password.Length > 0)
            {
                bool isRegisterIn = await LauncherSys.Create(login_create_box.Text, password_create_box.Password);
                if (isRegisterIn)
                {
                    DateTime time_now = DateTime.Now;
                    var userInfo = new
                    {
                        Login = login_create_box.Text,
                        Name = username.Text,
                        Password = password_create_box.Password,
                        Date_Of_Birth = userdateofbirth.Text,
                        Date_Of_Registration = time_now.ToString()
                    };
                    await LauncherSys.GetFirebaseClient().SetAsync($"Information/{LauncherSys.GetUserCredential().User.Uid}", userInfo);
                    Enter_to_System();
                    MessageBox.Show("Успешная регистрация!");
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль!");
                    Create_DataInsert_window.Visibility = Visibility.Hidden;
                    Create_window.Visibility = Visibility.Visible;
                    this.IsEnabled = true;
                }
            }
            else
            {
                MessageBox.Show("Неверное заполены поля!");
                Create_DataInsert_window.Visibility = Visibility.Hidden;
                Create_window.Visibility = Visibility.Visible;
                this.IsEnabled = true;
            }
        }
    }
}
    