using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WpfApp1
{
    public partial class CarDetailsWindow : Window
    {
        // Цена аренды за час
        private decimal ratePerHour = 450m;
        // Сохраняемый автомобиль
        private Car currentCar;
        
        public CarDetailsWindow(Car car)
        {
            InitializeComponent();
            // Обновляю элементы управления данными автомобиля
            currentCar = car;
            BrandTextBlock.Text = "Марка: " + car.Brand;
            ModelTextBlock.Text = "Модель: " + car.Model;
            YearTextBlock.Text = "Год: " + car.Year;
            // Использую цену аренды из объекта car
            ratePerHour = car.PricePerHour;
            PriceTextBlock.Text = "Цена аренды: " + ratePerHour.ToString("F2") + " руб/час";
            
            // Загружаем изображение автомобиля
            try
            {
                string imagePath = $"Photo/{car.Brand.ToLower()}.jpg";
                if (System.IO.File.Exists(imagePath))
                {
                    CarImage.Source = new BitmapImage(new Uri(imagePath, UriKind.Relative));
                }
                else
                {
                    // Если изображение не найдено, используем заглушку
                    CarImage.Source = new BitmapImage(new Uri("Photo/default_car.jpg", UriKind.Relative));
                }
            }
            catch (Exception ex)
            {
                // Если возникает ошибка при загрузке изображения, выводим сообщение в отладчик
                System.Diagnostics.Debug.WriteLine($"Ошибка при загрузке изображения автомобиля: {ex.Message}");
            }
        }

        public CarDetailsWindow()
        {
            InitializeComponent();
        }

        private void ConfirmRentalButton_Click(object sender, RoutedEventArgs e)
        {
            if (RentalDurationComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                if (int.TryParse(selectedItem.Content.ToString(), out int hours))
                {
                    if (hours > 0 && hours <= 24)
                    {
                        decimal totalCost = ratePerHour * hours;
                        MessageBox.Show("Общая стоимость аренды: " + totalCost.ToString("F2") + " руб.");
                        PaymentWindow paymentWindow = new PaymentWindow(totalCost, currentCar.CarID);
                        paymentWindow.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Срок аренды должен быть от 1 до 24 часов.");
                    }
                }
                else
                {
                    MessageBox.Show("Некорректное значение срока аренды.");
                }
            }
            else
            {
                MessageBox.Show("Выберите срок аренды.");
            }
        }
    }
} 