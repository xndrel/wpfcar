using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using WpfApp1.Validators;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        // Объединим переменные в один набор для упрощения логики
        private string pendingSMSCode;
        private string pendingFullName;
        private string pendingPhone;
        private string pendingEmail;
        private string pendingPassword;
        
        // Статический класс для хранения информации о текущем пользователе
        public static class CurrentUser
        {
            public static int UserID { get; set; }
            public static string FullName { get; set; }
            public static string Phone { get; set; }
            public static string Email { get; set; }
            public static bool IsAdmin { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string fullName = FullNameTextBox.Text.Trim();
            string phone = PhoneTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password;
            string errorMsg;
            if (!InputValidator.ValidateRegistration(fullName, phone, email, password, out errorMsg))
            {
                MessageBox.Show(errorMsg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (WpfApp1.Data.DatabaseHelper.CheckUserExists(phone))
            {
                MessageBox.Show("Пользователь с таким номером уже существует.");
                return;
            }
            Random rnd = new Random();
            string smsCode = rnd.Next(1000, 10000).ToString();
            MessageBox.Show("Ваш SMS код: " + smsCode);

            pendingSMSCode = smsCode;
            pendingFullName = fullName;
            pendingPhone = phone;
            pendingEmail = email;
            pendingPassword = password;

            SMSPanel.Visibility = Visibility.Visible;
        }

        private void ConfirmSMSButton_Click(object sender, RoutedEventArgs e)
        {
            string enteredCode = SMSCodeTextBox.Text.Trim();
            if (enteredCode == pendingSMSCode)
            {
                string passwordHash = WpfApp1.Data.DatabaseHelper.ComputeSha256Hash(pendingPassword);
                bool success = WpfApp1.Data.DatabaseHelper.RegisterUser(pendingPhone, passwordHash, pendingFullName, pendingPhone, pendingEmail);
                if (success)
                {
                    MessageBox.Show("Регистрация успешна!");
                    
                    // Получаем информацию о новом пользователе
                    var user = WpfApp1.Data.DatabaseHelper.GetUserByPhone(pendingPhone);
                    if (user != null)
                    {
                        // Сохраняем данные текущего пользователя
                        CurrentUser.UserID = user.UserID;
                        CurrentUser.FullName = user.FullName;
                        CurrentUser.Phone = user.Phone;
                        CurrentUser.Email = user.Email;
                        CurrentUser.IsAdmin = user.Role == "Admin";
                    }
                    
                    MainScreen ms = new MainScreen();
                    ms.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Ошибка при регистрации.");
                }
            }
            else
            {
                MessageBox.Show("Неверный SMS код: введено " + enteredCode + ", ожидалось " + pendingSMSCode);
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string phone = LoginPhoneTextBox.Text.Trim();
            string password = LoginPasswordBox.Password;
            if (WpfApp1.Data.DatabaseHelper.ValidateUser(phone, password))
            {
                // Получаем информацию о пользователе
                var user = WpfApp1.Data.DatabaseHelper.GetUserByPhone(phone);
                if (user != null)
                {
                    // Сохраняем данные текущего пользователя
                    CurrentUser.UserID = user.UserID;
                    CurrentUser.FullName = user.FullName;
                    CurrentUser.Phone = user.Phone;
                    CurrentUser.Email = user.Email;
                    CurrentUser.IsAdmin = user.Role == "Admin";
                    
                    MessageBox.Show($"Успешный вход! Добро пожаловать, {user.FullName}");
                }
                else
                {
                    MessageBox.Show("Успешный вход!");
                }
                
                MainScreen ms = new MainScreen();
                ms.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный номер или пароль!");
            }
        }

        // Этот метод используется для вычисления хеша пароля
        private string ComputeSha256Hash(string rawData)
        {
            using (var sha256Hash = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawData));
                var builder = new System.Text.StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Add handler for admin login button
        private void AdminLoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Устанавливаем статус администратора
            CurrentUser.IsAdmin = true;
            CurrentUser.UserID = 1; // Предполагаем, что админ имеет ID = 1
            
            // Открываем админ-панель без проверки учетных данных
            AdminPanel adminPanel = new AdminPanel();
            adminPanel.Show();
            this.Close();
        }

        // Add handler for support login button  
        private void SupportLoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно поддержки без проверки учетных данных
            // Для поддержки используем ID чата 1 (можно заменить на реальный ID из базы данных)
            SupportChatWindow supportWindow = new SupportChatWindow("1");
            supportWindow.Show();
            this.Close();
        }
    }
} 