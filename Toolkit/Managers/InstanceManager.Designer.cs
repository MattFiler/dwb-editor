namespace DWB_Toolkit
{
    partial class InstanceManager
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
            this.deleteInstance = new System.Windows.Forms.Button();
            this.editInstance = new System.Windows.Forms.Button();
            this.newInstance = new System.Windows.Forms.Button();
            this.instanceList = new System.Windows.Forms.ListBox();
            this.isHidden = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // deleteInstance
            // 
            this.deleteInstance.Location = new System.Drawing.Point(610, 171);
            this.deleteInstance.Name = "deleteInstance";
            this.deleteInstance.Size = new System.Drawing.Size(166, 34);
            this.deleteInstance.TabIndex = 21;
            this.deleteInstance.Text = "Delete Selected Prop";
            this.deleteInstance.UseVisualStyleBackColor = true;
            this.deleteInstance.Click += new System.EventHandler(this.deleteInstance_Click);
            // 
            // editInstance
            // 
            this.editInstance.Location = new System.Drawing.Point(610, 131);
            this.editInstance.Name = "editInstance";
            this.editInstance.Size = new System.Drawing.Size(166, 34);
            this.editInstance.TabIndex = 20;
            this.editInstance.Text = "Edit Selected Prop";
            this.editInstance.UseVisualStyleBackColor = true;
            this.editInstance.Click += new System.EventHandler(this.editInstance_Click);
            // 
            // newInstance
            // 
            this.newInstance.Location = new System.Drawing.Point(610, 91);
            this.newInstance.Name = "newInstance";
            this.newInstance.Size = new System.Drawing.Size(166, 34);
            this.newInstance.TabIndex = 19;
            this.newInstance.Text = "New Prop";
            this.newInstance.UseVisualStyleBackColor = true;
            this.newInstance.Click += new System.EventHandler(this.newInstance_Click);
            // 
            // instanceList
            // 
            this.instanceList.FormattingEnabled = true;
            this.instanceList.Location = new System.Drawing.Point(12, 12);
            this.instanceList.Name = "instanceList";
            this.instanceList.Size = new System.Drawing.Size(592, 277);
            this.instanceList.TabIndex = 18;
            this.instanceList.SelectedIndexChanged += new System.EventHandler(this.instanceList_SelectedIndexChanged);
            // 
            // isHidden
            // 
            this.isHidden.AutoSize = true;
            this.isHidden.Enabled = false;
            this.isHidden.Location = new System.Drawing.Point(610, 272);
            this.isHidden.Name = "isHidden";
            this.isHidden.Size = new System.Drawing.Size(93, 17);
            this.isHidden.TabIndex = 28;
            this.isHidden.Text = "Is Deprecated";
            this.isHidden.UseVisualStyleBackColor = true;
            // 
            // InstanceManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(787, 303);
            this.Controls.Add(this.isHidden);
            this.Controls.Add(this.deleteInstance);
            this.Controls.Add(this.editInstance);
            this.Controls.Add(this.newInstance);
            this.Controls.Add(this.instanceList);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "InstanceManager";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Prop Manager";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button deleteInstance;
        private System.Windows.Forms.Button editInstance;
        private System.Windows.Forms.Button newInstance;
        private System.Windows.Forms.ListBox instanceList;
        private System.Windows.Forms.CheckBox isHidden;
    }
}