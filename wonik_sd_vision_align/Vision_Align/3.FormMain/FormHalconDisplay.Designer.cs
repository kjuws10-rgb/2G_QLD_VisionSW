namespace Vision_Align
{
    partial class FormHalconDisplay
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
            this.hControl_Display = new HalconDotNet.HSmartWindowControl();
            this.SuspendLayout();
            // 
            // hControl_Display
            // 
            this.hControl_Display.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.hControl_Display.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
            this.hControl_Display.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hControl_Display.HDoubleClickToFitContent = true;
            this.hControl_Display.HDrawingObjectsModifier = HalconDotNet.HSmartWindowControl.DrawingObjectsModifier.None;
            this.hControl_Display.HImagePart = new System.Drawing.Rectangle(0, 0, 640, 480);
            this.hControl_Display.HKeepAspectRatio = false;
            this.hControl_Display.HMoveContent = true;
            this.hControl_Display.HZoomContent = HalconDotNet.HSmartWindowControl.ZoomContent.WheelForwardZoomsIn;
            this.hControl_Display.Location = new System.Drawing.Point(0, 0);
            this.hControl_Display.Margin = new System.Windows.Forms.Padding(0);
            this.hControl_Display.Name = "hControl_Display";
            this.hControl_Display.Size = new System.Drawing.Size(582, 429);
            this.hControl_Display.TabIndex = 1166;
            this.hControl_Display.WindowSize = new System.Drawing.Size(582, 429);
            // 
            // FormHalconDisplay
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(582, 429);
            this.Controls.Add(this.hControl_Display);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormHalconDisplay";
            this.Text = "FormHalconDisplay";
            this.ResumeLayout(false);

        }

        #endregion

        private HalconDotNet.HSmartWindowControl hControl_Display;
    }
}