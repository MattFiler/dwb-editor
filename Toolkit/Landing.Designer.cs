namespace DWB_Toolkit
{
    partial class Landing
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
            this.openPropManager = new System.Windows.Forms.Button();
            this.openTileManager = new System.Windows.Forms.Button();
            this.openLevelManager = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // openPropManager
            // 
            this.openPropManager.Location = new System.Drawing.Point(12, 12);
            this.openPropManager.Name = "openPropManager";
            this.openPropManager.Size = new System.Drawing.Size(174, 33);
            this.openPropManager.TabIndex = 0;
            this.openPropManager.Text = "Prop Manager";
            this.openPropManager.UseVisualStyleBackColor = true;
            this.openPropManager.Click += new System.EventHandler(this.openPropManager_Click);
            // 
            // openTileManager
            // 
            this.openTileManager.Location = new System.Drawing.Point(12, 51);
            this.openTileManager.Name = "openTileManager";
            this.openTileManager.Size = new System.Drawing.Size(174, 33);
            this.openTileManager.TabIndex = 1;
            this.openTileManager.Text = "Tile Manager";
            this.openTileManager.UseVisualStyleBackColor = true;
            this.openTileManager.Click += new System.EventHandler(this.openTileManager_Click);
            // 
            // openLevelManager
            // 
            this.openLevelManager.Location = new System.Drawing.Point(12, 90);
            this.openLevelManager.Name = "openLevelManager";
            this.openLevelManager.Size = new System.Drawing.Size(174, 33);
            this.openLevelManager.TabIndex = 2;
            this.openLevelManager.Text = "Level Manager";
            this.openLevelManager.UseVisualStyleBackColor = true;
            this.openLevelManager.Click += new System.EventHandler(this.openLevelManager_Click);
            // 
            // Landing
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(198, 134);
            this.Controls.Add(this.openLevelManager);
            this.Controls.Add(this.openTileManager);
            this.Controls.Add(this.openPropManager);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Landing";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Landing";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button openPropManager;
        private System.Windows.Forms.Button openTileManager;
        private System.Windows.Forms.Button openLevelManager;
    }
}