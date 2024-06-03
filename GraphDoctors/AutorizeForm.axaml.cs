using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Avalonia.Markup.Xaml;
using System.Linq;
using Avalonia.Interactivity;
using MySql.Data.MySqlClient;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Avalonia.Data;
using System.Security.Cryptography;

namespace GraphDoctors;

public partial class AutorizeForm : Window
{
    public AutorizeForm()
    {
        InitializeComponent();
    }
    
    string connectionString = "server=localhost;database=diplom;port=3306;User Id=root;password=Qwerty_123456";
    public void Authorization(object sender, RoutedEventArgs e)
    {
        try
        {
            string username = Login.Text;
            string password = Password.Text;
            string userRole;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                InfoWindow inf = new InfoWindow();
                inf.Show();
                inf.ExMess.Text = "Пожалуйста, заполните все поля!";
            }

            if (IsUserValid(username, password, out userRole))
            {
                CalendarForm frm = new CalendarForm(username, userRole); 
                Hide();
                frm.Show();
            }
            else
            {
                InfoWindow inf = new InfoWindow();
                inf.Show();
                inf.ExMess.Text = "Неверный логин или пароль пользователя.\nПроверьте правильность введенных данных и повторите попытку.";
            }
        }
        catch (Exception ex)
        {
            InfoWindow inf = new InfoWindow();
            inf.Show();
            inf.ExMess.Text = ex.Message;
        }
    }

    private bool IsUserValid(string username, string password, out string userRole)
    {
        bool isValid = false;
        userRole = null;

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            string query = "SELECT Название_роли FROM Персонал JOIN Роли ON Роль = ID_роль WHERE Логин = @Username AND Пароль = SHA2(@Password, 256)";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@Password", password);

                connection.Open();

                object result = command.ExecuteScalar();
                if (result != null)
                {
                    userRole = result.ToString();
                    isValid = true;
                }
            }
        }

        return isValid;
    }

    public void Exit_PR(object sender, RoutedEventArgs e)
    {
        Environment.Exit(0);
    }
}