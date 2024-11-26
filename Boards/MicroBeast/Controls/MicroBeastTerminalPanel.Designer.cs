namespace MicroBeast.Controls
{
    partial class MicroBeastTerminalPanel
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
            this.display1 = new MicroBeast.Controls.Display();
            this.keyboard1 = new MicroBeast.Controls.Keyboard();
            this.terminalControl1 = new Terminal.TerminalControl();
            this.SuspendLayout();
            // 
            // display1
            // 
            this.display1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.display1.Location = new System.Drawing.Point(-14, -10);
            this.display1.Name = "display1";
            this.display1.Size = new System.Drawing.Size(1269, 423);
            this.display1.TabIndex = 0;
            // 
            // keyboard1
            // 
            this.keyboard1.BackColor = System.Drawing.Color.IndianRed;
            this.keyboard1.Location = new System.Drawing.Point(3, 688);
            this.keyboard1.Name = "keyboard1";
            this.keyboard1.Size = new System.Drawing.Size(975, 282);
            this.keyboard1.TabIndex = 1;
            // 
            // terminalControl1
            // 
            this.terminalControl1.BackColor = System.Drawing.Color.Black;
            this.terminalControl1.CharHeight = 21;
            this.terminalControl1.CharWidth = 11;
            this.terminalControl1.Cols = 109;
            this.terminalControl1.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.terminalControl1.Location = new System.Drawing.Point(0, 420);
            this.terminalControl1.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.terminalControl1.Name = "terminalControl1";
            this.terminalControl1.Rows = 12;
            this.terminalControl1.Size = new System.Drawing.Size(1206, 261);
            this.terminalControl1.TabIndex = 2;
            // 
            // MicroBeastTerminalPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.terminalControl1);
            this.Controls.Add(this.keyboard1);
            this.Controls.Add(this.display1);
            this.Name = "MicroBeastTerminalPanel";
            this.Size = new System.Drawing.Size(1565, 1128);
            this.ResumeLayout(false);

        }

        #endregion
        private Display display1;
        private Keyboard keyboard1;
        private Terminal.TerminalControl terminalControl1;
    }
}
