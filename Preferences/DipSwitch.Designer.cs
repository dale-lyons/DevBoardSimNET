namespace Preferences
{
    partial class DipSwitch
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
            this.SuspendLayout();
            // 
            // DipSwitch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(22F, 42F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.IndianRed;
            this.Font = new System.Drawing.Font("Times New Roman", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(10);
            this.Name = "DipSwitch";
            this.Size = new System.Drawing.Size(1882, 802);
            this.Load += new System.EventHandler(this.DipSwitch_Load);
            this.SizeChanged += new System.EventHandler(this.DipSwitch_SizeChanged);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.DipSwitch_Paint);
            this.Resize += new System.EventHandler(this.DipSwitch_Resize);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
