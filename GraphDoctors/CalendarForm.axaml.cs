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
using Avalonia.Threading;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Charts;
using OfficeOpenXml;
using System.Globalization;
using System.Data;
using System.Text;

namespace GraphDoctors;

public partial class CalendarForm : Window
{
    private List<Doctor> doc;
    private List<Personal> pers;
    private List<Otdel> otd;
    private Scheduler _scheduler;
    private MySqlConnection _connection;
    private string connectionString = "server=localhost;database=Diplom;port=3306;User Id=root;password=Qwerty_123456";
    private string loggedInUser;
    private string loggedUserRole;

    private string fullTable =
        "SELECT дежурства.ID_дежурства, персонал.Фамилия, персонал.Имя, персонал.Отчество, дежурства.Дата_дежурства, отделения.Название_отделения FROM дежурства JOIN персонал on персонал.ID_врача = дежурства.Дежурный_врач JOIN отделения ON отделения.ID_отделения = дежурства.Отделение";

    public CalendarForm(string username, string userRole)
    {
        _scheduler = new Scheduler();
        _connection = new MySqlConnection(connectionString);
        loggedInUser = username; // Сохраняем имя вошедшего пользователя
        loggedUserRole = userRole;
        InitializeComponent();
        CmbViewMovieFill();
        LoadUserProfile(); // Загружаем данные профиля пользователя
        RolesUsers();
    }

    public void ShowTable(string sql)
    {
        doc = new List<Doctor>();
        _connection = new MySqlConnection(connectionString);
        _connection.Open();
        MySqlCommand command = new MySqlCommand(sql, _connection);
        MySqlDataReader reader = command.ExecuteReader();
        while (reader.Read() && reader.HasRows)
        {
            var Docs = new Doctor()
            {
                ID_дежурства = reader.GetInt32("ID_дежурства"),
                ID_врача = reader.GetString("Фамилия") + " " + reader.GetString("Имя") + " " + reader.GetString("Отчество"),
                Отделение = reader.GetString("Название_отделения"),
                Дата_дежурства = reader.GetDateTime("Дата_дежурства"),
            };
            doc.Add(Docs);
        }
        _connection.Close();
        DataGrid.ItemsSource = doc;
        Comment.Text = "Список всех врачей на дежурствах: ";
    }

    public void ShowPersonalTable()
    {
        string table = "SELECT ID_врача, Фамилия, Имя, Отчество, Логин, Телефон, Название_специализации, Название_роли FROM персонал JOIN специализации ON Специализация = ID_специализации JOIN роли ON Роль = ID_роль;";
        pers = new List<Personal>();
        _connection = new MySqlConnection(connectionString);
        _connection.Open();
        MySqlCommand command = new MySqlCommand(table, _connection);
        MySqlDataReader reader = command.ExecuteReader();
        while (reader.Read() && reader.HasRows)
        {
            var Perses = new Personal()
            {
                ID_персонал = reader.GetInt32("ID_врача"),
                ФИО = reader.GetString("Фамилия") + " " + reader.GetString("Имя") + " " + reader.GetString("Отчество"),
                Логин = reader.GetString("Логин"),
                Телефон = reader.GetString("Телефон"),
                Специализация = reader.GetString("Название_специализации"),
                Роль = reader.GetString("Название_роли"),
            };
            pers.Add(Perses);
        }
        _connection.Close();
        PersonalDataGrid.ItemsSource = pers;
    }


