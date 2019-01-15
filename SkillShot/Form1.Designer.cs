namespace SkillShot
{
    partial class SkillShot
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
            this.components = new System.ComponentModel.Container();
            this.startButton = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.RefreshTimer = new System.Windows.Forms.Timer(this.components);
            this.button1 = new System.Windows.Forms.Button();
            this.debugButton = new System.Windows.Forms.Button();
            this.botStats = new System.Windows.Forms.Label();
            this.status = new System.Windows.Forms.Label();
            this.resultBox = new System.Windows.Forms.ListBox();
            this.colors = new System.Windows.Forms.TextBox();
            this.clear = new System.Windows.Forms.Button();
            this.errorBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(3, 11);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(133, 46);
            this.startButton.TabIndex = 0;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(3, 65);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(1232, 128);
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // RefreshTimer
            // 
            this.RefreshTimer.Interval = 1;
            this.RefreshTimer.Tick += new System.EventHandler(this.RefreshTimer_Tick);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(231, 11);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(70, 46);
            this.button1.TabIndex = 19;
            this.button1.Text = "Reset";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.reset);
            // 
            // debugButton
            // 
            this.debugButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.debugButton.Location = new System.Drawing.Point(142, 11);
            this.debugButton.Name = "debugButton";
            this.debugButton.Size = new System.Drawing.Size(83, 46);
            this.debugButton.TabIndex = 1;
            this.debugButton.Text = "Debug";
            this.debugButton.UseVisualStyleBackColor = true;
            this.debugButton.Click += new System.EventHandler(this.button2_Click);
            // 
            // botStats
            // 
            this.botStats.AutoSize = true;
            this.botStats.Location = new System.Drawing.Point(312, 10);
            this.botStats.Name = "botStats";
            this.botStats.Size = new System.Drawing.Size(172, 20);
            this.botStats.TabIndex = 18;
            this.botStats.Text = "Bot Stats: No stats yet!";
            // 
            // status
            // 
            this.status.AutoSize = true;
            this.status.Location = new System.Drawing.Point(312, 37);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(206, 20);
            this.status.TabIndex = 14;
            this.status.Text = "Status: Waiting for SkillShot";
            // 
            // resultBox
            // 
            this.resultBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.resultBox.FormattingEnabled = true;
            this.resultBox.HorizontalScrollbar = true;
            this.resultBox.ItemHeight = 20;
            this.resultBox.Location = new System.Drawing.Point(3, 235);
            this.resultBox.Name = "resultBox";
            this.resultBox.Size = new System.Drawing.Size(1234, 322);
            this.resultBox.TabIndex = 15;
            // 
            // colors
            // 
            this.colors.Location = new System.Drawing.Point(3, 201);
            this.colors.Name = "colors";
            this.colors.Size = new System.Drawing.Size(1233, 26);
            this.colors.TabIndex = 14;
            // 
            // clear
            // 
            this.clear.Location = new System.Drawing.Point(1593, 142);
            this.clear.Name = "clear";
            this.clear.Size = new System.Drawing.Size(88, 32);
            this.clear.TabIndex = 16;
            this.clear.Text = "Clear";
            this.clear.UseVisualStyleBackColor = true;
            this.clear.Click += new System.EventHandler(this.clear_Click);
            // 
            // errorBox
            // 
            this.errorBox.Location = new System.Drawing.Point(3, 569);
            this.errorBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.errorBox.Multiline = true;
            this.errorBox.Name = "errorBox";
            this.errorBox.ReadOnly = true;
            this.errorBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.errorBox.Size = new System.Drawing.Size(1232, 136);
            this.errorBox.TabIndex = 17;
            this.errorBox.Text = "No Errors";
            // 
            // SkillShot
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1239, 712);
            this.Controls.Add(this.status);
            this.Controls.Add(this.botStats);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.debugButton);
            this.Controls.Add(this.errorBox);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.clear);
            this.Controls.Add(this.resultBox);
            this.Controls.Add(this.colors);
            this.Controls.Add(this.pictureBox1);
            this.MaximizeBox = false;
            this.Name = "SkillShot";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "SkillShot";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Timer RefreshTimer;
        private System.Windows.Forms.ListBox resultBox;
        private System.Windows.Forms.TextBox colors;
        private System.Windows.Forms.Button clear;
        private System.Windows.Forms.TextBox errorBox;
        private System.Windows.Forms.Button debugButton;
        private System.Windows.Forms.Label status;
        private System.Windows.Forms.Label botStats;
        private System.Windows.Forms.Button button1;
    }
}

