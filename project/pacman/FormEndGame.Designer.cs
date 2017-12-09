namespace pacman
{
    partial class FormEndGame
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
            this.textBoxWinner = new System.Windows.Forms.TextBox();
            this.labelWinner = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxScore = new System.Windows.Forms.TextBox();
            this.buttonNo = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBoxWinner
            // 
            this.textBoxWinner.Enabled = false;
            this.textBoxWinner.Location = new System.Drawing.Point(139, 32);
            this.textBoxWinner.Name = "textBoxWinner";
            this.textBoxWinner.Size = new System.Drawing.Size(244, 22);
            this.textBoxWinner.TabIndex = 0;
            // 
            // labelWinner
            // 
            this.labelWinner.AutoSize = true;
            this.labelWinner.Location = new System.Drawing.Point(22, 36);
            this.labelWinner.Name = "labelWinner";
            this.labelWinner.Size = new System.Drawing.Size(57, 17);
            this.labelWinner.TabIndex = 1;
            this.labelWinner.Text = "Winner:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 74);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "Score:";
            // 
            // textBoxScore
            // 
            this.textBoxScore.Enabled = false;
            this.textBoxScore.Location = new System.Drawing.Point(139, 74);
            this.textBoxScore.Name = "textBoxScore";
            this.textBoxScore.Size = new System.Drawing.Size(244, 22);
            this.textBoxScore.TabIndex = 3;
            // 
            // buttonNo
            // 
            this.buttonNo.Cursor = System.Windows.Forms.Cursors.Default;
            this.buttonNo.Location = new System.Drawing.Point(226, 140);
            this.buttonNo.Name = "buttonNo";
            this.buttonNo.Size = new System.Drawing.Size(75, 23);
            this.buttonNo.TabIndex = 5;
            this.buttonNo.Text = "Leave";
            this.buttonNo.UseVisualStyleBackColor = true;
            this.buttonNo.Click += new System.EventHandler(this.buttonLeave_Click);
            // 
            // FormEndGame
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(568, 201);
            this.Controls.Add(this.buttonNo);
            this.Controls.Add(this.textBoxScore);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelWinner);
            this.Controls.Add(this.textBoxWinner);
            this.Name = "FormEndGame";
            this.Text = "EndGame";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxWinner;
        private System.Windows.Forms.Label labelWinner;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxScore;
        private System.Windows.Forms.Button buttonNo;
    }
}