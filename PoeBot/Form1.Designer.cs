namespace PoeBot
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.Menu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startStopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnStartStop = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.testTab = new System.Windows.Forms.TabPage();
            this.tradeTab = new System.Windows.Forms.TabPage();
            this.followTab = new System.Windows.Forms.TabPage();
            this.farmTab = new System.Windows.Forms.TabPage();
            this.Menu.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.testTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.Menu;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "PoeBot";
            this.notifyIcon1.Visible = true;
            // 
            // Menu
            // 
            this.Menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.startStopToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.Menu.Name = "Menu";
            this.Menu.Size = new System.Drawing.Size(128, 70);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.OpenClick);
            // 
            // startStopToolStripMenuItem
            // 
            this.startStopToolStripMenuItem.Name = "startStopToolStripMenuItem";
            this.startStopToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.startStopToolStripMenuItem.Text = "Start/Stop";
            this.startStopToolStripMenuItem.Click += new System.EventHandler(this.StartStopClick);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitClick);
            // 
            // btnStartStop
            // 
            this.btnStartStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStartStop.Location = new System.Drawing.Point(348, 506);
            this.btnStartStop.Name = "btnStartStop";
            this.btnStartStop.Size = new System.Drawing.Size(72, 24);
            this.btnStartStop.TabIndex = 1;
            this.btnStartStop.Text = "Start";
            this.btnStartStop.UseVisualStyleBackColor = true;
            this.btnStartStop.Click += new System.EventHandler(this.StartStopClick);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(6, 6);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(127, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Test Currency Validator";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.testTab);
            this.tabControl1.Controls.Add(this.tradeTab);
            this.tabControl1.Controls.Add(this.followTab);
            this.tabControl1.Controls.Add(this.farmTab);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(417, 488);
            this.tabControl1.TabIndex = 3;
            // 
            // testTab
            // 
            this.testTab.AutoScroll = true;
            this.testTab.Controls.Add(this.button1);
            this.testTab.Location = new System.Drawing.Point(4, 22);
            this.testTab.Margin = new System.Windows.Forms.Padding(0);
            this.testTab.Name = "testTab";
            this.testTab.Size = new System.Drawing.Size(409, 462);
            this.testTab.TabIndex = 0;
            this.testTab.Text = "Test";
            this.testTab.UseVisualStyleBackColor = true;
            // 
            // tradeTab
            // 
            this.tradeTab.AutoScroll = true;
            this.tradeTab.Location = new System.Drawing.Point(4, 22);
            this.tradeTab.Name = "tradeTab";
            this.tradeTab.Padding = new System.Windows.Forms.Padding(3);
            this.tradeTab.Size = new System.Drawing.Size(409, 462);
            this.tradeTab.TabIndex = 1;
            this.tradeTab.Text = "Trade";
            this.tradeTab.UseVisualStyleBackColor = true;
            // 
            // followTab
            // 
            this.followTab.AutoScroll = true;
            this.followTab.Location = new System.Drawing.Point(4, 22);
            this.followTab.Name = "followTab";
            this.followTab.Padding = new System.Windows.Forms.Padding(3);
            this.followTab.Size = new System.Drawing.Size(409, 462);
            this.followTab.TabIndex = 2;
            this.followTab.Text = "Follow";
            this.followTab.UseVisualStyleBackColor = true;
            // 
            // farmTab
            // 
            this.farmTab.AutoScroll = true;
            this.farmTab.Location = new System.Drawing.Point(4, 22);
            this.farmTab.Name = "farmTab";
            this.farmTab.Padding = new System.Windows.Forms.Padding(3);
            this.farmTab.Size = new System.Drawing.Size(409, 462);
            this.farmTab.TabIndex = 3;
            this.farmTab.Text = "Farm";
            this.farmTab.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(432, 542);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.btnStartStop);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Menu.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.testTab.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip Menu;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startStopToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Button btnStartStop;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage testTab;
        private System.Windows.Forms.TabPage tradeTab;
        private System.Windows.Forms.TabPage followTab;
        private System.Windows.Forms.TabPage farmTab;
    }
}

