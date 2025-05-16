using System;
using System.Collections.Generic;
using System.Windows;

namespace WpfApp1
{
    public partial class ProfileWindow : Window
    {
        private int userId;

        public class RentalHistoryItem
        {
            public string RentalDate { get; set; }
            public string CarInfo { get; set; }
            public string Duration { get; set; }
            public string Cost { get; set; }
        }

        public class AnnouncementItem
        {
            public string PublishDate { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
        }

        public ProfileWindow(int userId)
        {
            InitializeComponent();
            
            // Всегда используем ID текущего пользователя из статического класса
            this.userId = MainWindow.CurrentUser.UserID;
            
            this.Title = "Профиль - " + MainWindow.CurrentUser.FullName;
            
            LoadUserInfo();
            LoadRentalHistory();
            LoadAnnouncements();
        }

        private void LoadUserInfo()
        {
            try
            {
                // Получаем информацию о пользователе из базы данных
                User user = Data.DatabaseHelper.GetUserById(userId);
                if (user != null)
                {
                    FullNameTextBlock.Text = user.FullName;
                    PhoneTextBlock.Text = user.Phone;
                    EmailTextBlock.Text = user.Email;
                    BalanceTextBlock.Text = user.Balance.ToString("F2") + " руб.";
                    StatusTextBlock.Text = user.IsBlocked ? "Заблокирован" : "Активен";
                }
                else
                {
                    // Если данные не найдены, используем данные из CurrentUser
                    FullNameTextBlock.Text = MainWindow.CurrentUser.FullName;
                    PhoneTextBlock.Text = MainWindow.CurrentUser.Phone;
                    EmailTextBlock.Text = MainWindow.CurrentUser.Email;
                    BalanceTextBlock.Text = "0.00 руб.";
                    StatusTextBlock.Text = "Активен";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных пользователя: " + ex.Message);
            }
        }

        private void LoadRentalHistory()
        {
            try
            {
                // Получаем историю аренд из базы данных
                var history = Data.DatabaseHelper.GetRentalHistory(userId);
                List<RentalHistoryItem> historyItems = new List<RentalHistoryItem>();

                // Выводим историю в консоль для отладки
                Console.WriteLine($"Получено {history.Count} записей истории аренд");
                
                // Преобразуем строки из БД в элементы истории аренд
                foreach (var record in history)
                {
                    Console.WriteLine($"Обрабатываем запись: {record}");
                    
                    try
                    {
                        // Примерный формат: RentalID: 1, CarID: 2, Start: 2023-05-15 10:00:00, End: 2023-05-15 12:00:00, Duration: 2 hours
                        string[] parts = record.Split(new string[] { ", " }, StringSplitOptions.None);
                        if (parts.Length < 5)
                        {
                            Console.WriteLine($"Недостаточно данных в записи: {record}");
                            continue;
                        }
                        
                        string rentalIdPart = parts[0];
                        string carIdPart = parts[1];
                        string startTimePart = parts[2];
                        string endTimePart = parts[3];
                        string durationPart = parts[4];
                        
                        int rentalId = int.Parse(rentalIdPart.Split(':')[1].Trim());
                        int carId = int.Parse(carIdPart.Split(':')[1].Trim());
                        
                        // Обрабатываем время начала - формат примерно "Start: 2023-05-15 10:00:00"
                        string startTimeStr = startTimePart.Substring(startTimePart.IndexOf(':') + 1).Trim();
                        DateTime startTime = DateTime.Parse(startTimeStr);
                        
                        // Обрабатываем время окончания - формат примерно "End: 2023-05-15 12:00:00"
                        string endTimeStr = endTimePart.Substring(endTimePart.IndexOf(':') + 1).Trim();
                        DateTime endTime = DateTime.Parse(endTimeStr);
                        
                        // Обрабатываем длительность - формат примерно "Duration: 2 hours"
                        string durationStr = durationPart.Substring(durationPart.IndexOf(':') + 1).Trim();
                        double hours = double.Parse(durationStr.Split(' ')[0]);
                        
                        // Получаем информацию об автомобиле
                        string carInfo = "Авто ID: " + carId;
                        try
                        {
                            Car car = Data.DatabaseHelper.GetCarById(carId);
                            if (car != null)
                            {
                                carInfo = car.Brand + " " + car.Model;
                            }
                        }
                        catch (Exception ex) 
                        { 
                            Console.WriteLine($"Ошибка при получении данных автомобиля: {ex.Message}"); 
                        }

                        // Рассчитываем стоимость (можно было бы получить реальную стоимость из БД)
                        decimal cost = (decimal)hours * 450; // Примерная стоимость

                        historyItems.Add(new RentalHistoryItem
                        {
                            RentalDate = startTime.ToShortDateString(),
                            CarInfo = carInfo,
                            Duration = hours + " ч.",
                            Cost = cost.ToString("F2") + " руб."
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при обработке записи: {ex.Message}");
                    }
                }

                // Если список пуст, добавляем информационное сообщение
                if (historyItems.Count == 0)
                {
                    // Можно создать заглушку, чтобы показать, что история пуста
                    historyItems.Add(new RentalHistoryItem
                    {
                        RentalDate = "-",
                        CarInfo = "У вас пока нет истории аренд",
                        Duration = "-",
                        Cost = "-"
                    });
                }

                RentalHistoryListView.ItemsSource = historyItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке истории аренд: " + ex.Message);
                Console.WriteLine($"Исключение при загрузке истории аренд: {ex.Message}");
            }
        }

        private void LoadAnnouncements()
        {
            try
            {
                // Получаем объявления из базы данных
                var announcements = Data.DatabaseHelper.GetAnnouncements();
                List<AnnouncementItem> announcementItems = new List<AnnouncementItem>();

                foreach (var announcement in announcements)
                {
                    announcementItems.Add(new AnnouncementItem
                    {
                        PublishDate = announcement.PublishDate.ToShortDateString(),
                        Title = announcement.Title,
                        Content = announcement.Content
                    });
                }

                AnnouncementsListView.ItemsSource = announcementItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке объявлений: " + ex.Message);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
} 