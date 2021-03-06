﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Xml;
using System.Net;
using System.Threading;

namespace updater {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e) { }


        private void timer1_Tick(object sender, EventArgs e) {
            if (progressBar1.Value < 100)
                progressBar1.Value++;
        }


        private void progressBar1_Click(object sender, EventArgs e) { }


        private void timer2_Tick(object sender, EventArgs e) {
            timer2.Enabled = false;
            if (File.Exists("autopatcher-new")) {
                if (File.Exists("AutoPatcher.exe"))
                    File.Delete("AutoPatcher.exe");

                File.Copy("autopatcher-new", "AutoPatcher.exe");
                File.Delete("autopatcher-new");
                System.Diagnostics.Process.Start("AutoPatcher.exe");
            }
            this.Close();
        }

    }
}
