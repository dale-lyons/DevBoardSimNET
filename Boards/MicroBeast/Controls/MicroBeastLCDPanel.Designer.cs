namespace MicroBeast.Controls
{
    partial class MicroBeastLCDPanel
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
            this.SuspendLayout();
            // 
            // display1
            // 
            this.display1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.display1.Location = new System.Drawing.Point(3, -9);
            this.display1.Name = "display1";
            this.display1.Size = new System.Drawing.Size(827, 574);
            this.display1.TabIndex = 0;
            // 
            // MicroBeastLCDPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.display1);
            this.Name = "MicroBeastLCDPanel";
            this.Size = new System.Drawing.Size(858, 575);
            this.ResumeLayout(false);

        }

        #endregion

        private Display display1;
    }
}
