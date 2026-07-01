namespace Level2.PlateLeveler.Client
{
   partial class PopupDialog
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
         this.components = new System.ComponentModel.Container();
         this.label2 = new System.Windows.Forms.Label();
         this.button1 = new System.Windows.Forms.Button();
         this.popupTimer = new System.Windows.Forms.Timer(this.components);
         this.SuspendLayout();
         // 
         // label2
         // 
         this.label2.AutoSize = true;
         this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.label2.Location = new System.Drawing.Point(55, 27);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(301, 20);
         this.label2.TabIndex = 1;
         this.label2.Text = "There is no avaliable PDI in the database!";
         // 
         // button1
         // 
         this.button1.Location = new System.Drawing.Point(114, 62);
         this.button1.Name = "button1";
         this.button1.Size = new System.Drawing.Size(169, 28);
         this.button1.TabIndex = 2;
         this.button1.Text = "OK";
         this.button1.UseVisualStyleBackColor = true;
         this.button1.Click += new System.EventHandler(this.button1_Click);
         // 
         // popupTimer
         // 
         this.popupTimer.Enabled = true;
         this.popupTimer.Interval = 20000;
         this.popupTimer.Tick += new System.EventHandler(this.popupTimer_Tick);
         // 
         // PopupDialog
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.AutoSize = true;
         this.ClientSize = new System.Drawing.Size(404, 102);
         this.Controls.Add(this.button1);
         this.Controls.Add(this.label2);
         this.MaximizeBox = false;
         this.MaximumSize = new System.Drawing.Size(420, 140);
         this.MinimizeBox = false;
         this.MinimumSize = new System.Drawing.Size(420, 140);
         this.Name = "PopupDialog";
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
         this.Text = "Missing PDI!";
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.Button button1;
      public System.Windows.Forms.Timer popupTimer;
   }
}