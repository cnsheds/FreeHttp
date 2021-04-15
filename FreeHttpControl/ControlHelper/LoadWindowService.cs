﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FreeHttp.FreeHttpControl.ControlHelper
{
    public class LoadWindowService
    {
        Form loadForm = null;
        LoadBitmap loadBitmap = new LoadBitmap(new System.Drawing.Size(100,100));
        PictureBox pictureBox = new PictureBox();
        Timer timer = new System.Windows.Forms.Timer();
        int loadTime = 0;
        bool isInload = false;

        public LoadWindowService()
        {
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            timer.Interval = 300;
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if(loadForm==null || loadForm.Created==false)
            {
                StopLoad();
                return;
            }
            pictureBox.Image = loadBitmap.DrawCircle(loadTime);
            loadTime++;
        }

        public void StartLoad(Form form)
        {
            if (isInload) return;
            loadForm = form;
            loadForm.Controls.Add(pictureBox);
            loadForm.FormClosed += new FormClosedEventHandler((o, e) => { StopLoad(); });
            pictureBox.Dock = DockStyle.Fill;
            pictureBox.BringToFront();
            loadBitmap.SetSize(pictureBox.Width > pictureBox.Height ? pictureBox.Height : pictureBox.Width);
            isInload = true;
            loadTime = 0;
            timer.Start();
        }

        public void StopLoad()
        {
            if (!isInload) return;
            loadForm?.Controls.Remove(pictureBox);
            loadForm = null;
            isInload = false;
            timer.Stop();
        }
    }
}
