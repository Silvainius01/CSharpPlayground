namespace CSharpPlayground
{
    partial class WumpusWindow
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
        public void InitializeComponent()
        {
            this.consoleLabel = new System.Windows.Forms.Label();
            this.consoleInput = new System.Windows.Forms.TextBox();
            this.consoleOutput = new System.Windows.Forms.TextBox();
            this.invWindow = new System.Windows.Forms.TextBox();
            this.invLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // consoleLabel
            // 
            this.consoleLabel.AutoSize = true;
            this.consoleLabel.Location = new System.Drawing.Point(27, 20);
            this.consoleLabel.Name = "consoleLabel";
            this.consoleLabel.Size = new System.Drawing.Size(59, 17);
            this.consoleLabel.TabIndex = 1;
            this.consoleLabel.Text = "Console";
            this.consoleLabel.Click += new System.EventHandler(this.label1_Click);
            // 
            // consoleInput
            // 
            this.consoleInput.Location = new System.Drawing.Point(30, 677);
            this.consoleInput.Name = "consoleInput";
            this.consoleInput.Size = new System.Drawing.Size(600, 22);
            this.consoleInput.TabIndex = 0;
            // 
            // consoleOutput
            // 
            this.consoleOutput.Location = new System.Drawing.Point(30, 40);
            this.consoleOutput.Multiline = true;
            this.consoleOutput.Name = "consoleOutput";
            this.consoleOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.consoleOutput.Size = new System.Drawing.Size(429, 620);
            this.consoleOutput.TabIndex = 2;
            // 
            // InvWindow
            // 
            this.invWindow.Location = new System.Drawing.Point(481, 40);
            this.invWindow.Multiline = true;
            this.invWindow.Name = "InvWindow";
            this.invWindow.Size = new System.Drawing.Size(149, 620);
            this.invWindow.TabIndex = 4;
            // 
            // invLabel
            // 
            this.invLabel.AutoSize = true;
            this.invLabel.Location = new System.Drawing.Point(478, 20);
            this.invLabel.Name = "invLabel";
            this.invLabel.Size = new System.Drawing.Size(66, 17);
            this.invLabel.TabIndex = 5;
            this.invLabel.Text = "Inventory";
            // 
            // WumpusWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 720);
            this.Controls.Add(this.invLabel);
            this.Controls.Add(this.invWindow);
            this.Controls.Add(this.consoleOutput);
            this.Controls.Add(this.consoleLabel);
            this.Controls.Add(this.consoleInput);
            this.Name = "WumpusWindow";
            this.Text = "Hunt the Wumpus";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.Label consoleLabel;
        public System.Windows.Forms.TextBox consoleInput;
        public System.Windows.Forms.TextBox consoleOutput;
        public System.Windows.Forms.TextBox invWindow;
        public System.Windows.Forms.Label invLabel;
    }
}