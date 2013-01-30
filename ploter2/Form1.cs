using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using SerialCOM;

namespace ploter2
{
    public partial class Form1 : Form
    {
        private COM com = new COM();
        private PLT plt = new PLT();
        private int bledy = 0;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Show();

            foreach(string a in com.Porty() )
            {
                comboBox1.Items.Add(a);
            }
           
            //com.ArduinoRestart();
            //wait(2000);
            //log(com.WyslijOdbierz(COM.komendy.nazwa) + " " + com.WyslijOdbierz(COM.komendy.wersja ));

            com.Odebrano +=new COM.odebrano(com_Odebrano);

            pictureBox1.Image = plt.bmp;
        }

        void com_Odebrano(COM.OdebraneDane dane)
        {
            
                plt.RysujKursor(dane.x, dane.y);
                pictureBox1.Image = plt.bmp1;
            this.Invoke((ThreadStart) delegate()
                                          {
                                              pictureBox1.Refresh();
                                              if (dane.kom == 0xFB)
                                              {
                                                  log("ok");
                                              }

                                              textBox2.Text = dane.x.ToString();
                                              textBox3.Text = dane.y.ToString();
                                              textBox4.Text = dane.speed.ToString();
                                              textBox5.Text = dane.pen.ToString();
                                          });
            
        }





        void wait(int ms)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (stopwatch.Elapsed.TotalMilliseconds < ms)
            {
                Application.DoEvents();
                Thread.Sleep(1);
            }

        }
        void log(string text)
        {
            this.Invoke((ThreadStart) delegate()
                                          {
                                              textBox1.AppendText(text + Environment.NewLine);
                                              textBox1.ScrollToCaret();
                                          });
        }


        private void wyswietlZawartoscPliku()
        {
            listBox1.Items.Clear();
            foreach(PLT.dane d in plt.plik )
            {
                listBox1.Items.Add(d.pen + d.x.ToString() + " " + d.y.ToString());
            }
        }


        private bool dziala=false ;
        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button4.Enabled = false;
            dziala = true;
            backgroundWorker1.RunWorkerAsync();
            
        }

        private void ładujToolStripMenuItem_Click(object sender, EventArgs e)
        {
            plt.LadujPlik();
            plt.RysujPodglad();
            wyswietlZawartoscPliku();
            pictureBox1.Image = plt.bmp;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            trackBar2.Value = 0;
            plt.przesunRysunek(trackBar2.Value , trackBar1.Value );
            plt.RysujPodglad();
            pictureBox1.Image = plt.bmp;
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            trackBar1.Value = 0;
            plt.przesunRysunek(trackBar2.Value, trackBar1.Value);
            plt.RysujPodglad();
            pictureBox1.Image = plt.bmp;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            com.SetPosition((byte)numericUpDown2.Value ,(int)numericUpDown1.Value ,0,0);
        }


        private int i;
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            for (  ; i < plt.plik.Count; i++)
            {
                if(dziala==false ) break;
             
                log(plt.plik[i].pen + plt.plik[i].x.ToString() + " " + plt.plik[i].y.ToString());
                byte pen;
                int speed;

                if(plt.plik[i].pen=="PU")
                {
                     pen = (byte) numericUpDown2.Value;
                     speed = (int)numericUpDown4.Value;
                }
                else
                {
                     pen = (byte) numericUpDown3.Value;
                     speed = (int)numericUpDown1.Value;
                }

             
                try
                {
                    com.SetPosition(pen, speed, plt.plik[i].x, plt.plik[i].y);
                }catch(Exception ex)
                {
                    log(ex.Message );
                }
                this.Invoke((ThreadStart) delegate()
                                              {
                                                  listBox1.SetSelected(i, true);
                                              });
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            byte pen = plt.plik[listBox1.SelectedIndex].pen == "PU" ? (byte)numericUpDown2.Value : (byte)numericUpDown3.Value;

            com.SetPosition(pen, (int)numericUpDown1.Value, plt.plik[listBox1.SelectedIndex].x, plt.plik[listBox1.SelectedIndex].y);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            button1.Enabled = true;
            button4.Enabled = true;
            dziala = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            i = 0;
            dziala = true;
            backgroundWorker1.RunWorkerAsync();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            log("otwieram port "+comboBox1.Text );
            com.OtworzPort(comboBox1.Text );
            if (com.Polaczony()) log("Połączono");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            com.ZamknijPort();
            log("port "+com.Port() +" zamkniety");
        }

       
    }
}
