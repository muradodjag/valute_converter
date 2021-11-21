using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pet_project1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        
        // Convert from other to AZN
        private void button1_Click(object sender, EventArgs e)
        {
            double rate = SqliteDataAccess.GetRate(comboBox1.Text);
            double amount = double.Parse(textBox1.Text.Replace('.', ',')); // replace . to , because floating number represents through ,
            textBox2.Text = (amount * rate).ToString();
        }

        
        // When you run application in this method program create table, insert into table rate from cbar.az
        //    Then load Currencies from table and populate comboBox.
        private void Form1_Load(object sender, EventArgs e)
        {
            
            SqliteDataAccess.CreateTable();
            SqliteDataAccess.InsertTable();
            var curr = SqliteDataAccess.LoadCurr();


            foreach(var el in curr)
            {
                comboBox1.Items.Add(el);
            }

        }
        
        
        // limit input: Only digit and floating points with '.' or ','
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            
            if (char.IsDigit(e.KeyChar) || e.KeyChar == '\b' || (e.KeyChar == '.' || e.KeyChar == ',') && ((sender as TextBox).Text.IndexOf('.') < 0 && (sender as TextBox).Text.IndexOf(',') < 0) 
                                                                                                                                                            && (sender as TextBox).Text.Length > 0)
            {
                textBox1.ReadOnly = false;
            }
            else
                textBox1.ReadOnly = true;
        }
    }
}
