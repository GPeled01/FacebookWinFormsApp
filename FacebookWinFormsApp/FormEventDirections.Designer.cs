namespace BasicFacebookFeatures
{
    partial class FormEventDirections
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
            this.webBrowserEventsDirection = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // webBrowserEventsDirection
            // 
            this.webBrowserEventsDirection.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowserEventsDirection.Location = new System.Drawing.Point(1, 0);
            this.webBrowserEventsDirection.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowserEventsDirection.Name = "webBrowserEventsDirection";
            this.webBrowserEventsDirection.Size = new System.Drawing.Size(1200, 700);
            this.webBrowserEventsDirection.TabIndex = 103;
            this.webBrowserEventsDirection.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowserEventsDirection_DocumentCompleted);
            // 
            // FormEventDirections
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(949, 587);
            this.Controls.Add(this.webBrowserEventsDirection);
            this.Name = "FormEventDirections";
            this.Text = "EventDirections";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser webBrowserEventsDirection;
    }
}