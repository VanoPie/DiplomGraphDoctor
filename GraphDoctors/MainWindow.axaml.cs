using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Linq;
using Avalonia.Interactivity;
using MySql.Data.MySqlClient;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Threading;

namespace GraphDoctors;

public partial class MainWindow : Window
{
    public MainWindow()
{
    InitializeComponent();
    var currentDate = DateTime.Now;
    var selectedMonth = currentDate.Month;
    var selectedYear = currentDate.Year;
    var daysInMonth = DateTime.DaysInMonth(selectedYear, selectedMonth);
    
    var grid = new Grid();
    grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // Для меток дней недели
    // Добавляем строку для меток дней недели
    grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // Для меток дней недели

    // Добавляем строку для меток дат
    grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // Для меток дат
    
    grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // Для комбобоксов
    for (int i = 0; i < daysInMonth; i++)
    {
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
    }
    
    var addButton = new Button
    {
        Content = "Добавить",
        Margin = new Thickness(5),
        HorizontalAlignment = HorizontalAlignment.Left
    };
    Grid.SetColumn(addButton, 0);
    Grid.SetRow(addButton, 1);
    grid.Children.Add(addButton);
    
    addButton.Click += (sender, args) =>
    {
        var newRow = new Grid();
        
        newRow.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // Для комбобоксов
        for (int i = 0; i < daysInMonth; i++)
        {
            newRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        }

        for (int i = 0; i < daysInMonth; i++)
        {
            var newComboBox = new ComboBox
            {
                ItemsSource = new List<string> { "Выходной", "Работа" },
                Margin = new Thickness(5, 30, 5, 0),
                Width = 65,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            newComboBox.SelectionChanged += (sender, args) =>
            {
                var selectedComboBox = (ComboBox)sender;
                var selectedValue = selectedComboBox.SelectedItem?.ToString();
                var backgroundColor = selectedValue switch
                {
                    "Выходной" => Brushes.Red,
                    "Работа" => Brushes.Green,
                    _ => Brushes.Transparent
                };
                selectedComboBox.Background = backgroundColor;
            };
            var newTextBox = new TextBox
            {
                Margin = new Thickness(5, 30, 5, 0),
                Width = 50,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            Grid.SetColumn(newComboBox, i);
            newRow.Children.Add(newTextBox); // Добавляем новый элемент TextBox вместо ComboBox
            newRow.Children.Add(newComboBox);
        }

        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        grid.Children.Add(newRow);
        Grid.SetRow(newRow, grid.RowDefinitions.Count - 1);
    };
   
    // Добавляем метки дней недели над таблицей
    for (int i = 0; i < daysInMonth; i++)
    {
        // Добавляем день недели с укороченным форматом
        var dayOfWeekLabel = new Label
        {
            Content = new DateTime(selectedYear, selectedMonth, i + 1)
                .ToString("ddd"), // Укороченный формат названия дня недели
            HorizontalAlignment = HorizontalAlignment.Center, // Выравнивание по центру
            Margin = new Thickness(5)
        };

        Grid.SetColumn(dayOfWeekLabel, i);
        Grid.SetRow(dayOfWeekLabel, 2);
        grid.Children.Add(dayOfWeekLabel);
    }

    // Добавляем метки дат над таблицей
    for (int i = 0; i < daysInMonth; i++)
    {
        // Добавляем дату
        var dateLabel = new Label
        {
            Content = new DateTime(selectedYear, selectedMonth, i + 1)
                .ToString("dd.MM"), // Формат даты "день.месяц"
            HorizontalAlignment = HorizontalAlignment.Center, // Выравнивание по центру

        };

        Grid.SetColumn(dateLabel, i);
        Grid.SetRow(dateLabel, 3); // Ряд для дат
        grid.Children.Add(dateLabel);
    }

    var scrollViewer = new ScrollViewer
    {
        HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        Content = grid
    };
    Content = scrollViewer;
}

}