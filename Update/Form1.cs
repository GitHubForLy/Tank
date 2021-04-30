using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace Update
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        private bool IsUpdateing=false;
        private UpdateClient client;
        private Thread thread;

        public void DoUpdate(string TargetDirectory)
        {
            thread = new Thread(() =>
            {
                try
                {
                    client = new UpdateClient(Program.ServerIpPort, TargetDirectory);
                    client.ProgressEvent += Client_ProgressEvent;
                    client.BytesEvent += Client_BytesEvent;
                    client.StateEvent += Client_StateEvent;
                    IsUpdateing = true;

                    client.Update(Program.Version);

                    IsUpdateing = false;
                }
                catch(Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("error:" + e.Message);
                }
            });
            thread.IsBackground = false;
            thread.Start();

        }

        private void Client_StateEvent(string text)
        {
            if(IsUpdateing)
            {
                try
                {
                    this.Invoke(new Action(() =>
                {
                    lb_tip.Text = text;
                }));
                }
                catch(Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }
        }

        private void Client_BytesEvent(int btyes)
        {
            if (IsUpdateing)
            {
                try
                {
                    lb_bytes.Invoke(new Action(() =>
                    {
                        if (btyes < 1024 * 1024)
                            lb_bytes.Text = Math.Round(btyes / 1024f, 2).ToString() + " kb / s";
                        else
                            lb_bytes.Text = Math.Round(btyes / 1024f / 1024f, 2).ToString() + " mb / s";
                    }));
                }
                catch(Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }
        }

        private void Client_ProgressEvent(int res)
        {  
            if(IsUpdateing)
            {
                try
                {
                    this.Invoke(new Action((() =>
                    {
                        pro_bar.Value = res;
                        lb_rate.Text = res + "%";
                    })));
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            DoUpdate(Program.TargetPath);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(IsUpdateing)
            {
                if(DialogResult.Yes== MessageBox.Show("尚未更新完成，是否退出?", "提示", MessageBoxButtons.YesNo))
                {
                    IsUpdateing = false;
                    client.StopUpdate();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
