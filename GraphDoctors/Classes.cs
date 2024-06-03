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
using Avalonia.Data;
using Avalonia.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;
using GraphDoctors;

namespace GraphDoctors;

public class Doctor
{
    public int ID_дежурства { get; set; }
    public string ID_врача { get; set; }
    public string Отделение { get; set; }
    public DateTime Дата_дежурства { get; set; }
}

public class Otdel
{
    public int ID_отделения { get; set; }
    public string Название_отделения { get; set; }
}

public class Personal
{
    public int ID_персонал{ get; set; }
    public string ФИО { get; set; }
    public string Логин { get; set; }
    public string Пароль { get; set; }
    public string Телефон { get; set; }
    public string Специализация { get; set; }
    public string Роль { get; set; }

}
