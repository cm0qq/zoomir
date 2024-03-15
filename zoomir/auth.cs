using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace zoomir
{
    public partial class auth : Form
    {
        public auth()
        {
            InitializeComponent();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string login = maskedTextBox1.Text;
            string password = textBox2.Text;
            string connectionString = "Host=localhost;Username=postgres;Password=1111;Database=zoomir";
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    string selectQuery = "SELECT COUNT(*) FROM Пользователи WHERE Номер_телефона = @Номер_телефона AND Пароль = @Пароль";

                    using (NpgsqlCommand command = new NpgsqlCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Номер_телефона", login);
                        command.Parameters.AddWithValue("@Пароль", password);
                    }

                    Form1 form1 = new Form1();
                    form1.Show();
                    this.Hide();

                }
            }

            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
            this.Hide();
        }
    }
}
