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
    /// Логика взаимодействия для MainScreen.xaml
    /// </summary>
    public partial class MainScreen : Window
    {
        // Список всех доступных автомобилей
        private List<Car> availableCars;
        
        public Visibility AdminVisibility { get; set; }
        
        public MainScreen()
        {
            InitializeComponent();
            
            // Устанавливаем контекст данных для привязки видимости кнопки администратора
            AdminVisibility = MainWindow.CurrentUser.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
            this.DataContext = this;
            AdminPanelButton.Visibility = AdminVisibility;
            
            BodyTypeComboBox.SelectionChanged += BodyTypeComboBox_SelectionChanged;
            
            // Загружаем все автомобили
            availableCars = WpfApp1.Data.DatabaseHelper.GetAvailableCars();
            
            // Отображаем автомобили на карте
            LoadCarMarkers(availableCars);
        }

        private void BodyTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedType = (BodyTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            
            // Очищаем текущие маркеры
            MarkersCanvas.Children.Clear();
            
            // Если выбран "Все", отображаем все автомобили
            if (selectedType == "Все")
            {
                LoadCarMarkers(availableCars);
            }
            else
            {
                // Фильтруем автомобили по выбранному типу кузова
                var filteredCars = availableCars.Where(car => car.Type == selectedType).ToList();
                
                // Загружаем отфильтрованные маркеры
                LoadCarMarkers(filteredCars);
            }
        }

        private void LoadCarMarkers(List<Car> cars)
        {
            // Очищаем существующие маркеры
            MarkersCanvas.Children.Clear();
            
            // Словарь для отслеживания позиций, чтобы избежать наложения
            Dictionary<string, bool> occupiedPositions = new Dictionary<string, bool>();
            Random random = new Random();
            
            foreach (var car in cars)
            {
                Button btn = new Button();
                btn.Width = 80;
                btn.Height = 40;
                btn.Tag = car;
                btn.Background = new SolidColorBrush(Colors.LightBlue);
                btn.BorderBrush = new SolidColorBrush(Colors.DarkBlue);
                btn.Click += CarMarker_Click;
                
                // Базовые координаты из БД
                try
                {
                    string imagePath = $"Photo/{car.Brand.ToLower()}.jpg";
                    if (System.IO.File.Exists(imagePath))
                    {
                        Image carImage = new Image();
                        carImage.Source = new BitmapImage(new Uri(imagePath, UriKind.Relative));
                        carImage.Width = 60;
                        carImage.Height = 30;
                        btn.Content = carImage;
                    }
                    else
                    {
                        // Если изображение не найдено, используем текст
                        btn.Content = $"{car.Brand} {car.Model}";
                    }
                }
                catch
                {
                    // Если возникает ошибка при загрузке изображения, используем текст
                    btn.Content = $"{car.Brand} {car.Model}";
                }
                
                // Базовые координаты из БД
                double left = (double)car.Latitude;
                double top = (double)car.Longitude;
                
                // Создаем ключ для позиции
                string posKey = $"{(int)left},{(int)top}";
                
                // Если позиция уже занята, добавляем небольшое смещение
                if (occupiedPositions.ContainsKey(posKey))
                {
                    // Добавляем случайное смещение в пределах 50 пикселей
                    left += random.Next(-50, 50);
                    top += random.Next(-50, 50);
                    
                    // Убеждаемся, что автомобиль остается в пределах видимой области карты
                    left = Math.Max(50, Math.Min(700, left));
                    top = Math.Max(50, Math.Min(500, top));
                }
                
                // Отмечаем позицию как занятую
                occupiedPositions[posKey] = true;
                
                // Устанавливаем позицию кнопки на канвасе
                Canvas.SetLeft(btn, left);
                Canvas.SetTop(btn, top);
                
                MarkersCanvas.Children.Add(btn);
            }
        }

        private void CarMarker_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Car car)
            {
                CarDetailsWindow detailsWindow = new CarDetailsWindow(car);
                detailsWindow.Show();
            }
            else
            {
                MessageBox.Show("Ошибка при выборе автомобиля.");
            }
        }

        private void SupportChatButton_Click(object sender, RoutedEventArgs e)
        {
            SupportChatWindow chatWindow = new SupportChatWindow();
            chatWindow.Show();
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            // Используем идентификатор текущего пользователя
            int currentUserId = MainWindow.CurrentUser.UserID;
            ProfileWindow profileWindow = new ProfileWindow(currentUserId);
            profileWindow.Show();
        }

        private void AdminPanelButton_Click(object sender, RoutedEventArgs e)
        {
            AdminPanel adminPanel = new AdminPanel();
            adminPanel.Show();
        }
        
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Сбрасываем информацию о текущем пользователе
            MainWindow.CurrentUser.UserID = 0;
            MainWindow.CurrentUser.FullName = null;
            MainWindow.CurrentUser.Phone = null;
            MainWindow.CurrentUser.Email = null;
            MainWindow.CurrentUser.IsAdmin = false;
            
            // Открываем окно авторизации
            MainWindow loginWindow = new MainWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
