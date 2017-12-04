namespace pacman {
    partial class FormStage {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormStage));
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.textBoxMessage = new System.Windows.Forms.TextBox();
            this.textBoxChatHistory = new System.Windows.Forms.TextBox();
            this.panelGame = new System.Windows.Forms.Panel();
            this.textboxPlayers = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.labelScore = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 20;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // textBoxMessage
            // 
            this.textBoxMessage.Enabled = false;
            this.textBoxMessage.Location = new System.Drawing.Point(458, 248);
            this.textBoxMessage.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBoxMessage.Multiline = true;
            this.textBoxMessage.Name = "textBoxMessage";
            this.textBoxMessage.Size = new System.Drawing.Size(261, 37);
            this.textBoxMessage.TabIndex = 145;
            this.textBoxMessage.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxMessage_OnKeyDown);
            // 
            // textBoxChatHistory
            // 
            this.textBoxChatHistory.Enabled = false;
            this.textBoxChatHistory.HideSelection = false;
            this.textBoxChatHistory.Location = new System.Drawing.Point(458, 32);
            this.textBoxChatHistory.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBoxChatHistory.Multiline = true;
            this.textBoxChatHistory.Name = "textBoxChatHistory";
            this.textBoxChatHistory.ReadOnly = true;
            this.textBoxChatHistory.Size = new System.Drawing.Size(261, 213);
            this.textBoxChatHistory.TabIndex = 146;
            // 
            // panelGame
            // 
            this.panelGame.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.panelGame.Location = new System.Drawing.Point(12, 32);
            this.panelGame.Name = "panelGame";
            this.panelGame.Size = new System.Drawing.Size(322, 253);
            this.panelGame.TabIndex = 149;
            // 
            // textboxPlayers
            // 
            this.textboxPlayers.Enabled = false;
            this.textboxPlayers.Location = new System.Drawing.Point(339, 32);
            this.textboxPlayers.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textboxPlayers.Multiline = true;
            this.textboxPlayers.Name = "textboxPlayers";
            this.textboxPlayers.ReadOnly = true;
            this.textboxPlayers.Size = new System.Drawing.Size(115, 253);
            this.textboxPlayers.TabIndex = 150;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 15);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 151;
            this.label1.Text = "Score:";
            // 
            // labelScore
            // 
            this.labelScore.AutoSize = true;
            this.labelScore.Location = new System.Drawing.Point(50, 15);
            this.labelScore.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelScore.Name = "labelScore";
            this.labelScore.Size = new System.Drawing.Size(13, 13);
            this.labelScore.TabIndex = 151;
            this.labelScore.Text = "0";
            // 
            // FormStage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(731, 296);
            this.Controls.Add(this.labelScore);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textboxPlayers);
            this.Controls.Add(this.panelGame);
            this.Controls.Add(this.textBoxChatHistory);
            this.Controls.Add(this.textBoxMessage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormStage";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DADman";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.keyIsDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.keyIsUp);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TextBox textBoxMessage;
        private System.Windows.Forms.TextBox textBoxChatHistory;
        private System.Windows.Forms.Panel panelGame;
        private System.Windows.Forms.TextBox textboxPlayers;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelScore;
    }
}

