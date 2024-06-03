using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Layout;
using Avalonia.Media;

namespace GraphDoctors;

public partial class InfoWindow : Window
{
    public InfoWindow()
    {
        InitializeComponent();
    }
    
    private void Ok_Exit(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }
}