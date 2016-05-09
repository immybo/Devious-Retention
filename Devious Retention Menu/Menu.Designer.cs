namespace Devious_Retention_Menu
{
    partial class Menu
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
            this.singleplayerButton = new System.Windows.Forms.Button();
            this.multiplayerHostButton = new System.Windows.Forms.Button();
            this.multiplayerJoinButton = new System.Windows.Forms.Button();
            this.settingsButton = new System.Windows.Forms.Button();
            this.title1 = new System.Windows.Forms.Label();
            this.title2 = new System.Windows.Forms.Label();
            this.ipTextbox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // singleplayerButton
            // 
            this.singleplayerButton.Location = new System.Drawing.Point(12, 119);
            this.singleplayerButton.Name = "singleplayerButton";
            this.singleplayerButton.Size = new System.Drawing.Size(260, 51);
            this.singleplayerButton.TabIndex = 0;
            this.singleplayerButton.Text = "Singleplayer";
            this.singleplayerButton.UseVisualStyleBackColor = true;
            this.singleplayerButton.Click += new System.EventHandler(this.singleplayerButton_Click);
            // 
            // multiplayerHostButton
            // 
            this.multiplayerHostButton.Location = new System.Drawing.Point(12, 176);
            this.multiplayerHostButton.Name = "multiplayerHostButton";
            this.multiplayerHostButton.Size = new System.Drawing.Size(260, 51);
            this.multiplayerHostButton.TabIndex = 0;
            this.multiplayerHostButton.Text = "Host Multiplayer";
            this.multiplayerHostButton.UseVisualStyleBackColor = true;
            this.multiplayerHostButton.Click += new System.EventHandler(this.multiplayerHostButton_Click);
            // 
            // multiplayerJoinButton
            // 
            this.multiplayerJoinButton.Location = new System.Drawing.Point(12, 233);
            this.multiplayerJoinButton.Name = "multiplayerJoinButton";
            this.multiplayerJoinButton.Size = new System.Drawing.Size(260, 51);
            this.multiplayerJoinButton.TabIndex = 0;
            this.multiplayerJoinButton.Text = "Join Multiplayer";
            this.multiplayerJoinButton.UseVisualStyleBackColor = true;
            this.multiplayerJoinButton.Click += new System.EventHandler(this.multiplayerJoinButton_Click);
            // 
            // settingsButton
            // 
            this.settingsButton.Location = new System.Drawing.Point(12, 317);
            this.settingsButton.Name = "settingsButton";
            this.settingsButton.Size = new System.Drawing.Size(260, 51);
            this.settingsButton.TabIndex = 0;
            this.settingsButton.Text = "Settings";
            this.settingsButton.UseVisualStyleBackColor = true;
            // 
            // title1
            // 
            this.title1.AutoSize = true;
            this.title1.Font = new System.Drawing.Font("Cambria", 27.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.title1.Location = new System.Drawing.Point(66, 9);
            this.title1.Name = "title1";
            this.title1.Size = new System.Drawing.Size(146, 43);
            this.title1.TabIndex = 1;
            this.title1.Text = "Devious";
            // 
            // title2
            // 
            this.title2.AutoSize = true;
            this.title2.Font = new System.Drawing.Font("Cambria", 27.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.title2.Location = new System.Drawing.Point(46, 52);
            this.title2.Name = "title2";
            this.title2.Size = new System.Drawing.Size(175, 43);
            this.title2.TabIndex = 1;
            this.title2.Text = "Retention";
            // 
            // ipTextbox
            // 
            this.ipTextbox.Location = new System.Drawing.Point(12, 290);
            this.ipTextbox.Name = "ipTextbox";
            this.ipTextbox.Size = new System.Drawing.Size(260, 20);
            this.ipTextbox.TabIndex = 2;
            this.ipTextbox.Text = "Enter IP Address";
            // 
            // Menu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 382);
            this.Controls.Add(this.ipTextbox);
            this.Controls.Add(this.title2);
            this.Controls.Add(this.title1);
            this.Controls.Add(this.settingsButton);
            this.Controls.Add(this.multiplayerJoinButton);
            this.Controls.Add(this.multiplayerHostButton);
            this.Controls.Add(this.singleplayerButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Menu";
            this.Text = "Menu";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button singleplayerButton;
        private System.Windows.Forms.Button multiplayerHostButton;
        private System.Windows.Forms.Button multiplayerJoinButton;
        private System.Windows.Forms.Button settingsButton;
        private System.Windows.Forms.Label title1;
        private System.Windows.Forms.Label title2;
        private System.Windows.Forms.TextBox ipTextbox;
    }
}