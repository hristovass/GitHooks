
namespace VerigaBlokov
{
    partial class VerigaBlokov
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
            this.label1 = new System.Windows.Forms.Label();
            this.tbLocalPort = new System.Windows.Forms.TextBox();
            this.rtbBlockChain = new System.Windows.Forms.RichTextBox();
            this.rtbLog = new System.Windows.Forms.RichTextBox();
            this.btStart = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.btConnect = new System.Windows.Forms.Button();
            this.tbRemotePort = new System.Windows.Forms.TextBox();
            this.btMine = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Listener port:";
            // 
            // tbLocalPort
            // 
            this.tbLocalPort.Location = new System.Drawing.Point(87, 9);
            this.tbLocalPort.Name = "tbLocalPort";
            this.tbLocalPort.Size = new System.Drawing.Size(52, 20);
            this.tbLocalPort.TabIndex = 1;
            this.tbLocalPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // rtbBlockChain
            // 
            this.rtbBlockChain.Location = new System.Drawing.Point(12, 49);
            this.rtbBlockChain.Name = "rtbBlockChain";
            this.rtbBlockChain.Size = new System.Drawing.Size(616, 117);
            this.rtbBlockChain.TabIndex = 7;
            this.rtbBlockChain.Text = "";
            // 
            // rtbLog
            // 
            this.rtbLog.Location = new System.Drawing.Point(12, 172);
            this.rtbLog.Name = "rtbLog";
            this.rtbLog.Size = new System.Drawing.Size(616, 118);
            this.rtbLog.TabIndex = 8;
            this.rtbLog.Text = "";
            // 
            // btStart
            // 
            this.btStart.Location = new System.Drawing.Point(145, 8);
            this.btStart.Name = "btStart";
            this.btStart.Size = new System.Drawing.Size(75, 23);
            this.btStart.TabIndex = 2;
            this.btStart.Text = "Start";
            this.btStart.UseVisualStyleBackColor = true;
            this.btStart.Click += new System.EventHandler(this.btStart_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(246, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Remote port:";
            // 
            // btConnect
            // 
            this.btConnect.Location = new System.Drawing.Point(378, 6);
            this.btConnect.Name = "btConnect";
            this.btConnect.Size = new System.Drawing.Size(75, 23);
            this.btConnect.TabIndex = 5;
            this.btConnect.Text = "Connect";
            this.btConnect.UseVisualStyleBackColor = true;
            this.btConnect.Click += new System.EventHandler(this.btConnect_Click);
            // 
            // tbRemotePort
            // 
            this.tbRemotePort.Location = new System.Drawing.Point(320, 8);
            this.tbRemotePort.Name = "tbRemotePort";
            this.tbRemotePort.Size = new System.Drawing.Size(52, 20);
            this.tbRemotePort.TabIndex = 4;
            this.tbRemotePort.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // btMine
            // 
            this.btMine.Location = new System.Drawing.Point(553, 8);
            this.btMine.Name = "btMine";
            this.btMine.Size = new System.Drawing.Size(75, 23);
            this.btMine.TabIndex = 6;
            this.btMine.Text = "Mine";
            this.btMine.UseVisualStyleBackColor = true;
            this.btMine.Click += new System.EventHandler(this.btMine_Click);
            // 
            // VerigaBlokov
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 302);
            this.Controls.Add(this.btMine);
            this.Controls.Add(this.tbRemotePort);
            this.Controls.Add(this.btConnect);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btStart);
            this.Controls.Add(this.rtbLog);
            this.Controls.Add(this.rtbBlockChain);
            this.Controls.Add(this.tbLocalPort);
            this.Controls.Add(this.label1);
            this.Name = "VerigaBlokov";
            this.Text = "Veriga Blokov";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbLocalPort;
        private System.Windows.Forms.RichTextBox rtbBlockChain;
        private System.Windows.Forms.RichTextBox rtbLog;
        private System.Windows.Forms.Button btStart;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btConnect;
        private System.Windows.Forms.TextBox tbRemotePort;
        private System.Windows.Forms.Button btMine;
    }
}

