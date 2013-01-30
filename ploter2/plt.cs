using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ploter2
{
    class PLT
    {
        public  List<dane> plik = new List<dane>();
        public  Bitmap bmp = new Bitmap(1000,1000);
        public Bitmap bmp1;
        private Graphics g;

        private int maxX = 7800, maxY = 8800;

        public PLT()
        {
            g = Graphics.FromImage(bmp);
            g.Clear(Color.Black);
            rysujPoleRysowania();
        }


        public void RysujKursor(int x, int y)
        {
            bmp1 = bmp.Clone(new Rectangle(0, 0, 1000, 1000), bmp.PixelFormat);
            Graphics g1 = Graphics.FromImage(bmp1);

            g1.DrawLine(Pens.GreenYellow , map(x, 0, 10000, 0, 1000), 0, map(x, 0, 10000, 0, 1000), 1000);
            g1.DrawLine(Pens.GreenYellow , 0,map(y, 0, 10000, 1000, 0), 1000, map(y, 0, 10000, 1000, 0));

        }

        public void RysujPodglad()
        {
            

            g = Graphics.FromImage(bmp);
            g.Clear(Color.Black);

            dane oldd = new dane("PU",0,0);
            foreach(dane d in plik  )
            {
                if ( d.pen=="PD")
                {
                    g.DrawLine(Pens.Yellow, map(oldd.x, 0, 10000, 0, 1000), map(oldd.y, 0, 10000, 1000, 0), map(d.x, 0, 10000, 0, 1000), map(d.y, 0, 10000, 1000, 0));
                }
                oldd = d;
            }

            rysujPoleRysowania();
        }


        private int MinX=0;
        private int MinY=0;
        private void znajdzMinimum()
        {
            foreach (dane d in plik)
            {
                if(d.x<MinX ) MinX = d.x;
                if (d.y < MinY) MinY = d.y;
            }
        }


        private void przesunRysunek()
        {
            foreach (dane d in plik)
            {
                d.x = d.x - MinX;
                d.y = d.y - MinY;
            }
        }


        public void przesunRysunek(int x, int y)
        {
            foreach (dane d in plik)
            {
                d.x = d.x + x;
                d.y = d.y + y;
            }
        }

        private void rysujPoleRysowania()
        {
            g.DrawLine(Pens.White, map(0, 0, 10000, 0, 1000), map(maxY, 0, 10000, 1000, 0), map(maxX, 0, 10000, 0, 1000), map(maxY, 0, 10000, 1000, 0));
            g.DrawLine(Pens.White, map(maxX, 0, 10000, 0, 1000), map(maxY, 0, 10000, 1000, 0), map(maxX, 0, 10000, 0, 1000), map(0, 0, 10000, 1000, 0));

            //g.DrawLine(Pens.White, map(400, 0, 10000, 0, 1000), map(maxY, 0, 10000, 1000, 0), map(400, 0, 10000, 0, 1000), map(0, 0, 10000, 1000, 0));
            //g.DrawLine(Pens.White, map(400, 0, 10000, 0, 1000), map(maxY, 0, 10000, 1000, 0), map(6700, 0, 10000, 0, 1000), map(maxY, 0, 10000, 1000, 0));
            //g.DrawLine(Pens.White, map(6700, 0, 10000, 0, 1000), map(maxY, 0, 10000, 1000, 0), map(6700, 0, 10000, 0, 1000), map(0, 0, 10000, 1000, 0));


            g.DrawEllipse(Pens.WhiteSmoke, map(0, 0, 10000, 0, 1000), map(maxY, 0, 10000, 1000, 0), map(3800, 0, 10000, 0, 1000), map(6200, 0, 10000, 1000, 0));

            int x = 1200, y = 7600, r = 1400;
            g.DrawEllipse(Pens.WhiteSmoke, map(x, 0, 10000, 0, 1000), map(y, 0, 10000, 1000, 0), map(r, 0, 10000, 0, 1000), map(10000 - r, 0, 10000, 1000, 0));

        }
        private int map(int x, int in_min, int in_max, int out_min, int out_max)
        {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }

        private string otworzPlik()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter =
           "plt files (*.plt)|*.plt|All files (*.*)|*.*";
            //dialog.InitialDirectory = initialDirectory;
            dialog.Title = "Select a text file";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                return dialog.FileName;
            }
            else
            {
                return "";
            }
        }

        public void LadujPlik()
        {
            using (StreamReader sr = new StreamReader(otworzPlik()))
            {
                String line;
                plik.Clear();
                

                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Contains("PU") || line.Contains("PD"))
                    {
                        string pen = line.Substring(0, 2);
                        int x =int.Parse(  line.Substring(2, line.LastIndexOf(' ')-1));
                        int y = int.Parse(line.Substring(line.LastIndexOf(' '), line.LastIndexOf(';')  - line.LastIndexOf(' ')));
                        plik.Add(new dane(pen, x, y));
                        Application.DoEvents();
                    }

                }
            }
            plik.Add(new dane("PU", 0, 0));
            znajdzMinimum();
            przesunRysunek();

        }

        public class dane
        {
            public dane(string _pen, int _x,int _y )
            {
                pen = _pen;
                x = _x;
                y = _y;
            }

            public  string pen;
            public int x;
            public int y;
        }
    }
}
