using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AutoPatcher
{
    public partial class Registracia : Form
    {
        int rand1, rand2;

        public Registracia()
        {
            InitializeComponent();

            Random r = new Random();
            this.rand1 = r.Next(1, 30);
            this.rand2 = r.Next(1, 30);
            label9.Text = this.rand1.ToString() + " + " + this.rand2.ToString();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Registracia_Load(object sender, EventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
             /*   if (Convert.ToInt32(textBox10.Text) == (this.rand2 + this.rand1))
                {
                    if (textBox1.Text.Length > 0 && textBox2.Text.Length > 0 && textBox3.Text.Length > 0 && textBox4.Text.Length > 0 && textBox5.Text.Length > 0 &&
                        textBox6.Text.Length > 0 && textBox7.Text.Length > 0 && textBox8.Text.Length > 0 && textBox9.Text.Length > 0)
                    {*/
                        Rules rules = new Rules();
                        if (rules.ShowDialog() == DialogResult.Yes)
                        {
                            MessageBox.Show("Boli ste uspesne zaregistrovany, teraz sa mozete prihlasit!", "Registration success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Pre uspesnu registraciu do nasho herneho systemu musite suhlasit s nasimi podmienkami!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                  /*  }
                    else
                    {
                        MessageBox.Show("Zle vyplnene udaje!!");
                    }
                    
                }
                else
                {
                    MessageBox.Show("Blbo ste vypocitaly zadany priklad!");
                }*/
            }
            catch (Exception x) 
            {
                MessageBox.Show("Blbo ste vypocitaly zadany priklad!");
            }
        }
    }
}
