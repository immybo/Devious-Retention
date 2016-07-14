namespace Devious_Retention_Menu
{
    partial class MultiplayerLobby
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
            this.exitButton = new System.Windows.Forms.Button();
            this.nameBox = new System.Windows.Forms.TextBox();
            this.factionBox = new System.Windows.Forms.TextBox();
            this.colorBox = new System.Windows.Forms.TextBox();
            this.startButton = new System.Windows.Forms.Button();
            this.nameChooserLabel = new System.Windows.Forms.Label();
            this.factionChooserLabel = new System.Windows.Forms.Label();
            this.colorChooserLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // exitButton
            // 
            this.exitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.exitButton.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.exitButton.Location = new System.Drawing.Point(772, 549);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(200, 50);
            this.exitButton.TabIndex = 1;
            this.exitButton.Text = "Exit";
            this.exitButton.UseVisualStyleBackColor = true;
            // 
            // nameBox
            // 
            this.nameBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nameBox.Location = new System.Drawing.Point(12, 579);
            this.nameBox.Name = "nameBox";
            this.nameBox.Size = new System.Drawing.Size(168, 20);
            this.nameBox.TabIndex = 2;
            this.nameBox.TextChanged += new System.EventHandler(this.nameBox_TextChanged);
            // 
            // factionBox
            // 
            this.factionBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.factionBox.Location = new System.Drawing.Point(186, 579);
            this.factionBox.Name = "factionBox";
            this.factionBox.Size = new System.Drawing.Size(168, 20);
            this.factionBox.TabIndex = 2;
            this.factionBox.TextChanged += new System.EventHandler(this.factionBox_TextChanged);
            // 
            // colorBox
            // 
            this.colorBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.colorBox.Location = new System.Drawing.Point(360, 579);
            this.colorBox.Name = "colorBox";
            this.colorBox.Size = new System.Drawing.Size(168, 20);
            this.colorBox.TabIndex = 2;
            this.colorBox.TextChanged += new System.EventHandler(this.colorBox_TextChanged);
            // 
            // startButton
            // 
            this.startButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.startButton.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.startButton.Location = new System.Drawing.Point(566, 549);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(200, 50);
            this.startButton.TabIndex = 0;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // nameChooserLabel
            // 
            this.nameChooserLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nameChooserLabel.AutoSize = true;
            this.nameChooserLabel.Location = new System.Drawing.Point(9, 560);
            this.nameChooserLabel.Name = "nameChooserLabel";
            this.nameChooserLabel.Size = new System.Drawing.Size(35, 13);
            this.nameChooserLabel.TabIndex = 3;
            this.nameChooserLabel.Text = "Name";
            // 
            // factionChooserLabel
            // 
            this.factionChooserLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.factionChooserLabel.AutoSize = true;
            this.factionChooserLabel.Location = new System.Drawing.Point(183, 560);
            this.factionChooserLabel.Name = "factionChooserLabel";
            this.factionChooserLabel.Size = new System.Drawing.Size(73, 13);
            this.factionChooserLabel.TabIndex = 3;
            this.factionChooserLabel.Text = "Faction Name";
            // 
            // colorChooserLabel
            // 
            this.colorChooserLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.colorChooserLabel.AutoSize = true;
            this.colorChooserLabel.Location = new System.Drawing.Point(357, 560);
            this.colorChooserLabel.Name = "colorChooserLabel";
            this.colorChooserLabel.Size = new System.Drawing.Size(88, 13);
            this.colorChooserLabel.TabIndex = 3;
            this.colorChooserLabel.Text = "Color (6 digit hex)";
            // 
            // MultiplayerLobby
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 611);
            this.Controls.Add(this.colorChooserLabel);
            this.Controls.Add(this.factionChooserLabel);
            this.Controls.Add(this.nameChooserLabel);
            this.Controls.Add(this.colorBox);
            this.Controls.Add(this.factionBox);
            this.Controls.Add(this.nameBox);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.startButton);
            this.Name = "MultiplayerLobby";
            this.Text = "Lobby";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.TextBox nameBox;
        private System.Windows.Forms.TextBox factionBox;
        private System.Windows.Forms.TextBox colorBox;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Label nameChooserLabel;
        private System.Windows.Forms.Label factionChooserLabel;
        private System.Windows.Forms.Label colorChooserLabel;
    }
}