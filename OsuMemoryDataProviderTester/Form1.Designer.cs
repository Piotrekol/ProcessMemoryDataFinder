namespace OsuMemoryDataProviderTester
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
            this.textBox_mapId = new System.Windows.Forms.TextBox();
            this.textBox_strings = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_Status = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_CurrentPlayData = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.numericUpDown_readDelay = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox_mapData = new System.Windows.Forms.TextBox();
            this.textBox_time = new System.Windows.Forms.TextBox();
            this.textBox_TourneyStuff = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_readDelay)).BeginInit();
            this.SuspendLayout();
            // 
            // textBox_mapId
            // 
            this.textBox_mapId.Location = new System.Drawing.Point(11, 55);
            this.textBox_mapId.Name = "textBox_mapId";
            this.textBox_mapId.Size = new System.Drawing.Size(100, 20);
            this.textBox_mapId.TabIndex = 0;
            // 
            // textBox_strings
            // 
            this.textBox_strings.Location = new System.Drawing.Point(173, 28);
            this.textBox_strings.Multiline = true;
            this.textBox_strings.Name = "textBox_strings";
            this.textBox_strings.Size = new System.Drawing.Size(603, 101);
            this.textBox_strings.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "mapId";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(170, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Strings";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 79);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Status";
            // 
            // textBox_Status
            // 
            this.textBox_Status.Location = new System.Drawing.Point(11, 95);
            this.textBox_Status.Name = "textBox_Status";
            this.textBox_Status.Size = new System.Drawing.Size(100, 20);
            this.textBox_Status.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 158);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(157, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "CurrentPlay (need to be playing)";
            // 
            // textBox_CurrentPlayData
            // 
            this.textBox_CurrentPlayData.Location = new System.Drawing.Point(14, 174);
            this.textBox_CurrentPlayData.Multiline = true;
            this.textBox_CurrentPlayData.Name = "textBox_CurrentPlayData";
            this.textBox_CurrentPlayData.Size = new System.Drawing.Size(364, 143);
            this.textBox_CurrentPlayData.TabIndex = 6;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(384, 174);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(336, 39);
            this.label5.TabIndex = 8;
            this.label5.Text = "when accessing specific value (map data/ status etc.) for the first time\r\nit migh" +
    "t take several seconds for it to appear(initalize) here.\r\nHowever any following " +
    "accesses should be pretty much instant.";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(384, 224);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(181, 13);
            this.label6.TabIndex = 9;
            this.label6.Text = "Read delay in ms (set 0 for no delay):";
            // 
            // numericUpDown_readDelay
            // 
            this.numericUpDown_readDelay.Location = new System.Drawing.Point(387, 241);
            this.numericUpDown_readDelay.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDown_readDelay.Name = "numericUpDown_readDelay";
            this.numericUpDown_readDelay.Size = new System.Drawing.Size(178, 20);
            this.numericUpDown_readDelay.TabIndex = 10;
            this.numericUpDown_readDelay.Value = new decimal(new int[] {
            33,
            0,
            0,
            0});
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(14, 119);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(52, 13);
            this.label7.TabIndex = 12;
            this.label7.Text = "Map data";
            // 
            // textBox_mapData
            // 
            this.textBox_mapData.Location = new System.Drawing.Point(11, 135);
            this.textBox_mapData.Name = "textBox_mapData";
            this.textBox_mapData.Size = new System.Drawing.Size(233, 20);
            this.textBox_mapData.TabIndex = 11;
            // 
            // textBox_time
            // 
            this.textBox_time.Location = new System.Drawing.Point(11, 9);
            this.textBox_time.Name = "textBox_time";
            this.textBox_time.Size = new System.Drawing.Size(100, 20);
            this.textBox_time.TabIndex = 13;
            // 
            // textBox_TourneyStuff
            // 
            this.textBox_TourneyStuff.Location = new System.Drawing.Point(14, 323);
            this.textBox_TourneyStuff.Multiline = true;
            this.textBox_TourneyStuff.Name = "textBox_TourneyStuff";
            this.textBox_TourneyStuff.Size = new System.Drawing.Size(364, 143);
            this.textBox_TourneyStuff.TabIndex = 14;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(791, 477);
            this.Controls.Add(this.textBox_TourneyStuff);
            this.Controls.Add(this.textBox_time);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.textBox_mapData);
            this.Controls.Add(this.numericUpDown_readDelay);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox_CurrentPlayData);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_Status);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_strings);
            this.Controls.Add(this.textBox_mapId);
            this.Name = "Form1";
            this.Text = "OsuMemoryDataProvider Tester";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_readDelay)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_mapId;
        private System.Windows.Forms.TextBox textBox_strings;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_Status;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_CurrentPlayData;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown numericUpDown_readDelay;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox_mapData;
        private System.Windows.Forms.TextBox textBox_time;
        private System.Windows.Forms.TextBox textBox_TourneyStuff;
    }
}

