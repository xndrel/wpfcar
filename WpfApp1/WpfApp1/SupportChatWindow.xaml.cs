using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Windows.Input; // Add for KeyDown event

namespace WpfApp1
{
    public partial class SupportChatWindow : Window
    {
        private string currentChatId;
        private Dictionary<string, List<ChatMessage>> chatMessages = new Dictionary<string, List<ChatMessage>>();
        private bool isAdmin;

        public class ChatMessage
        {
            public int MessageID { get; set; }
            public string SenderName { get; set; }
            public string Text { get; set; }
            public DateTime SentTime { get; set; }
            public bool IsAdminMessage { get; set; }

            public override string ToString()
            {
                return $"[{SentTime.ToShortTimeString()}] {SenderName}: {Text}";
            }
        }

        // Конструктор для открытия конкретного чата (используется из админ-панели)
        public SupportChatWindow(string chatId) : this()
        {
            this.Title = "Чат поддержки - " + chatId;
            currentChatId = chatId;
            isAdmin = MainWindow.CurrentUser.IsAdmin;
            
            // Если администратор, проверяем что список чатов загружен
            if (isAdmin && ChatsListBox.Items.Count == 0)
            {
                LoadActiveChats();
            }
            
            LoadChatMessages(chatId);
            
            // Получаем имя пользователя для заголовка
            string userName = Data.DatabaseHelper.GetChatUserName(chatId);
            if (!string.IsNullOrEmpty(userName))
            {
                SelectedChatTitle.Text = $"Чат с {userName} (ID: {chatId})";
            }
            else
            {
                SelectedChatTitle.Text = $"Чат #{chatId}";
            }
            
            // Если администратор, выбираем чат из списка
            if (isAdmin)
            {
                bool found = false;
                for (int i = 0; i < ChatsListBox.Items.Count; i++)
                {
                    string item = ChatsListBox.Items[i].ToString();
                    if (item.Contains($"ID: {chatId}"))
                    {
                        ChatsListBox.SelectedIndex = i;
                        found = true;
                        break;
                    }
                }
                
                // Если чат не найден в списке, добавляем его
                if (!found)
                {
                    string chatDisplay = $"Чат (ID: {chatId})";
                    string userName2 = Data.DatabaseHelper.GetChatUserName(chatId);
                    if (!string.IsNullOrEmpty(userName2))
                    {
                        chatDisplay = $"{userName2} (ID: {chatId})";
                    }
                    
                    ChatsListBox.Items.Add(chatDisplay);
                    ChatsListBox.SelectedItem = chatDisplay;
                }
            }
        }

        // Основной конструктор
        public SupportChatWindow()
        {
            InitializeComponent();
            isAdmin = MainWindow.CurrentUser.IsAdmin;
            
            if (isAdmin)
            {
                // Если администратор, загружаем список всех открытых чатов
                LoadActiveChats();
            }
            else
            {
                // Если обычный пользователь, создаем или находим его чат
                InitializeUserChat();
            }
        }
        
        // Инициализация чата для обычного пользователя
        private void InitializeUserChat()
        {
            int userId = MainWindow.CurrentUser.UserID;
            string chatId = Data.DatabaseHelper.GetUserChatId(userId);
            
            if (string.IsNullOrEmpty(chatId))
            {
                // Создаем новый чат для пользователя
                chatId = Data.DatabaseHelper.CreateNewChat(userId);
            }
            
            if (!string.IsNullOrEmpty(chatId))
            {
                currentChatId = chatId;
                SelectedChatTitle.Text = $"Чат поддержки - {MainWindow.CurrentUser.FullName}";
                LoadChatMessages(chatId);
            }
            
            // Скрываем панель списка чатов для обычного пользователя
            Grid mainGrid = (Grid)this.Content;
            Grid.SetColumnSpan(mainGrid.Children[1], 2);
            mainGrid.Children[0].Visibility = Visibility.Collapsed;
        }
        
