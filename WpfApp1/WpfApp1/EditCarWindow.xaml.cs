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
using System.IO;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для EditCarWindow.xaml
    /// </summary>
    public partial class EditCarWindow : Window
    {
        public Car CarData { get; set; }
        private string selectedImageName;
        
        public EditCarWindow(Car car)
        {
            InitializeComponent();
            CarData = car;
            
            // Get full car details if we only have basic information
            if (string.IsNullOrEmpty(car.LicensePlate) || string.IsNullOrEmpty(car.Type))
            {
                var fullCar = Data.DatabaseHelper.GetCarById(car.CarID);
                if (fullCar != null)
                {
                    CarData = fullCar;
                }
            }
            
            // Initialize input fields with car details
            LoadCarDataToForm();
            
            // Load available images from Photo folder
            LoadAvailableImages();
            
            // Try to select the current brand image
            SelectBrandImage();
        }
        
        private void LoadAvailableImages()
        {
            try
            {
                // Clear existing items except the first one
                ImageComboBox.Items.Clear();
                
                // Get all jpg files from the Photo folder
                string photoDir = "Photo";
                if (Directory.Exists(photoDir))
                {
                    string[] files = Directory.GetFiles(photoDir, "*.jpg");
                    
                    foreach (string file in files)
                    {
                        // Extract file name without extension
                        string fileName = System.IO.Path.GetFileNameWithoutExtension(file);
                        
                        // Skip system images like default_car.jpg or tyumen.jpg
                        if (fileName.ToLower() == "default_car" || fileName.ToLower() == "tyumen")
                            continue;
                        
                        ImageComboBox.Items.Add(new ComboBoxItem { Content = fileName });
                    }
                }
                
                // Add default brands if no images found
                if (ImageComboBox.Items.Count == 0)
                {
                    ImageComboBox.Items.Add(new ComboBoxItem { Content = "BMW" });
                    ImageComboBox.Items.Add(new ComboBoxItem { Content = "Audi" });
                    ImageComboBox.Items.Add(new ComboBoxItem { Content = "Toyota" });
                    ImageComboBox.Items.Add(new ComboBoxItem { Content = "KIA" });
                    ImageComboBox.Items.Add(new ComboBoxItem { Content = "Hyundai" });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void SelectBrandImage()
        {
            // Try to find and select brand in combo box
            string brand = CarData.Brand.ToLower();
            
            foreach (ComboBoxItem item in ImageComboBox.Items)
            {
                if (item.Content.ToString().ToLower() == brand)
                {
                    ImageComboBox.SelectedItem = item;
                    return;
                }
            }
            
            // If not found and items exist, select first item
            if (ImageComboBox.SelectedItem == null && ImageComboBox.Items.Count > 0)
            {
                ImageComboBox.SelectedIndex = 0;
            }
        }
        
        private void LoadCarDataToForm()
        {
            try
            {
                // Load car data into form fields
                BrandTextBox.Text = CarData.Brand;
                ModelTextBox.Text = CarData.Model;
                YearTextBox.Text = CarData.Year.ToString();
                
                // Select appropriate Type in ComboBox
                foreach (ComboBoxItem item in TypeComboBox.Items)
                {
                    if (item.Content.ToString() == CarData.Type)
                    {
                        TypeComboBox.SelectedItem = item;
                        break;
                    }
                }
                
                // If not found, select first item
                if (TypeComboBox.SelectedItem == null && TypeComboBox.Items.Count > 0)
                {
                    TypeComboBox.SelectedIndex = 0;
                }
                
                EngineVolumeTextBox.Text = CarData.EngineVolume.ToString();
                
                // Select appropriate FuelType in ComboBox
                foreach (ComboBoxItem item in FuelTypeComboBox.Items)
                {
                    if (item.Content.ToString() == CarData.FuelType)
                    {
                        FuelTypeComboBox.SelectedItem = item;
                        break;
                    }
                }
                
                // If not found, select first item
                if (FuelTypeComboBox.SelectedItem == null && FuelTypeComboBox.Items.Count > 0)
                {
                    FuelTypeComboBox.SelectedIndex = 0;
                }
                
                LicensePlateTextBox.Text = CarData.LicensePlate;
                ColorTextBox.Text = CarData.Color;
                LatitudeTextBox.Text = CarData.Latitude.ToString();
                LongitudeTextBox.Text = CarData.Longitude.ToString();
                PricePerHourTextBox.Text = CarData.PricePerHour.ToString();
                IsAvailableCheckBox.IsChecked = CarData.IsAvailable;
                
                // Load car image if available
                LoadCarImage(CarData.Brand);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void LoadCarImage(string brand)
        {
            try
            {
                string imagePath = $"Photo/{brand.ToLower()}.jpg";
                if (File.Exists(imagePath))
                {
                    CarImage.Source = new BitmapImage(new Uri(imagePath, UriKind.Relative));
                    selectedImageName = brand.ToLower();
                }
                else
                {
                    CarImage.Source = new BitmapImage(new Uri("Photo/default_car.jpg", UriKind.Relative));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке изображения: {ex.Message}");
                CarImage.Source = new BitmapImage(new Uri("Photo/default_car.jpg", UriKind.Relative));
            }
        }

        private void ImageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ImageComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string imageName = selectedItem.Content.ToString();
                LoadCarImage(imageName);
                
                // Update brand text to match selected image
                BrandTextBox.Text = imageName;
            }
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

                // Update car object with form data
                CarData.Brand = BrandTextBox.Text.Trim();
                CarData.Model = ModelTextBox.Text.Trim();
                CarData.Year = int.Parse(YearTextBox.Text);
                CarData.Type = ((ComboBoxItem)TypeComboBox.SelectedItem)?.Content.ToString() ?? "Седан";
                CarData.EngineVolume = decimal.Parse(EngineVolumeTextBox.Text);
                CarData.FuelType = ((ComboBoxItem)FuelTypeComboBox.SelectedItem)?.Content.ToString() ?? "Бензин";
                CarData.LicensePlate = LicensePlateTextBox.Text.Trim();
                CarData.Color = ColorTextBox.Text.Trim();
                CarData.Latitude = decimal.Parse(LatitudeTextBox.Text);
                CarData.Longitude = decimal.Parse(LongitudeTextBox.Text);
                CarData.PricePerHour = decimal.Parse(PricePerHourTextBox.Text);
                CarData.IsAvailable = IsAvailableCheckBox.IsChecked ?? true;

                // Update the car in the database
                if (UpdateCarInDatabase(CarData))
                {
                    DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Не удалось обновить информацию об автомобиле в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private bool UpdateCarInDatabase(Car car)
        {
            try
            {
                // Update both CarDetails and Car information
                bool updatedDetails = Data.DatabaseHelper.UpdateCarDetails(car.CarDetailID, car.Type, car.Brand, car.Model, car.Year, car.EngineVolume, car.FuelType);
                bool updatedCar = Data.DatabaseHelper.UpdateCar(car.CarID, car.LicensePlate, car.Color, car.Latitude, car.Longitude, car.IsAvailable, car.PricePerHour);
                
                return updatedDetails && updatedCar;
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
