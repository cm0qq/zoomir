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
using System.IO;
using Aspose.Pdf;
using Aspose.Pdf.Text;
using System.Diagnostics;

namespace zoomir
{
    public partial class zakaz : Form
    {
        public zakaz()
        {
            InitializeComponent();
        }

        private void zakaz_Load(object sender, EventArgs e)
        {
            string connectionString = "Host=localhost;Username=postgres;Password=1111;Database=zoomir";

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    string selectQuery = "SELECT id, номер_заказа, состав_заказа, сумма_заказа FROM Заказы";

                    using (NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(selectQuery, connection))
                    {
                        DataTable dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);

                        dataGridView1.DataSource = dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
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
                    // Извлекаем значения из соответствующих столбцов
                    string orderDetails = row.Cells["состав_заказа"].Value?.ToString() ?? "";
                    decimal orderPrice = 0;

                    // Пытаемся преобразовать значение в десятичное число
                    decimal.TryParse(row.Cells["сумма_заказа"].Value?.ToString(), out orderPrice);

                    // Добавляем информацию о товаре к составу заказа
                    orderComposition.AppendLine($"{orderDetails} - {orderPrice:C}");

                    // Суммируем стоимость товаров
                    totalAmount += orderPrice;
                }

                // Получаем значение из ComboBox
                string pickupLocation = comboBox1.SelectedItem?.ToString() ?? "Не выбрано";

                // Генерируем код получения заказа из трех цифр
                int pickupCode = random.Next(100, 999);

                // Вызываем метод для создания PDF
                GeneratePdf(orderNumber.ToString(), orderComposition.ToString(), totalAmount, pickupLocation, pickupCode.ToString());
            }
            else
            {
                MessageBox.Show("Выберите заказы для формирования PDF.");
            }
        }

        private void GeneratePdf(string orderNumber, string orderComposition, decimal totalAmount, string pickupLocation, string pickupCode)
        {
            // Создаем имя PDF-файла
            string pdfFileName = $"Order_{orderNumber}.pdf";

            // Создаем документ
            Document pdfDocument = new Document();

            // Создаем страницу
            Page page = pdfDocument.Pages.Add();

            // Создаем текстовый блок
            TextFragment textFragment = new TextFragment();
            textFragment.Text = $"Номер заказа: {orderNumber}\n" +
                                $"Состав заказа: {orderComposition}\n" +
                                $"Сумма заказа: {totalAmount:C}\n" +
                                $"Пункт выдачи: {pickupLocation}\n" +
                                $"Код получения заказа: {(Convert.ToInt32(pickupCode) % 1000):D3}";

            // Добавляем текстовый блок на страницу
            page.Paragraphs.Add(textFragment);

            // Сохраняем документ
            pdfDocument.Save(pdfFileName);

            // Открываем созданный PDF-файл
            Process.Start(pdfFileName);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