    private void SearchDoc(object? sender, TextChangedEventArgs e)
    {
        ShowTable(fullTable);
        var filmName = doc;
        filmName = filmName.Where(x => x.ID_врача.Contains(Search_Doctor.Text))
            .ToList(); //фильтруем список по введенному значению в поле поиска
        DataGrid.ItemsSource = filmName; //обновление источника данных DataGrid отфильтрованным списком
        Comment.Text = "Список всех врачей на дежурствах: ";
    }
    private void OtdFilter_OnClick(object? sender, SelectionChangedEventArgs e)
    {
        ShowTable(fullTable);
        var ComboBox = (ComboBox)sender;
        var currentOtd = ComboBox.SelectedItem as Otdel;
        var filteredArtist = doc
            .Where(x => x.Отделение == currentOtd.Название_отделения)
            .ToList();
        DataGrid.ItemsSource = filteredArtist;
        Comment.Text = "Список дежурных по отделениям: ";
    }
    public void CmbViewMovieFill()
    {
        otd = new List<Otdel>();
        _connection = new MySqlConnection(connectionString);
        _connection.Open();
        MySqlCommand
            command = new MySqlCommand("select * from отделения", _connection); //создание команды для выполнения запроса
        MySqlDataReader reader = command.ExecuteReader(); //выполнение запроса и получение результата
        while (reader.Read() && reader.HasRows) //чтение результатов запроса
        {
            var Mov = new Otdel()
            {
                //получение результатов столбцов по классу 
                ID_отделения = reader.GetInt32("ID_отделения"),
                Название_отделения = reader.GetString("Название_отделения")
            };
            otd.Add(Mov);
        }

        _connection.Close();
        var GenreCMB = this.Find<ComboBox>("CmbOtdelen");
        GenreCMB.ItemsSource = otd.DistinctBy(x => x.Название_отделения);
    }
    private void AddButton_Click(object? sender, RoutedEventArgs e)
    {
        EditorForm addDutyForm = new EditorForm(_connection, loggedInUser, loggedUserRole); // Создаем новую форму для добавления данных
        addDutyForm.Show(); // Открываем новую форму 
        this.Close();
    }
    private void ToAutorizeForm_Click(object? sender, RoutedEventArgs e)
    {
        AutorizeForm auto = new AutorizeForm();
        auto.Show();
        this.Close();
    }

    private void ResetPass_OnClick(object? sender, RoutedEventArgs e)
    {
        ChangePassForPersonal reset = new ChangePassForPersonal(loggedInUser, loggedUserRole);
        reset.Show();
        this.Close();
    }

    private void ResetLogin_OnClick(object? sender, RoutedEventArgs e)
    {
        ChangeLoginForPersonal reset = new ChangeLoginForPersonal(loggedInUser, loggedUserRole);
        reset.Show();
        this.Close();
    }

