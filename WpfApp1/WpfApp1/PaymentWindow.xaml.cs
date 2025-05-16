using System.Windows;

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

            // Имитация обработки платежа
            MessageBox.Show("Оплата прошла успешно!");

            // Открываем экран управления арендой
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