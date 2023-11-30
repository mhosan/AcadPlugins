
namespace pruebaAcadForm
{
    partial class MainForm
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
            this.button1 = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lboxResponse = new System.Windows.Forms.ListBox();
            this.btnRegister = new System.Windows.Forms.Button();
            this.btnUnRegister = new System.Windows.Forms.Button();
            this.statStripRegister = new System.Windows.Forms.StatusStrip();
            this.labelRegister = new System.Windows.Forms.ToolStripStatusLabel();
            this.statStripRegister.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(102, 35);
            this.button1.TabIndex = 4;
            this.button1.Text = "Preview controles";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(418, 200);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(102, 35);
            this.btnClose.TabIndex = 7;
            this.btnClose.Text = "Cerrar";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lboxResponse
            // 
            this.lboxResponse.FormattingEnabled = true;
            this.lboxResponse.Location = new System.Drawing.Point(129, 14);
            this.lboxResponse.Name = "lboxResponse";
            this.lboxResponse.Size = new System.Drawing.Size(391, 173);
            this.lboxResponse.TabIndex = 13;
            // 
            // btnRegister
            // 
            this.btnRegister.Location = new System.Drawing.Point(13, 78);
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.Size = new System.Drawing.Size(102, 35);
            this.btnRegister.TabIndex = 14;
            this.btnRegister.Text = "Register";
            this.btnRegister.UseVisualStyleBackColor = true;
            this.btnRegister.Click += new System.EventHandler(this.btnRegister_Click);
            // 
            // btnUnRegister
            // 
            this.btnUnRegister.Location = new System.Drawing.Point(13, 144);
            this.btnUnRegister.Name = "btnUnRegister";
            this.btnUnRegister.Size = new System.Drawing.Size(102, 35);
            this.btnUnRegister.TabIndex = 15;
            this.btnUnRegister.Text = "Unregister";
            this.btnUnRegister.UseVisualStyleBackColor = true;
            this.btnUnRegister.Click += new System.EventHandler(this.btnUnRegister_Click);
            // 
            // statStripRegister
            // 
            this.statStripRegister.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.labelRegister});
            this.statStripRegister.Location = new System.Drawing.Point(0, 242);
            this.statStripRegister.Name = "statStripRegister";
            this.statStripRegister.Size = new System.Drawing.Size(532, 22);
            this.statStripRegister.TabIndex = 16;
            this.statStripRegister.Text = "statusStrip1";
            // 
            // labelRegister
            // 
            this.labelRegister.Name = "labelRegister";
            this.labelRegister.Size = new System.Drawing.Size(0, 17);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(532, 264);
            this.Controls.Add(this.statStripRegister);
            this.Controls.Add(this.btnUnRegister);
            this.Controls.Add(this.btnRegister);
            this.Controls.Add(this.lboxResponse);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.button1);
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Preview controles geográficos";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.statStripRegister.ResumeLayout(false);
            this.statStripRegister.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ListBox lboxResponse;
        private System.Windows.Forms.Button btnRegister;
        private System.Windows.Forms.Button btnUnRegister;
        private System.Windows.Forms.StatusStrip statStripRegister;
        private System.Windows.Forms.ToolStripStatusLabel labelRegister;
    }
}