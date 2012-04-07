using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Security.Cryptography;
using System.IO;
using System.Xml;
using System.Net;
using System.Threading;

namespace AutoPatcher
{
    public partial class Form1 : Form
    {
        string filesURL = "http://www.sampko.wz.cz/";
        string launcherURL = "http://sampko.wz.cz/launcher/";
        int v = 6;

        public string[] data;

        public Form1()
        {
            InitializeComponent();
            button1.Enabled = false;

            timer1.Enabled = false;
        }


        /**
         * Vyhlada vsetky subory v metinovy a posle ich spet na ref string[] files
         * 
         * string dir - cesta k metinovy, vedsinou "."
         */
        public void GetAllFiles(ref string[] files, string dir)
        {
            string[] folder = System.IO.Directory.GetDirectories(dir);
            string[] file = System.IO.Directory.GetFiles(dir);

            for (int i = 0; i < folder.Count(); i++)
            {
                string[] other = { };
                GetAllFiles(ref other, folder[i]);
                files = files.Concat(other).ToArray();
            }

            files = files.Concat(file).ToArray();
        }



        /**
         * Zahashuje obsah suboru do MD5 a potom ho vrati
         * 
         * string file - nazov vstupneho suboru
         * return hash - hash vstupneho suboru
         */
        public string GetHashFromFile(string file)
        {
            /* Oznamy uzivatelovy co aktualne roby */
            progressBar2.Value++;
            label1.Text = "Kontrolujem subor... " + file;
            label1.Refresh();

            string hash = "";
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

            try
            {
                FileStream open = new FileStream(file, FileMode.Open);
                md5.ComputeHash(open);
                open.Close();

                byte[] bHash = md5.Hash;
                foreach (byte b in bHash)
                {
                    hash += string.Format("{0:X2}", b);
                }
            }
            catch (System.IO.IOException e)
            {
                return "XXXX";
            }
            return hash;
        }



        /**
         * Stiahne a vyparsruje XML subor z webu
         * 
         * string URL - URL webu
         * 
         * @todo Treba opravit to xml.Read() aby tam nebolo 2x
         */
        public int parseFiles(string URL, ref string[] na, ref string[] ha)
        {
            try
            {
                int i = 0;
                /* opravit */
                XmlTextReader xml = new XmlTextReader(URL);
                XmlTextReader xml2 = new XmlTextReader(URL);

                while (xml2.Read())
                {
                    i++;
                }

                string[] name = new string[i];
                string[] hash = new string[i];
                i = 0;

                while (xml.Read())
                {
                    if (xml.NodeType == XmlNodeType.Element)
                    {
                        if (xml.Name == "data")
                        {
                            string n = "", h = "";

                            xml.MoveToNextAttribute();
                            if (xml.Name == "name")
                            {
                                n = xml.Value;
                            }

                            xml.MoveToNextAttribute();
                            if (xml.Name == "hash")
                            {
                                h = xml.Value;
                            }

                            if (n.Length > 3 && h.Length > 3)
                            {
                                name[i] = n;
                                hash[i] = h;
                                i++;
                            }
                        }
                    }
                }

                na = name;
                ha = hash;
            }
            catch (System.Xml.XmlException e)
            {
                MessageBox.Show("Chyba pri spojeni zo serverom!", "Error 4", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 1;
            }
            return 0;
        }



        /**
         * Skontroluje a porovna vsetky subory na disku zo subormy na servery a ak sa nezhoduju tak vykona prislusne operacie
         * 
         */
        public void checkFiles(string[] localName, string[] localHash, string[] remoteName, string[] remoteHash, ref string[] toDown, ref string[] toDel)
        {
            string[] download = new string[localName.Count() + remoteName.Count()];
            string[] delete = new string[localName.Count() + remoteName.Count()];
            int del = 0, down = 0;

            for (int i = 0; i < localName.Count(); i++)
            {
                int key = Array.IndexOf(remoteName, localName[i]);

                if (key == -1)
                {
                    delete[del] = localName[i];
                    del++;
                }
                else if (localHash[i] != remoteHash[key])
                {
                    download[down] = localName[i];
                    down++;
                    remoteName[key] = null;
                }
                else
                {
                    remoteName[key] = null;
                }
            }

            for (int i = 0; i < remoteName.Count(); i++)
            {
                if (remoteName[i] != null)
                {
                    download[down] = remoteName[i];
                    down++;
                }
            }

            string[] dwnl = new string[down];
            for (int i = 0; i < down; i++)
            {
                dwnl[i] = download[i];
            }

            string[] dlt = new string[del];
            for (int i = 0; i < del; i++)
            {
                dlt[i] = delete[i];
            }
            toDown = dwnl;
            toDel = dlt;
        }


        /* ProgressBar Handler */
        void downloadProgressBar(object sender, DownloadProgressChangedEventArgs e)
        {
            label1.Text = "Stahujem..." + this.data[progressBar2.Value - 1];
            label1.Refresh();
            progressBar1.Value = e.ProgressPercentage;
        }

        /* ProgressBar Handler */
        void downloadProgreessBarCompleted(object sender, AsyncCompletedEventArgs e)
        {
            progressBar1.Value = 0;
            progressBar2.Value++;
        }



        public void aktualizovat()
        {
            /* Zisti lokalne mena suborov */
            string[] localName = { };
            GetAllFiles(ref localName, ".");
            for (int i = 0; i < localName.Count(); i++)
            {
                localName[i] = localName[i].Remove(0, 2);
            }


            /* Vygeneruje hashe suborov */
            progressBar2.Maximum = localName.Count();
            progressBar2.Value = 0;

            string[] localHash = new string[localName.Count()];
            for (int i = 0; i < localName.Count(); i++)
            {
                localHash[i] = GetHashFromFile(localName[i]);
            }

            label1.Text = "Kontrola suborov... Hotovo!";


            /* Stiahne zo serveru pathinfo.xml */
            string[] remoteName = { };
            string[] remoteHash = { };
            if (parseFiles(this.filesURL + "pathinfo.xml", ref remoteName, ref remoteHash) == 1)
            {
                return;
            }


            /* Skotroluje subory na disku zo subormy na servery */
            string[] toDel = { };
            string[] toDown = { };
            checkFiles(localName, localHash, remoteName, remoteHash, ref toDown, ref toDel);


            /* Zmaze nepotrebne subory */
            progressBar2.Value = 0;
            progressBar2.Maximum = toDel.Count() + toDown.Count() + 1;

            for (int i = 0; i < toDel.Count(); i++)
            {
                try
                {
                    label1.Text = "Mazem subor... " + toDel[i];
                    progressBar2.Value++;
                    File.Delete(toDel[i]);
                }
                catch (Exception e) { }
            }


            /* Stiahne potrebne subory zo serveru */
            label1.Text = "";
            this.data = toDown;
            
            for (int i = 0; i < toDown.Count(); i++)
            {
                if (toDown[i] != "metin2.cfg" && toDown[i] != Path.GetFileName(Application.ExecutablePath))
                {
                    string[] test = toDown[i].Split('\\');
                    string dir = "";
                    for (int x = 0; x < test.Count() - 1; x++)
                    {
                        dir += "\\" + test[x];
                    }

                    if (dir.Length > 0 && !Directory.Exists("." + dir))
                    {
                        Directory.CreateDirectory("." + dir);
                    }

                    try
                    {
                        WebClient client = new WebClient();
                        client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloadProgressBar);
                        client.DownloadFileCompleted += new AsyncCompletedEventHandler(downloadProgreessBarCompleted);
                        client.DownloadFileAsync(new Uri(this.filesURL + "metin/" + toDown[i]), toDown[i]);
                    }
                    catch (Exception e)
                    {
                        //MessageBox.Show(e.ToString());
                        progressBar2.Value++;
                    }
                }
                else
                {
                    progressBar2.Value++;
                }
            }

            timer3.Enabled = true;
            selfUpdate();
        }


