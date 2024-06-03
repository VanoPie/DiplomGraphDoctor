using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Avalonia.Interactivity;
using MySql.Data.MySqlClient;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Avalonia.Data;
using Avalonia.Threading;

namespace GraphDoctors
{
    public partial class EditorForm : Window
    {
        private readonly MySqlConnection _connection;

        private string _loggedInUser;
        private string _loggedUserRole;

        public EditorForm(MySqlConnection connection, string loggedInUser, string loggedUserRole)
        {
            InitializeComponent();
            _loggedInUser = loggedInUser;
            _connection = connection;
            _loggedUserRole = loggedUserRole;
            WorkDatePicker.SelectedDate = DateTime.Today;

            // Загрузка данных отделений и персонала
            LoadDepartments();
            LoadPersonnel();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string doctorName = DoctorNameTextBox.Text;
            string specialization = SpecializationTextBox.Text;
            DateTime workDate = WorkDatePicker.SelectedDate.GetValueOrDefault().Date;

            if (!string.IsNullOrEmpty(doctorName) && !string.IsNullOrEmpty(specialization))
            {
                int departmentId = GetDepartmentId(specialization);

                if (departmentId > 0)
                {
                    int doctorId = GetDoctorId(doctorName);

                    if (doctorId > 0)
                    {
                        AddDoctorToDatabase(doctorId, departmentId, workDate);
                        CalendarForm main = new CalendarForm(_loggedInUser, _loggedUserRole);
                        main.Show();
                        Close();
                    }
                    else
                    {
                        InfoWindow inf = new InfoWindow();
                        inf.Show();
                        inf.ExMess.Text = "Доктор не найден. Пожалуйста, проверьте введенное имя.";
                    }
                }
                else
                {
                    InfoWindow inf = new InfoWindow();
                    inf.Show();
                    inf.ExMess.Text = "Отделение не найдено. Пожалуйста, проверьте введенное название.";
                }
            }
            else
            {
                InfoWindow inf = new InfoWindow();
                inf.Show();
                inf.ExMess.Text = "Пожалуйста, заполните все поля!";
            }
        }

        private void BackButton_OnClick(object? sender, RoutedEventArgs e)
        {
            CalendarForm main = new CalendarForm(_loggedInUser, _loggedUserRole);
            main.Show();
            this.Close();
        }

        private void AddDoctorToDatabase(int doctorId, int departmentId, DateTime workDate)
        {
            string queryString = "INSERT INTO Дежурства (Дежурный_врач, Отделение, Дата_дежурства) VALUES (@doctorId, @departmentId, @workDate)";

            using (MySqlCommand cmd = new MySqlCommand(queryString, _connection))
            {
                cmd.Parameters.AddWithValue("@doctorId", doctorId);
                cmd.Parameters.AddWithValue("@departmentId", departmentId);
                cmd.Parameters.AddWithValue("@workDate", workDate);

                _connection.Open();
                cmd.ExecuteNonQuery();
                _connection.Close();
            }
        }

        // Метод для загрузки данных отделений
        private void LoadDepartments()
        {
            _connection.Open();
            string queryString = "SELECT ID_отделения, Название_отделения FROM Отделения";

            using (MySqlCommand cmd = new MySqlCommand(queryString, _connection))
            {
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int departmentId = reader.GetInt32("ID_отделения");
                        string departmentName = reader.GetString("Название_отделения");
                        DepartmentList.Add(new Department { Id = departmentId, Название = departmentName });
                    }
                }
            }
            _connection.Close();
        }

        // Метод для загрузки данных персонала
        private void LoadPersonnel()
        {
            _connection.Open();
            string queryString = "SELECT ID_врача, Фамилия, Имя, Отчество FROM Персонал";

            using (MySqlCommand cmd = new MySqlCommand(queryString, _connection))
            {
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int doctorId = reader.GetInt32("ID_врача");
                        string lastName = reader.GetString("Фамилия");
                        string firstName = reader.GetString("Имя");
                        string middleName = reader.GetString("Отчество");
                        DoctorList.Add(new Doctor { Id = doctorId, ФИО = $"{lastName} {firstName} {middleName}" });
                    }
                }
            }
            _connection.Close();
        }

        // Метод для получения ID отделения по названию
        private int GetDepartmentId(string departmentName)
        {
            int departmentId = 0;

            foreach (Department department in DepartmentList)
            {
                if (department.Название.Equals(departmentName, StringComparison.OrdinalIgnoreCase))
                {
                    departmentId = department.Id;
                    break;
                }
            }

            return departmentId;
        }

        // Метод для получения ID доктора по имени
        private int GetDoctorId(string doctorName)
        {
            int doctorId = 0;

            foreach (Doctor doctor in DoctorList)
            {
                if (doctor.ФИО.Equals(doctorName, StringComparison.OrdinalIgnoreCase))
                {
                    doctorId = doctor.Id;
                    break;
                }
            }

            return doctorId;
        }

        // Класс для хранения данных отделения
        public class Department
        {
            public int Id { get; set; }
            public string Название { get; set; }
        }

        // Класс для хранения данных доктора
        public class Doctor
        {
            public int Id { get; set; }
            public string ФИО { get; set; }
        }

        // Списки для хранения данных отделений и персонала
        public List<Department> DepartmentList { get; set; } = new List<Department>();
        public List<Doctor> DoctorList { get; set; } = new List<Doctor>();
    }
}
