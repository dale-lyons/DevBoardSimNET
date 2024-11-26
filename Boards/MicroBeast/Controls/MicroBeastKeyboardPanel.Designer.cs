namespace MicroBeast.Controls
{
    partial class MicroBeastKeyboardPanel
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
            this.keyboard1 = new MicroBeast.Controls.Keyboard();
            this.SuspendLayout();
            // 
            // keyboard1
            // 
            this.keyboard1.BackColor = System.Drawing.Color.IndianRed;
            this.keyboard1.Location = new System.Drawing.Point(0, 0);
            this.keyboard1.Name = "keyboard1";
            this.keyboard1.Size = new System.Drawing.Size(975, 282);
            this.keyboard1.TabIndex = 0;
            // 
            // MicroBeastKeyboardPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.keyboard1);
            this.Name = "MicroBeastKeyboardPanel";
            this.Size = new System.Drawing.Size(984, 292);
            this.ResumeLayout(false);

        }

        #endregion

        private Keyboard keyboard1;
    }
}
