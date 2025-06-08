using System.Linq;
using System.Windows;

namespace WpfApp1.Validators
{
    public static class InputValidator
    {
        public static bool ValidateRegistration(string fullName, string phone, string email, string password, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(fullName) || fullName.Length < 3)
            {
                errorMessage = "Имя должно быть не короче 3 символов.";
                return false;
            }
            if (!ValidatePhoneNumber(phone, out errorMessage))
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(email) || email.Length < 5 || !email.Contains("@"))
            {
                errorMessage = "Email должен быть валидным и не короче 5 символов.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                errorMessage = "Пароль должен быть не короче 6 символов.";
                return false;
            }
            errorMessage = "";
            return true;
        }

        public static bool ValidateLogin(string email, string password, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(email) || email.Length < 5 || !email.Contains("@"))
            {
                errorMessage = "Email должен быть валидным и не короче 5 символов.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                errorMessage = "Пароль должен быть не короче 6 символов.";
                return false;
            }
            errorMessage = "";
            return true;
        }

        public static bool ValidatePhoneNumber(string phone, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                errorMessage = "Номер телефона не может быть пустым.";
                return false;
            }
            if (!phone.StartsWith("+"))
            {
                errorMessage = "Номер телефона должен начинаться с '+'";
                return false;
            }
            string digitsPart = phone.Substring(1);
            if (!digitsPart.All(char.IsDigit))
            {
                errorMessage = "Номер телефона должен содержать только цифры после '+'";
                return false;
            }
            if (digitsPart.Length < 10)
            {
                errorMessage = "Номер телефона должен содержать не менее 10 цифр после '+'";
                return false;
            }
            if (digitsPart.Length > 15)
            {
                errorMessage = "Номер телефона не должен содержать больше 15 цифр после '+'";
                return false;
            }
            errorMessage = "";
            return true;
        }

        public static bool ValidateFieldLength(string fieldData, int minLength, string fieldName, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(fieldData) || fieldData.Length < minLength)
            {
                errorMessage = $"{fieldName} должно быть не короче {minLength} символов.";
                return false;
            }
            errorMessage = "";
            return true;
        }
    }
} 