namespace DWB_Toolkit
{
    partial class LevelManager
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.isCampaignMap = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.isLast = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.isFirst = new System.Windows.Forms.CheckBox();
            this.nextLevel = new System.Windows.Forms.ComboBox();
            this.levelName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.nextLevelEnabled = new System.Windows.Forms.CheckBox();
            this.saveBtn = new System.Windows.Forms.Button();
            this.moveDown = new System.Windows.Forms.Button();
            this.moveUp = new System.Windows.Forms.Button();
            this.deleteBtn = new System.Windows.Forms.Button();
            this.levelList = new System.Windows.Forms.ListBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.isCampaignMap);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Controls.Add(this.levelName);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.nextLevelEnabled);
            this.groupBox1.Location = new System.Drawing.Point(12, 464);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(444, 155);
            this.groupBox1.TabIndex = 24;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Selected Level Metadata";
            // 
            // isCampaignMap
            // 
            this.isCampaignMap.AutoSize = true;
            this.isCampaignMap.Location = new System.Drawing.Point(120, 64);
            this.isCampaignMap.Name = "isCampaignMap";
            this.isCampaignMap.Size = new System.Drawing.Size(90, 17);
            this.isCampaignMap.TabIndex = 18;
            this.isCampaignMap.Text = "Is official map";
            this.isCampaignMap.UseVisualStyleBackColor = true;
            this.isCampaignMap.Visible = false;
            this.isCampaignMap.CheckedChanged += new System.EventHandler(this.isCampaignMap_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.isLast);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.isFirst);
            this.groupBox2.Controls.Add(this.nextLevel);
            this.groupBox2.Location = new System.Drawing.Point(6, 81);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(432, 67);
            this.groupBox2.TabIndex = 17;
            this.groupBox2.TabStop = false;
            // 
            // isLast
            // 
            this.isLast.AutoSize = true;
            this.isLast.Enabled = false;
            this.isLast.Location = new System.Drawing.Point(305, 12);
            this.isLast.Name = "isLast";
            this.isLast.Size = new System.Drawing.Size(127, 17);
            this.isLast.TabIndex = 18;
            this.isLast.Text = "Is last campaign level";
            this.isLast.UseVisualStyleBackColor = true;
            this.isLast.CheckedChanged += new System.EventHandler(this.isLast_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "NEXT LEVEL";
            // 
            // isFirst
            // 
            this.isFirst.AutoSize = true;
            this.isFirst.Enabled = false;
            this.isFirst.Location = new System.Drawing.Point(172, 12);
            this.isFirst.Name = "isFirst";
            this.isFirst.Size = new System.Drawing.Size(127, 17);
            this.isFirst.TabIndex = 17;
            this.isFirst.Text = "Is first campaign level";
            this.isFirst.UseVisualStyleBackColor = true;
            this.isFirst.CheckedChanged += new System.EventHandler(this.isFirst_CheckedChanged_1);
            // 
            // nextLevel
            // 
            this.nextLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.nextLevel.Enabled = false;
            this.nextLevel.FormattingEnabled = true;
            this.nextLevel.Location = new System.Drawing.Point(9, 31);
            this.nextLevel.Name = "nextLevel";
            this.nextLevel.Size = new System.Drawing.Size(417, 21);
            this.nextLevel.TabIndex = 14;
            // 
            // levelName
            // 
            this.levelName.Location = new System.Drawing.Point(6, 38);
            this.levelName.Name = "levelName";
            this.levelName.Size = new System.Drawing.Size(432, 20);
            this.levelName.TabIndex = 13;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "LEVEL NAME";
            // 
            // nextLevelEnabled
            // 
            this.nextLevelEnabled.AutoSize = true;
            this.nextLevelEnabled.Location = new System.Drawing.Point(6, 64);
            this.nextLevelEnabled.Name = "nextLevelEnabled";
            this.nextLevelEnabled.Size = new System.Drawing.Size(108, 17);
            this.nextLevelEnabled.TabIndex = 16;
            this.nextLevelEnabled.Text = "Is campaign level";
            this.nextLevelEnabled.UseVisualStyleBackColor = true;
            this.nextLevelEnabled.CheckedChanged += new System.EventHandler(this.nextLevelEnabled_CheckedChanged);
            // 
            // saveBtn
            // 
            this.saveBtn.Location = new System.Drawing.Point(462, 584);
            this.saveBtn.Name = "saveBtn";
            this.saveBtn.Size = new System.Drawing.Size(57, 35);
            this.saveBtn.TabIndex = 23;
            this.saveBtn.Text = "SAVE";
            this.saveBtn.UseVisualStyleBackColor = true;
            this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
            // 
            // moveDown
            // 
            this.moveDown.Location = new System.Drawing.Point(462, 53);
            this.moveDown.Name = "moveDown";
            this.moveDown.Size = new System.Drawing.Size(57, 35);
            this.moveDown.TabIndex = 22;
            this.moveDown.Text = "MOVE DOWN";
            this.moveDown.UseVisualStyleBackColor = true;
            this.moveDown.Click += new System.EventHandler(this.moveDown_Click);
            // 
            // moveUp
            // 
            this.moveUp.Location = new System.Drawing.Point(462, 12);
            this.moveUp.Name = "moveUp";
            this.moveUp.Size = new System.Drawing.Size(57, 35);
            this.moveUp.TabIndex = 21;
            this.moveUp.Text = "MOVE UP";
            this.moveUp.UseVisualStyleBackColor = true;
            this.moveUp.Click += new System.EventHandler(this.moveUp_Click);
            // 
            // deleteBtn
            // 
            this.deleteBtn.Location = new System.Drawing.Point(462, 423);
            this.deleteBtn.Name = "deleteBtn";
            this.deleteBtn.Size = new System.Drawing.Size(57, 35);
            this.deleteBtn.TabIndex = 20;
            this.deleteBtn.Text = "DELETE";
            this.deleteBtn.UseVisualStyleBackColor = true;
            this.deleteBtn.Click += new System.EventHandler(this.deleteBtn_Click);
            // 
            // levelList
            // 
            this.levelList.FormattingEnabled = true;
            this.levelList.Location = new System.Drawing.Point(12, 12);
            this.levelList.Name = "levelList";
            this.levelList.Size = new System.Drawing.Size(444, 446);
            this.levelList.TabIndex = 19;
            this.levelList.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // LevelManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(529, 627);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.saveBtn);
            this.Controls.Add(this.moveDown);
            this.Controls.Add(this.moveUp);
            this.Controls.Add(this.deleteBtn);
            this.Controls.Add(this.levelList);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.Name = "LevelManager";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Level Manager";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox isFirst;
        private System.Windows.Forms.ComboBox nextLevel;
        private System.Windows.Forms.TextBox levelName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox nextLevelEnabled;
        private System.Windows.Forms.Button saveBtn;
        private System.Windows.Forms.Button moveDown;
        private System.Windows.Forms.Button moveUp;
        private System.Windows.Forms.Button deleteBtn;
        private System.Windows.Forms.ListBox levelList;
        private System.Windows.Forms.CheckBox isCampaignMap;
        private System.Windows.Forms.CheckBox isLast;
    }
}