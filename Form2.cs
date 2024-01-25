using Microsoft.Data.Sqlite;
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
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Diagnostics;
using static System.Windows.Forms.LinkLabel;
using System.Drawing.Drawing2D;

namespace WindowsFormsApp_computer_components
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            Open_db();
            
            dataGridView1.Columns[0].HeaderText = "ID";
            dataGridView1.Columns[1].HeaderText = "Назва";
            dataGridView1.Columns[2].HeaderText = "Характеристика";
            dataGridView1.Columns[3].HeaderText = "Опис";
            dataGridView1.Columns[4].Visible= false;
            dataGridView1.AllowUserToAddRows = false;
            
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
           

        }
        string connectionString = "Data Source=bd_computer_components.db;Version=3;";
        public void Open_db()
        {
            try
            {
                string query = "SELECT * FROM Components";
                SQLiteConnection connection = new SQLiteConnection(connectionString);
                connection.Open();

                //SQLiteCommand command = new SQLiteCommand(query, connection);
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, connection);
                DataTable table = new DataTable();
                adapter.Fill(table);
                dataGridView1.DataSource = table;
                dataGridView1.ReadOnly = true;
                connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка {ex.Message}", "Повідомлення про помилку", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private void button_clear_Click(object sender, EventArgs e)
        {
            textBox_name.Text = string.Empty;
            textBox_description.Text=string.Empty;
            textBox_characteristic.Text = string.Empty;
            textBox_price.Text = string.Empty;
            dataGridView1.ClearSelection();
            textBox_name.Focus();
        }

        private void textBox_price_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(textBox_price.Text == string.Empty) {
                if (!char.IsDigit(e.KeyChar))
                {
                    e.Handled = true;
                }
            }
            else
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '.' && e.KeyChar != '\b' && e.KeyChar != (char)Keys.Delete)
            {
                e.Handled = true;
            }
            if (e.KeyChar == '.')
            {
                // Перевіряємо, чи вже є крапка в тексті
                if (textBox_price.Text.Contains("."))
                {
                    e.Handled = true; // Якщо так, блокуємо введення додаткової крапки
                }
            }
            if (textBox_price.Text.Contains('.'))
            {
                int dotIndex = textBox_price.Text.IndexOf('.');
                if (textBox_price.Text.Length - dotIndex > 2&& e.KeyChar != '\b' && e.KeyChar != (char)Keys.Delete)
                {
                    e.Handled = true; // Блокуємо введення більше 2 знаків після крапки
                }
            }
        }

        private void button_edit_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count > 0)
            {
                // Отримати індекс виділеного рядка
                int selectedIndex = dataGridView1.SelectedCells[0].RowIndex;

                // Отримати значення ідентифікатора рядка (припустимо, що це перший стовпець)
                int rowId = Convert.ToInt32(dataGridView1.Rows[selectedIndex].Cells[0].Value);

                //редагувати рядок в базі даних
                //оновити грідвю
                string name=textBox_name.Text;
                string description = textBox_description.Text;
                string characteristic = textBox_characteristic.Text;
                string price= textBox_price.Text;
                
                if (name == "")
                {
                    MessageBox.Show($"Введіть назву", "Повідомлення про помилку", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (price == "")
                {
                    MessageBox.Show($"Введіть ціну", "Повідомлення про помилку", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                for ( int index=0;index< dataGridView1.Rows.Count;index++)
                {
                    if (index == rowId) continue;
                    if (dataGridView1.Rows[index].Cells[1].Value.ToString()== name)
                    {
                        MessageBox.Show($"Така назва вже існує в іншого товару", "Повідомлення про помилку", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                try
                {
                    using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                    {
                        connection.Open();

                        string insertQuery = "UPDATE Components SET Name = @Name, Characteristic=@Characteristic, Description=@Description, Price=@Price WHERE Id = @Id;";
                        using (SQLiteCommand command = new SQLiteCommand(insertQuery, connection))
                        {
                            command.Parameters.AddWithValue("@Name", name);
                            command.Parameters.AddWithValue("@Id", rowId);
                            command.Parameters.AddWithValue("@Characteristic", characteristic);
                            command.Parameters.AddWithValue("@Description", description);
                            command.Parameters.AddWithValue("@Price", price);

                            command.ExecuteNonQuery();
                            Open_db();
                        }
                        connection.Close();

                    }
                }

                catch (Exception ex)
                {
                    MessageBox.Show("Проблема з підключенням до бази даних", "Повідомлення про помилку", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }





            }
            else
            {
                MessageBox.Show("Будь ласка, виберіть рядок для редагування.", "Попередження", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void DeleteSelectedRow()
        {
            // Перевірка, чи вибраний хоча б один рядок
            if (dataGridView1.SelectedCells.Count > 0)
            {
                // Отримати індекс виділеного рядка
                int selectedIndex = dataGridView1.SelectedCells[0].RowIndex;

                // Отримати значення ідентифікатора рядка (припустимо, що це перший стовпець)
                int rowId = Convert.ToInt32(dataGridView1.Rows[selectedIndex].Cells[0].Value);

                // Видалити рядок з DataGridView
                dataGridView1.Rows.RemoveAt(selectedIndex);

                // Видалити рядок з бази даних (припустимо, що у вас є об'єкт SQLiteConnection з ім'ям connection)
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string deleteQuery = $"DELETE FROM Components WHERE Id = @Id";
                    using (SQLiteCommand command = new SQLiteCommand(deleteQuery, connection))
                    {

                        command.Parameters.AddWithValue("@Id", rowId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                MessageBox.Show("Будь ласка, виберіть рядок для видалення.", "Попередження", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DeleteSelectedRow();
            button_clear_Click(sender, e);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //додати елемент
            string name = textBox_name.Text;
            if (textBox_name.Text.Length < 1)
            {
                MessageBox.Show("Не внесена назва товару", "Повідомлення про помилку", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            double price = 0.00;
            double.TryParse(textBox_price.Text, out price);
            if (price==0)
            {
                MessageBox.Show("Не внесена ціна товару", "Повідомлення про помилку", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (item_exists(name))
            {
                MessageBox.Show("Такий товар вже існує в базі даних", "Повідомлення про помилку", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            else
            {
                try
                {
                    using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                    {
                        connection.Open();

                        string insertQuery = "INSERT INTO Components (Name, Characteristic, Description, Price) VALUES (@Name, @Characteristic, @Description, @Price)";
                        using (SQLiteCommand command = new SQLiteCommand(insertQuery, connection))
                        {
                            command.Parameters.AddWithValue("@Name", textBox_name.Text);
                            command.Parameters.AddWithValue("@Characteristic", textBox_characteristic.Text);
                            command.Parameters.AddWithValue("@Description", textBox_description.Text);
                            command.Parameters.AddWithValue("@Price", textBox_price.Text);

                            command.ExecuteNonQuery();
                            Open_db();
                        }
                        connection.Close();
                        
                    }
                }
                
                catch (Exception ex)
                {
                    MessageBox.Show("Проблема з підключенням до бази даних", "Повідомлення про помилку", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                
            }
        }
        private bool item_exists(string name)
        {
            // Припустимо, що у вас є об'єкт SQLiteConnection з ім'ям connection
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Припустимо, що у вас є таблиця "Author" зі стовпцями "Name" і "Surname"
                string selectQuery = "SELECT COUNT(*) FROM Components WHERE Name = @Name";
                using (SQLiteCommand command = new SQLiteCommand(selectQuery, connection))
                {
                    command.Parameters.AddWithValue("@Name", name);

                    int count = Convert.ToInt32(command.ExecuteScalar());
                    connection.Close();
                    return count > 0;
                }

            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            
            if (dataGridView1.SelectedCells.Count > 0)
            {
                // Отримати індекс виділеного рядка
                int selectedIndex = dataGridView1.SelectedCells[0].RowIndex;
                textBox_name.Text = dataGridView1.Rows[selectedIndex].Cells["Name"].Value.ToString();
                textBox_characteristic.Text = dataGridView1.Rows[selectedIndex].Cells["Characteristic"].Value.ToString();
                textBox_description.Text = dataGridView1.Rows[selectedIndex].Cells["Description"].Value.ToString();
                textBox_price.Text = dataGridView1.Rows[selectedIndex].Cells["Price"].Value.ToString();




            }

        }
    }
}