        public void selfUpdate()
        {
            int myVersion = this.v;
            int version = myVersion;

            try
            {
                XmlTextReader xml = new XmlTextReader(this.launcherURL + "version.xml");

                while (xml.Read())
                {
                    if (xml.Name == "launcher")
                    {
                        xml.MoveToNextAttribute();
                        if (xml.Name == "version")
                        {
                            version = Convert.ToInt32(xml.Value);
                            break;
                        }
                    }
                }

                if (version > myVersion)
                {
                    WebClient update = new WebClient();
                    update.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloadProgressBar);
                    update.DownloadFileCompleted += new AsyncCompletedEventHandler(downloadProgreessBarCompleted);
                    update.DownloadFileAsync(new Uri(this.launcherURL + "launcher.exe"), "launcher-new");
                    timer2.Enabled = true;
                }
                else
                {
                    progressBar2.Value++;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Chyba pri spojeni zo serverom!", "Error 3", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void timer2_Tick(object sender, EventArgs e)
        {
            if (progressBar2.Value == progressBar2.Maximum)
            {
                timer2.Enabled = false;

                if (File.Exists("updater.exe"))
                {
                    try
                    {
                        System.Diagnostics.Process.Start("updater.exe");
                        this.Close();
                    }
                    catch (Exception err) { }
                }
            }
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            aktualizovat();
        }





        private void button1_Click(object sender, EventArgs e)
        {
            if (File.Exists("Game.bin"))
            {
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = "Game.bin";
                p.StartInfo.UseShellExecute = false;
                p.Start();
                this.Close();
            }
            else
            {
                MessageBox.Show("Chyba hry!", "Error 2", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (File.Exists("config.exe"))
            {
                System.Diagnostics.Process.Start("config.exe");
            }
            else
            {
                MessageBox.Show("Konfiguracny program neexistuje!", "Error 1", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            if (progressBar2.Value == progressBar2.Maximum)
            {
                timer3.Enabled = false;
                button1.Enabled = true;
                label1.Text = "Hotovo!";
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Browser br = new Browser();
            br.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Registracia reg = new Registracia();
            reg.ShowDialog();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
