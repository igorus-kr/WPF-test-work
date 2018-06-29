using System;
using System.Windows;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace NoteApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string connectionString;
        SqlDataAdapter adapter;
        DataTable peopleTable;

        public MainWindow()
        {
            InitializeComponent();
            // получаем строку подключения из app.config
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string sql = "SELECT * FROM People";
            peopleTable = new DataTable();
            SqlConnection connection = null;
            try
            {
                connection = new SqlConnection(connectionString);
                SqlCommand command = new SqlCommand(sql, connection);
                adapter = new SqlDataAdapter(command);

                // установка команды на добавление для вызова хранимой процедуры
                adapter.InsertCommand = new SqlCommand("sp_InsertHuman", connection);
                adapter.InsertCommand.CommandType = CommandType.StoredProcedure;
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@LastName", SqlDbType.NVarChar, 50, "LastName"));
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@FirstName", SqlDbType.NVarChar, 50, "FirstName"));
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@SecondName", SqlDbType.NVarChar, 50, "SecondName"));
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@BirthDate", SqlDbType.Date, 0, "BirthDate"));
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@PhoneNumber", SqlDbType.NVarChar, 50, "PhoneNumber"));
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 50, "Email"));
                SqlParameter parameter = adapter.InsertCommand.Parameters.Add("@Id", SqlDbType.Int, 0, "Id");
                parameter.Direction = ParameterDirection.Output;

                connection.Open();
                adapter.Fill(peopleTable);
                peopleGrid.ItemsSource = peopleTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
        }

        private void UpdateDB()
        {
            try
            { 
                SqlCommandBuilder comandbuilder = new SqlCommandBuilder(adapter);
                adapter.Update(peopleTable);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateDB();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (peopleGrid.SelectedItems != null)
            {
                for (int i = 0; i < peopleGrid.SelectedItems.Count; i++)
                {
                    DataRowView datarowView = peopleGrid.SelectedItems[i] as DataRowView;
                    if (datarowView != null)
                    {
                        DataRow dataRow = datarowView.Row;
                        dataRow.Delete();
                    }
                }
            }
            UpdateDB();
        }
    }
}
