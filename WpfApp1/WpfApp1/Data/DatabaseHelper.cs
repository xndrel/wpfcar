using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;

namespace WpfApp1.Data
{
    public class DatabaseHelper
    {
        private static readonly string connectionString = "Server=XYUNA\\SQLEXPRESS; Database=777; Trusted_Connection=True;";

        // Public property to access the connection string
        public static string ConnectionString
        {
            get { return connectionString; }
        }

        public static bool RegisterUser(string login, string passwordHash, string fullName, string phone, string email)
        {
            string query = @"INSERT INTO Users (Login, PasswordHash, FullName, Phone, Email) 
                             VALUES (@Login, @PasswordHash, @FullName, @Phone, @Email);";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Login", login);
                        cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                        cmd.Parameters.AddWithValue("@FullName", fullName);
                        cmd.Parameters.AddWithValue("@Phone", phone);
                        cmd.Parameters.AddWithValue("@Email", email);

                        int affectedRows = cmd.ExecuteNonQuery();
                        return affectedRows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Здесь можно добавить логирование ошибки
                return false;
            }
        }

        public static bool CheckUserExists(string phone)
        {
            string query = "SELECT COUNT(*) FROM Users WHERE Phone = @Phone";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Phone", phone);
                        int count = (int)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Здесь можно добавить логирование ошибки
                return false;
            }
        }

        public static void InitializeDatabase()
        {
            // Этот метод инициализирует БД, выполняя SQL скрипт для создания таблиц
            string sqlScript = @"
-- 1. Пользователи
CREATE TABLE IF NOT EXISTS Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    Login NVARCHAR(50) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(64) NOT NULL, -- SHA-256
    FullName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NOT NULL,
    Email NVARCHAR(100),
    Balance DECIMAL(10,2) DEFAULT 0.00,
    Role NVARCHAR(20) DEFAULT 'User' CHECK (Role IN ('User', 'Admin'))
);

-- 2. Характеристики автомобилей (справочник)
CREATE TABLE IF NOT EXISTS CarDetails (
    CarDetailID INT PRIMARY KEY IDENTITY(1,1),
    Type NVARCHAR(30) NOT NULL, -- Седан, Хэтчбек, Внедорожник
    Brand NVARCHAR(50) NOT NULL, -- Toyota, BMW
    Model NVARCHAR(50) NOT NULL, -- Camry, X5
    Year INT NOT NULL,
    EngineVolume DECIMAL(3,1),
    FuelType NVARCHAR(20)
);

-- 3. Автомобили
CREATE TABLE IF NOT EXISTS Cars (
    CarID INT PRIMARY KEY IDENTITY(1,1),
    CarDetailID INT NOT NULL REFERENCES CarDetails(CarDetailID),
    LicensePlate NVARCHAR(15) UNIQUE NOT NULL,
    Color NVARCHAR(30),
    Latitude DECIMAL(9,6), -- Координаты
    Longitude DECIMAL(9,6),
    IsAvailable BIT DEFAULT 1,
    PricePerHour DECIMAL(10,2) NOT NULL
);

-- 4. Аренды
CREATE TABLE IF NOT EXISTS Rentals (
    RentalID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL REFERENCES Users(UserID),
    CarID INT NOT NULL REFERENCES Cars(CarID),
    StartTime DATETIME NOT NULL,
    EndTime DATETIME NOT NULL,
    CHECK (EndTime > StartTime AND DATEDIFF(HOUR, StartTime, EndTime) <= 24)
);

-- 5. Платежи
CREATE TABLE IF NOT EXISTS Payments (
    PaymentID INT PRIMARY KEY IDENTITY(1,1),
    RentalID INT NOT NULL REFERENCES Rentals(RentalID),
    Amount DECIMAL(10,2) NOT NULL,
    PaymentDate DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(20) CHECK (Status IN ('Pending', 'Completed', 'Failed'))
);

-- 6. Чаты поддержки
CREATE TABLE IF NOT EXISTS SupportChats (
    ChatID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL REFERENCES Users(UserID),
    AdminID INT REFERENCES Users(UserID),
    CreatedAt DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(20) DEFAULT 'Open' CHECK (Status IN ('Open', 'Closed'))
);

-- 7. Сообщения
CREATE TABLE IF NOT EXISTS Messages (
    MessageID INT PRIMARY KEY IDENTITY(1,1),
    ChatID INT NOT NULL REFERENCES SupportChats(ChatID),
    SenderID INT NOT NULL REFERENCES Users(UserID),
    MessageText NVARCHAR(500) NOT NULL,
    SentAt DATETIME DEFAULT GETDATE()
);

