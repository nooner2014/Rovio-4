namespace PredatorPreyAssignment
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
            this.components = new System.ComponentModel.Container();
            this.picboxCameraImage = new System.Windows.Forms.PictureBox();
            this.buttonPredator = new System.Windows.Forms.Button();
            this.buttonUser = new System.Windows.Forms.Button();
            this.labelDocked = new System.Windows.Forms.Label();
            this.updateTimer = new System.Windows.Forms.Timer(this.components);
            this.label6 = new System.Windows.Forms.Label();
            this.labelDirection = new System.Windows.Forms.Label();
            this.labelDirectionLabel = new System.Windows.Forms.Label();
            this.textBoxIP = new System.Windows.Forms.TextBox();
            this.buttonStop = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.labelBattery = new System.Windows.Forms.Label();
            this.labelLum = new System.Windows.Forms.Label();
            this.labelSat = new System.Windows.Forms.Label();
            this.labelHue = new System.Windows.Forms.Label();
            this.buttonPredatorFSM = new System.Windows.Forms.Button();
            this.picBoxUserLabels = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picboxCameraImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxUserLabels)).BeginInit();
            this.SuspendLayout();
            // 
            // picboxCameraImage
            // 
            this.picboxCameraImage.Location = new System.Drawing.Point(12, 12);
            this.picboxCameraImage.Name = "picboxCameraImage";
            this.picboxCameraImage.Size = new System.Drawing.Size(260, 238);
            this.picboxCameraImage.TabIndex = 0;
            this.picboxCameraImage.TabStop = false;
            // 
            // buttonPredator
            // 
            this.buttonPredator.Location = new System.Drawing.Point(12, 332);
            this.buttonPredator.Name = "buttonPredator";
            this.buttonPredator.Size = new System.Drawing.Size(75, 23);
            this.buttonPredator.TabIndex = 0;
            this.buttonPredator.Text = "Predator";
            this.buttonPredator.UseVisualStyleBackColor = true;
            // 
            // buttonUser
            // 
            this.buttonUser.Location = new System.Drawing.Point(93, 332);
            this.buttonUser.Name = "buttonUser";
            this.buttonUser.Size = new System.Drawing.Size(75, 23);
            this.buttonUser.TabIndex = 0;
            this.buttonUser.Text = "User";
            this.buttonUser.UseVisualStyleBackColor = true;
            // 
            // labelDocked
            // 
            this.labelDocked.AutoSize = true;
            this.labelDocked.Location = new System.Drawing.Point(248, 337);
            this.labelDocked.Name = "labelDocked";
            this.labelDocked.Size = new System.Drawing.Size(73, 13);
            this.labelDocked.TabIndex = 6;
            this.labelDocked.Text = "dockedStatus";
            this.labelDocked.Visible = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(195, 337);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(47, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "Status:";
            this.label6.Visible = false;
            // 
            // labelDirection
            // 
            this.labelDirection.AutoSize = true;
            this.labelDirection.Location = new System.Drawing.Point(404, 337);
            this.labelDirection.Name = "labelDirection";
            this.labelDirection.Size = new System.Drawing.Size(59, 13);
            this.labelDirection.TabIndex = 10;
            this.labelDirection.Text = "<direction>";
            this.labelDirection.Visible = false;
            // 
            // labelDirectionLabel
            // 
            this.labelDirectionLabel.AutoSize = true;
            this.labelDirectionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDirectionLabel.Location = new System.Drawing.Point(336, 337);
            this.labelDirectionLabel.Name = "labelDirectionLabel";
            this.labelDirectionLabel.Size = new System.Drawing.Size(62, 13);
            this.labelDirectionLabel.TabIndex = 11;
            this.labelDirectionLabel.Text = "Direction:";
            this.labelDirectionLabel.Visible = false;
            // 
            // textBoxIP
            // 
            this.textBoxIP.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxIP.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.textBoxIP.Location = new System.Drawing.Point(174, 361);
            this.textBoxIP.Name = "textBoxIP";
            this.textBoxIP.Size = new System.Drawing.Size(123, 20);
            this.textBoxIP.TabIndex = 0;
            this.textBoxIP.Text = "http://10.82.0.41";
            this.textBoxIP.TextChanged += new System.EventHandler(this.textBoxIP_TextChanged);
            // 
            // buttonStop
            // 
            this.buttonStop.Enabled = false;
            this.buttonStop.Location = new System.Drawing.Point(94, 359);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(74, 23);
            this.buttonStop.TabIndex = 0;
            this.buttonStop.Text = "Stop";
            this.buttonStop.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(384, 180);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(51, 13);
            this.label7.TabIndex = 9;
            this.label7.Text = "Charge:";
            // 
            // labelBattery
            // 
            this.labelBattery.AutoSize = true;
            this.labelBattery.Location = new System.Drawing.Point(384, 155);
            this.labelBattery.Name = "labelBattery";
            this.labelBattery.Size = new System.Drawing.Size(62, 13);
            this.labelBattery.TabIndex = 7;
            this.labelBattery.Text = "labelBattery";
            // 
            // labelLum
            // 
            this.labelLum.AutoSize = true;
            this.labelLum.Location = new System.Drawing.Point(384, 129);
            this.labelLum.Name = "labelLum";
            this.labelLum.Size = new System.Drawing.Size(77, 13);
            this.labelLum.TabIndex = 5;
            this.labelLum.Text = "Lum (min, max)";
            // 
            // labelSat
            // 
            this.labelSat.AutoSize = true;
            this.labelSat.Location = new System.Drawing.Point(384, 105);
            this.labelSat.Name = "labelSat";
            this.labelSat.Size = new System.Drawing.Size(73, 13);
            this.labelSat.TabIndex = 4;
            this.labelSat.Text = "Sat (min, max)";
            // 
            // labelHue
            // 
            this.labelHue.AutoSize = true;
            this.labelHue.Location = new System.Drawing.Point(384, 83);
            this.labelHue.Name = "labelHue";
            this.labelHue.Size = new System.Drawing.Size(51, 13);
            this.labelHue.TabIndex = 3;
            this.labelHue.Text = "Hue (r, v)";
            // 
            // buttonPredatorFSM
            // 
            this.buttonPredatorFSM.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonPredatorFSM.Location = new System.Drawing.Point(13, 361);
            this.buttonPredatorFSM.Name = "buttonPredatorFSM";
            this.buttonPredatorFSM.Size = new System.Drawing.Size(75, 23);
            this.buttonPredatorFSM.TabIndex = 0;
            this.buttonPredatorFSM.Text = "PredatorSimple";
            this.buttonPredatorFSM.UseVisualStyleBackColor = true;
            // 
            // picBoxUserLabels
            // 
            this.picBoxUserLabels.Location = new System.Drawing.Point(387, 33);
            this.picBoxUserLabels.Name = "picBoxUserLabels";
            this.picBoxUserLabels.Size = new System.Drawing.Size(253, 217);
            this.picBoxUserLabels.TabIndex = 12;
            this.picBoxUserLabels.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(751, 397);
            this.Controls.Add(this.picBoxUserLabels);
            this.Controls.Add(this.buttonPredatorFSM);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.textBoxIP);
            this.Controls.Add(this.labelDirectionLabel);
            this.Controls.Add(this.labelDirection);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.labelBattery);
            this.Controls.Add(this.labelDocked);
            this.Controls.Add(this.labelLum);
            this.Controls.Add(this.labelSat);
            this.Controls.Add(this.labelHue);
            this.Controls.Add(this.buttonUser);
            this.Controls.Add(this.buttonPredator);
            this.Controls.Add(this.picboxCameraImage);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(767, 435);
            this.MinimumSize = new System.Drawing.Size(767, 435);
            this.Name = "MainForm";
            this.Text = "ImageViewer";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ImageViewer_FormClosed);
            this.Load += new System.EventHandler(this.ImageViewer_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ImageViewer_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ImageViewer_KeyUp);
            ((System.ComponentModel.ISupportInitialize)(this.picboxCameraImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxUserLabels)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picboxCameraImage;
        private System.Windows.Forms.Button buttonPredator;
        private System.Windows.Forms.Button buttonUser;
        private System.Windows.Forms.Label labelDocked;
        private System.Windows.Forms.Timer updateTimer;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label labelDirection;
        private System.Windows.Forms.Label labelDirectionLabel;
        private System.Windows.Forms.TextBox textBoxIP;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label labelBattery;
        private System.Windows.Forms.Label labelLum;
        private System.Windows.Forms.Label labelSat;
        private System.Windows.Forms.Label labelHue;
        private System.Windows.Forms.Button buttonPredatorFSM;
        private System.Windows.Forms.PictureBox picBoxUserLabels;
    }
}