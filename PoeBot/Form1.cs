using PoeBot.Core;
using PoeBot.Core.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PoeBot
{
    public partial class Form1 : Form
    {
        PoeBot.Core.BotEgine engine;
        public Form1()
        {
            InitializeComponent();
            _Logger = new LoggerService();
            notifyIcon1.Visible = true;
        }

        private void OpenClick(object sender, EventArgs e)
        {
            this.Show();
        }
        bool isRunning = false;
        private void StartStopClick(object sender, EventArgs e)
        {
            if (isRunning)
            {
                isRunning = false;
                engine.Stop();
                btnStartStop.Text = "Start";
            }
            else
            {
                try
                {

                    if (engine == null)
                        engine = new Core.BotEgine(_Logger);
                    engine.Start();
                    btnStartStop.Text = "Stop";
                }
                catch(Exception ex)
                {
                    _Logger.Log(ex.Message);
                    btnStartStop.Text = "Start";
                    isRunning = false;
                    engine.Stop();
                    return;
                }
                isRunning = true;
                this.Hide();
            }
        }
        LoggerService _Logger;

        private void ExitClick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Tests.TradeisGreen();
        }
    }
}
