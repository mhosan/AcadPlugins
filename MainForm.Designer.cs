
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
            this.lblStart = new System.Windows.Forms.Label();
            this.lblEnd = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnRequest = new System.Windows.Forms.Button();
            this.lblResponseCode = new System.Windows.Forms.Label();
            this.txtResponse = new System.Windows.Forms.TextBox();
            this.txtResponseString = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(102, 35);
            this.button1.TabIndex = 4;
            this.button1.Text = "Exportar a dxf";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // lblStart
            // 
            this.lblStart.AutoSize = true;
            this.lblStart.Location = new System.Drawing.Point(12, 55);
            this.lblStart.Name = "lblStart";
            this.lblStart.Size = new System.Drawing.Size(76, 13);
            this.lblStart.TabIndex = 5;
            this.lblStart.Text = "Inicio exportar:";
            // 
            // lblEnd
            // 
            this.lblEnd.AutoSize = true;
            this.lblEnd.Location = new System.Drawing.Point(13, 76);
            this.lblEnd.Name = "lblEnd";
            this.lblEnd.Size = new System.Drawing.Size(65, 13);
            this.lblEnd.TabIndex = 6;
            this.lblEnd.Text = "Fin exportar:";
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(427, 282);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(82, 27);
            this.btnClose.TabIndex = 7;
            this.btnClose.Text = "Cerrar";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnRequest
            // 
            this.btnRequest.Location = new System.Drawing.Point(12, 139);
            this.btnRequest.Name = "btnRequest";
            this.btnRequest.Size = new System.Drawing.Size(102, 35);
            this.btnRequest.TabIndex = 8;
            this.btnRequest.Text = "Request";
            this.btnRequest.UseVisualStyleBackColor = true;
            this.btnRequest.Click += new System.EventHandler(this.btnRequest_Click);
            // 
            // lblResponseCode
            // 
            this.lblResponseCode.AutoSize = true;
            this.lblResponseCode.Location = new System.Drawing.Point(123, 150);
            this.lblResponseCode.Name = "lblResponseCode";
            this.lblResponseCode.Size = new System.Drawing.Size(92, 13);
            this.lblResponseCode.TabIndex = 9;
            this.lblResponseCode.Text = "Código respuesta:";
            // 
            // txtResponse
            // 
            this.txtResponse.Location = new System.Drawing.Point(120, 77);
            this.txtResponse.Multiline = true;
            this.txtResponse.Name = "txtResponse";
            this.txtResponse.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtResponse.Size = new System.Drawing.Size(389, 48);
            this.txtResponse.TabIndex = 11;
            // 
            // txtResponseString
            // 
            this.txtResponseString.Location = new System.Drawing.Point(120, 177);
            this.txtResponseString.Multiline = true;
            this.txtResponseString.Name = "txtResponseString";
            this.txtResponseString.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtResponseString.Size = new System.Drawing.Size(389, 100);
            this.txtResponseString.TabIndex = 12;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(521, 321);
            this.Controls.Add(this.txtResponseString);
            this.Controls.Add(this.txtResponse);
            this.Controls.Add(this.lblResponseCode);
            this.Controls.Add(this.btnRequest);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lblEnd);
            this.Controls.Add(this.lblStart);
            this.Controls.Add(this.button1);
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Formulario de prueba";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label lblStart;
        private System.Windows.Forms.Label lblEnd;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnRequest;
        private System.Windows.Forms.Label lblResponseCode;
        private System.Windows.Forms.TextBox txtResponse;
        private System.Windows.Forms.TextBox txtResponseString;
    }
}