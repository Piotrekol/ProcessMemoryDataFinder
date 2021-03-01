
namespace StructuredOsuMemoryProviderTester
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button_ResetReadTimeMinMax = new System.Windows.Forms.Button();
            this.textBox_readTime = new System.Windows.Forms.TextBox();
            this.numericUpDown_readDelay = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.textBox_Data = new System.Windows.Forms.TextBox();
            this.textBox_ReadTimes = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_readDelay)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_ResetReadTimeMinMax
            // 
            this.button_ResetReadTimeMinMax.Location = new System.Drawing.Point(178, 86);
            this.button_ResetReadTimeMinMax.Name = "button_ResetReadTimeMinMax";
            this.button_ResetReadTimeMinMax.Size = new System.Drawing.Size(75, 23);
            this.button_ResetReadTimeMinMax.TabIndex = 21;
            this.button_ResetReadTimeMinMax.Text = "Reset";
            this.button_ResetReadTimeMinMax.UseVisualStyleBackColor = true;
            this.button_ResetReadTimeMinMax.Click += new System.EventHandler(this.button_ResetReadTimeMinMax_Click);
            // 
            // textBox_readTime
            // 
            this.textBox_readTime.Location = new System.Drawing.Point(4, 56);
            this.textBox_readTime.Multiline = true;
            this.textBox_readTime.Name = "textBox_readTime";
            this.textBox_readTime.Size = new System.Drawing.Size(168, 53);
            this.textBox_readTime.TabIndex = 20;
            // 
            // numericUpDown_readDelay
            // 
            this.numericUpDown_readDelay.Location = new System.Drawing.Point(4, 20);
            this.numericUpDown_readDelay.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDown_readDelay.Name = "numericUpDown_readDelay";
            this.numericUpDown_readDelay.Size = new System.Drawing.Size(178, 23);
            this.numericUpDown_readDelay.TabIndex = 19;
            this.numericUpDown_readDelay.Value = new decimal(new int[] {
            33,
            0,
            0,
            0});
            this.numericUpDown_readDelay.ValueChanged += new System.EventHandler(this.numericUpDown_readDelay_ValueChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(1, 2);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(200, 15);
            this.label6.TabIndex = 18;
            this.label6.Text = "Read delay in ms (set 0 for no delay):";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.button_ResetReadTimeMinMax);
            this.panel1.Controls.Add(this.numericUpDown_readDelay);
            this.panel1.Controls.Add(this.textBox_readTime);
            this.panel1.Location = new System.Drawing.Point(550, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(264, 121);
            this.panel1.TabIndex = 22;
            // 
            // textBox_Data
            // 
            this.textBox_Data.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_Data.Font = new System.Drawing.Font("Segoe UI", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.textBox_Data.Location = new System.Drawing.Point(0, 0);
            this.textBox_Data.Multiline = true;
            this.textBox_Data.Name = "textBox_Data";
            this.textBox_Data.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_Data.Size = new System.Drawing.Size(544, 578);
            this.textBox_Data.TabIndex = 23;
            // 
            // textBox_ReadTimes
            // 
            this.textBox_ReadTimes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_ReadTimes.Location = new System.Drawing.Point(550, 139);
            this.textBox_ReadTimes.Multiline = true;
            this.textBox_ReadTimes.Name = "textBox_ReadTimes";
            this.textBox_ReadTimes.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_ReadTimes.Size = new System.Drawing.Size(407, 439);
            this.textBox_ReadTimes.TabIndex = 24;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(957, 578);
            this.Controls.Add(this.textBox_ReadTimes);
            this.Controls.Add(this.textBox_Data);
            this.Controls.Add(this.panel1);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_readDelay)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_ResetReadTimeMinMax;
        private System.Windows.Forms.TextBox textBox_readTime;
        private System.Windows.Forms.NumericUpDown numericUpDown_readDelay;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox textBox_Data;
        private System.Windows.Forms.TextBox textBox_ReadTimes;
    }
}

