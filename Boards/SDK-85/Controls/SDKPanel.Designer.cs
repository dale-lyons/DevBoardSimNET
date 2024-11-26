namespace SDK_85.Controls
{
    partial class SDKPanel
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ioPort1 = new SDK_85.Controls.IOPort();
            this.eightSegmentDisplay7 = new SDK_85.Controls.EightSegmentDisplay();
            this.eightSegmentDisplay6 = new SDK_85.Controls.EightSegmentDisplay();
            this.eightSegmentDisplay5 = new SDK_85.Controls.EightSegmentDisplay();
            this.eightSegmentDisplay4 = new SDK_85.Controls.EightSegmentDisplay();
            this.eightSegmentDisplay3 = new SDK_85.Controls.EightSegmentDisplay();
            this.eightSegmentDisplay2 = new SDK_85.Controls.EightSegmentDisplay();
            this.eightSegmentDisplay1 = new SDK_85.Controls.EightSegmentDisplay();
            this.keypad1 = new SDK_85.Controls.Keypad();
            this.ioPort2 = new SDK_85.Controls.IOPort();
            this.ioPort3 = new SDK_85.Controls.IOPort();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.ioPort5 = new SDK_85.Controls.IOPort();
            this.ioPort4 = new SDK_85.Controls.IOPort();
            this.csr1 = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // ioPort1
            // 
            this.ioPort1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ioPort1.IOPortTitle = "Port A";
            this.ioPort1.Location = new System.Drawing.Point(0, 3);
            this.ioPort1.Name = "ioPort1";
            this.ioPort1.Size = new System.Drawing.Size(442, 137);
            this.ioPort1.TabIndex = 14;
            // 
            // eightSegmentDisplay7
            // 
            this.eightSegmentDisplay7.Code = ((byte)(0));
            this.eightSegmentDisplay7.Location = new System.Drawing.Point(891, 23);
            this.eightSegmentDisplay7.Name = "eightSegmentDisplay7";
            this.eightSegmentDisplay7.Size = new System.Drawing.Size(42, 62);
            this.eightSegmentDisplay7.TabIndex = 13;
            // 
            // eightSegmentDisplay6
            // 
            this.eightSegmentDisplay6.Code = ((byte)(0));
            this.eightSegmentDisplay6.Location = new System.Drawing.Point(843, 23);
            this.eightSegmentDisplay6.Name = "eightSegmentDisplay6";
            this.eightSegmentDisplay6.Size = new System.Drawing.Size(42, 62);
            this.eightSegmentDisplay6.TabIndex = 12;
            // 
            // eightSegmentDisplay5
            // 
            this.eightSegmentDisplay5.Code = ((byte)(0));
            this.eightSegmentDisplay5.Location = new System.Drawing.Point(795, 23);
            this.eightSegmentDisplay5.Name = "eightSegmentDisplay5";
            this.eightSegmentDisplay5.Size = new System.Drawing.Size(42, 62);
            this.eightSegmentDisplay5.TabIndex = 11;
            // 
            // eightSegmentDisplay4
            // 
            this.eightSegmentDisplay4.Code = ((byte)(0));
            this.eightSegmentDisplay4.Location = new System.Drawing.Point(747, 23);
            this.eightSegmentDisplay4.Name = "eightSegmentDisplay4";
            this.eightSegmentDisplay4.Size = new System.Drawing.Size(42, 62);
            this.eightSegmentDisplay4.TabIndex = 10;
            // 
            // eightSegmentDisplay3
            // 
            this.eightSegmentDisplay3.Code = ((byte)(0));
            this.eightSegmentDisplay3.Location = new System.Drawing.Point(699, 23);
            this.eightSegmentDisplay3.Name = "eightSegmentDisplay3";
            this.eightSegmentDisplay3.Size = new System.Drawing.Size(42, 62);
            this.eightSegmentDisplay3.TabIndex = 9;
            // 
            // eightSegmentDisplay2
            // 
            this.eightSegmentDisplay2.Code = ((byte)(0));
            this.eightSegmentDisplay2.Location = new System.Drawing.Point(651, 23);
            this.eightSegmentDisplay2.Name = "eightSegmentDisplay2";
            this.eightSegmentDisplay2.Size = new System.Drawing.Size(42, 62);
            this.eightSegmentDisplay2.TabIndex = 8;
            // 
            // eightSegmentDisplay1
            // 
            this.eightSegmentDisplay1.Code = ((byte)(0));
            this.eightSegmentDisplay1.Location = new System.Drawing.Point(603, 23);
            this.eightSegmentDisplay1.Name = "eightSegmentDisplay1";
            this.eightSegmentDisplay1.Size = new System.Drawing.Size(42, 62);
            this.eightSegmentDisplay1.TabIndex = 7;
            // 
            // keypad1
            // 
            this.keypad1.Location = new System.Drawing.Point(603, 134);
            this.keypad1.Name = "keypad1";
            this.keypad1.Size = new System.Drawing.Size(540, 362);
            this.keypad1.TabIndex = 6;
            this.keypad1.OnKeypress += new SDK_85.Controls.Keypad.KeypressDelegate(this.keypad1_OnKeypress);
            this.keypad1.onReset += new SDK_85.Controls.Keypad.ResetDelegate(this.keypad1_onReset);
            // 
            // ioPort2
            // 
            this.ioPort2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ioPort2.IOPortTitle = "Port B";
            this.ioPort2.Location = new System.Drawing.Point(3, 146);
            this.ioPort2.Name = "ioPort2";
            this.ioPort2.Size = new System.Drawing.Size(442, 137);
            this.ioPort2.TabIndex = 15;
            // 
            // ioPort3
            // 
            this.ioPort3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ioPort3.IOPortTitle = "Port C";
            this.ioPort3.Location = new System.Drawing.Point(3, 289);
            this.ioPort3.Name = "ioPort3";
            this.ioPort3.Size = new System.Drawing.Size(442, 137);
            this.ioPort3.TabIndex = 16;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(3, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(594, 549);
            this.tabControl1.TabIndex = 17;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.csr1);
            this.tabPage1.Controls.Add(this.ioPort1);
            this.tabPage1.Controls.Add(this.ioPort3);
            this.tabPage1.Controls.Add(this.ioPort2);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(586, 523);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "8155-1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.ioPort5);
            this.tabPage2.Controls.Add(this.ioPort4);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(586, 523);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "8755-1";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // ioPort5
            // 
            this.ioPort5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ioPort5.IOPortTitle = "Port B";
            this.ioPort5.Location = new System.Drawing.Point(6, 149);
            this.ioPort5.Name = "ioPort5";
            this.ioPort5.Size = new System.Drawing.Size(442, 137);
            this.ioPort5.TabIndex = 16;
            // 
            // ioPort4
            // 
            this.ioPort4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ioPort4.IOPortTitle = "Port A";
            this.ioPort4.Location = new System.Drawing.Point(6, 6);
            this.ioPort4.Name = "ioPort4";
            this.ioPort4.Size = new System.Drawing.Size(442, 137);
            this.ioPort4.TabIndex = 15;
            // 
            // csr1
            // 
            this.csr1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.csr1.Location = new System.Drawing.Point(6, 448);
            this.csr1.Name = "csr1";
            this.csr1.Size = new System.Drawing.Size(182, 23);
            this.csr1.TabIndex = 17;
            this.csr1.Text = "label1";
            // 
            // SDKPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.eightSegmentDisplay7);
            this.Controls.Add(this.eightSegmentDisplay6);
            this.Controls.Add(this.eightSegmentDisplay5);
            this.Controls.Add(this.eightSegmentDisplay4);
            this.Controls.Add(this.eightSegmentDisplay3);
            this.Controls.Add(this.eightSegmentDisplay2);
            this.Controls.Add(this.eightSegmentDisplay1);
            this.Controls.Add(this.keypad1);
            this.Name = "SDKPanel";
            this.Size = new System.Drawing.Size(1168, 555);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private Keypad keypad1;
        private EightSegmentDisplay eightSegmentDisplay1;
        private EightSegmentDisplay eightSegmentDisplay2;
        private EightSegmentDisplay eightSegmentDisplay3;
        private EightSegmentDisplay eightSegmentDisplay4;
        private EightSegmentDisplay eightSegmentDisplay5;
        private EightSegmentDisplay eightSegmentDisplay6;
        private EightSegmentDisplay eightSegmentDisplay7;
        private IOPort ioPort1;
        private IOPort ioPort2;
        private IOPort ioPort3;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private IOPort ioPort5;
        private IOPort ioPort4;
        private System.Windows.Forms.Label csr1;
    }
}
