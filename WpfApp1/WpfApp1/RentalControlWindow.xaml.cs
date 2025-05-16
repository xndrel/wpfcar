using System;
using System.Windows;

namespace WpfApp1
{
    public partial class RentalControlWindow : Window
    {
        private bool _isOpen = false;
        private DateTime _startTime;
        private int _carId;

        public RentalControlWindow(int carId = 1)
        {
            InitializeComponent();
            _startTime = DateTime.Now;
            _carId = carId; // Сохраняем ID автомобиля
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            _isOpen = true;
            StatusTextBlock.Text = "Статус автомобиля: Открыт";
            MessageBox.Show("Команда на открытие автомобиля отправлена.");
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _isOpen = false;
            StatusTextBlock.Text = "Статус автомобиля: Закрыт";
            MessageBox.Show("Команда на закрытие автомобиля отправлена.");
        }

        private void EndRentalButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите завершить аренду?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                DateTime endTime = DateTime.Now;
                TimeSpan duration = endTime - _startTime;
                
                // Сохраняем историю аренды в базу данных
                int userId = MainWindow.CurrentUser.UserID;
                bool success = Data.DatabaseHelper.SaveRentalHistory(userId, _carId, _startTime, endTime);
                
                if (success)
                {
                    MessageBox.Show($"Аренда завершена. Продолжительность аренды: {duration.TotalHours:F1} ч. Спасибо!");
                }
                else
                {
                    MessageBox.Show("Аренда не завершена, но не удалось обновить историю аренды.");
                }
                
                this.Close();
            }
        }

        private void BackToMainButton_Click(object sender, RoutedEventArgs e)
        {
            MainScreen mainScreen = new MainScreen();
            mainScreen.Show();
            this.Close();
        }
    }
} 