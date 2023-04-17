namespace WinFormsIDialogUI
{
	partial class Form1
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
			this.btnSynchronous = new System.Windows.Forms.Button();
			this.btnAsynchronous = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// btnSynchronous
			// 
			this.btnSynchronous.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnSynchronous.Location = new System.Drawing.Point(12, 12);
			this.btnSynchronous.Name = "btnSynchronous";
			this.btnSynchronous.Size = new System.Drawing.Size(350, 440);
			this.btnSynchronous.TabIndex = 0;
			this.btnSynchronous.Text = "Synchronous";
			this.btnSynchronous.UseVisualStyleBackColor = true;
			this.btnSynchronous.Click += new System.EventHandler(this.btnSynchronous_Click);
			// 
			// btnAsynchronous
			// 
			this.btnAsynchronous.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnAsynchronous.Location = new System.Drawing.Point(409, 12);
			this.btnAsynchronous.Name = "btnAsynchronous";
			this.btnAsynchronous.Size = new System.Drawing.Size(350, 440);
			this.btnAsynchronous.TabIndex = 1;
			this.btnAsynchronous.Text = "Asynchronous";
			this.btnAsynchronous.UseVisualStyleBackColor = true;
			this.btnAsynchronous.Click += new System.EventHandler(this.btnAsynchronous_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(770, 464);
			this.Controls.Add(this.btnAsynchronous);
			this.Controls.Add(this.btnSynchronous);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button btnSynchronous;
		private System.Windows.Forms.Button btnAsynchronous;
	}
}

