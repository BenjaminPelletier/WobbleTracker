
namespace DataCollector
{
    partial class Main
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tsslGPS = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslSensors = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslError = new System.Windows.Forms.ToolStripStatusLabel();
            this.txtSensors = new System.Windows.Forms.TextBox();
            this.txtGPS = new System.Windows.Forms.TextBox();
            this.tsbReset = new System.Windows.Forms.ToolStripDropDownButton();
            this.cmdStartStop = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1153, 36);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(54, 29);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsslGPS,
            this.tsslSensors,
            this.tsslError,
            this.tsbReset});
            this.statusStrip1.Location = new System.Drawing.Point(0, 722);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1153, 32);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tsslGPS
            // 
            this.tsslGPS.Name = "tsslGPS";
            this.tsslGPS.Size = new System.Drawing.Size(176, 25);
            this.tsslGPS.Text = "Awaiting first GPS fix";
            // 
            // tsslSensors
            // 
            this.tsslSensors.Name = "tsslSensors";
            this.tsslSensors.Size = new System.Drawing.Size(243, 25);
            this.tsslSensors.Text = "No sensor measurements yet";
            // 
            // tsslError
            // 
            this.tsslError.Name = "tsslError";
            this.tsslError.Size = new System.Drawing.Size(116, 25);
            this.tsslError.Text = "No errors yet";
            // 
            // txtSensors
            // 
            this.txtSensors.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSensors.BackColor = System.Drawing.Color.Black;
            this.txtSensors.Font = new System.Drawing.Font("Arial Narrow", 60F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSensors.ForeColor = System.Drawing.Color.White;
            this.txtSensors.Location = new System.Drawing.Point(12, 175);
            this.txtSensors.Multiline = true;
            this.txtSensors.Name = "txtSensors";
            this.txtSensors.Size = new System.Drawing.Size(1129, 544);
            this.txtSensors.TabIndex = 3;
            // 
            // txtGPS
            // 
            this.txtGPS.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtGPS.BackColor = System.Drawing.Color.Black;
            this.txtGPS.Font = new System.Drawing.Font("Arial Narrow", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtGPS.ForeColor = System.Drawing.Color.White;
            this.txtGPS.Location = new System.Drawing.Point(12, 36);
            this.txtGPS.Multiline = true;
            this.txtGPS.Name = "txtGPS";
            this.txtGPS.Size = new System.Drawing.Size(1129, 133);
            this.txtGPS.TabIndex = 4;
            // 
            // tsbReset
            // 
            this.tsbReset.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbReset.Image = ((System.Drawing.Image)(resources.GetObject("tsbReset.Image")));
            this.tsbReset.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbReset.Name = "tsbReset";
            this.tsbReset.Size = new System.Drawing.Size(42, 29);
            this.tsbReset.Text = "toolStripDropDownButton1";
            this.tsbReset.Click += new System.EventHandler(this.tsbReset_Click);
            // 
            // cmdStartStop
            // 
            this.cmdStartStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmdStartStop.BackColor = System.Drawing.Color.Maroon;
            this.cmdStartStop.Font = new System.Drawing.Font("Arial Narrow", 28F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdStartStop.ForeColor = System.Drawing.Color.White;
            this.cmdStartStop.Location = new System.Drawing.Point(12, 614);
            this.cmdStartStop.Name = "cmdStartStop";
            this.cmdStartStop.Size = new System.Drawing.Size(218, 105);
            this.cmdStartStop.TabIndex = 5;
            this.cmdStartStop.Text = "Start";
            this.cmdStartStop.UseVisualStyleBackColor = false;
            this.cmdStartStop.Click += new System.EventHandler(this.cmdStartStop_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1153, 754);
            this.Controls.Add(this.cmdStartStop);
            this.Controls.Add(this.txtGPS);
            this.Controls.Add(this.txtSensors);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WobbleTracker";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.Load += new System.EventHandler(this.Main_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tsslGPS;
        private System.Windows.Forms.ToolStripStatusLabel tsslError;
        private System.Windows.Forms.ToolStripStatusLabel tsslSensors;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.TextBox txtSensors;
        private System.Windows.Forms.TextBox txtGPS;
        private System.Windows.Forms.ToolStripDropDownButton tsbReset;
        private System.Windows.Forms.Button cmdStartStop;
    }
}

