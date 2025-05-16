using System;
using System.Windows;
using System.Windows.Markup;

namespace WpfApp1
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Инициализируем базу данных при старте приложения
            Data.DatabaseHelper.InitializeDatabase();
        }

        // ... existing code ...
    }
}