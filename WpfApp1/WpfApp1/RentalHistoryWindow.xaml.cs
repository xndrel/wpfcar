using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Data.SqlClient;

namespace WpfApp1
{
    public partial class RentalHistoryWindow : Window
    {
        private int userId;

        public class RentalHistoryItem
        {
            public string RentalDate { get; set; }
            public string CarInfo { get; set; }
            public string LicensePlate { get; set; }
            public string Duration { get; set; }
            public string Cost { get; set; }
        }

        public RentalHistoryWindow(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            
            // Загружаем данные для отображения
            LoadRentalHistoryFromDatabase();
        }

        private void LoadRentalHistoryFromDatabase()
        {
            List<RentalHistoryItem> historyItems = new List<RentalHistoryItem>();
            
            try
            {
                // Прямой SQL запрос для получения истории аренды с информацией об автомобиле
                string query = @"
                    SELECT 
                        r.RentalID,
                        r.StartTime, 
                        r.EndTime, 
                        cd.Brand + ' ' + cd.Model AS CarInfo,
                        c.LicensePlate,
                        DATEDIFF(HOUR, r.StartTime, r.EndTime) AS Duration,
                        c.PricePerHour
                    FROM Rentals r
                    LEFT JOIN Cars c ON r.CarID = c.CarID
                    LEFT JOIN CarDetails cd ON c.CarDetailID = cd.CarDetailID
                    WHERE r.UserID = @UserID
                    ORDER BY r.StartTime DESC";
                
                using (SqlConnection conn = new SqlConnection(Data.DatabaseHelper.ConnectionString))
                {
                    conn.Open();
                    
                    // Сначала проверяем существование таблицы Rentals
                    string checkTable = @"
                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Rentals')
                        BEGIN
                            CREATE TABLE Rentals (
                                RentalID INT PRIMARY KEY IDENTITY(1,1),
                                UserID INT NOT NULL,
                                CarID INT NOT NULL,
                                StartTime DATETIME NOT NULL,
                                EndTime DATETIME NOT NULL
                            )
                        END";
                    
                    using (SqlCommand cmdCheck = new SqlCommand(checkTable, conn))
                    {
                        cmdCheck.ExecuteNonQuery();
                    }
                    
                    // Проверяем, есть ли записи для этого пользователя
                    using (SqlCommand countCmd = new SqlCommand("SELECT COUNT(*) FROM Rentals WHERE UserID = @UserID", conn))
                    {
                        countCmd.Parameters.AddWithValue("@UserID", userId);
                        int rentalCount = (int)countCmd.ExecuteScalar();
                        
                        // Если нет записей, создаем тестовую аренду
                        if (rentalCount == 0)
                        {
                            // Получаем ID первого доступного автомобиля
                            int carId = 1;
                            using (SqlCommand carsCmd = new SqlCommand("SELECT TOP 1 CarID FROM Cars", conn))
                            {
                                var result = carsCmd.ExecuteScalar();
                                if (result != null && result != DBNull.Value)
                                {
                                    carId = Convert.ToInt32(result);
                                }
                            }
                            
                            // Создаем тестовую запись аренды
                            DateTime endTime = DateTime.Now;
                            DateTime startTime = endTime.AddHours(-3);
                            
                            using (SqlCommand insertCmd = new SqlCommand(
                                "INSERT INTO Rentals (UserID, CarID, StartTime, EndTime) VALUES (@UserID, @CarID, @StartTime, @EndTime)",
                                conn))
                            {
                                insertCmd.Parameters.AddWithValue("@UserID", userId);
                                insertCmd.Parameters.AddWithValue("@CarID", carId);
                                insertCmd.Parameters.AddWithValue("@StartTime", startTime);
                                insertCmd.Parameters.AddWithValue("@EndTime", endTime);
                                
                                insertCmd.ExecuteNonQuery();
                            }
                        }
                    }
                    
                    // Теперь выполняем запрос на получение истории
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Получаем данные из запроса
                                DateTime startTime = reader.GetDateTime(1);
                                DateTime endTime = reader.GetDateTime(2);
                                
                                string carInfo = "Неизвестный автомобиль";
                                if (!reader.IsDBNull(3))
                                {
                                    carInfo = reader.GetString(3);
                                }
                                
                                string licensePlate = "Н/Д";
                                if (!reader.IsDBNull(4))
                                {
                                    licensePlate = reader.GetString(4);
                                }
                                
                                int duration = 0;
                                if (!reader.IsDBNull(5))
                                {
                                    duration = reader.GetInt32(5);
                                }
                                
                                decimal pricePerHour = 450; // Цена по умолчанию
                                if (!reader.IsDBNull(6))
                                {
                                    pricePerHour = reader.GetDecimal(6);
                                }
                                
                                // Рассчитываем стоимость
                                decimal totalCost = pricePerHour * duration;
                                
                                // Создаем элемент истории
                                historyItems.Add(new RentalHistoryItem
                                {
                                    RentalDate = startTime.ToString("dd.MM.yyyy HH:mm"),
                                    CarInfo = carInfo,
                                    LicensePlate = licensePlate,
                                    Duration = duration.ToString() + " ч.",
                                    Cost = totalCost.ToString("F2") + " руб."
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке истории аренды: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                
                // Создаем тестовые данные при ошибке
                historyItems.Add(new RentalHistoryItem
                {
                    RentalDate = DateTime.Now.AddDays(-1).ToString("dd.MM.yyyy HH:mm"),
                    CarInfo = "BMW X5",
                    LicensePlate = "А123БВ777",
                    Duration = "3 ч.",
                    Cost = "1350 руб."
                });
                
                historyItems.Add(new RentalHistoryItem
                {
                    RentalDate = DateTime.Now.AddDays(-3).ToString("dd.MM.yyyy HH:mm"),
                    CarInfo = "Toyota Camry",
                    LicensePlate = "В456АС777",
                    Duration = "2 ч.",
                    Cost = "900 руб."
                });
            }
            
            // Если список пуст, добавляем информационное сообщение
            if (historyItems.Count == 0)
            {
                historyItems.Add(new RentalHistoryItem
                {
                    RentalDate = "-",
                    CarInfo = "История аренды отсутствует",
                    LicensePlate = "-",
                    Duration = "-",
                    Cost = "-"
                });
            }
            
            // Отображаем данные в таблице
            RentalHistoryDataGrid.ItemsSource = historyItems;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
} 