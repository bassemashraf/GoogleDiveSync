
namespace GoogleDriveSync
{
    partial class GoogleDriveSync
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
            this.StartLocalFileWatcherButton = new System.Windows.Forms.Button();
            this.StartDriveWatcherButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // StartLocalFileWatcherButton
            // 
            this.StartLocalFileWatcherButton.Location = new System.Drawing.Point(129, 108);
            this.StartLocalFileWatcherButton.Name = "StartLocalFileWatcherButton";
            this.StartLocalFileWatcherButton.Size = new System.Drawing.Size(159, 131);
            this.StartLocalFileWatcherButton.TabIndex = 4;
            this.StartLocalFileWatcherButton.Text = "Start Local File Watcher";
            this.StartLocalFileWatcherButton.UseVisualStyleBackColor = true;
            this.StartLocalFileWatcherButton.Click += new System.EventHandler(this.StartLocalFileWatcherButton_Click);
            // 
            // StartDriveWatcherButton
            // 
            this.StartDriveWatcherButton.Location = new System.Drawing.Point(442, 128);
            this.StartDriveWatcherButton.Name = "StartDriveWatcherButton";
            this.StartDriveWatcherButton.Size = new System.Drawing.Size(168, 139);
            this.StartDriveWatcherButton.TabIndex = 5;
            this.StartDriveWatcherButton.Text = "StartDriveWatcher";
            this.StartDriveWatcherButton.UseVisualStyleBackColor = true;
            this.StartDriveWatcherButton.Click += new System.EventHandler(this.StartDriveWatcherButton_Click);
            // 
            // GoogleDriveSync
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.StartDriveWatcherButton);
            this.Controls.Add(this.StartLocalFileWatcherButton);
            this.Name = "GoogleDriveSync";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button StartLocalFileWatcherButton;
        private System.Windows.Forms.Button StartDriveWatcherButton;
    }
}

