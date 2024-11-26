namespace GUI.Output
{
    partial class OutputView
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
            this.lb1 = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // lb1
            // 
            this.lb1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lb1.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb1.FormattingEnabled = true;
            this.lb1.ItemHeight = 18;
            this.lb1.Location = new System.Drawing.Point(0, 0);
            this.lb1.Name = "lb1";
            this.lb1.Size = new System.Drawing.Size(800, 450);
            this.lb1.TabIndex = 0;
            // 
            // OutputView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.lb1);
            this.Name = "OutputView";
            this.Text = "OutputView";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lb1;
    }
}