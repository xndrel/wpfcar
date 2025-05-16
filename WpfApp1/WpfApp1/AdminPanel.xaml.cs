using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;
using WpfApp1.Data;

namespace WpfApp1
{
    public partial class AdminPanel : Window
    {
        private List<Car> allCars;
        
        public AdminPanel()
        {
            InitializeComponent();
            LoadUsers();
            LoadCars();
            LoadChats();

            // Позволяет редактировать данные пользователя двойным кликом по элементу списка
            UsersListView.MouseDoubleClick += UsersListView_MouseDoubleClick;
            
            // Загрузка данных для карты
            allCars = Data.DatabaseHelper.GetCars();
            
            // Добавляем обработчик события выбора вкладки
            ((TabControl)((Grid)this.Content).Children[0]).SelectionChanged += AdminPanel_TabSelectionChanged;
        }
        
        private void AdminPanel_TabSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl tabControl = (TabControl)sender;
            
            // Проверяем, выбрана ли вкладка "Карта автомобилей" (индекс 3)
            if (tabControl.SelectedIndex == 3)
            {
                // Загружаем автомобили на карту
                LoadCarMarkersOnMap(allCars);
            }
        }

        // Метод для отображения автомобилей на карте
        private void LoadCarMarkersOnMap(List<Car> cars)
        {
            // Очищаем существующие маркеры
            AdminMapCanvas.Children.Clear();
            
            foreach (var car in cars)
            {
                Button btn = new Button();
                btn.Width = 80;
                btn.Height = 40;
                btn.Tag = car;
                btn.Background = new SolidColorBrush(car.IsAvailable ? Colors.LightGreen : Colors.LightCoral);
                btn.BorderBrush = new SolidColorBrush(Colors.DarkBlue);
                btn.Click += AdminCarMarker_Click;
                
                // Добавляем обработчики для перетаскивания
                btn.MouseLeftButtonDown += AdminCarMarker_MouseLeftButtonDown;
                btn.MouseMove += AdminCarMarker_MouseMove;
                btn.MouseLeftButtonUp += AdminCarMarker_MouseLeftButtonUp;
                
                // Пытаемся загрузить изображение автомобиля
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
                    // В случае ошибки используем текст
                    btn.Content = $"{car.Brand} {car.Model}";
                }
                
                // Используем сохраненные координаты из БД
                double left = (double)car.Latitude;
                double top = (double)car.Longitude;
                
                // Убеждаемся, что автомобиль находится в пределах видимой области карты
                left = Math.Max(50, Math.Min(700, left));
                top = Math.Max(50, Math.Min(500, top));
                
                // Устанавливаем позицию кнопки на канвасе
                Canvas.SetLeft(btn, left);
                Canvas.SetTop(btn, top);
                
                AdminMapCanvas.Children.Add(btn);
            }
        }
        
        // Переменные для перетаскивания
        private bool isDragging = false;
        private Point startPoint;
        private UIElement draggedElement = null;
        
        // Обработчик нажатия на маркер автомобиля
        private void AdminCarMarker_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button btn)
            {
                isDragging = true;
                startPoint = e.GetPosition(AdminMapCanvas);
                draggedElement = btn;
                btn.CaptureMouse();
                e.Handled = true;
            }
        }
        
        // Обработчик движения мыши для перетаскивания
        private void AdminCarMarker_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && draggedElement != null)
            {
                Point currentPosition = e.GetPosition(AdminMapCanvas);
                
                // Вычисляем новую позицию
                double newLeft = Canvas.GetLeft(draggedElement) + (currentPosition.X - startPoint.X);
                double newTop = Canvas.GetTop(draggedElement) + (currentPosition.Y - startPoint.Y);
                
                // Устанавливаем ограничения, чтобы не выходить за пределы карты
                newLeft = Math.Max(0, Math.Min(AdminMapCanvas.ActualWidth - draggedElement.RenderSize.Width, newLeft));
                newTop = Math.Max(0, Math.Min(AdminMapCanvas.ActualHeight - draggedElement.RenderSize.Height, newTop));
                
                // Обновляем позицию
                Canvas.SetLeft(draggedElement, newLeft);
                Canvas.SetTop(draggedElement, newTop);
                
                // Обновляем стартовую точку
                startPoint = currentPosition;
                e.Handled = true;
            }
        }
        
        // Обработчик отпускания кнопки мыши
        private void AdminCarMarker_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging && draggedElement != null)
            {
                isDragging = false;
                draggedElement.ReleaseMouseCapture();
                
                if (draggedElement is Button btn && btn.Tag is Car car)
                {
                    // Сохраняем новые координаты в объект автомобиля
                    car.Latitude = (decimal)Canvas.GetLeft(draggedElement);
                    car.Longitude = (decimal)Canvas.GetTop(draggedElement);
                    
                    // Сохраняем новые координаты в базу данных
                    bool success = Data.DatabaseHelper.UpdateCar(
                        car.CarID, car.LicensePlate, car.Color, 
                        car.Latitude, car.Longitude, car.IsAvailable, car.PricePerHour);
                    
                    if (success)
                    {
                        // Обновляем данные автомобиля в списке allCars
                        int index = allCars.FindIndex(c => c.CarID == car.CarID);
                        if (index >= 0)
                        {
                            allCars[index].Latitude = car.Latitude;
                            allCars[index].Longitude = car.Longitude;
                        }
                        
                        // Можно добавить обратную связь для пользователя
                        // MessageBox.Show("Местоположение автомобиля обновлено");
                    }
                }
                
                draggedElement = null;
                e.Handled = true;
            }
        }
        
        private void AdminCarMarker_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Car car)
            {
                // Выбираем автомобиль в списке на вкладке Автопарк
                foreach (Car listCar in CarsListView.Items)
                {
                    if (listCar.CarID == car.CarID)
                    {
                        CarsListView.SelectedItem = listCar;
                        // Переключаемся на вкладку Автопарк
                        ((TabControl)((Grid)this.Content).Children[0]).SelectedIndex = 1;
                        break;
                    }
                }
            }
        }
        
        private void AdminMapFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (allCars == null) return;
            
            string selectedType = (AdminMapFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            
            // Если выбран "Все", отображаем все автомобили
            if (selectedType == "Все")
            {
                LoadCarMarkersOnMap(allCars);
            }
            else
            {
                // Фильтруем автомобили по выбранному типу кузова
                var filteredCars = allCars.Where(car => car.Type == selectedType).ToList();
                
                // Загружаем отфильтрованные маркеры
                LoadCarMarkersOnMap(filteredCars);
            }
        }
        
        private void RefreshMap_Click(object sender, RoutedEventArgs e)
        {
            // Обновляем список всех автомобилей
            allCars = Data.DatabaseHelper.GetCars();
            
            // Обновляем маркеры на карте
            string selectedType = (AdminMapFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            
            if (selectedType == "Все")
            {
                LoadCarMarkersOnMap(allCars);
            }
            else
            {
                var filteredCars = allCars.Where(car => car.Type == selectedType).ToList();
                LoadCarMarkersOnMap(filteredCars);
            }
        }

        private void LoadUsers()
        {
            try
            {
                var users = Data.DatabaseHelper.GetUsers();
                UsersListView.Items.Clear();
                foreach (var user in users)
                {
                    UsersListView.Items.Add(user);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки пользователей: " + ex.Message);
            }
        }

        private void LoadCars()
        {
            try
            {
                var cars = Data.DatabaseHelper.GetCars();
                CarsListView.Items.Clear();
                foreach (var car in cars)
                {
                    // Получаем полные данные о машине, если не все поля заполнены
                    if (string.IsNullOrEmpty(car.Color) || string.IsNullOrEmpty(car.LicensePlate))
                    {
                        var fullCar = Data.DatabaseHelper.GetCarById(car.CarID);
                        if (fullCar != null)
                        {
                            car.Color = fullCar.Color;
                            car.LicensePlate = fullCar.LicensePlate;
                            car.Type = fullCar.Type;
                            car.IsAvailable = fullCar.IsAvailable;
                        }
                    }
                    CarsListView.Items.Add(car);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки автомобилей: " + ex.Message);
            }
        }

        private void LoadChats()
        {
            try
            {
                var chats = Data.DatabaseHelper.GetSupportChats();
                ChatsListView.Items.Clear();
                foreach (var chat in chats)
                {
                    ChatsListView.Items.Add(chat);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки чатов: " + ex.Message);
            }
        }

        // Пользователи
        private void BlockUser_Click(object sender, RoutedEventArgs e)
        {
            if (UsersListView.SelectedItem is User selectedUser)
            {
                bool success = Data.DatabaseHelper.UpdateUserBlockStatus(selectedUser.UserID, true);
                if (success)
                {
                    MessageBox.Show("Пользователь заблокирован.");
                    LoadUsers();
                }
                else
                {
                    MessageBox.Show("Ошибка блокировки пользователя.");
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для блокировки.");
            }
        }

        private void UnblockUser_Click(object sender, RoutedEventArgs e)
        {
            if (UsersListView.SelectedItem is User selectedUser)
            {
                bool success = Data.DatabaseHelper.UpdateUserBlockStatus(selectedUser.UserID, false);
                if (success)
                {
                    MessageBox.Show("Пользователь разблокирован.");
                    LoadUsers();
                }
                else
                {
                    MessageBox.Show("Ошибка при разблокировке пользователя.");
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для разблокировки.");
            }
        }

        private void ShowRentalHistory_Click(object sender, RoutedEventArgs e)
        {
            if (UsersListView.SelectedItem is User selectedUser)
            {
                var history = Data.DatabaseHelper.GetRentalHistory(selectedUser.UserID);
                string historyMessage = $"История аренды пользователя {selectedUser.FullName}:\n";
                foreach (var record in history)
                {
                    historyMessage += record + "\n";
                }
                MessageBox.Show(historyMessage);
            }
            else
            {
                MessageBox.Show("Выберите пользователя для просмотра истории аренды.");
            }
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            if (UsersListView.SelectedItem is User selectedUser)
            {
                EditUserWindow editWindow = new EditUserWindow(selectedUser);
                
                if (editWindow.ShowDialog() == true)
                {
                    User editedUser = editWindow.UserData;
                    
                    try
                    {
                        string query = @"UPDATE Users SET FullName = @FullName, Login = @Login, 
                                        Phone = @Phone, Email = @Email, Balance = @Balance, 
                                        Role = @Role, IsBlocked = @IsBlocked 
                                        WHERE UserID = @UserID";
                        
                        using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(Data.DatabaseHelper.ConnectionString))
                        {
                            conn.Open();
                            using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@UserID", editedUser.UserID);
                                cmd.Parameters.AddWithValue("@FullName", editedUser.FullName);
                                cmd.Parameters.AddWithValue("@Login", editedUser.Login);
                                cmd.Parameters.AddWithValue("@Phone", editedUser.Phone);
                                cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(editedUser.Email) ? DBNull.Value : (object)editedUser.Email);
                                cmd.Parameters.AddWithValue("@Balance", editedUser.Balance);
                                cmd.Parameters.AddWithValue("@Role", editedUser.Role);
                                cmd.Parameters.AddWithValue("@IsBlocked", editedUser.IsBlocked);
                                
                                int rowsAffected = cmd.ExecuteNonQuery();
                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Пользователь успешно обновлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                                    LoadUsers();
                                }
                                else
                                {
                                    MessageBox.Show("Не удалось обновить пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при обновлении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для редактирования.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void UsersListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditUser_Click(sender, e);
        }

        // Автопарк
        private void AddCar_Click(object sender, RoutedEventArgs e)
        {
            AddCarWindow addCarWindow = new AddCarWindow();
            if (addCarWindow.ShowDialog() == true)
            {
                LoadCars();
            }
        }

        private void EditCar_Click(object sender, RoutedEventArgs e)
        {
            if (CarsListView.SelectedItem is Car selectedCar)
            {
                EditCarWindow editCarWindow = new EditCarWindow(selectedCar);
                if (editCarWindow.ShowDialog() == true)
                {
                    LoadCars();
                }
            }
            else
            {
                MessageBox.Show("Выберите автомобиль для редактирования.");
            }
        }

        private void DeleteCar_Click(object sender, RoutedEventArgs e)
        {
            if (CarsListView.SelectedItem is Car selectedCar)
            {
                bool success = Data.DatabaseHelper.DeleteCar(selectedCar.CarID);
                if (success)
                {
                    MessageBox.Show("Автомобиль удален.");
                    LoadCars();
                }
                else
                {
                    MessageBox.Show("Ошибка при удалении автомобиля.");
                }
            }
            else
            {
                MessageBox.Show("Выберите автомобиль для удаления.");
            }
        }

        private void TestRental_Click(object sender, RoutedEventArgs e)
        {
            if (CarsListView.SelectedItem is Car selectedCar)
            {
                bool success = Data.DatabaseHelper.CreateTestRental(selectedCar.CarID);
                if (success)
                {
                    MessageBox.Show($"Тестовая аренда автомобиля {selectedCar.Brand} {selectedCar.Model} оформлена без оплаты.");
                }
                else
                {
                    MessageBox.Show("Ошибка при создании тестовой аренды.");
                }
            }
            else
            {
                MessageBox.Show("Выберите автомобиль для тестовой аренды.");
            }
        }

        // Поддержка
        private void OpenChat_Click(object sender, RoutedEventArgs e)
        {
            if (ChatsListView.SelectedItem != null)
            {
                string chatId = ChatsListView.SelectedItem.ToString();
                SupportChatWindow chat = new SupportChatWindow(chatId);
                chat.Show();
            }
            else
            {
                MessageBox.Show("Выберите чат для открытия.");
            }
        }

        private void CloseChat_Click(object sender, RoutedEventArgs e)
        {
            if (ChatsListView.SelectedItem != null)
            {
                string chatId = ChatsListView.SelectedItem.ToString();
                bool success = Data.DatabaseHelper.CloseSupportChat(chatId);
                if (success)
                {
                    MessageBox.Show("Чат закрыт.");
                    LoadChats();
                }
                else
                {
                    MessageBox.Show("Ошибка при закрытии чата.");
                }
            }
            else
            {
                MessageBox.Show("Выберите чат для закрытия.");
            }
        }

        // Объявления
        private void PublishAnnouncement_Click(object sender, RoutedEventArgs e)
        {
            string title = AnnouncementTitleTextBox.Text.Trim();
            string content = AnnouncementContentTextBox.Text.Trim();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
            {
                MessageBox.Show("Заполните заголовок и текст объявления.");
                return;
            }

            bool success = Data.DatabaseHelper.PublishAnnouncement(title, content);
            if (success)
            {
                MessageBox.Show("Объявление опубликовано.");
                AnnouncementTitleTextBox.Clear();
                AnnouncementContentTextBox.Clear();
            }
            else
            {
                MessageBox.Show("Ошибка при публикации объявления.");
            }
        }

        // Выход из админ-панели
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Создаем новое окно главного меню и отображаем его
            MainScreen mainScreen = new MainScreen();
            mainScreen.Show();
            
            // Закрываем текущее окно админ-панели
            this.Close();
        }
    }

    // Модель пользователя для админки (ожидается, что в таблице Users есть поле IsBlocked)
    public class User
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public bool IsBlocked { get; set; }
        public override string ToString()
        {
            return $"{FullName} (ID: {UserID}) {(IsBlocked ? "[Заблокирован]" : "")}";
        }
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public decimal Balance { get; set; }
        public string Role { get; set; }
    }
}