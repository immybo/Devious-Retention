using Devious_Retention_SP.HumanPlayerView;

namespace Devious_Retention_SP
{
    partial class HumanPlayerWindow
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
            this.resourceBar = new ResourceBar();
            this.bottomRightPanel = new BottomRightPanel();
            this.topRightPanel = new TopRightPanel();
            this.gameArea = new GameArea(this.world);
            this.SuspendLayout();
            // 
            // resourceBar
            // 
            this.resourceBar.Location = new System.Drawing.Point(0, 698);
            this.resourceBar.Name = "resourceBar";
            this.resourceBar.Size = new System.Drawing.Size(1000, 30);
            this.resourceBar.TabIndex = 0;
            // 
            // bottomRightPanel
            // 
            this.bottomRightPanel.Location = new System.Drawing.Point(997, 364);
            this.bottomRightPanel.Name = "bottomRightPanel";
            this.bottomRightPanel.Size = new System.Drawing.Size(266, 364);
            this.bottomRightPanel.TabIndex = 1;
            // 
            // topRightPanel
            // 
            this.topRightPanel.Location = new System.Drawing.Point(997, 0);
            this.topRightPanel.Name = "topRightPanel";
            this.topRightPanel.Size = new System.Drawing.Size(266, 364);
            this.topRightPanel.TabIndex = 2;
            // 
            // gameArea
            // 
            this.gameArea.Location = new System.Drawing.Point(0, 1);
            this.gameArea.Name = "gameArea";
            this.gameArea.Size = new System.Drawing.Size(997, 699);
            this.gameArea.TabIndex = 3;
            // 
            // HumanPlayerWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1264, 729);
            this.Controls.Add(this.gameArea);
            this.Controls.Add(this.topRightPanel);
            this.Controls.Add(this.bottomRightPanel);
            this.Controls.Add(this.resourceBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "HumanPlayerWindow";
            this.Text = "HumanPlayerWindow";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel resourceBar;
        private System.Windows.Forms.Panel bottomRightPanel;
        private System.Windows.Forms.Panel topRightPanel;
        private System.Windows.Forms.Panel gameArea;
    }
}