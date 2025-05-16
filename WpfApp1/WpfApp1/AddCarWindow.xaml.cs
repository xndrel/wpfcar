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

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для AddCarWindow.xaml
    /// </summary>
    public partial class AddCarWindow : Window
    {
        public Car CarData { get; set; }
        
        public AddCarWindow()
        {
            InitializeComponent();
            // Set default values
            YearTextBox.Text = DateTime.Now.Year.ToString();
            EngineVolumeTextBox.Text = "2.0";
            PricePerHourTextBox.Text = "500";
            
            // Генерируем случайные координаты для разных частей карты
            Random random = new Random();
            
            // Определяем больше зон по всей карте для размещения автомобилей
            (int minX, int maxX, int minY, int maxY)[] zones = new[]
            {
                // Левая часть карты
                (50, 150, 50, 150),        // Верхний левый угол
                (50, 150, 200, 300),       // Средняя левая часть
                (50, 150, 350, 450),       // Нижний левый угол
                
                // Центральная часть карты
                (250, 350, 50, 150),       // Верхний центр
                (250, 350, 200, 300),      // Самый центр
                (250, 350, 350, 450),      // Нижний центр
                
                // Правая часть карты
                (450, 550, 50, 150),       // Верхний правый угол
                (450, 550, 200, 300),      // Средняя правая часть
                (450, 550, 350, 450),      // Нижний правый угол
                
                // Добавим зоны по краям
                (150, 250, 50, 150),       // Верхняя часть между левым углом и центром
                (150, 250, 350, 450),      // Нижняя часть между левым углом и центром
                (350, 450, 50, 150),       // Верхняя часть между правым углом и центром
                (350, 450, 350, 450),      // Нижняя часть между правым углом и центром
                
                // Промежуточные зоны
                (200, 400, 150, 250),      // Выше центра
                (200, 400, 300, 400)       // Ниже центра
            };
            
            // Выбираем случайную зону
            var zone = zones[random.Next(zones.Length)];
            
            // Генерируем случайные координаты в пределах выбранной зоны
            int latitude = random.Next(zone.minX, zone.maxX);
            int longitude = random.Next(zone.minY, zone.maxY);
            
            // Добавляем небольшое случайное смещение, чтобы машины не накладывались
            latitude += random.Next(-10, 10);
            longitude += random.Next(-10, 10);
            
            LatitudeTextBox.Text = latitude.ToString();
            LongitudeTextBox.Text = longitude.ToString();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(BrandTextBox.Text) || 
                    string.IsNullOrWhiteSpace(ModelTextBox.Text) ||
                    string.IsNullOrWhiteSpace(LicensePlateTextBox.Text) ||
                    string.IsNullOrWhiteSpace(ColorTextBox.Text))
                {
                    MessageBox.Show("Пожалуйста, заполните все обязательные поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Create a new car object
                CarData = new Car
                {
                    Brand = BrandTextBox.Text.Trim(),
                    Model = ModelTextBox.Text.Trim(),
                    Year = int.Parse(YearTextBox.Text),
                    Type = ((ComboBoxItem)TypeComboBox.SelectedItem)?.Content.ToString() ?? "Седан",
                    EngineVolume = decimal.Parse(EngineVolumeTextBox.Text),
                    FuelType = ((ComboBoxItem)FuelTypeComboBox.SelectedItem)?.Content.ToString() ?? "Бензин",
                    LicensePlate = LicensePlateTextBox.Text.Trim(),
                    Color = ColorTextBox.Text.Trim(),
                    Latitude = decimal.Parse(LatitudeTextBox.Text),
                    Longitude = decimal.Parse(LongitudeTextBox.Text),
                    PricePerHour = decimal.Parse(PricePerHourTextBox.Text),
                    IsAvailable = IsAvailableCheckBox.IsChecked ?? true
                };

                // Add the car to the database
                if (AddCarToDatabase(CarData))
                {
                    DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Не удалось добавить автомобиль в базу данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Проверьте правильность ввода числовых данных.", "Ошибка формата", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool AddCarToDatabase(Car car)
        {
            // Add a method to DatabaseHelper to insert a new car
            // For now, we'll simulate success
            try
            {
                // First add CarDetails
                int carDetailId = Data.DatabaseHelper.AddCarDetails(car.Type, car.Brand, car.Model, car.Year, car.EngineVolume, car.FuelType);
                
                if (carDetailId > 0)
                {
                    // Then add the Car with the new CarDetailID
                    car.CarDetailID = carDetailId;
                    bool success = Data.DatabaseHelper.AddCar(car);
                    return success;
                }
                
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
