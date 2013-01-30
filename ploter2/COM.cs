using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SerialCOM
{
    class COM
    {
        public delegate void odebrano(OdebraneDane  dane);
        public event odebrano Odebrano;

        SerialPort serial = new SerialPort();
        private bool odbieranieEvent = true;

        public string Port()
        {
            return serial.PortName;
        }

        public string[] Porty()
        {
            return SerialPort.GetPortNames();
        }

        public bool Polaczony()
        {
            return serial.IsOpen;
        }
        public void OtworzPort(string nazwa)

        {

            serial.BaudRate = 115200;
            //serial.DataReceived += new SerialDataReceivedEventHandler(serial_DataReceived);
            serial.PortName = nazwa;
            serial.Open();
        }


        public void ZamknijPort()
        {
            serial.Close();
        }


        void serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            //if (odbieranieEvent && Odebrano!=null)
            //{
            //   // wait(10);
            //    //Odebrano(serial.ReadExisting());
            //}
        }

        public string WyslijOdbierz(komendy  komenda)
        {
            
            odbieranieEvent = false;

            byte[] buffer = {(byte)komenda};

            serial.Write(buffer, 0, 1);
            

            Stopwatch stoper = new Stopwatch();
            stoper.Start();

            string odebrane = serial.ReadExisting();
            while(!odebrane.Contains("\r\n"))
            {
                odebrane += serial.ReadExisting();
                Application.DoEvents();
                if (stoper.Elapsed.Seconds >= 1)
                {
                    odbieranieEvent = true;
                    throw new Exception("TIME OUT");
                }
            }

            odbieranieEvent = true;
            return odebrane.Replace("\r\n","");
        }


        public string WyslijOdbierz(string  komenda)
        {
            odbieranieEvent = false;
            serial.WriteLine( komenda);
            
            Stopwatch stoper = new Stopwatch();
            stoper.Start();

            string odebrane = serial.ReadExisting();
            while (!odebrane.Contains(";\r\n"))
            {
                odebrane += serial.ReadExisting();
                Application.DoEvents();
                if (stoper.Elapsed.Seconds >= 1)
                {
                    odbieranieEvent = true;
                    throw new Exception("TIME OUT");
                }
            }

            odbieranieEvent = true;
            return odebrane;//.Replace(";\r\n", "");
        }

        public void SetPosition(byte pen, int speed, int x, int y)
        {

            if (Polaczony())
            {
                byte[] dane = new byte[8];
                dane[0] = 0xFA;

                dane[1] = pen;

                byte[] tmp = BitConverter.GetBytes((Int16) speed);
                dane[2] = tmp[0];
                dane[3] = tmp[1];

                tmp = BitConverter.GetBytes((Int16) x);

                dane[4] = tmp[0];
                dane[5] = tmp[1];

                tmp = BitConverter.GetBytes((Int16) y);
                dane[6] = tmp[0];
                dane[7] = tmp[1];

                serial.Write(dane, 0, dane.Length);

                Stopwatch stoper = new Stopwatch();
                stoper.Start();

                do
                {

                    Application.DoEvents();
                    if (serial.BytesToRead >= 8)
                    {
                        byte[] tmp1 = new byte[1];
                        do
                        {
                            serial.Read(tmp1, 0, 1);
                            // log(tmp1[0].ToString());
                            if (tmp1[0] != 0xFB) break;
                            if (tmp1[0] != 0xFA) break;
                            Application.DoEvents();

                        } while (true);


                        byte[] dane1 = new byte[7];
                        serial.Read(dane1, 0, 7);

                        OdebraneDane odebraneDane = new OdebraneDane(tmp1[0],dane1 );

                        if (tmp1[0] == 0xFB)
                        {
                            Odebrano(odebraneDane);

                            return;
                        }

                        if (tmp1[0] == 0xFA)
                        {
                            Odebrano(odebraneDane);
                            stoper.Reset();
                            stoper.Start();
                        }
                    }
                } while (stoper.Elapsed.TotalSeconds<5);
                throw (new SystemException( "Time out"));
            }
        }

        public string WyslijOdbierzOdpowiednia(string komenda, string coMaZawierac)
        {
            odbieranieEvent = false;
            serial.WriteLine(komenda);
            wait(10);

            Stopwatch stoper = new Stopwatch();
            stoper.Start();

           string odebrane ="";//= serial.ReadExisting();

            while (!odebrane.Contains(coMaZawierac))
            {
                stoper.Reset();
                stoper.Start();
                odebrane = "";
                while (!odebrane.Contains(";\r\n"))
                {
                    odebrane += serial.ReadExisting();
                    Application.DoEvents();
                    //wait(2);
                    if (stoper.Elapsed.TotalSeconds >= 10)
                    {
                        odbieranieEvent = true;
                        throw new Exception("TIME OUT");
                    }
                }
               // Odebrano(odebrane);//.Replace(";\r\n", ""));
            }
            odbieranieEvent = true;
            return odebrane;//.Replace(";\r\n", "");
        }

        public void ArduinoRestart()
        {
            serial.DtrEnable = false;
            wait(100);
            serial.DtrEnable = true;
            
        }

        void wait(int ms)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (stopwatch.Elapsed.TotalMilliseconds  < ms)
            {
                Application.DoEvents();
                Thread.Sleep(1);
            }

        }

        public enum komendy
        {
            ping = 0x01,
            wersja = 0x02,
            stanBramek=0x03,
            nazwa=0x10
        }

        public class OdebraneDane
        {
            public byte kom;
            public  byte pen;
            public int speed;
            public int x;
            public int y;

            public OdebraneDane (byte _kom,byte[] dane)
            {
                kom = _kom;
                pen = dane[0];
                 speed = BitConverter.ToInt16(dane, 1);
                 x = BitConverter.ToInt16(dane, 3);
                 y = BitConverter.ToInt16(dane, 5);
            }
           
        }
    }
}
