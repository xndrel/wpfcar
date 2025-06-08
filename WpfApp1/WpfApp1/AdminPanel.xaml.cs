using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;
using WpfApp1.Data;
using System.Text;
using System.Text.RegularExpressions;
using LiveCharts;
using LiveCharts.Wpf;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;

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
            LoadAdminStatsChart();
            ExportAdminPdfButton.Click += ExportAdminPdfButton_Click;

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
            
            // Если координаты автомобилей не заданы, распределим их случайно по карте
            Random random = new Random();
            
            foreach (var car in cars)
            {
                Button btn = new Button();
                btn.Width = 80;
                btn.Height = 40;
                btn.Tag = car;
                btn.Background = new SolidColorBrush(System.Windows.Media.Colors.LightGreen);
                btn.BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.DarkBlue);
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
                
                // Используем сохраненные координаты из БД или случайные, если в БД (0,0)
                double left = (double)car.Latitude;
                double top = (double)car.Longitude;
                
                // Если координаты не заданы (0,0) или выходят за пределы карты, генерируем случайные
                if (left < 10 || left > 750 || top < 10 || top > 550)
                {
                    // Увеличиваем зону случайного размещения на всю карту
                    left = random.Next(80, 720); // Расширяем диапазон для большего разброса
                    top = random.Next(80, 520);
                    
                    // Обновляем координаты в объекте автомобиля и в БД
                    car.Latitude = (decimal)left;
                    car.Longitude = (decimal)top;
                    
                    // Сохраняем новые координаты в базу данных
                    Data.DatabaseHelper.UpdateCar(
                        car.CarID, car.LicensePlate, car.Color, 
                        car.Latitude, car.Longitude, car.IsAvailable, car.PricePerHour);
                }
                
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
                if (MessageBox.Show("Вы уверены, что хотите заблокировать пользователя?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    bool result = Data.DatabaseHelper.UpdateUserBlockStatus(selectedUser.UserID, true);
                    if (result)
                    {
                        MessageBox.Show("Пользователь успешно заблокирован.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        selectedUser.IsBlocked = true;
                        LoadUsers();
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при блокировке пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите пользователя для блокировки.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UnblockUser_Click(object sender, RoutedEventArgs e)
        {
            if (UsersListView.SelectedItem is User selectedUser)
            {
                if (MessageBox.Show("Вы уверены, что хотите разблокировать пользователя?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    bool result = Data.DatabaseHelper.UpdateUserBlockStatus(selectedUser.UserID, false);
                    if (result)
                    {
                        MessageBox.Show("Пользователь успешно разблокирован.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        selectedUser.IsBlocked = false;
                        LoadUsers();
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при разблокировке пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите пользователя для разблокировки.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ShowRentalHistory_Click(object sender, RoutedEventArgs e)
        {
            if (UsersListView.SelectedItem is User selectedUser)
            {
                try
                {
                    var history = Data.DatabaseHelper.GetRentalHistory(selectedUser.UserID);
                    
                    if (history.Count == 0)
                    {
                        // Если история пуста, предлагаем создать тестовую запись
                        MessageBoxResult result = MessageBox.Show(
                            $"У пользователя {selectedUser.FullName} нет истории аренды. Создать тестовую запись?",
                            "История аренды пуста",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);
                            
                        if (result == MessageBoxResult.Yes)
                        {
                            // Создаем тестовую аренду
                            DateTime endTime = DateTime.Now;
                            DateTime startTime = endTime.AddHours(-3);
                            
                            bool success = Data.DatabaseHelper.CreateRentalRecord(selectedUser.UserID, 1, startTime, endTime);
                            if (success)
                            {
                                MessageBox.Show("Тестовая запись аренды создана.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                                history = Data.DatabaseHelper.GetRentalHistory(selectedUser.UserID);
                            }
                            else
                            {
                                MessageBox.Show("Не удалось создать тестовую запись аренды.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                // Добавляем заглушку в историю, чтобы пользователь видел хоть что-то
                                history.Add($"Тестовая аренда: {startTime.ToString("dd.MM.yyyy HH:mm")} - {endTime.ToString("dd.MM.yyyy HH:mm")}, Продолжительность: 3 ч., Стоимость: 1350 руб.");
                            }
                        }
                    }
                    
                    // Собираем историю аренды в текстовую строку
                    StringBuilder historyText = new StringBuilder();
                    historyText.AppendLine($"История аренды пользователя {selectedUser.FullName}:");
                    historyText.AppendLine(new string('-', 50));
                    
                    foreach (var record in history)
                    {
                        historyText.AppendLine(record);
                    }
                    
                    // Выводим историю аренды через MessageBox
                    MessageBox.Show(historyText.ToString(), $"История аренды - {selectedUser.FullName}", 
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке истории аренды: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для просмотра истории аренды.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
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
                            
                            // Проверяем существование таблицы и необходимых полей
                            string checkQuery = @"
                                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users')
                                BEGIN
                                    CREATE TABLE Users (
                                        UserID INT PRIMARY KEY IDENTITY(1,1),
                                        Login NVARCHAR(50) NOT NULL,
                                        PasswordHash NVARCHAR(64) NOT NULL,
                                        FullName NVARCHAR(100) NOT NULL,
                                        Phone NVARCHAR(20) NOT NULL,
                                        Email NVARCHAR(100),
                                        Balance DECIMAL(10,2) DEFAULT 0.00,
                                        Role NVARCHAR(20) DEFAULT 'User',
                                        IsBlocked BIT DEFAULT 0 NOT NULL
                                    )
                                END
                                
                                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                                              WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'IsBlocked')
                                BEGIN
                                    ALTER TABLE Users ADD IsBlocked BIT DEFAULT 0 NOT NULL
                                END";
                                
                            using (System.Data.SqlClient.SqlCommand checkCmd = new System.Data.SqlClient.SqlCommand(checkQuery, conn))
                            {
                                checkCmd.ExecuteNonQuery();
                            }
                            
                            // Выполняем обновление
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
                                    LoadUsers(); // Перезагружаем список после обновления
                                }
                                else
                                {
                                    // Если строки не обновились, возможно пользователя нет - пробуем создать
                                    string insertQuery = @"INSERT INTO Users (Login, PasswordHash, FullName, Phone, Email, Balance, Role, IsBlocked)
                                                        VALUES (@Login, @PasswordHash, @FullName, @Phone, @Email, @Balance, @Role, @IsBlocked)";
                                    
                                    using (System.Data.SqlClient.SqlCommand insertCmd = new System.Data.SqlClient.SqlCommand(insertQuery, conn))
                                    {
                                        insertCmd.Parameters.AddWithValue("@FullName", editedUser.FullName);
                                        insertCmd.Parameters.AddWithValue("@Login", editedUser.Login);
                                        insertCmd.Parameters.AddWithValue("@PasswordHash", editedUser.PasswordHash ?? Data.DatabaseHelper.ComputeSha256Hash("password"));
                                        insertCmd.Parameters.AddWithValue("@Phone", editedUser.Phone);
                                        insertCmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(editedUser.Email) ? DBNull.Value : (object)editedUser.Email);
                                        insertCmd.Parameters.AddWithValue("@Balance", editedUser.Balance);
                                        insertCmd.Parameters.AddWithValue("@Role", editedUser.Role);
                                        insertCmd.Parameters.AddWithValue("@IsBlocked", editedUser.IsBlocked);
                                        
                                        try
                                        {
                                            int inserted = insertCmd.ExecuteNonQuery();
                                            if (inserted > 0)
                                            {
                                                MessageBox.Show("Новый пользователь успешно добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                                                LoadUsers();
                                            }
                                            else
                                            {
                                                MessageBox.Show("Не удалось обновить или создать пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            MessageBox.Show($"Ошибка при создании пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                        }
                                    }
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

        // ----- Новые обработчики CRUD операций для админ-панели -----

        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersListView.SelectedItem is User selectedUser)
            {
                if (MessageBox.Show("Вы уверены, что хотите удалить пользователя?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    bool result = Data.DatabaseHelper.DeleteUser(selectedUser.UserID);
                    if (result)
                    {
                        MessageBox.Show("Пользователь успешно удален.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadUsers();
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при удалении пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите пользователя для удаления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteCarButton_Click(object sender, RoutedEventArgs e)
        {
            if (CarsListView.SelectedItem is Car selectedCar)
            {
                if (MessageBox.Show("Вы уверены, что хотите удалить машину?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    bool result = Data.DatabaseHelper.DeleteCar(selectedCar.CarID);
                    if (result)
                    {
                        MessageBox.Show("Машина успешно удалена.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadCars();
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при удалении машины.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите машину для удаления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteChatButton_Click(object sender, RoutedEventArgs e)
        {
            if (ChatsListView.SelectedItem is string selectedChatString)
            {
                // Используем регулярное выражение для извлечения ID из строки вида "Имя (ID: 123)"
                Match match = Regex.Match(selectedChatString, @"ID:\s*(\d+)");
                if(match.Success)
                {
                    string idStr = match.Groups[1].Value;
                    if (MessageBox.Show("Вы уверены, что хотите удалить чат поддержки?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        bool result = Data.DatabaseHelper.DeleteSupportChat(idStr);
                        if(result)
                        {
                            MessageBox.Show("Чат поддержки успешно удален.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadChats();
                        }
                        else
                        {
                            MessageBox.Show("Ошибка при удалении чата поддержки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Не удалось определить ID чата из выбранной строки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void LoadAdminStatsChart()
        {
            // Получаем статистику аренд по автомобилям за сегодня
            var stats = Data.DatabaseHelper.GetRentalsStatsByCarForToday();
            var carNames = stats.Select(s => $"{s.CarName} ({s.TotalRevenue}₽)").ToArray();
            var counts = stats.Select(s => (double)s.Count).ToArray();
            AdminStatsChart.Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Аренды",
                    Values = new ChartValues<double>(counts)
                }
            };
            AdminStatsChart.AxisX.Clear();
            AdminStatsChart.AxisX.Add(new Axis { Title = "Автомобиль", Labels = carNames });
            AdminStatsChart.AxisY.Clear();
            AdminStatsChart.AxisY.Add(new Axis { Title = "Кол-во аренд" });
        }

        private void ExportAdminPdfButton_Click(object sender, RoutedEventArgs e)
        {
            var stats = Data.DatabaseHelper.GetRentalsStatsByCarForToday();
            var dlg = new Microsoft.Win32.SaveFileDialog { Filter = "PDF files (*.pdf)|*.pdf", FileName = "admin_report.pdf" };
            if (dlg.ShowDialog() == true)
            {
                var doc = new Document();
                var section = doc.AddSection();
                var title = section.AddParagraph("Отчёт по арендам за сегодня");
                title.Format.Font.Size = 16;
                title.Format.Font.Bold = true;
                title.Format.SpaceAfter = "1cm";
                decimal total = 0;
                foreach (var stat in stats)
                {
                    var p = section.AddParagraph($"{stat.CarName}: {stat.Count} ({stat.TotalRevenue}₽)");
                    p.Format.Font.Size = 12;
                    total += stat.TotalRevenue;
                }
                section.AddParagraph($"Итого: {total}₽").Format.Font.Bold = true;
                var renderer = new MigraDoc.Rendering.PdfDocumentRenderer(true);
                renderer.Document = doc;
                renderer.RenderDocument();
                renderer.PdfDocument.Save(dlg.FileName);
            }
        }

        // Конец класса AdminPanel
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