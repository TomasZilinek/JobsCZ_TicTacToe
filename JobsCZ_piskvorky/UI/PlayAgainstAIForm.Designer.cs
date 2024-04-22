
namespace JobsCZ_piskvorky.UI
{
    partial class PlayAgainstAIForm
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
            this.checkScoreButton = new System.Windows.Forms.Button();
            this.drawingCheckBox = new System.Windows.Forms.CheckBox();
            this.sequenceButton = new System.Windows.Forms.Button();
            this.rainbowCheckBox = new System.Windows.Forms.CheckBox();
            this.restartButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // checkScoreButton
            // 
            this.checkScoreButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkScoreButton.Location = new System.Drawing.Point(1099, 81);
            this.checkScoreButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.checkScoreButton.Name = "checkScoreButton";
            this.checkScoreButton.Size = new System.Drawing.Size(101, 34);
            this.checkScoreButton.TabIndex = 0;
            this.checkScoreButton.Text = "Check score";
            this.checkScoreButton.UseVisualStyleBackColor = true;
            this.checkScoreButton.Click += new System.EventHandler(this.checkScoreButton_Click);
            // 
            // drawingCheckBox
            // 
            this.drawingCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.drawingCheckBox.AutoSize = true;
            this.drawingCheckBox.Location = new System.Drawing.Point(1121, 15);
            this.drawingCheckBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.drawingCheckBox.Name = "drawingCheckBox";
            this.drawingCheckBox.Size = new System.Drawing.Size(79, 21);
            this.drawingCheckBox.TabIndex = 1;
            this.drawingCheckBox.Text = "drawing";
            this.drawingCheckBox.UseVisualStyleBackColor = true;
            // 
            // sequenceButton
            // 
            this.sequenceButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.sequenceButton.Location = new System.Drawing.Point(1062, 119);
            this.sequenceButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.sequenceButton.Name = "sequenceButton";
            this.sequenceButton.Size = new System.Drawing.Size(138, 34);
            this.sequenceButton.TabIndex = 0;
            this.sequenceButton.Text = "Zamazat sequence";
            this.sequenceButton.UseVisualStyleBackColor = true;
            this.sequenceButton.Click += new System.EventHandler(this.sequenceButton_Click);
            // 
            // rainbowCheckBox
            // 
            this.rainbowCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.rainbowCheckBox.AutoSize = true;
            this.rainbowCheckBox.Location = new System.Drawing.Point(1121, 40);
            this.rainbowCheckBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.rainbowCheckBox.Name = "rainbowCheckBox";
            this.rainbowCheckBox.Size = new System.Drawing.Size(79, 21);
            this.rainbowCheckBox.TabIndex = 1;
            this.rainbowCheckBox.Text = "rainbow";
            this.rainbowCheckBox.UseVisualStyleBackColor = true;
            // 
            // restartButton
            // 
            this.restartButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.restartButton.Location = new System.Drawing.Point(1062, 157);
            this.restartButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.restartButton.Name = "restartButton";
            this.restartButton.Size = new System.Drawing.Size(138, 34);
            this.restartButton.TabIndex = 0;
            this.restartButton.Text = "Restart";
            this.restartButton.UseVisualStyleBackColor = true;
            this.restartButton.Click += new System.EventHandler(this.restartButton_Click);
            // 
            // PlayAgainstAIForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1212, 610);
            this.Controls.Add(this.rainbowCheckBox);
            this.Controls.Add(this.drawingCheckBox);
            this.Controls.Add(this.restartButton);
            this.Controls.Add(this.sequenceButton);
            this.Controls.Add(this.checkScoreButton);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "PlayAgainstAIForm";
            this.Text = "PlayAgainstAIForm";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.PlayAgainstAIForm_Paint);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PlayAgainstAIForm_MouseClick);
            this.Resize += new System.EventHandler(this.PlayAgainstAIForm_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button checkScoreButton;
        private System.Windows.Forms.CheckBox drawingCheckBox;
        private System.Windows.Forms.Button sequenceButton;
        private System.Windows.Forms.CheckBox rainbowCheckBox;
        private System.Windows.Forms.Button restartButton;
    }
}