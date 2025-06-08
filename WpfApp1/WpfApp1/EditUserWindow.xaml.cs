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
using WpfApp1.Validators;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для EditUserWindow.xaml
    /// </summary>
    public partial class EditUserWindow : Window
    {
        public User UserData { get; set; }
        
        public EditUserWindow(User user)
        {
            InitializeComponent();
            UserData = user;
            
            // Initialize input fields with user details if necessary
            if (user != null)
            {
                UserIdTextBox.Text = user.UserID.ToString();
                FullNameTextBox.Text = user.FullName;
                LoginTextBox.Text = user.Login;
                PhoneTextBox.Text = user.Phone;
                EmailTextBox.Text = user.Email;
                BalanceTextBox.Text = user.Balance.ToString();
                
                // Populate RoleComboBox
                foreach (ComboBoxItem item in RoleComboBox.Items)
                {
                    if (item.Content.ToString() == user.Role)
                    {
                        RoleComboBox.SelectedItem = item;
                        break;
                    }
                }
                
                // Populate IsBlockedCheckBox
                IsBlockedCheckBox.IsChecked = user.IsBlocked;
            }
            else
            {
                // If user is null, create a new User object
                UserData = new User();
                // Populate default values
                RoleComboBox.SelectedIndex = 0; // Assuming "User" is the default role
                BalanceTextBox.Text = "0";
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate input fields
                if (string.IsNullOrWhiteSpace(FullNameTextBox.Text) || string.IsNullOrWhiteSpace(LoginTextBox.Text))
                {
                    MessageBox.Show("Пожалуйста, заполните все обязательные поля: Имя и Логин", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                string phone = PhoneTextBox.Text.Trim();
                string phoneErrorMsg;
                if (!InputValidator.ValidatePhoneNumber(phone, out phoneErrorMsg))
                {
                    MessageBox.Show(phoneErrorMsg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                // Get selected role
                string selectedRole = "User";
                if (RoleComboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    selectedRole = selectedItem.Content.ToString();
                }
                
                // Validate balance format
                decimal balance = 0;
                if (!decimal.TryParse(BalanceTextBox.Text, out balance))
                {
                    MessageBox.Show("Некорректный формат суммы. Пожалуйста, введите числовое значение.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                // Populate UserData properties
                UserData.FullName = FullNameTextBox.Text;
                UserData.Login = LoginTextBox.Text;
                UserData.Phone = phone;
                UserData.Email = EmailTextBox.Text;
                UserData.Balance = balance;
                UserData.Role = selectedRole;
                UserData.IsBlocked = IsBlockedCheckBox.IsChecked ?? false;
                
                // Save changes
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
