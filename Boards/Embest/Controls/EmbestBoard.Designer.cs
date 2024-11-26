namespace ARMSim.Plugins.UIControls
{
    partial class EmbestBoard
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
            this.leds1 = new ARMSim.Plugins.UIControls.Leds();
            this.lcd1 = new ARMSim.Plugins.UIControls.Lcd();
            this.blueButtons1 = new ARMSim.Plugins.UIControls.BlueButtons();
            this.blackButtons1 = new ARMSim.Plugins.UIControls.BlackButtons();
            this.eightSegmentDisplayControl1 = new ARMSim.Plugins.UIControls.EightSegmentDisplay();
            this.SuspendLayout();
            // 
            // leds1
            // 
            this.leds1.LeftLed = false;
            this.leds1.Location = new System.Drawing.Point(52, 0);
            this.leds1.Name = "leds1";
            this.leds1.RightLed = false;
            this.leds1.Size = new System.Drawing.Size(123, 62);
            this.leds1.TabIndex = 10;
            // 
            // lcd1
            // 
            this.lcd1.Location = new System.Drawing.Point(489, 3);
            this.lcd1.Name = "lcd1";
            this.lcd1.Size = new System.Drawing.Size(320, 240);
            this.lcd1.TabIndex = 9;
            // 
            // blueButtons1
            // 
            this.blueButtons1.Location = new System.Drawing.Point(305, 3);
            this.blueButtons1.Name = "blueButtons1";
            this.blueButtons1.Size = new System.Drawing.Size(178, 178);
            this.blueButtons1.TabIndex = 8;
            // 
            // blackButtons1
            // 
            this.blackButtons1.Location = new System.Drawing.Point(181, 0);
            this.blackButtons1.Name = "blackButtons1";
            this.blackButtons1.Size = new System.Drawing.Size(118, 58);
            this.blackButtons1.TabIndex = 7;
            // 
            // eightSegmentDisplayControl1
            // 
            this.eightSegmentDisplayControl1.Code = ((byte)(0));
            this.eightSegmentDisplayControl1.Location = new System.Drawing.Point(4, 3);
            this.eightSegmentDisplayControl1.Name = "eightSegmentDisplayControl1";
            this.eightSegmentDisplayControl1.Size = new System.Drawing.Size(42, 62);
            this.eightSegmentDisplayControl1.TabIndex = 6;
            // 
            // EmbestBoard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.leds1);
            this.Controls.Add(this.lcd1);
            this.Controls.Add(this.blueButtons1);
            this.Controls.Add(this.blackButtons1);
            this.Controls.Add(this.eightSegmentDisplayControl1);
            this.Name = "EmbestBoard";
            this.Size = new System.Drawing.Size(814, 249);
            this.ResumeLayout(false);

        }

        #endregion

        private Leds leds1;
        private Lcd lcd1;
        private BlueButtons blueButtons1;
        private BlackButtons blackButtons1;
        private EightSegmentDisplay eightSegmentDisplayControl1;
    }
}
