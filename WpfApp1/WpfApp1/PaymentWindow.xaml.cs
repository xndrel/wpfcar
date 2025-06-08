using System.Windows;
using System;
using System.Data.SqlClient;

namespace WpfApp1
{
    public partial class PaymentWindow : Window
    {
        private decimal _amount;
        private int _carId;

        public PaymentWindow(decimal amount)
        {
            InitializeComponent();
            _amount = amount;
            _carId = 1; // Default car ID
            AmountTextBlock.Text = _amount.ToString("F2") + " руб.";
        }
        
        public PaymentWindow(decimal amount, int carId)
        {
            InitializeComponent();
            _amount = amount;
            _carId = carId;
            AmountTextBlock.Text = _amount.ToString("F2") + " руб.";
        }

        private void PayButton_Click(object sender, RoutedEventArgs e)
        {
            // Если выбран способ оплаты банковской картой, проверяем введенные данные
            if (CardRadioButton.IsChecked == true)
            {
                if (string.IsNullOrWhiteSpace(CardNumberTextBox.Text) ||
                    string.IsNullOrWhiteSpace(ExpiryTextBox.Text) ||
                    string.IsNullOrWhiteSpace(CVCTextBox.Text))
                {
                    MessageBox.Show("Пожалуйста, введите данные банковской карты.");
                    return;
                }
            }

            // --- Новый блок: создаём аренду и платёж ---
            try
            {
                int userId = MainWindow.CurrentUser.UserID;
                DateTime startTime = DateTime.Now;
                // Для простоты считаем, что сумма = цена за все часы, значит часы = сумма / цена за час
                decimal pricePerHour = 450; // По умолчанию
                int hours = 1;
                using (var conn = new SqlConnection(Data.DatabaseHelper.ConnectionString))
                {
                    conn.Open();
                    // Получаем цену за час для машины
                    using (var cmd = new SqlCommand("SELECT PricePerHour FROM Cars WHERE CarID = @CarID", conn))
                    {
                        cmd.Parameters.AddWithValue("@CarID", _carId);
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            pricePerHour = Convert.ToDecimal(result);
                        }
                    }
                    hours = (int)Math.Round(_amount / pricePerHour);
                    if (hours < 1) hours = 1;
                    DateTime endTime = startTime.AddHours(hours);
                    // Создаём аренду
                    int rentalId = 0;
                    using (var cmd = new SqlCommand(@"INSERT INTO Rentals (UserID, CarID, StartTime, EndTime) VALUES (@UserID, @CarID, @StartTime, @EndTime); SELECT SCOPE_IDENTITY();", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.Parameters.AddWithValue("@CarID", _carId);
                        cmd.Parameters.AddWithValue("@StartTime", startTime);
                        cmd.Parameters.AddWithValue("@EndTime", endTime);
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            rentalId = Convert.ToInt32(result);
                        }
                    }
                    // Добавляем платёж
                    if (rentalId > 0)
                    {
                        Data.DatabaseHelper.AddPayment(rentalId, _amount);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании аренды или платёжа: {ex.Message}");
                return;
            }
            // --- Конец нового блока ---

            MessageBox.Show("Оплата прошла успешно!");
            RentalControlWindow rentalWindow = new RentalControlWindow(_carId);
            rentalWindow.Show();
            this.Close();
        }

        // Add the following event handler for confirming payment
        private void ConfirmPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            // Simulate a successful payment
            bool paymentSuccessful = true;
            if (paymentSuccessful)
            {
                MessageBox.Show("Оплата прошла успешно.");
                RentalControlWindow rentalControlWindow = new RentalControlWindow(_carId);
                rentalControlWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Ошибка оплаты.");
            }
        }
    }
} 