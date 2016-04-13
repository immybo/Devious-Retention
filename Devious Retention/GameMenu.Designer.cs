namespace Devious_Retention
{
    partial class GameMenu
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
            this.hostGameButton = new System.Windows.Forms.Button();
            this.joinGameButton = new System.Windows.Forms.Button();
            this.menuTitle = new System.Windows.Forms.Label();
            this.ipBox = new System.Windows.Forms.TextBox();
            this.playerNameBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // hostGameButton
            // 
            this.hostGameButton.Location = new System.Drawing.Point(52, 93);
            this.hostGameButton.Name = "hostGameButton";
            this.hostGameButton.Size = new System.Drawing.Size(260, 57);
            this.hostGameButton.TabIndex = 0;
            this.hostGameButton.Text = "Host Lobby";
            this.hostGameButton.UseVisualStyleBackColor = true;
            this.hostGameButton.Click += new System.EventHandler(this.hostGameButton_Click);
            // 
            // joinGameButton
            // 
            this.joinGameButton.Location = new System.Drawing.Point(474, 93);
            this.joinGameButton.Name = "joinGameButton";
            this.joinGameButton.Size = new System.Drawing.Size(260, 39);
            this.joinGameButton.TabIndex = 1;
            this.joinGameButton.Text = "Join Lobby";
            this.joinGameButton.UseVisualStyleBackColor = true;
            this.joinGameButton.Click += new System.EventHandler(this.joinGameButton_Click);
            // 
            // menuTitle
            // 
            this.menuTitle.AutoSize = true;
            this.menuTitle.Font = new System.Drawing.Font("Impact", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.menuTitle.ForeColor = System.Drawing.SystemColors.WindowText;
            this.menuTitle.Location = new System.Drawing.Point(206, 9);
            this.menuTitle.Name = "menuTitle";
            this.menuTitle.Size = new System.Drawing.Size(386, 60);
            this.menuTitle.TabIndex = 2;
            this.menuTitle.Text = "Devious Retention";
            this.menuTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ipBox
            // 
            this.ipBox.Location = new System.Drawing.Point(474, 130);
            this.ipBox.Name = "ipBox";
            this.ipBox.Size = new System.Drawing.Size(260, 20);
            this.ipBox.TabIndex = 3;
            this.ipBox.Text = "0.0.0.0";
            this.ipBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // playerNameBox
            // 
            this.playerNameBox.Location = new System.Drawing.Point(12, 556);
            this.playerNameBox.Name = "playerNameBox";
            this.playerNameBox.Size = new System.Drawing.Size(100, 20);
            this.playerNameBox.TabIndex = 4;
            this.playerNameBox.TextChanged += new System.EventHandler(this.playerNameBox_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 537);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Player Name";
            // 
            // GameMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(794, 588);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.playerNameBox);
            this.Controls.Add(this.ipBox);
            this.Controls.Add(this.menuTitle);
            this.Controls.Add(this.joinGameButton);
            this.Controls.Add(this.hostGameButton);
            this.Name = "GameMenu";
            this.Text = "GameMenu";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button hostGameButton;
        private System.Windows.Forms.Button joinGameButton;
        private System.Windows.Forms.Label menuTitle;
        private System.Windows.Forms.TextBox ipBox;
        private System.Windows.Forms.TextBox playerNameBox;
        private System.Windows.Forms.Label label1;
    }
}