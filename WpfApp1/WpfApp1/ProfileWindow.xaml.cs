using System;
using System.Collections.Generic;
using System.Windows;
using WpfApp1;
using LiveCharts;
using LiveCharts.Wpf;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.Linq;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;

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
            LoadUserStatsChart();
            ExportUserPdfButton.Click += ExportUserPdfButton_Click;
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
                        // Новый формат:
                        // Аренда №1: 10.05.2023 10:00 - 10.05.2023 12:00, Автомобиль: BMW X5, Продолжительность: 2 ч., Стоимость: 900 руб.
                        
                        // Извлекаем дату аренды
                        string rentalDate = "";
                        string carInfo = "";
                        string duration = "";
                        string cost = "";
                        
                        if (record.Contains("Аренда №"))
                        {
                            // Обрабатываем новый формат данных
                            string[] mainParts = record.Split(new string[] { ", " }, StringSplitOptions.None);
                            
                            // Обрабатываем первую часть с датой
                            if (mainParts.Length > 0 && mainParts[0].Contains(":"))
                            {
                                string[] dateParts = mainParts[0].Split(new string[] { ": ", " - " }, StringSplitOptions.None);
                                if (dateParts.Length >= 2)
                                {
                                    rentalDate = dateParts[1]; // Берем дату начала
                                }
                            }
                            
                            // Информация об автомобиле
                            if (mainParts.Length > 1 && mainParts[1].StartsWith("Автомобиль:"))
                            {
                                carInfo = mainParts[1].Substring("Автомобиль:".Length).Trim();
                            }
                            
                            // Продолжительность
                            if (mainParts.Length > 2 && mainParts[2].StartsWith("Продолжительность:"))
                            {
                                duration = mainParts[2].Substring("Продолжительность:".Length).Trim();
                            }
                            
                            // Стоимость
                            if (mainParts.Length > 3 && mainParts[3].StartsWith("Стоимость:"))
                            {
                                cost = mainParts[3].Substring("Стоимость:".Length).Trim();
                            }
                        }
                        else if (record.Contains("CarID:"))
                        {
                            // Обрабатываем старый формат данных
                            string[] parts = record.Split(new string[] { ", " }, StringSplitOptions.None);
                            
                            string startTimePart = "";
                            string durationPart = "";
                            int carId = 0;
                            
                            for (int i = 0; i < parts.Length; i++)
                            {
                                if (parts[i].StartsWith("CarID:"))
                                {
                                    carId = int.Parse(parts[i].Split(':')[1].Trim());
                                }
                                else if (parts[i].StartsWith("Start:"))
                                {
                                    startTimePart = parts[i].Substring(parts[i].IndexOf(':') + 1).Trim();
                                }
                                else if (parts[i].StartsWith("Duration:"))
                                {
                                    durationPart = parts[i].Substring(parts[i].IndexOf(':') + 1).Trim();
                                }
                            }
                            
                            // Обрабатываем время начала
                            if (!string.IsNullOrEmpty(startTimePart))
                            {
                                try
                                {
                                    DateTime startTime = DateTime.Parse(startTimePart);
                                    rentalDate = startTime.ToShortDateString();
                                }
                                catch
                                {
                                    rentalDate = startTimePart;
                                }
                            }
                            
                            // Обрабатываем длительность
                            if (!string.IsNullOrEmpty(durationPart))
                            {
                                string[] durationParts = durationPart.Split(' ');
                                if (durationParts.Length > 0)
                                {
                                    double hours = 0;
                                    double.TryParse(durationParts[0], out hours);
                                    duration = hours + " ч.";
                                    
                                    // Рассчитываем стоимость
                                    decimal costValue = (decimal)hours * 450;
                                    cost = costValue.ToString("F2") + " руб.";
                                }
                            }
                            
                            // Получаем информацию об автомобиле
                            carInfo = "Авто ID: " + carId;
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
                        }
                        else if (record.Contains("Тестовая аренда:") || record.Contains("Последняя аренда:"))
                        {
                            // Обрабатываем формат заглушки
                            string[] parts = record.Split(new string[] { ", " }, StringSplitOptions.None);
                            
                            // Дата
                            if (parts.Length > 0)
                            {
                                string[] dateParts = parts[0].Split(':');
                                if (dateParts.Length > 1)
                                {
                                    rentalDate = dateParts[1].Trim().Split(' ')[0];
                                }
                            }
                            
                            // Автомобиль
                            if (parts.Length > 1)
                            {
                                carInfo = parts[1].Replace("Автомобиль:", "").Trim();
                            }
                            
                            // Продолжительность
                            if (parts.Length > 2)
                            {
                                duration = parts[2].Replace("Продолжительность:", "").Trim();
                            }
                            
                            // Стоимость
                            if (parts.Length > 3)
                            {
                                cost = parts[3].Replace("Стоимость:", "").Trim();
                            }
                        }
                        
                        // Проверяем, что у нас есть все данные
                        if (string.IsNullOrEmpty(rentalDate)) rentalDate = "-";
                        if (string.IsNullOrEmpty(carInfo)) carInfo = "Нет данных";
                        if (string.IsNullOrEmpty(duration)) duration = "-";
                        if (string.IsNullOrEmpty(cost)) cost = "-";
                        
                        historyItems.Add(new RentalHistoryItem
                        {
                            RentalDate = rentalDate,
                            CarInfo = carInfo,
                            Duration = duration,
                            Cost = cost
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при обработке записи: {ex.Message}");
                        // Добавляем запись "как есть" для отладки
                        historyItems.Add(new RentalHistoryItem
                        {
                            RentalDate = "Ошибка",
                            CarInfo = record,
                            Duration = "-",
                            Cost = ex.Message
                        });
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

                // Сохраняем историю аренд для использования в окне истории
                this.rentalHistory = historyItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке истории аренд: " + ex.Message);
                Console.WriteLine($"Исключение при загрузке истории аренд: {ex.Message}");
                
                // Создаем пустую историю при ошибке
                this.rentalHistory = new List<RentalHistoryItem>();
                this.rentalHistory.Add(new RentalHistoryItem
                {
                    RentalDate = "Ошибка",
                    CarInfo = "Не удалось загрузить историю аренд",
                    Duration = "-",
                    Cost = "-"
                });
            }
        }

        // Добавляем новое поле для хранения истории аренд
        private List<RentalHistoryItem> rentalHistory = new List<RentalHistoryItem>();

        // Реализация обработчика нажатия кнопки показа истории
        private void ShowHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Открываем окно истории аренды, передавая ID пользователя
                RentalHistoryWindow historyWindow = new RentalHistoryWindow(userId);
                historyWindow.Owner = this;
                historyWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при открытии окна истории: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

        public void LoadUserStatsChart()
        {
            // Получаем статистику аренд по автомобилям за сегодня для текущего пользователя
            var stats = Data.DatabaseHelper.GetUserRentalsStatsByCarForToday(MainWindow.CurrentUser.UserID);
            var carNames = stats.Select(s => $"{s.CarName} ({s.TotalRevenue}₽)").ToArray();
            var counts = stats.Select(s => (double)s.Count).ToArray();
            UserStatsChart.Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Аренды",
                    Values = new ChartValues<double>(counts)
                }
            };
            UserStatsChart.AxisX.Clear();
            UserStatsChart.AxisX.Add(new Axis { Title = "Автомобиль", Labels = carNames });
            UserStatsChart.AxisY.Clear();
            UserStatsChart.AxisY.Add(new Axis { Title = "Кол-во аренд" });
        }

        private void ExportUserPdfButton_Click(object sender, RoutedEventArgs e)
        {
            var stats = Data.DatabaseHelper.GetUserRentalsStatsByCarForToday(MainWindow.CurrentUser.UserID);
            var dlg = new Microsoft.Win32.SaveFileDialog { Filter = "PDF files (*.pdf)|*.pdf", FileName = "user_report.pdf" };
            if (dlg.ShowDialog() == true)
            {
                var doc = new Document();
                var section = doc.AddSection();
                var title = section.AddParagraph("Ваши аренды за сегодня");
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
    }
} 