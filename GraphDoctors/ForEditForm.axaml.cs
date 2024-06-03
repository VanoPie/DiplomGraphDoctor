using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphDoctors
{
    public partial class ForEditForm : Window
    {
        private MySqlConnection _connection;
        private Doctor _selectedDoctor;
        private string _loggedInUser; // Сохраняем имя вошедшего пользователя
        private string _loggedUserRole;

        public ForEditForm(Doctor selectedDoctor, MySqlConnection connection, string loggedInUser, string loggedUserRole)
        {
            _loggedUserRole = loggedUserRole;
            _connection = connection;
            _selectedDoctor = selectedDoctor;
            _loggedInUser = loggedInUser; // Сохраняем имя вошедшего пользователя
            InitializeComponent();
            LoadDoctorData();
            LoadDepartments();
            LoadPersonnel();
        }

        private void LoadDoctorData()
        {
            IDTextBox.Text = _selectedDoctor.ID_дежурства.ToString();
            DoctorNameTextBox.Text = _selectedDoctor.ID_врача.ToString();
            SpecializationTextBox.Text = _selectedDoctor.Отделение.ToString();
            WorkDatePicker.SelectedDate = _selectedDoctor.Дата_дежурства.Date;
        }

        private void EditButton_Click(object? sender, RoutedEventArgs e)
        {
            string doctorName = DoctorNameTextBox.Text;
            string specialization = SpecializationTextBox.Text;

            if (string.IsNullOrEmpty(doctorName) || string.IsNullOrEmpty(specialization))
            {
                InfoWindow inf = new InfoWindow();
                inf.Show();
                inf.ExMess.Text = "Пожалуйста, заполните все поля!";
            }

            string queryString = "UPDATE Дежурства SET Дежурный_врач = @id_врача, Отделение = @отделение, Дата_дежурства = @дата_дежурства WHERE ID_дежурства = @id_дежурства";

            using (MySqlCommand cmd = new MySqlCommand(queryString, _connection))
            {
                cmd.Parameters.AddWithValue("@id_врача", GetDoctorId(DoctorNameTextBox.Text));
                cmd.Parameters.AddWithValue("@отделение", GetDepartmentId(SpecializationTextBox.Text));
                cmd.Parameters.AddWithValue("@дата_дежурства", WorkDatePicker.SelectedDate.GetValueOrDefault().Date);
                cmd.Parameters.AddWithValue("@id_дежурства", Convert.ToInt32(IDTextBox.Text));

                _connection.Open();
                cmd.ExecuteNonQuery();
                _connection.Close();
            }

            CalendarForm main = new CalendarForm(_loggedInUser, _loggedUserRole); // Передаем имя вошедшего пользователя
            main.Show();
            this.Close();
        }


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
                        DoctorList.Add(new Doctors { Id = doctorId, ФИО = $"{lastName} {firstName} {middleName}" });
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

            foreach (Doctors doctor in DoctorList)
            {
                if (doctor.ФИО.Equals(doctorName, StringComparison.OrdinalIgnoreCase))
                {
                    doctorId = doctor.Id;
                    break;
                }
            }

            return doctorId;
        }

        public class Department
        {
            public int Id { get; set; }
            public string Название { get; set; }
        }

        // Класс для хранения данных доктора
        public class Doctors
        {
            public int Id { get; set; }
            public string ФИО { get; set; }
        }

        // Списки для хранения данных отделений и персонала
        public List<Department> DepartmentList { get; set; } = new List<Department>();
        public List<Doctors> DoctorList { get; set; } = new List<Doctors>();

        private void BackButton_OnClick(object? sender, RoutedEventArgs e)
        {
            CalendarForm main = new CalendarForm(_loggedInUser, _loggedUserRole); // Передаем имя вошедшего пользователя
            main.Show();
            this.Close();
        }
    }
}