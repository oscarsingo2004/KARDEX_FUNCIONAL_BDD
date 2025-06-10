using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KARDEX_FUNCIONAL_BDD
{
    public partial class Form1 : Form
    {


        string connectionString = ConfigurationManager.ConnectionStrings["Miconexion"].ConnectionString;


        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {


            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    MessageBox.Show("Conexión exitosa.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al conectar: " + ex.Message);
                }
            }



        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
          
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT IdProducto, NombreProducto FROM Productos", conn);
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                    comboBox1.Items.Add(new ComboBoxItem
                        {
                            Text = dr["NombreProducto"].ToString(),
                            Value = Convert.ToInt32(dr["IdProducto"])
                        });
                    }

                comboBox2.Items.Add("Entrada");
                comboBox2.Items.Add("Salida");
                }
            

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
                var selected = (ComboBoxItem)comboBox1.SelectedItem;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT StockActual FROM Productos WHERE IdProducto = @id", conn);
                    cmd.Parameters.AddWithValue("@id", selected.Value);
                label5.Text = "Stock actual: " + cmd.ExecuteScalar().ToString();

                    // Cargar movimientos
                    SqlDataAdapter da = new SqlDataAdapter(
                        "SELECT FechaMovimiento, TipoMovimiento, Cantidad, PrecioUnitario, Total, StockResultante " +
                        "FROM Movimientos WHERE IdProducto = @id ORDER BY FechaMovimiento", conn);
                    da.SelectCommand.Parameters.AddWithValue("@id", selected.Value);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                dataGridView1.DataSource = dt;
               }
            

        }
        public class ComboBoxItem
        {
            public string Text { get; set; }
            public int Value { get; set; }
            public override string ToString() => Text;
        }

        private void button2_Click(object sender, EventArgs e)
        {        
                var producto = (ComboBoxItem)comboBox1.SelectedItem;
                int cantidad = int.Parse(textBox1.Text);
                decimal precio = decimal.Parse(textBox2.Text);
                string tipo = comboBox2.SelectedItem.ToString();
                int stockActual = int.Parse(label5.Text.Split(':')[1]);

                int nuevoStock = tipo == "Entrada" ? stockActual + cantidad : stockActual - cantidad;
                if (nuevoStock < 0)
                {
                    MessageBox.Show("No hay suficiente stock para realizar la salida.");
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlTransaction trx = conn.BeginTransaction();
                    try
                    {
                        SqlCommand insert = new SqlCommand(
                            "INSERT INTO Movimientos (IdProducto, FechaMovimiento, TipoMovimiento, Cantidad, PrecioUnitario, Total, StockResultante) " +
                            "VALUES (@id, @fecha, @tipo, @cant, @precio, @total, @saldo)", conn, trx);

                        insert.Parameters.AddWithValue("@id", producto.Value);
                        insert.Parameters.AddWithValue("@fecha", dateTimePicker1.Value);
                        insert.Parameters.AddWithValue("@tipo", tipo);
                        insert.Parameters.AddWithValue("@cant", cantidad);
                        insert.Parameters.AddWithValue("@precio", precio);
                        insert.Parameters.AddWithValue("@total", cantidad * precio);
                        insert.Parameters.AddWithValue("@saldo", nuevoStock);
                        insert.ExecuteNonQuery();

                        SqlCommand update = new SqlCommand("UPDATE Productos SET StockActual = @nuevo WHERE IdProducto = @id", conn, trx);
                        update.Parameters.AddWithValue("@nuevo", nuevoStock);
                        update.Parameters.AddWithValue("@id", producto.Value);
                        update.ExecuteNonQuery();

                        trx.Commit();
                        MessageBox.Show("Movimiento registrado.");
                    comboBox1_SelectedIndexChanged(null, null); // recarga
                    }
                    catch (Exception ex)
                    {
                        trx.Rollback();
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