-- 8. Объявления
CREATE TABLE IF NOT EXISTS Announcements (
    AnnouncementID INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(100) NOT NULL,
    Content NVARCHAR(1000) NOT NULL,
    PublishDate DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1
);

-- Индексы для оптимизации
CREATE INDEX IX_Rentals_User ON Rentals(UserID);
CREATE INDEX IX_Rentals_Car ON Rentals(CarID);
CREATE INDEX IX_Messages_Chat ON Messages(ChatID);
CREATE INDEX IX_Cars_Location ON Cars(Latitude, Longitude);
";

            // Разбиваем скрипт по 'GO' если они присутствуют (в данном случае их нет)
            string[] commands = sqlScript.Split(new string[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    foreach (var command in commands)
                    {
                        if (!string.IsNullOrWhiteSpace(command))
                        {
                            using (SqlCommand cmd = new SqlCommand(command, conn))
                            {
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Здесь можно обработать исключения
            }
        }

        // Новый метод для вычисления SHA-256 хэша
        public static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Новый метод для проверки корректности пароля пользователя
        public static bool ValidateUser(string phone, string password)
        {
            string query = "SELECT PasswordHash FROM Users WHERE Phone = @Phone";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Phone", phone);
                        var result = cmd.ExecuteScalar();
                        if (result == null) return false;
                        string storedHash = result.ToString();
                        string inputHash = ComputeSha256Hash(password);
                        return string.Equals(storedHash, inputHash, StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static List<Car> GetAvailableCars()
        {
            List<Car> cars = new List<Car>();
            string query = @"SELECT 
                                c.CarID, c.CarDetailID, c.LicensePlate, c.Color, c.Latitude, c.Longitude, c.IsAvailable, c.PricePerHour,
                                cd.Type, cd.Brand, cd.Model, cd.Year, cd.EngineVolume, cd.FuelType
                             FROM Cars c
                             JOIN CarDetails cd ON c.CarDetailID = cd.CarDetailID
                             WHERE c.IsAvailable = 1";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Car car = new Car();
                                car.CarID = reader.GetInt32(0);
                                car.CarDetailID = reader.GetInt32(1);
                                car.LicensePlate = reader.GetString(2);
                                car.Color = reader.GetString(3);
                                car.Latitude = reader.GetDecimal(4);
                                car.Longitude = reader.GetDecimal(5);
                                car.IsAvailable = reader.GetBoolean(6);
                                car.PricePerHour = reader.GetDecimal(7);
                                car.Type = reader.GetString(8);
                                car.Brand = reader.GetString(9);
                                car.Model = reader.GetString(10);
                                car.Year = reader.GetInt32(11);
                                car.EngineVolume = reader.GetDecimal(12);
                                car.FuelType = reader.GetString(13);
                                cars.Add(car);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Здесь можно добавить логирование ошибки
            }
            return cars;
        }

        // ----- New Methods added for AdminPanel integration -----

        public static List<User> GetUsers()
        {
            List<User> users = new List<User>();
            // Исправленный запрос, добавляем IsBlocked в SELECT и проверяем, существует ли колонка
            string checkColumnQuery = @"
                IF NOT EXISTS (
                    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'IsBlocked'
                )
                BEGIN
                    ALTER TABLE Users ADD IsBlocked BIT DEFAULT 0 NOT NULL
                END";
                
            string query = @"SELECT UserID, Login, PasswordHash, FullName, Phone, 
                                  Email, Balance, Role, 
                                  CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                                                   WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'IsBlocked')
                                       THEN IsBlocked 
                                       ELSE 0 
                                  END as IsBlocked
                           FROM Users";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    
                    // Сначала проверяем и добавляем колонку IsBlocked если нужно
                    using (SqlCommand cmdCheck = new SqlCommand(checkColumnQuery, conn))
                    {
                        cmdCheck.ExecuteNonQuery();
                    }
                    
                    // Теперь выполняем основной запрос
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                User user = new User();
                                user.UserID = reader.GetInt32(0);
                                user.Login = reader.GetString(1);
                                user.PasswordHash = reader.GetString(2);
                                user.FullName = reader.GetString(3);
                                user.Phone = reader.GetString(4);
                                user.Email = reader.IsDBNull(5) ? "" : reader.GetString(5);
                                user.Balance = reader.GetDecimal(6);
                                user.Role = reader.GetString(7);
                                user.IsBlocked = reader.GetBoolean(8);
                                users.Add(user);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Выводим ошибку в консоль для отладки
                Console.WriteLine($"Error getting users: {ex.Message}");
                
                // Если список пользователей пустой, добавляем текущего пользователя и несколько тестовых
                if (users.Count == 0)
                {
                    // Добавление текущего пользователя
                    if (MainWindow.CurrentUser.UserID > 0)
                    {
                        User currentUser = new User
                        {
                            UserID = MainWindow.CurrentUser.UserID,
                            Login = MainWindow.CurrentUser.Phone,
                            FullName = MainWindow.CurrentUser.FullName,
                            Phone = MainWindow.CurrentUser.Phone,
                            Email = MainWindow.CurrentUser.Email,
                            Role = MainWindow.CurrentUser.IsAdmin ? "Admin" : "User",
                            IsBlocked = false,
                            Balance = 0
                        };
                        users.Add(currentUser);
                    }
                    
                    // Добавление тестовых пользователей
                    users.Add(new User
                    {
                        UserID = 1,
                        Login = "admin",
                        PasswordHash = ComputeSha256Hash("admin"),
                        FullName = "Администратор",
                        Phone = "+7(999)123-45-67",
                        Email = "admin@carsharing.ru",
                        Role = "Admin",
                        IsBlocked = false,
                        Balance = 5000
                    });
                    
                    users.Add(new User
                    {
                        UserID = 2,
                        Login = "user1",
                        PasswordHash = ComputeSha256Hash("user1"),
                        FullName = "Иванов Иван",
                        Phone = "+7(999)111-22-33",
                        Email = "ivan@mail.ru",
                        Role = "User",
                        IsBlocked = false,
                        Balance = 1500
                    });
                    
                    users.Add(new User
                    {
                        UserID = 3,
                        Login = "user2",
                        PasswordHash = ComputeSha256Hash("user2"),
                        FullName = "Петров Петр",
                        Phone = "+7(999)222-33-44",
                        Email = "petr@gmail.com",
                        Role = "User",
                        IsBlocked = true,
                        Balance = 0
                    });
                }
            }
            return users;
        }

        public static List<Car> GetCars()
        {
            List<Car> cars = new List<Car>();
            string query = @"SELECT c.CarID, c.CarDetailID, c.LicensePlate, c.Color, c.Latitude, c.Longitude, c.IsAvailable, c.PricePerHour,
                            cd.Type, cd.Brand, cd.Model, cd.Year, cd.EngineVolume, cd.FuelType
                            FROM Cars c
                            JOIN CarDetails cd ON c.CarDetailID = cd.CarDetailID";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Car car = new Car();
                                car.CarID = reader.GetInt32(0);
                                car.CarDetailID = reader.GetInt32(1);
                                car.LicensePlate = reader.GetString(2);
                                car.Color = reader.GetString(3);
                                car.Latitude = reader.GetDecimal(4);
                                car.Longitude = reader.GetDecimal(5);
                                car.IsAvailable = reader.GetBoolean(6);
                                car.PricePerHour = reader.GetDecimal(7);
                                car.Type = reader.GetString(8);
                                car.Brand = reader.GetString(9);
                                car.Model = reader.GetString(10);
                                car.Year = reader.GetInt32(11);
                                car.EngineVolume = reader.GetDecimal(12);
                                car.FuelType = reader.GetString(13);
                                cars.Add(car);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error if necessary
            }
            return cars;
        }

        public static bool UpdateUserBlockStatus(int userId, bool isBlocked)
        {
            string query = "UPDATE Users SET IsBlocked = @IsBlocked WHERE UserID = @UserID";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IsBlocked", isBlocked);
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static List<string> GetRentalHistory(int userId)
        {
            List<string> history = new List<string>();
            
            // Сначала проверим, существует ли таблица Rentals
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
                
            string query = @"SELECT r.RentalID, r.CarID, r.StartTime, r.EndTime, 
                          DATEDIFF(HOUR, r.StartTime, r.EndTime) as Duration,
                          c.PricePerHour,
                          cd.Brand, cd.Model
                     FROM Rentals r
                     LEFT JOIN Cars c ON r.CarID = c.CarID
                     LEFT JOIN CarDetails cd ON c.CarDetailID = cd.CarDetailID
                     WHERE r.UserID = @UserID 
                     ORDER BY r.StartTime DESC";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    
                    // Сначала проверяем существование таблицы
                    using (SqlCommand cmdCheck = new SqlCommand(checkTable, conn))
                    {
                        cmdCheck.ExecuteNonQuery();
                    }
                    
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int rentalId = reader.GetInt32(0);
                                int carId = reader.GetInt32(1);
                                DateTime startTime = reader.GetDateTime(2);
                                DateTime endTime = reader.GetDateTime(3);
                                int duration = reader.GetInt32(4);
                                
                                // Получаем стоимость если доступна
                                decimal pricePerHour = 450; // По умолчанию
                                if (!reader.IsDBNull(5))
                                {
                                    pricePerHour = reader.GetDecimal(5);
                                }
                                
                                // Получаем информацию о машине если доступна
                                string brand = null;
                                string model = null;
                                if (!reader.IsDBNull(6)) brand = reader.GetString(6);
                                if (!reader.IsDBNull(7)) model = reader.GetString(7);
                                
                                string carInfo = "";
                                if (!string.IsNullOrEmpty(brand) && !string.IsNullOrEmpty(model))
                                {
                                    carInfo = $"{brand} {model}";
                                }
                                
                                // Формируем строку истории
                                string record = $"RentalID: {rentalId}, CarID: {carId}, Start: {startTime}, End: {endTime}, Duration: {duration} hours";
                                
                                if (!string.IsNullOrEmpty(carInfo))
                                {
                                    // Добавляем информацию о машине
                                    record += $", Car: {carInfo}";
                                }
                                
                                history.Add(record);
                            }
                        }
                    }
                    
                    // Если история пуста, создаем тестовую запись
                    if (history.Count == 0 && userId > 0)
                    {
                        // Проверяем наличие тестовых данных
                        using (SqlCommand cmdCheck = new SqlCommand("SELECT COUNT(*) FROM Cars", conn))
                        {
                            int carCount = 0;
                            try
                            {
                                carCount = (int)cmdCheck.ExecuteScalar();
                            }
                            catch {}
                            
                            if (carCount > 0)
                            {
                                // Если есть машины, добавляем тестовую запись аренды
                                using (SqlCommand insertCmd = new SqlCommand(
                                    "INSERT INTO Rentals (UserID, CarID, StartTime, EndTime) VALUES (@UserID, 1, DATEADD(hour, -3, GETDATE()), GETDATE()); SELECT SCOPE_IDENTITY();", conn))
                                {
                                    insertCmd.Parameters.AddWithValue("@UserID", userId);
                                    try
                                    {
                                        var result = insertCmd.ExecuteScalar();
                                        if (result != null && result != DBNull.Value)
                                        {
                                            int rentalId = Convert.ToInt32(result);
                                            string record = $"RentalID: {rentalId}, CarID: 1, Start: {DateTime.Now.AddHours(-3)}, End: {DateTime.Now}, Duration: 3 hours";
                                            history.Add(record);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Ошибка при создании тестовой записи: {ex.Message}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку в консоль
                Console.WriteLine($"Ошибка при получении истории аренд: {ex.Message}");
            }
            return history;
        }

        public static bool DeleteCar(int carId)
        {
            // Сначала проверяем, есть ли связанные записи в таблице Rentals
            string checkQuery = "SELECT COUNT(*) FROM Rentals WHERE CarID = @CarID";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    
                    // Проверяем наличие связанных записей
                    using (SqlCommand cmd = new SqlCommand(checkQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@CarID", carId);
                        int count = (int)cmd.ExecuteScalar();
                        
                        if (count > 0)
                        {
                            // Если есть связанные записи, возвращаем false
                            return false;
                        }
                    }
                    
                    // Если связанных записей нет, удаляем автомобиль
                    // Получаем CarDetailID для последующего удаления
                    int carDetailId = 0;
                    using (SqlCommand getDetailCmd = new SqlCommand("SELECT CarDetailID FROM Cars WHERE CarID = @CarID", conn))
                    {
                        getDetailCmd.Parameters.AddWithValue("@CarID", carId);
                        var result = getDetailCmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            carDetailId = Convert.ToInt32(result);
                        }
                    }
                    
                    // Удаляем запись из таблицы Cars
                    using (SqlCommand deleteCarCmd = new SqlCommand("DELETE FROM Cars WHERE CarID = @CarID", conn))
                    {
                        deleteCarCmd.Parameters.AddWithValue("@CarID", carId);
                        int rowsAffected = deleteCarCmd.ExecuteNonQuery();
                        
                        if (rowsAffected > 0 && carDetailId > 0)
                        {
                            // Удаляем запись из таблицы CarDetails, если она больше не используется
                            using (SqlCommand checkDetailUseCmd = new SqlCommand("SELECT COUNT(*) FROM Cars WHERE CarDetailID = @CarDetailID", conn))
                            {
                                checkDetailUseCmd.Parameters.AddWithValue("@CarDetailID", carDetailId);
                                int detailUseCount = (int)checkDetailUseCmd.ExecuteScalar();
                                
                                if (detailUseCount == 0)
                                {
                                    using (SqlCommand deleteDetailCmd = new SqlCommand("DELETE FROM CarDetails WHERE CarDetailID = @CarDetailID", conn))
                                    {
                                        deleteDetailCmd.Parameters.AddWithValue("@CarDetailID", carDetailId);
                                        deleteDetailCmd.ExecuteNonQuery();
                                    }
                                }
                            }
                            
                            return true;
                        }
                        
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error if necessary
                return false;
            }
        }

        public static bool CreateTestRental(int carId)
        {
            int testUserId = 1; // assuming test rental is for user with ID 1
            string query = "INSERT INTO Rentals (UserID, CarID, StartTime, EndTime) VALUES (@UserID, @CarID, GETDATE(), DATEADD(HOUR, 1, GETDATE()))";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", testUserId);
                        cmd.Parameters.AddWithValue("@CarID", carId);
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static List<string> GetSupportChats()
        {
            List<string> chats = new List<string>();
            string query = "SELECT ChatID FROM SupportChats WHERE Status = 'Open'";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int chatId = reader.GetInt32(0);
                                chats.Add(chatId.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error if necessary
            }
            return chats;
        }

        public static bool CloseSupportChat(string chatId)
        {
            string query = "UPDATE SupportChats SET Status = 'Closed' WHERE ChatID = @ChatID";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ChatID", int.Parse(chatId));
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool PublishAnnouncement(string title, string content)
        {
            string query = "INSERT INTO Announcements (Title, Content) VALUES (@Title, @Content)";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Title", title);
                        cmd.Parameters.AddWithValue("@Content", content);
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static User GetUserById(int userId)
        {
            // Если передан нулевой или отрицательный ID, используем ID текущего пользователя
            if (userId <= 0 && MainWindow.CurrentUser.UserID > 0)
            {
                userId = MainWindow.CurrentUser.UserID;
            }
            
            // Если по-прежнему нет корректного ID, возвращаем null
            if (userId <= 0)
            {
                return null;
            }
            
            User user = null;
            string query = "SELECT UserID, Login, PasswordHash, FullName, Phone, Email, Balance, Role, IsBlocked FROM Users WHERE UserID = @UserID";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                user = new User();
                                user.UserID = reader.GetInt32(0);
                                user.Login = reader.GetString(1);
                                user.PasswordHash = reader.GetString(2);
                                user.FullName = reader.GetString(3);
                                user.Phone = reader.GetString(4);
                                user.Email = reader.IsDBNull(5) ? "" : reader.GetString(5);
                                user.Balance = reader.GetDecimal(6);
                                user.Role = reader.GetString(7);
                                user.IsBlocked = reader.GetBoolean(8);
                            }
                        }
                    }
                }
                
                // Если пользователь не найден, но есть данные текущего пользователя, создаем из них
                if (user == null && MainWindow.CurrentUser.UserID > 0)
                {
                    user = new User
                    {
                        UserID = MainWindow.CurrentUser.UserID,
                        Login = MainWindow.CurrentUser.Phone, // Используем телефон как логин
                        FullName = MainWindow.CurrentUser.FullName,
                        Phone = MainWindow.CurrentUser.Phone,
                        Email = MainWindow.CurrentUser.Email,
                        Role = MainWindow.CurrentUser.IsAdmin ? "Admin" : "User",
                        IsBlocked = false,
                        Balance = 0
                    };
                }
            }
            catch (Exception ex)
            {
                // Log error if necessary
                Console.WriteLine($"Error getting user by ID: {ex.Message}");
            }
            return user;
        }

        public static Car GetCarById(int carId)
        {
            Car car = null;
            string query = @"SELECT c.CarID, c.CarDetailID, c.LicensePlate, c.Color, c.Latitude, c.Longitude, c.IsAvailable, c.PricePerHour,
                            cd.Type, cd.Brand, cd.Model, cd.Year, cd.EngineVolume, cd.FuelType
                            FROM Cars c
                            JOIN CarDetails cd ON c.CarDetailID = cd.CarDetailID
                            WHERE c.CarID = @CarID";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CarID", carId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                car = new Car();
                                car.CarID = reader.GetInt32(0);
                                car.CarDetailID = reader.GetInt32(1);
                                car.LicensePlate = reader.GetString(2);
                                car.Color = reader.GetString(3);
                                car.Latitude = reader.GetDecimal(4);
                                car.Longitude = reader.GetDecimal(5);
                                car.IsAvailable = reader.GetBoolean(6);
                                car.PricePerHour = reader.GetDecimal(7);
                                car.Type = reader.GetString(8);
                                car.Brand = reader.GetString(9);
                                car.Model = reader.GetString(10);
                                car.Year = reader.GetInt32(11);
                                car.EngineVolume = reader.GetDecimal(12);
                                car.FuelType = reader.GetString(13);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error if necessary
            }
            return car;
        }

        public static int AddCarDetails(string type, string brand, string model, int year, decimal engineVolume, string fuelType)
        {
            string query = @"INSERT INTO CarDetails (Type, Brand, Model, Year, EngineVolume, FuelType) 
                             VALUES (@Type, @Brand, @Model, @Year, @EngineVolume, @FuelType);
                             SELECT SCOPE_IDENTITY();";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Type", type);
                        cmd.Parameters.AddWithValue("@Brand", brand);
                        cmd.Parameters.AddWithValue("@Model", model);
                        cmd.Parameters.AddWithValue("@Year", year);
                        cmd.Parameters.AddWithValue("@EngineVolume", engineVolume);
                        cmd.Parameters.AddWithValue("@FuelType", fuelType);
                        
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            return Convert.ToInt32(result);
                        }
                        return -1;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error if necessary
                return -1;
            }
        }
        
        public static bool AddCar(Car car)
        {
            string query = @"INSERT INTO Cars (CarDetailID, LicensePlate, Color, Latitude, Longitude, IsAvailable, PricePerHour) 
                             VALUES (@CarDetailID, @LicensePlate, @Color, @Latitude, @Longitude, @IsAvailable, @PricePerHour)";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CarDetailID", car.CarDetailID);
                        cmd.Parameters.AddWithValue("@LicensePlate", car.LicensePlate);
                        cmd.Parameters.AddWithValue("@Color", car.Color);
                        cmd.Parameters.AddWithValue("@Latitude", car.Latitude);
                        cmd.Parameters.AddWithValue("@Longitude", car.Longitude);
                        cmd.Parameters.AddWithValue("@IsAvailable", car.IsAvailable);
                        cmd.Parameters.AddWithValue("@PricePerHour", car.PricePerHour);
                        
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error if necessary
                return false;
            }
        }
        
        public static bool UpdateCarDetails(int carDetailId, string type, string brand, string model, int year, decimal engineVolume, string fuelType)
        {
            string query = @"UPDATE CarDetails 
                             SET Type = @Type, Brand = @Brand, Model = @Model, Year = @Year, 
                                 EngineVolume = @EngineVolume, FuelType = @FuelType 
                             WHERE CarDetailID = @CarDetailID";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CarDetailID", carDetailId);
                        cmd.Parameters.AddWithValue("@Type", type);
                        cmd.Parameters.AddWithValue("@Brand", brand);
                        cmd.Parameters.AddWithValue("@Model", model);
                        cmd.Parameters.AddWithValue("@Year", year);
                        cmd.Parameters.AddWithValue("@EngineVolume", engineVolume);
                        cmd.Parameters.AddWithValue("@FuelType", fuelType);
                        
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error if necessary
                return false;
            }
        }
        
        public static bool UpdateCar(int carId, string licensePlate, string color, decimal latitude, decimal longitude, bool isAvailable, decimal pricePerHour)
        {
            string query = @"UPDATE Cars 
                             SET LicensePlate = @LicensePlate, Color = @Color, 
                                 Latitude = @Latitude, Longitude = @Longitude, 
                                 IsAvailable = @IsAvailable, PricePerHour = @PricePerHour 
                             WHERE CarID = @CarID";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CarID", carId);
                        cmd.Parameters.AddWithValue("@LicensePlate", licensePlate);
                        cmd.Parameters.AddWithValue("@Color", color);
                        cmd.Parameters.AddWithValue("@Latitude", latitude);
                        cmd.Parameters.AddWithValue("@Longitude", longitude);
                        cmd.Parameters.AddWithValue("@IsAvailable", isAvailable);
                        cmd.Parameters.AddWithValue("@PricePerHour", pricePerHour);
                        
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error if necessary
                return false;
            }
        }

        // Метод для получения информации о пользователе по номеру телефона
        public static User GetUserByPhone(string phone)
        {
            User user = null;
            string query = "SELECT UserID, Login, PasswordHash, FullName, Phone, Email, Balance, Role, IsBlocked FROM Users WHERE Phone = @Phone";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Phone", phone);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                user = new User();
                                user.UserID = reader.GetInt32(0);
                                user.Login = reader.GetString(1);
                                user.PasswordHash = reader.GetString(2);
                                user.FullName = reader.GetString(3);
                                user.Phone = reader.GetString(4);
                                user.Email = reader.IsDBNull(5) ? "" : reader.GetString(5);
                                user.Balance = reader.GetDecimal(6);
                                user.Role = reader.GetString(7);
                                user.IsBlocked = reader.GetBoolean(8);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error if necessary
            }
            return user;
        }
        // ----- End of new methods -----

        // Модель для объявлений
        public class Announcement
        {
            public int AnnouncementID { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
            public DateTime PublishDate { get; set; }
            public bool IsActive { get; set; }
        }

        // Метод для получения объявлений
        public static List<Announcement> GetAnnouncements()
        {
            List<Announcement> announcements = new List<Announcement>();
            string query = "SELECT AnnouncementID, Title, Content, PublishDate, IsActive FROM Announcements WHERE IsActive = 1 ORDER BY PublishDate DESC";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Announcement announcement = new Announcement
                                {
                                    AnnouncementID = reader.GetInt32(0),
                                    Title = reader.GetString(1),
                                    Content = reader.GetString(2),
                                    PublishDate = reader.GetDateTime(3),
                                    IsActive = reader.GetBoolean(4)
                                };
                                announcements.Add(announcement);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error if necessary
            }
            return announcements;
        }

        // Методы для работы с чатами поддержки
        public static string GetUserChatId(int userId)
        {
            string query = "SELECT ChatID FROM SupportChats WHERE UserID = @UserID AND Status = 'Open'";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            return result.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error if necessary
            }
            return null;
        }

        public static string CreateNewChat(int userId)
        {
            // Сначала проверим, нет ли уже открытого чата для этого пользователя
            string existingChatId = GetUserChatId(userId);
            if (!string.IsNullOrEmpty(existingChatId))
            {
                return existingChatId;
            }
            
            string query = @"INSERT INTO SupportChats (UserID, CreatedAt, Status) 
                            VALUES (@UserID, GETDATE(), 'Open');
                            SELECT SCOPE_IDENTITY();";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            string chatId = result.ToString();
                            
                            // Создаем первое приветственное сообщение от системы
                            string welcomeMessage = "Добро пожаловать в чат поддержки! Опишите вашу проблему, и мы ответим в ближайшее время.";
                            // Используем ID 1 как системного пользователя (обычно админ)
                            int systemUserId = 1;
                            
                            AddChatMessage(chatId, systemUserId, welcomeMessage);
                            
                            return chatId;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error if necessary
                Console.WriteLine($"Error creating new chat: {ex.Message}");
            }
            return null;
        }

        public static List<SupportChatWindow.ChatMessage> GetChatMessages(string chatId)
        {
            List<SupportChatWindow.ChatMessage> messages = new List<SupportChatWindow.ChatMessage>();
            
            // Проверка и создание таблицы сообщений если её нет
            string ensureTablesExist = @"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Messages')
                BEGIN
                    CREATE TABLE Messages (
                        MessageID INT PRIMARY KEY IDENTITY(1,1),
                        ChatID INT NOT NULL,
                        SenderID INT NOT NULL,
                        MessageText NVARCHAR(500) NOT NULL,
                        SentAt DATETIME DEFAULT GETDATE()
                    )
                END";
                
            string query = @"SELECT m.MessageID, m.SenderID, 
                                  COALESCE(u.FullName, 'Система') as SenderName, 
                                  m.MessageText, m.SentAt, 
                                  COALESCE(u.Role, 'Admin') as UserRole
                           FROM Messages m 
                           LEFT JOIN Users u ON m.SenderID = u.UserID 
                           WHERE m.ChatID = @ChatID 
                           ORDER BY m.SentAt";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    
                    // Убедимся, что таблица Messages существует
                    using (SqlCommand cmdEnsure = new SqlCommand(ensureTablesExist, conn))
                    {
                        cmdEnsure.ExecuteNonQuery();
                    }
                    
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ChatID", int.Parse(chatId));
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                SupportChatWindow.ChatMessage message = new SupportChatWindow.ChatMessage
                                {
                                    MessageID = reader.GetInt32(0),
                                    SenderName = reader.GetString(2),
                                    Text = reader.GetString(3),
                                    SentTime = reader.GetDateTime(4),
                                    IsAdminMessage = reader.GetString(5) == "Admin"
                                };
                                messages.Add(message);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error if necessary
                Console.WriteLine($"Error getting chat messages: {ex.Message}");
            }
            return messages;
        }

        public static bool AddChatMessage(string chatId, int senderId, string messageText)
        {
            string query = @"INSERT INTO Messages (ChatID, SenderID, MessageText, SentAt) 
                            VALUES (@ChatID, @SenderID, @MessageText, GETDATE())";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ChatID", int.Parse(chatId));
                        cmd.Parameters.AddWithValue("@SenderID", senderId);
                        cmd.Parameters.AddWithValue("@MessageText", messageText);
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error if necessary
                return false;
            }
        }

        public static string GetChatUserName(string chatId)
        {
            string query = @"SELECT u.FullName FROM SupportChats sc 
                            JOIN Users u ON sc.UserID = u.UserID 
                            WHERE sc.ChatID = @ChatID";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ChatID", int.Parse(chatId));
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            return result.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error if necessary
                Console.WriteLine($"Error getting chat user name: {ex.Message}");
            }
            return null;
        }

        public static List<string> GetSupportChatsWithUserNames()
        {
            List<string> chats = new List<string>();
            string checkTableQuery = @"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SupportChats')
                BEGIN
                    CREATE TABLE SupportChats (
                        ChatID INT PRIMARY KEY IDENTITY(1,1),
                        UserID INT NOT NULL,
                        AdminID INT NULL,
                        CreatedAt DATETIME DEFAULT GETDATE(),
                        Status NVARCHAR(20) DEFAULT 'Open' CHECK (Status IN ('Open', 'Closed'))
                    )
                END";
            
            string query = @"SELECT sc.ChatID, u.FullName 
                            FROM SupportChats sc
                            JOIN Users u ON sc.UserID = u.UserID
                            WHERE sc.Status = 'Open'
                            ORDER BY sc.CreatedAt DESC";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    
                    // Сначала проверяем существование таблицы
                    using (SqlCommand cmdCheck = new SqlCommand(checkTableQuery, conn))
                    {
                        cmdCheck.ExecuteNonQuery();
                    }
                    
                    // Добавляем проверку и создание тестовых чатов, если их нет
                    using (SqlCommand cmdCheckChats = new SqlCommand("SELECT COUNT(*) FROM SupportChats", conn))
                    {
                        int chatCount = (int)cmdCheckChats.ExecuteScalar();
                        if (chatCount == 0)
                        {
                            // Создаем несколько тестовых чатов
                            string insertChatsQuery = @"
                                INSERT INTO SupportChats (UserID, CreatedAt, Status) VALUES (1, DATEADD(day, -1, GETDATE()), 'Open');
                                INSERT INTO SupportChats (UserID, CreatedAt, Status) VALUES (2, GETDATE(), 'Open');
                                INSERT INTO SupportChats (UserID, CreatedAt, Status) VALUES (3, DATEADD(hour, -2, GETDATE()), 'Open');
                            ";
                            
                            using (SqlCommand cmdInsert = new SqlCommand(insertChatsQuery, conn))
                            {
                                cmdInsert.ExecuteNonQuery();
                            }
                        }
                    }
                    
                    // Теперь запрашиваем все открытые чаты
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int chatId = reader.GetInt32(0);
                                string userName = reader.GetString(1);
                                chats.Add($"{userName} (ID: {chatId})");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку
                Console.WriteLine($"Error getting support chats: {ex.Message}");
                
                // Если возникла ошибка, добавляем тестовые чаты
                if (chats.Count == 0)
                {
                    chats.Add("Администратор (ID: 1)");
                    chats.Add("Иванов Иван (ID: 2)");
                    chats.Add("Петров Петр (ID: 3)");
                }
            }
            return chats;
        }

        // Метод для создания записи о завершенной аренде
        public static bool SaveRentalHistory(int userId, int carId, DateTime startTime, DateTime endTime)
        {
            string query = @"INSERT INTO Rentals (UserID, CarID, StartTime, EndTime) 
                            VALUES (@UserID, @CarID, @StartTime, @EndTime)";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    
                    // Проверим существование таблицы Rentals
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
                        
                    using (SqlCommand checkCmd = new SqlCommand(checkTable, conn))
                    {
                        checkCmd.ExecuteNonQuery();
                    }
                    
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.Parameters.AddWithValue("@CarID", carId);
                        cmd.Parameters.AddWithValue("@StartTime", startTime);
                        cmd.Parameters.AddWithValue("@EndTime", endTime);
                        
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving rental history: {ex.Message}");
                return false;
            }
        }
    }
}