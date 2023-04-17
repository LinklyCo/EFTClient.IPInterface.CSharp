namespace WinFormsIDialogUI
{
	partial class WinFormDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WinFormDialog));
			this.BtnOK = new System.Windows.Forms.Button();
			this.BtnCancel = new System.Windows.Forms.Button();
			this.txtDisplayLine1 = new System.Windows.Forms.Label();
			this.txtDisplayLine2 = new System.Windows.Forms.Label();
			this.txtInputData = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.axWindowsMediaPlayer1 = new AxWMPLib.AxWindowsMediaPlayer();
			this.panel1 = new System.Windows.Forms.Panel();
			this.splitter1 = new System.Windows.Forms.Splitter();
			((System.ComponentModel.ISupportInitialize)(this.axWindowsMediaPlayer1)).BeginInit();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BtnOK
			// 
			this.BtnOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.BtnOK.Location = new System.Drawing.Point(33, 144);
			this.BtnOK.Name = "BtnOK";
			this.BtnOK.Size = new System.Drawing.Size(76, 39);
			this.BtnOK.TabIndex = 0;
			this.BtnOK.Text = "OK";
			this.BtnOK.UseVisualStyleBackColor = true;
			this.BtnOK.Visible = false;
			// 
			// BtnCancel
			// 
			this.BtnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.BtnCancel.Location = new System.Drawing.Point(178, 144);
			this.BtnCancel.Name = "BtnCancel";
			this.BtnCancel.Size = new System.Drawing.Size(98, 39);
			this.BtnCancel.TabIndex = 1;
			this.BtnCancel.Text = "Cancel";
			this.BtnCancel.UseVisualStyleBackColor = true;
			this.BtnCancel.Visible = false;
			// 
			// txtDisplayLine1
			// 
			this.txtDisplayLine1.AutoSize = true;
			this.txtDisplayLine1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtDisplayLine1.Location = new System.Drawing.Point(12, 9);
			this.txtDisplayLine1.MinimumSize = new System.Drawing.Size(380, 35);
			this.txtDisplayLine1.Name = "txtDisplayLine1";
			this.txtDisplayLine1.Size = new System.Drawing.Size(380, 35);
			this.txtDisplayLine1.TabIndex = 2;
			this.txtDisplayLine1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// txtDisplayLine2
			// 
			this.txtDisplayLine2.AutoSize = true;
			this.txtDisplayLine2.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtDisplayLine2.Location = new System.Drawing.Point(12, 44);
			this.txtDisplayLine2.MinimumSize = new System.Drawing.Size(380, 35);
			this.txtDisplayLine2.Name = "txtDisplayLine2";
			this.txtDisplayLine2.Size = new System.Drawing.Size(380, 35);
			this.txtDisplayLine2.TabIndex = 3;
			this.txtDisplayLine2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// txtInputData
			// 
			this.txtInputData.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtInputData.Location = new System.Drawing.Point(85, 82);
			this.txtInputData.Name = "txtInputData";
			this.txtInputData.Size = new System.Drawing.Size(231, 32);
			this.txtInputData.TabIndex = 4;
			this.txtInputData.Visible = false;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(13, 85);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(66, 26);
			this.label1.TabIndex = 5;
			this.label1.Text = "Input:";
			this.label1.Visible = false;
			// 
			// axWindowsMediaPlayer1
			// 
			this.axWindowsMediaPlayer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.axWindowsMediaPlayer1.Enabled = true;
			this.axWindowsMediaPlayer1.Location = new System.Drawing.Point(0, 0);
			this.axWindowsMediaPlayer1.Name = "axWindowsMediaPlayer1";
			this.axWindowsMediaPlayer1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axWindowsMediaPlayer1.OcxState")));
			this.axWindowsMediaPlayer1.Size = new System.Drawing.Size(128, 100);
			this.axWindowsMediaPlayer1.TabIndex = 6;
			this.axWindowsMediaPlayer1.Visible = false;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.splitter1);
			this.panel1.Controls.Add(this.axWindowsMediaPlayer1);
			this.panel1.Location = new System.Drawing.Point(368, 83);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(128, 100);
			this.panel1.TabIndex = 7;
			// 
			// splitter1
			// 
			this.splitter1.Location = new System.Drawing.Point(0, 0);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(3, 100);
			this.splitter1.TabIndex = 7;
			this.splitter1.TabStop = false;
			// 
			// WinFormDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(508, 209);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.txtInputData);
			this.Controls.Add(this.txtDisplayLine2);
			this.Controls.Add(this.txtDisplayLine1);
			this.Controls.Add(this.BtnCancel);
			this.Controls.Add(this.BtnOK);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(524, 248);
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(524, 248);
			this.Name = "WinFormDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "WinFormDialog";
			((System.ComponentModel.ISupportInitialize)(this.axWindowsMediaPlayer1)).EndInit();
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label label1;
		public System.Windows.Forms.Button BtnOK;
		public System.Windows.Forms.Button BtnCancel;
		public System.Windows.Forms.Label txtDisplayLine1;
		public System.Windows.Forms.Label txtDisplayLine2;
		public System.Windows.Forms.TextBox txtInputData;
		public AxWMPLib.AxWindowsMediaPlayer axWindowsMediaPlayer1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Splitter splitter1;
	}
}