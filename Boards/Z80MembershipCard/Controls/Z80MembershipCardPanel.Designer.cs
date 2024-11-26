namespace Z80MembershipCard.Controls
{
    partial class Z80MembershipCardPanel
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
            this.terminalControl1 = new Terminal.TerminalControl();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sendHexFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadHexFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.preferencesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sendSpaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // terminalControl1
            // 
            this.terminalControl1.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.terminalControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.terminalControl1.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.terminalControl1.Location = new System.Drawing.Point(0, 24);
            this.terminalControl1.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.terminalControl1.Name = "terminalControl1";
            this.terminalControl1.Size = new System.Drawing.Size(599, 289);
            this.terminalControl1.TabIndex = 0;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.debugToolStripMenuItem,
            this.preferencesToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(599, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // debugToolStripMenuItem
            // 
            this.debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sendHexFileToolStripMenuItem,
            this.loadHexFileToolStripMenuItem,
            this.sendSpaceToolStripMenuItem});
            this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            this.debugToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.debugToolStripMenuItem.Text = "Debug";
            // 
            // sendHexFileToolStripMenuItem
            // 
            this.sendHexFileToolStripMenuItem.Name = "sendHexFileToolStripMenuItem";
            this.sendHexFileToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.sendHexFileToolStripMenuItem.Text = "SendHexFile";
            this.sendHexFileToolStripMenuItem.Click += new System.EventHandler(this.sendHexFileToolStripMenuItem_Click);
            // 
            // loadHexFileToolStripMenuItem
            // 
            this.loadHexFileToolStripMenuItem.Name = "loadHexFileToolStripMenuItem";
            this.loadHexFileToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.loadHexFileToolStripMenuItem.Text = "LoadHexFile";
            this.loadHexFileToolStripMenuItem.Click += new System.EventHandler(this.loadHexFileToolStripMenuItem_Click);
            // 
            // preferencesToolStripMenuItem
            // 
            this.preferencesToolStripMenuItem.Name = "preferencesToolStripMenuItem";
            this.preferencesToolStripMenuItem.Size = new System.Drawing.Size(80, 20);
            this.preferencesToolStripMenuItem.Text = "Preferences";
            this.preferencesToolStripMenuItem.Click += new System.EventHandler(this.preferencesToolStripMenuItem_Click);
            // 
            // sendSpaceToolStripMenuItem
            // 
            this.sendSpaceToolStripMenuItem.Name = "sendSpaceToolStripMenuItem";
            this.sendSpaceToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.sendSpaceToolStripMenuItem.Text = "SendSpace";
            this.sendSpaceToolStripMenuItem.Click += new System.EventHandler(this.sendSpaceToolStripMenuItem_Click);
            // 
            // Z80MembershipCardPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.terminalControl1);
            this.Controls.Add(this.menuStrip1);
            this.Name = "Z80MembershipCardPanel";
            this.Size = new System.Drawing.Size(599, 313);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Terminal.TerminalControl terminalControl1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sendHexFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadHexFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem preferencesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sendSpaceToolStripMenuItem;
    }
}
