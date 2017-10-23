namespace pacman
{
    partial class FormWelcome
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
            this.labelGameName = new System.Windows.Forms.Label();
            this.labelTitleApp = new System.Windows.Forms.Label();
            this.buttonJoin = new System.Windows.Forms.Button();
            this.buttonQuit = new System.Windows.Forms.Button();
            this.labelError = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelGameName
            // 
            this.labelGameName.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.labelGameName.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.labelGameName.Location = new System.Drawing.Point(232, 51);
            this.labelGameName.Name = "labelGameName";
            this.labelGameName.Size = new System.Drawing.Size(133, 41);
            this.labelGameName.TabIndex = 0;
            this.labelGameName.Text = "PACMAN";
            // 
            // labelTitleApp
            // 
            this.labelTitleApp.AutoSize = true;
            this.labelTitleApp.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.labelTitleApp.Location = new System.Drawing.Point(75, 92);
            this.labelTitleApp.Name = "labelTitleApp";
            this.labelTitleApp.Size = new System.Drawing.Size(426, 29);
            this.labelTitleApp.TabIndex = 1;
            this.labelTitleApp.Text = "Distributed Online Gaming Plataform";
            // 
            // buttonJoin
            // 
            this.buttonJoin.Location = new System.Drawing.Point(131, 199);
            this.buttonJoin.Name = "buttonJoin";
            this.buttonJoin.Size = new System.Drawing.Size(103, 35);
            this.buttonJoin.TabIndex = 2;
            this.buttonJoin.Text = "Join";
            this.buttonJoin.UseVisualStyleBackColor = true;
            this.buttonJoin.Click += new System.EventHandler(this.buttonJoin_Click);
            // 
            // buttonQuit
            // 
            this.buttonQuit.Location = new System.Drawing.Point(338, 199);
            this.buttonQuit.Name = "buttonQuit";
            this.buttonQuit.Size = new System.Drawing.Size(103, 35);
            this.buttonQuit.TabIndex = 3;
            this.buttonQuit.Text = "Quit";
            this.buttonQuit.UseVisualStyleBackColor = true;
            // 
            // labelError
            // 
            this.labelError.AutoSize = true;
            this.labelError.ForeColor = System.Drawing.Color.Red;
            this.labelError.Location = new System.Drawing.Point(267, 155);
            this.labelError.Name = "labelError";
            this.labelError.Size = new System.Drawing.Size(39, 17);
            this.labelError.TabIndex = 4;
            this.labelError.Text = "error";
            // 
            // FormWelcome
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 266);
            this.Controls.Add(this.labelError);
            this.Controls.Add(this.buttonQuit);
            this.Controls.Add(this.buttonJoin);
            this.Controls.Add(this.labelTitleApp);
            this.Controls.Add(this.labelGameName);
            this.Name = "FormWelcome";
            this.Text = "FormWelcome";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelGameName;
        private System.Windows.Forms.Label labelTitleApp;
        private System.Windows.Forms.Button buttonJoin;
        private System.Windows.Forms.Button buttonQuit;
        private System.Windows.Forms.Label labelError;
    }
}