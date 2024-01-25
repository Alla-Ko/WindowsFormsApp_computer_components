using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Diagnostics;
using System.Xml.Linq;
using System.Drawing.Printing;

namespace WindowsFormsApp_computer_components
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            LoadNamesIntoComboBox();
            TotalPrice_Fill();

        }
        private List<Component>Components=new List<Component>();
        private List<Component> My_Components = new List<Component>();
        string connectionString = "Data Source=bd_computer_components.db;Version=3;";
        private void button_adm_Click(object sender, EventArgs e)
        {
            Form2 form = new Form2();
            form.ShowDialog();
            LoadNamesIntoComboBox();
            //SetPriceIntoTextBox();


        }
        private void LoadNamesIntoComboBox()
        {
            comboBox1.DataSource = null;
            comboBox1.Items.Clear();
            Components.Clear();


            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();


                string selectQuery = "SELECT Id, Name, Characteristic, Description, Price FROM Components ORDER BY Name";
                using (SQLiteCommand command = new SQLiteCommand(selectQuery, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id=reader.GetInt32(reader.GetOrdinal("Id"));
                            string name= reader.GetString(reader.GetOrdinal("Name"));
                            string characteristic = reader.GetString(reader.GetOrdinal("Characteristic"));
                            string description = reader.GetString(reader.GetOrdinal("Description"));
                            decimal price = reader.GetDecimal(reader.GetOrdinal("Price"));
                            

                            // Створюємо об'єкт автора і додаємо його до списку
                            Component component = new Component(id, name, characteristic, description, price);
                            Components.Add(component);
                        }
                    }
                }
            }
            // Додаємо авторів до ComboBox
            comboBox1.DataSource = Components;
            comboBox1.DisplayMember = "Name";
            comboBox1.ValueMember = "Price";
            SetPriceIntoTextBox();
        }
        private void SetPriceIntoTextBox()
        {
            if (comboBox1.SelectedValue != null )
            {
                textBox1.Text = comboBox1.SelectedValue.ToString();
            }
            else textBox1.Text = "";

        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            SetPriceIntoTextBox();
        }

        private void button_add_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedValue != null)
            {
                My_Components.Add((Component)comboBox1.SelectedItem);
                dataGridView1_Fill();
            }
            TotalPrice_Fill();
        }
        private void dataGridView1_Fill()
        {
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = My_Components;
            dataGridView1.ReadOnly = true;
            dataGridView1.Columns[0].Visible = false;
            dataGridView1.Columns[1].HeaderText = "Назва";
            dataGridView1.Columns[2].Visible = false;
            dataGridView1.Columns[3].Visible = false;
            dataGridView1.Columns[4].HeaderText = "Ціна";
            dataGridView1.AllowUserToAddRows = false;


            DataGridViewCellStyle headerStyle = new DataGridViewCellStyle();
            headerStyle.BackColor = Color.SandyBrown;
            headerStyle.ForeColor = Color.Linen;
            headerStyle.SelectionBackColor = Color.Sienna;
            headerStyle.SelectionForeColor = Color.Linen;
            headerStyle.Font = new Font("Microsoft Sans Serif", 10, FontStyle.Bold);
            
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.HeaderCell.Style = headerStyle;

            }



            //dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.SandyBrown;
            //dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.Linen;
            //dataGridView1.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.Sienna;
            //dataGridView1.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.Linen;
            //dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 10, FontStyle.Bold); 

            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            dataGridView1.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        }

        private void button_del_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count < 0|| My_Components.Count<1)
            {
                MessageBox.Show($"Не виділено жодного елемента в списку", "Повідомлення про помилку", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int selectedIndex = dataGridView1.SelectedCells[0].RowIndex;
            int rowId = Convert.ToInt32(dataGridView1.Rows[selectedIndex].Cells[0].Value);
            
            Component componentToRemove = My_Components.Find(c => c.id == rowId);
            if (componentToRemove != null)
            {
                My_Components.Remove(componentToRemove);
            }
            dataGridView1_Fill();
            TotalPrice_Fill();
        }
        private void TotalPrice_Fill()
        {
            decimal totalPrice = 0;
            foreach(var component in My_Components)
            {
                totalPrice+= component.price;
            }
            textBox2.Text = totalPrice.ToString();
        }

        private void button_print_Click(object sender, EventArgs e)
        {
            PrintDocument printDocument = new PrintDocument();
            printDocument.PrintPage += PrintDocument_PrintPage;

            PrintDialog printDialog = new PrintDialog();
            printDialog.Document = printDocument;

            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                PrintPreviewDialog printPreviewDialog = new PrintPreviewDialog();
                printPreviewDialog.Document = printDocument;
                // Додавання іконки до PrintPreviewDialog
                Icon icon = (System.Drawing.Icon)(this.Icon);
                printPreviewDialog.Icon = icon;

                // Зміна розміру вікна PrintPreviewDialog
                
                printPreviewDialog.Load += (sender1, e1) =>
                {
                    printPreviewDialog.Width = (int)(Screen.PrimaryScreen.Bounds.Height * 0.8);
                    printPreviewDialog.Height = (int)(Screen.PrimaryScreen.Bounds.Height * 0.8);

                    printPreviewDialog.Left = (Screen.PrimaryScreen.Bounds.Width - printPreviewDialog.Width) / 2;
                    printPreviewDialog.Top = (Screen.PrimaryScreen.Bounds.Height - printPreviewDialog.Height) / 2;
                };

                printPreviewDialog.ShowDialog();
            }
        }
        private int currentPage = 0;
        private const int itemsPerPage = 13; // Кількість елементів на сторінці

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            Font font = new Font("Microsoft Sans Serif", 12, FontStyle.Bold);
            Icon icon = (System.Drawing.Icon)(this.Icon);
            Bitmap iconBitmap = icon.ToBitmap();
            e.Graphics.DrawImage(iconBitmap, new Rectangle(80, 80, iconBitmap.Width, iconBitmap.Height));
            // Додавання заголовка
            if (currentPage == 0)
            {
                
                string header = "Список компонентів комп'ютера";
                

                e.Graphics.DrawString(header, font, Brushes.Black, new PointF(250, 150));
                e.Graphics.DrawString("", font, Brushes.Black, new PointF(100, 100));
            }
            


            // Відображення таблиці

            // Визначення положення для початку таблиці
            int tableX = 100;
            int tableY = 250;

            // Розмір клітинок
            int cellWidth = 500;
            int cellHeight = 60;

            // Встановлення шрифта для таблиці
            Font tableFont = new Font("Microsoft Sans Serif", 10, FontStyle.Regular);

            // Розміщення заголовків колонок
            e.Graphics.DrawString("Назва компонента", font, Brushes.Black, new Rectangle(tableX, tableY, cellWidth, cellHeight));
            e.Graphics.DrawString("Ціна", font, Brushes.Black, new Rectangle(tableX + cellWidth, tableY, cellWidth, cellHeight));

            // Збільшити Y для наступного рядка
            tableY += cellHeight;

            // Вивід значень зі списку My_Components
            int itemsPerPage = (e.MarginBounds.Height - tableY) / cellHeight;
            int startIndex = currentPage * itemsPerPage;
            int endIndex = startIndex + itemsPerPage - 1;

            for (int i = startIndex; i < My_Components.Count && i <= endIndex; i++)
            {
                Component component = My_Components[i];
                e.Graphics.DrawString(component.name, tableFont, Brushes.Black, new Rectangle(tableX, tableY, cellWidth, cellHeight));
                e.Graphics.DrawString(component.price.ToString("C"), tableFont, Brushes.Black, new Rectangle(tableX + cellWidth, tableY, cellWidth, cellHeight));

                // Збільште Y для наступного рядка
                tableY += cellHeight;
            }

            // Визначення кількості сторінок
            int totalPages = (int)Math.Ceiling((double)My_Components.Count / itemsPerPage);

            // Збільшення Y для подальших сторінок
            tableY += cellHeight;

            // Перевірка, чи є ще сторінки
            if (currentPage < totalPages - 1)
            {
                e.HasMorePages = true;
                currentPage++;
            }
            else
            {
                e.HasMorePages = false;
                currentPage = 0; // Скидання лічильника сторінок для наступного друку
                e.Graphics.DrawString("Загальна вартість", font, Brushes.Black, new Rectangle(tableX, tableY, cellWidth, cellHeight));
                decimal totalprice = 0;
                foreach (Component component in My_Components)
                {
                    totalprice += component.price;
                }
                e.Graphics.DrawString(totalprice.ToString("C"), font, Brushes.Black, new Rectangle(tableX + cellWidth, tableY, cellWidth, cellHeight));
            }

            
        }
    }
    public class Component
    {
        public int id { get; set; }
        public string name { get; set; }
        public string characteristic { get; set; }
        public string description { get; set; }
        public decimal price { get; set; }
        public Component(int id, string name, string characteristic, string description, decimal price)
        {
            this.id = id;
            this.name = name;
            this.characteristic = characteristic;
            this.description= description;
            this.price= price;
        }
    }
}
