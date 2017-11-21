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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormWelcome));
            this.labelGameName = new System.Windows.Forms.Label();
            this.labelTitleApp = new System.Windows.Forms.Label();
            this.buttonJoin = new System.Windows.Forms.Button();
            this.buttonQuit = new System.Windows.Forms.Button();
            this.labelError = new System.Windows.Forms.Label();
            this.textBoxUsername = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // labelGameName
            // 
            this.labelGameName.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.labelGameName.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.labelGameName.Location = new System.Drawing.Point(287, 62);
            this.labelGameName.Name = "labelGameName";
            this.labelGameName.Size = new System.Drawing.Size(133, 41);
            this.labelGameName.TabIndex = 0;
            this.labelGameName.Text = "PACMAN";
            // 
            // labelTitleApp
            // 
            this.labelTitleApp.AutoSize = true;
            this.labelTitleApp.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.labelTitleApp.Location = new System.Drawing.Point(149, 102);
            this.labelTitleApp.Name = "labelTitleApp";
            this.labelTitleApp.Size = new System.Drawing.Size(412, 29);
            this.labelTitleApp.TabIndex = 1;
            this.labelTitleApp.Text = "Distributed Online Gaming Platform";
            // 
            // buttonJoin
            // 
            this.buttonJoin.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.buttonJoin.Location = new System.Drawing.Point(229, 245);
            this.buttonJoin.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonJoin.Name = "buttonJoin";
            this.buttonJoin.Size = new System.Drawing.Size(244, 34);
            this.buttonJoin.TabIndex = 2;
            this.buttonJoin.Text = "Join";
            this.buttonJoin.UseVisualStyleBackColor = true;
            this.buttonJoin.Click += new System.EventHandler(this.joinClick);
            // 
            // buttonQuit
            // 
            this.buttonQuit.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.buttonQuit.Location = new System.Drawing.Point(229, 284);
            this.buttonQuit.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonQuit.Name = "buttonQuit";
            this.buttonQuit.Size = new System.Drawing.Size(244, 34);
            this.buttonQuit.TabIndex = 3;
            this.buttonQuit.Text = "Quit";
            this.buttonQuit.UseVisualStyleBackColor = true;
            this.buttonQuit.Click += new System.EventHandler(this.buttonQuit_Click);
            // 
            // labelError
            // 
            this.labelError.AutoSize = true;
            this.labelError.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.labelError.ForeColor = System.Drawing.Color.Red;
            this.labelError.Location = new System.Drawing.Point(225, 164);
            this.labelError.Name = "labelError";
            this.labelError.Size = new System.Drawing.Size(45, 20);
            this.labelError.TabIndex = 4;
            this.labelError.Text = "error";
            // 
            // textBoxUsername
            // 
            this.textBoxUsername.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.textBoxUsername.ForeColor = System.Drawing.SystemColors.InactiveCaption;
            this.textBoxUsername.Location = new System.Drawing.Point(229, 206);
            this.textBoxUsername.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBoxUsername.Multiline = true;
            this.textBoxUsername.Name = "textBoxUsername";
            this.textBoxUsername.Size = new System.Drawing.Size(243, 34);
            this.textBoxUsername.TabIndex = 5;
            this.textBoxUsername.Text = "Username";
            this.textBoxUsername.Enter += new System.EventHandler(this.textBoxUsername_OnFocus);
            this.textBoxUsername.Leave += new System.EventHandler(this.textBoxUsername_OnLostFocus);
            // 
            // FormWelcome
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.ClientSize = new System.Drawing.Size(719, 418);
            this.Controls.Add(this.textBoxUsername);
            this.Controls.Add(this.labelError);
            this.Controls.Add(this.buttonQuit);
            this.Controls.Add(this.buttonJoin);
            this.Controls.Add(this.labelTitleApp);
            this.Controls.Add(this.labelGameName);
            this.ForeColor = System.Drawing.SystemColors.MenuText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "FormWelcome";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = " PACMAN";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formWelcome_OnClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelGameName;
        private System.Windows.Forms.Label labelTitleApp;
        public System.Windows.Forms.Button buttonJoin;
        private System.Windows.Forms.Button buttonQuit;
        private System.Windows.Forms.Label labelError;
        public System.Windows.Forms.TextBox textBoxUsername;
    }
}