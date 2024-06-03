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
using ScottPlot.Avalonia;
using MySql.Data.MySqlClient;
using ScottPlot.Plottables;
using DocumentFormat.OpenXml.Drawing;
using DynamicData;

namespace GraphDoctors;

public partial class DiagramForm : Window
{
    public DiagramForm()
    {

        InitializeComponent();

        using (MySqlConnection conn = new MySqlConnection("server=localhost;database=Diplom;port=3306;User Id=root;password=Qwerty_123456"))
        {
            conn.Open();

            string query = "SELECT п.Фамилия, п.Имя, COUNT(д.ID_дежурства) AS кол_дежурств " +
                           "FROM Персонал п " +
                           "JOIN Дежурства д ON п.ID_врача = д.Дежурный_врач " +
                           "WHERE MONTH(д.Дата_дежурства) = MONTH(CURRENT_DATE) AND YEAR(д.Дата_дежурства) = YEAR(CURRENT_DATE) " +
                           "GROUP BY п.Фамилия, п.Имя " +
                           "ORDER BY кол_дежурств DESC";

            MySqlCommand cmd = new MySqlCommand(query, conn);
            MySqlDataReader reader = cmd.ExecuteReader();

            List<string> врачиСДежурствами = new List<string>();
            List<double> колДежурств = new List<double>();

            while (reader.Read())
            {
                string фамилия = reader.GetString("Фамилия");
                string имя = reader.GetString("Имя");
                int колДежурствInt = reader.GetInt32("кол_дежурств");
                врачиСДежурствами.Add($"{фамилия} {имя}:\n{колДежурствInt} дежурства");
                колДежурств.Add(колДежурствInt);
            }

            reader.Close();
            conn.Close();

            AvaPlot avaPlot1 = this.Find<AvaPlot>("AvaPlot1");

            var barPlot = avaPlot1.Plot.Add.Bars(колДежурств.ToArray());
            foreach (var bar in barPlot.Bars)
            {
                bar.Label = врачиСДежурствами[barPlot.Bars.IndexOf(bar)];
            }
            barPlot.ValueLabelStyle.Bold = true;
            barPlot.ValueLabelStyle.FontSize = 12;

            avaPlot1.Plot.XLabel("Врачи");
            avaPlot1.Plot.YLabel("Количество отработанных дежурств за месяц");
        }
    }
}