        // Загрузка списка активных чатов (для администратора)
        private void LoadActiveChats()
        {
            ChatsListBox.Items.Clear();
            
            try
            {
                // Получаем все чаты с именами пользователей
                var chats = Data.DatabaseHelper.GetSupportChatsWithUserNames();
                
                // Проверяем, что список не пустой
                if (chats.Count == 0)
                {
                    // Если список пуст, создаем тестовые чаты
                    chats.Add("Иванов Иван (ID: 1)");
                    chats.Add("Петров Петр (ID: 2)");
                    chats.Add("Сидоров Сидор (ID: 3)");
                    
                    // Добавляем информационное сообщение о том, что это тестовые чаты
                    MessageBox.Show("Не найдено активных чатов. Отображаются тестовые чаты.", 
                                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                
                // Добавляем чаты в список
                foreach (var chat in chats)
                {
                    ChatsListBox.Items.Add(chat);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке чатов: {ex.Message}");
                
                // В случае ошибки добавляем тестовые чаты
                ChatsListBox.Items.Add("Чат поддержки (ID: 1)");
                ChatsListBox.Items.Add("Чат поддержки (ID: 2)");
                ChatsListBox.Items.Add("Чат поддержки (ID: 3)");
            }
            
            // Если есть хотя бы один чат в списке, выбираем первый по умолчанию
            if (ChatsListBox.Items.Count > 0)
            {
                ChatsListBox.SelectedIndex = 0;
            }
            else
            {
                // Если список пуст, добавляем информационное сообщение
                ChatsListBox.Items.Add("Нет активных чатов");
            }
        }
        
        // Загрузка сообщений для выбранного чата
        private void LoadChatMessages(string chatId)
        {
            if (string.IsNullOrEmpty(chatId)) return;
            
            currentChatId = chatId;
            
            // Загружаем сообщения этого чата
            var messages = Data.DatabaseHelper.GetChatMessages(chatId);
            
            if (!chatMessages.ContainsKey(chatId))
            {
                chatMessages[chatId] = new List<ChatMessage>();
            }
            
            chatMessages[chatId].Clear();
            MessagesListBox.Items.Clear();
            
            foreach (var msg in messages)
            {
                chatMessages[chatId].Add(msg);
                MessagesListBox.Items.Add(msg);
            }
            
            // Прокручиваем список к последнему сообщению
            if (MessagesListBox.Items.Count > 0)
            {
                MessagesListBox.ScrollIntoView(MessagesListBox.Items[MessagesListBox.Items.Count - 1]);
            }
        }

        // Обработчик выбора чата из списка
        private void ChatsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChatsListBox.SelectedItem != null)
            {
                string selectedItem = ChatsListBox.SelectedItem.ToString();
                
                // Используем регулярное выражение для извлечения ID из строки вида "Имя пользователя (ID: 123)"
                Match match = Regex.Match(selectedItem, @"ID: (\d+)");
                if (match.Success)
                {
                    string chatId = match.Groups[1].Value;
                    LoadChatMessages(chatId);
                    
                    // Устанавливаем заголовок
                    SelectedChatTitle.Text = selectedItem;
                }
                else
                {
                    // Если формат не соответствует ожидаемому, выводим предупреждение
                    MessageBox.Show("Не удалось определить ID чата. Пожалуйста, выберите другой чат.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        // Отправка сообщения
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = MessageTextBox.Text.Trim();
            if (string.IsNullOrEmpty(msg) || string.IsNullOrEmpty(currentChatId))
            {
                return;
            }
            
            int senderId = MainWindow.CurrentUser.UserID;
            string senderName = MainWindow.CurrentUser.FullName;
            
            // Создаем новое сообщение
            ChatMessage message = new ChatMessage
            {
                SenderName = senderName,
                Text = msg,
                SentTime = DateTime.Now,
                IsAdminMessage = isAdmin
            };
            
            // Сохраняем в базу
            bool success = Data.DatabaseHelper.AddChatMessage(currentChatId, senderId, msg);
            
            if (success)
            {
                if (!chatMessages.ContainsKey(currentChatId))
                {
                    chatMessages[currentChatId] = new List<ChatMessage>();
                }
                
                chatMessages[currentChatId].Add(message);
                MessagesListBox.Items.Add(message);
                MessagesListBox.ScrollIntoView(message);
                MessageTextBox.Clear();
            }
        }
        
        // Обработчик нажатия клавиши в поле ввода сообщения
        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendButton_Click(sender, e);
                e.Handled = true;
            }
        }
        
        // Обработчик нажатия кнопки выхода
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Открываем главное меню
            MainScreen mainScreen = new MainScreen();
            mainScreen.Show();
            
            // Закрываем окно чата
            this.Close();
        }
    }
}