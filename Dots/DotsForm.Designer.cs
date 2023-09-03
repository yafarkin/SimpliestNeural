namespace Dots
{
    partial class DotsForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            pnlControl = new Panel();
            btnStartStop = new Button();
            pnlView = new Panel();
            pbImg = new PictureBox();
            tCount = new System.Windows.Forms.Timer(components);
            tDraw = new System.Windows.Forms.Timer(components);
            pnlControl.SuspendLayout();
            pnlView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pbImg).BeginInit();
            SuspendLayout();
            // 
            // pnlControl
            // 
            pnlControl.Controls.Add(btnStartStop);
            pnlControl.Dock = DockStyle.Top;
            pnlControl.Location = new Point(0, 0);
            pnlControl.Name = "pnlControl";
            pnlControl.Size = new Size(800, 58);
            pnlControl.TabIndex = 1;
            // 
            // btnStartStop
            // 
            btnStartStop.Location = new Point(12, 12);
            btnStartStop.Name = "btnStartStop";
            btnStartStop.Size = new Size(112, 34);
            btnStartStop.TabIndex = 1;
            btnStartStop.Text = "Start";
            btnStartStop.UseVisualStyleBackColor = true;
            btnStartStop.Click += btnStartStop_Click;
            // 
            // pnlView
            // 
            pnlView.Controls.Add(pbImg);
            pnlView.Dock = DockStyle.Fill;
            pnlView.Location = new Point(0, 58);
            pnlView.Name = "pnlView";
            pnlView.Size = new Size(800, 392);
            pnlView.TabIndex = 2;
            // 
            // pbImg
            // 
            pbImg.Dock = DockStyle.Fill;
            pbImg.Location = new Point(0, 0);
            pbImg.Name = "pbImg";
            pbImg.Size = new Size(800, 392);
            pbImg.SizeMode = PictureBoxSizeMode.StretchImage;
            pbImg.TabIndex = 0;
            pbImg.TabStop = false;
            pbImg.Click += pbImg_Click;
            // 
            // tCount
            // 
            tCount.Interval = 1000;
            tCount.Tick += tCount_Tick;
            // 
            // tDraw
            // 
            tDraw.Interval = 10;
            tDraw.Tick += tDraw_Tick;
            // 
            // DotsForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(pnlView);
            Controls.Add(pnlControl);
            MinimizeBox = false;
            Name = "DotsForm";
            ShowIcon = false;
            Text = "DotsForm";
            Load += DotsForm_Load;
            SizeChanged += DotsForm_SizeChanged;
            pnlControl.ResumeLayout(false);
            pnlView.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pbImg).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlControl;
        private Button btnStartStop;
        private Panel pnlView;
        private PictureBox pbImg;
        private System.Windows.Forms.Timer tCount;
        private System.Windows.Forms.Timer tDraw;
    }
}