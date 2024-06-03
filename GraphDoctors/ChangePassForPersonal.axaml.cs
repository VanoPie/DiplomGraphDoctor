using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MySql.Data.MySqlClient;
using System;
using System.Linq;

namespace GraphDoctors;

public partial class ChangePassForPersonal : Window
{
    private MySqlConnection connection;
    private MySqlCommand command;
    private string _loggedInUser;
    private string _loggedUserRole;

    public ChangePassForPersonal(string loggedInUser, string loggedUserRole)
    {
        InitializeComponent();
        connection = new MySqlConnection("server=localhost;database=diplom;port=3306;User Id=root;password=Qwerty_123456");
        _loggedInUser = loggedInUser;
        _loggedUserRole = loggedUserRole;
    }

    private void ResetPassword_OnClick(object sender, RoutedEventArgs e)
    {
        string login = Login.Text;
        string newPassword = Password.Text;

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(newPassword))
        {
            InfoWindow inf = new InfoWindow();
            inf.Show();
            inf.ExMess.Text = "Пожалуйста, заполните все поля!";
            return;
        }

        string query = "UPDATE Персонал SET Пароль = SHA2(@newPassword, 256) WHERE Логин = @login";

        try
        {
            connection.Open();
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@login", login);
            command.Parameters.AddWithValue("@newPassword", newPassword);
            int rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
                InfoWindow inf = new InfoWindow();
                inf.Show();
                inf.Titl.Text = "Успех";
                inf.ExMess.Text = "Пароль успешно изменен!";
            }
            else
            {
                InfoWindow inf = new InfoWindow();
                inf.Show();
                inf.ExMess.Text = "Пользователь с таким логином не найден. Убедитесь в правильности введенных данных и повторите попытку.";
            }
        }
        catch (MySqlException ex)
        {
            InfoWindow inf = new InfoWindow();
            inf.Show();
            inf.ExMess.Text = "Ошибка при изменении пароля: " + ex.Message;
        }
        finally
        {
            connection.Close();
        }
    }

    private void Back_OnClick(object sender, RoutedEventArgs e)
    {
        CalendarForm main = new CalendarForm(_loggedInUser, _loggedUserRole); // Передаем имя вошедшего пользователя
        main.Show();
        this.Close();
    }

}