    private void ShowDiagram_OnClick(object? sender, RoutedEventArgs e)
    {
        DiagramForm diag = new DiagramForm();
        diag.Title = "Итоги за " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.Month) + " " + DateTime.Now.Year + "г.";
        diag.Show();
    }

    private void DeleteData_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            Doctor bck = DataGrid.SelectedItem as Doctor;
            if (bck == null)
            {
                return;
            }

            _connection = new MySqlConnection(connectionString);
            _connection.Open();
            string sql = "DELETE FROM Дежурства WHERE ID_дежурства = " + bck.ID_дежурства;
            MySqlCommand cmd = new MySqlCommand(sql, _connection);
            cmd.ExecuteNonQuery();
            _connection.Close();
            InfoWindow del = new InfoWindow();
            del.Titl.Text = "Удаление записи прошло успешно";
            del.ExMess.Text = "Выбранная запись дежурства была успешно удалена.";
            del.Show();
            ShowTable(fullTable);
        }
        catch (Exception ex)
        {
            InfoWindow del = new InfoWindow();
            del.Titl.Text = "Ошибка удаления";
            del.ExMess.Text = "Во время удаления записи возникла ошибка. Проверьте существует ли данная запись и повторите попытку.\n" + ex.Message;
            del.Show();
        }
    }

    private void Calendar_SelectedDateChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selectedDate = Calendar.SelectedDate.Value.Date;

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            DataGrid.ItemsSource = _scheduler.GetDoctorsForDate(selectedDate, _connection);
        });
        Comment.Text = "Дежурные в выбранный день: ";
    }

    private void ProfileCalendar_SelectedDateChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selectedDate = ProfileCalendar.SelectedDate.Value.Date;

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            ProfileDataGrid.ItemsSource = _scheduler.GetDoctorsForUser(loggedInUser, selectedDate, _connection);
        });
    }


    private void EditButton_OnClick(object? sender, RoutedEventArgs e)
    {
        // Получаем выбранный элемент из DataGrid
        var selectedDoctor = DataGrid.SelectedItem as Doctor;

        if (selectedDoctor != null)
        {
            // Создаем новую форму для редактирования данных
            ForEditForm editDutyForm = new ForEditForm(selectedDoctor, _connection, loggedInUser, loggedUserRole);

            // Открываем новую форму
            editDutyForm.Show();
            this.Close();
        }
    }

    private void ExitButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Environment.Exit(0);
    }

    private void DocumentButton_OnClick(object? sender, RoutedEventArgs e)
    {
        string outputFile = @"C:\Users\VanoP\Desktop\otchet.xlsx";
        string query = "SELECT дежурства.ID_дежурства, персонал.Фамилия, дежурства.Дата_дежурства, отделения.Название_отделения FROM дежурства JOIN персонал on персонал.ID_врача = дежурства.Дежурный_врач JOIN отделения ON отделения.ID_отделения = дежурства.Отделение WHERE MONTH(дежурства.Дата_дежурства) = MONTH(CURRENT_DATE()) AND YEAR(дежурства.Дата_дежурства) = YEAR(CURRENT_DATE()) ORDER BY Дата_дежурства;";
        MySqlCommand command = new MySqlCommand(query, _connection);
        _connection.Open();
        MySqlDataReader dataReader = command.ExecuteReader();
        using (ExcelPackage excelPackage = new ExcelPackage())
        {
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Дежурства");
            int row = 1;

            for (int i = 1; i <= dataReader.FieldCount; i++)
            {
                worksheet.Cells[row, i].Value = dataReader.GetName(i - 1);
            }

            while (dataReader.Read())
            {
                row++;
                for (int i = 1; i <= dataReader.FieldCount; i++)
                {
                    if (dataReader.GetName(i - 1) == "Дата_дежурства" && dataReader[i - 1] != DBNull.Value)
                    {
                        // Преобразование даты в строку с заданным форматом
                        worksheet.Cells[row, i].Value = ((DateTime)dataReader[i - 1]).ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        worksheet.Cells[row, i].Value = dataReader[i - 1];
                    }
                }
            }

            excelPackage.SaveAs(new FileInfo(outputFile));
        }
        dataReader.Close();
        _connection.Close();
        Console.WriteLine("Данные успешно экспортированы в Excel файл.");
    }

    private void RolesUsers()
    {
        switch (loggedUserRole)
        {
            case "Врач":
                this.AddButton.IsVisible = false;
                this.EditButton.IsVisible = false;
                this.DeleteButton.IsVisible = false;
                this.DocumentButton.IsVisible = false;
                this.ShowDiagram.IsVisible = false;
                DataGrid.Columns[0].IsVisible = false; 
                ProfileDataGrid.Columns[0].IsVisible = false;
                break;

            case "Главный врач":
                this.AddButton.IsVisible = false;
                this.EditButton.IsVisible = false;
                this.DeleteButton.IsVisible = false;
                this.Title = "Список дежурств (Главный врач)";
                DataGrid.Columns[0].IsVisible = false;
                ProfileDataGrid.Columns[0].IsVisible = false;
                break;

            case "Администратор":
                this.DocumentButton.IsVisible = false;
                this.ShowDiagram.IsVisible = false;
                this.PersonalPassChangeButton.IsVisible = true;
                this.PersonalLoginChangeButton.IsVisible = true;
                this.ProfileCalendar.IsVisible = false;
                this.ProfileDataGrid.IsVisible = false;
                this.ShiftsCountLabel.IsVisible = false;
                this.DutyDatesLabel.IsVisible = false;
                this.Title = "Список дежурств (Администратор)";
                this.PersonalDataGrid.IsVisible = true;
                ShowPersonalTable();
                break;

            case "Отдел кадров":
                this.AddButton.IsVisible = false;
                this.EditButton.IsVisible = false;
                this.DeleteButton.IsVisible = false;
                this.ProfileTab.IsVisible = false;
                this.GeneralTab.Header = null;
                this.Title = "Список дежурств (Отдел кадров)";
                break;
        }
    }

    private void LoadUserProfile()
    {
        string getUserShifts = "SELECT COUNT(ID_дежурства) AS 'Количество смен', персонал.Фамилия, персонал.Имя, персонал.Отчество, COALESCE(MAX(Дата_дежурства), 'Нет данных') AS 'Последняя дата дежурства' FROM персонал LEFT JOIN дежурства ON персонал.ID_врача = дежурства.Дежурный_врач WHERE персонал.Логин = @username GROUP BY персонал.Фамилия, персонал.Имя, персонал.Отчество;";

        using (MySqlCommand cmd = new MySqlCommand(getUserShifts, _connection))
        {
            cmd.Parameters.AddWithValue("@username", loggedInUser);

            _connection.Open();
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    int shiftCount = reader.GetInt32("Количество смен");
                    string fullName = reader.GetString("Фамилия") + " " + reader.GetString("Имя") + " " + reader.GetString("Отчество");
                    ProfileTab.Header = $"Профиль: {fullName}"; // Обновляем заголовок вкладки профиля
                    ShiftsCountLabel.Text = $"Количество смен: {shiftCount}"; // Обновляем информацию о количестве смен
                    FullNameLabel.Text = $"ФИО: {fullName}"; // Обновляем информацию о ФИО

                    string lastDutyDate = reader.GetString("Последняя дата дежурства");
                    if (lastDutyDate == "Нет данных")
                    {
                        DutyDatesLabel.Text = "Последняя дата дежурства: нет данных";
                    }
                    else
                    {
                        DutyDatesLabel.Text = "Последняя дата дежурства: " + DateTime.Parse(lastDutyDate).ToShortDateString();
                    }
                }
            }
            _connection.Close();
        }
    }
}


