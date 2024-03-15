using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;

namespace zoomir
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            string connectionString = "Host=localhost;Username=postgres;Password=1111;Database=zoomir";

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    string selectQuery = "SELECT id, Название, Стоимость, Описание, Картинка FROM Товары";

                    using (NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(selectQuery, connection))
                    {
                        DataTable dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);

                        // Изменяем размеры изображений
                        foreach (DataRow row in dataTable.Rows)
                        {
                            byte[] imageData = (byte[])row["Картинка"];
                            Image originalImage = ByteArrayToImage(imageData);

                            // Уменьшаем размер изображения
                            int desiredWidth = 100; // Замените на ваше желаемое значение ширины
                            int desiredHeight = 100; // Замените на ваше желаемое значение высоты
                            Image resizedImage = ResizeImage(originalImage, desiredWidth, desiredHeight);

                            // Сохраняем уменьшенное изображение обратно в DataTable
                            row["Картинка"] = ImageToByteArray(resizedImage);
                        }

                        // Привязываем DataTable с измененными данными к DataGridView
                        dataGridView1.DataSource = dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
            dataGridView1.RowTemplate.Height = 100; // Замените 50 на нужную вам высоту

            // Применяем высоту ко всем строкам, если она одинакова для всех
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                row.Height = dataGridView1.RowTemplate.Height;
            }
        }

        // Метод для изменения размера изображения
        private Image ResizeImage(Image image, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(image, 0, 0, width, height);
            }
            return result;
        }

        // Метод для преобразования массива байт в изображение
        private Image ByteArrayToImage(byte[] byteArray)
        {
            using (MemoryStream ms = new MemoryStream(byteArray))
            {
                return Image.FromStream(ms);
            }
        }

        // Метод для преобразования изображения в массив байт
        private byte[] ImageToByteArray(Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg); // Используйте нужный формат
                return ms.ToArray();
            }
        }


        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void добавитьКЗаказуToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var selectedRows = dataGridView1.SelectedRows;

            if (selectedRows.Count > 0)
            {
                // Генерируем уникальный номер заказа
                Random random = new Random();
                int orderNumber = random.Next(100000, 999999);

                // Собираем информацию о заказе
                StringBuilder orderComposition = new StringBuilder();
                decimal totalAmount = 0;

                foreach (DataGridViewRow row in selectedRows)
                {
                    string productName = row.Cells["Название"].Value.ToString();
                    decimal productPrice = (decimal)row.Cells["Стоимость"].Value; // Приведение типа к decimal

                    // Добавляем информацию о товаре к составу заказа
                    orderComposition.AppendLine($"{productName} - {productPrice:C}");

                    // Суммируем стоимость товаров
                    totalAmount += productPrice;
                }

                // Вносим данные в базу данных (замените на свой код вставки данных)
                InsertOrderData();

            }
            else
            {
                MessageBox.Show("Выберите товары для заказа.");
            }

            zakaz zakaz = new zakaz();
            zakaz.Show();
            this.Hide(); 
        }

        private void InsertOrderData()
        {
            string connectionString = "Host=localhost;Username=postgres;Password=1111;Database=zoomir";

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                // Генерация случайного номера заказа
                Random random = new Random();
                int orderNumber = random.Next(1000, 9999);

                // Сбор данных о выбранных товарах
                StringBuilder orderDetails = new StringBuilder();
                decimal totalAmount = 0;

                foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                {
                    string productName = row.Cells["Название"].Value.ToString();
                    decimal productPrice = Convert.ToDecimal(row.Cells["Стоимость"].Value);

                    // Добавление информации о товаре в состав заказа
                    orderDetails.AppendLine($"{productName}: {productPrice:C}");

                    // Добавление стоимости товара к общей сумме заказа
                    totalAmount += productPrice;
                }

                // Вставка данных заказа в базу данных
                string insertOrderQuery = "INSERT INTO Заказы (номер_заказа, состав_заказа, сумма_заказа) " +
                                          "VALUES (@номерЗаказа, @составЗаказа, @суммаЗаказа)";

                using (NpgsqlCommand command = new NpgsqlCommand(insertOrderQuery, connection))
                {
                    command.Parameters.AddWithValue("@номерЗаказа", orderNumber);
                    command.Parameters.AddWithValue("@составЗаказа", orderDetails.ToString());
                    command.Parameters.AddWithValue("@суммаЗаказа", totalAmount);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Заказ успешно добавлен!");
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при добавлении заказа.");
                    }
                }
            }
        }

    }
}
