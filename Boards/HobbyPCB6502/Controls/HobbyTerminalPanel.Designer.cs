namespace HobbyPCB6502.Controls
{
    partial class HobbyTerminalPanel
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
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.terminalControl1 = new Terminal.TerminalControl();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // terminalControl1
            // 
            this.terminalControl1.BackColor = System.Drawing.Color.Black;
            this.terminalControl1.CharHeight = 21;
            this.terminalControl1.CharWidth = 11;
            this.terminalControl1.Cols = 81;
            this.terminalControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.terminalControl1.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.terminalControl1.Location = new System.Drawing.Point(0, 0);
            this.terminalControl1.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.terminalControl1.Name = "terminalControl1";
            this.terminalControl1.Rows = 25;
            this.terminalControl1.Size = new System.Drawing.Size(893, 536);
            this.terminalControl1.TabIndex = 3;
            // 
            // HobbyTerminalPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.terminalControl1);
            this.Name = "HobbyTerminalPanel";
            this.Size = new System.Drawing.Size(893, 536);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private Terminal.TerminalControl terminalControl1;
    }
}