// Класс для управления списком дежурств
public class Scheduler
{
    public IEnumerable<Doctor> GetDoctorsForDate(DateTime date, MySqlConnection connection)
    {
        List<Doctor> doctors = new List<Doctor>();
        string queryString =
            "SELECT дежурства.ID_дежурства, персонал.Фамилия, персонал.Имя, персонал.Отчество, дежурства.Дата_дежурства, отделения.Название_отделения FROM дежурства JOIN персонал on персонал.ID_врача = дежурства.Дежурный_врач JOIN отделения ON отделения.ID_отделения = дежурства.Отделение WHERE Дата_дежурства = @selectedDate";

        using (MySqlCommand cmd = new MySqlCommand(queryString, connection))
        {
            cmd.Parameters.AddWithValue("@selectedDate", date);

            connection.Open();
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Doctor doctor = new Doctor
                    {
                        ID_дежурства = reader.GetInt32("ID_дежурства"),
                        ID_врача = reader.GetString("Фамилия") + " " + reader.GetString("Имя") + " " + reader.GetString("Отчество"),
                        Отделение = reader.GetString("Название_отделения"),
                        Дата_дежурства = reader.GetDateTime("Дата_дежурства")
                    };
                    doctors.Add(doctor);
                }
            }
            connection.Close();
        }
        return doctors;
    }


    public IEnumerable<Doctor> GetDoctorsForUser(string username, DateTime date, MySqlConnection connection)
    {
        List<Doctor> doctors = new List<Doctor>();
        string queryString =
            "SELECT дежурства.ID_дежурства, персонал.Фамилия, персонал.Имя, персонал.Отчество, дежурства.Дата_дежурства, отделения.Название_отделения FROM дежурства JOIN персонал on персонал.ID_врача = дежурства.Дежурный_врач JOIN отделения ON отделения.ID_отделения = дежурства.Отделение WHERE персонал.Логин = @username AND Дата_дежурства = @selectedDate";

        using (MySqlCommand cmd = new MySqlCommand(queryString, connection))
        {
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@selectedDate", date);

            connection.Open();
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Doctor doctor = new Doctor
                    {
                        ID_дежурства = reader.GetInt32("ID_дежурства"),
                        ID_врача = reader.GetString("Фамилия") + " " + reader.GetString("Имя") + " " + reader.GetString("Отчество"),
                        Отделение = reader.GetString("Название_отделения"),
                        Дата_дежурства = reader.GetDateTime("Дата_дежурства")
                    };
                    doctors.Add(doctor);
                }
            }
            connection.Close();
        }
        return doctors;
    }
}

