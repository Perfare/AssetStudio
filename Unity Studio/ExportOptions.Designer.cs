namespace Unity_Studio
{
    partial class ExportOptions
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
            this.includeBox = new System.Windows.Forms.GroupBox();
            this.convertDummies = new System.Windows.Forms.CheckBox();
            this.embedBox = new System.Windows.Forms.CheckBox();
            this.lightsBox = new System.Windows.Forms.CheckBox();
            this.camerasBox = new System.Windows.Forms.CheckBox();
            this.exportDeformers = new System.Windows.Forms.CheckBox();
            this.geometryBox = new System.Windows.Forms.GroupBox();
            this.exportColors = new System.Windows.Forms.CheckBox();
            this.exportUVs = new System.Windows.Forms.CheckBox();
            this.exportTangents = new System.Windows.Forms.CheckBox();
            this.exportNormals = new System.Windows.Forms.CheckBox();
            this.advancedBox = new System.Windows.Forms.GroupBox();
            this.axisLabel = new System.Windows.Forms.Label();
            this.upAxis = new System.Windows.Forms.ComboBox();
            this.scaleFactor = new System.Windows.Forms.NumericUpDown();
            this.scaleLabel = new System.Windows.Forms.Label();
            this.fbxOKbutton = new System.Windows.Forms.Button();
            this.fbxCancel = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.showExpOpt = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.convertfsb = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tojpg = new System.Windows.Forms.RadioButton();
            this.topng = new System.Windows.Forms.RadioButton();
            this.tobmp = new System.Windows.Forms.RadioButton();
            this.converttexture = new System.Windows.Forms.CheckBox();
            this.includeBox.SuspendLayout();
            this.geometryBox.SuspendLayout();
            this.advancedBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scaleFactor)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // includeBox
            // 
            this.includeBox.AutoSize = true;
            this.includeBox.Controls.Add(this.convertDummies);
            this.includeBox.Controls.Add(this.embedBox);
            this.includeBox.Controls.Add(this.lightsBox);
            this.includeBox.Controls.Add(this.camerasBox);
            this.includeBox.Controls.Add(this.exportDeformers);
            this.includeBox.Controls.Add(this.geometryBox);
            this.includeBox.Location = new System.Drawing.Point(16, 15);
            this.includeBox.Margin = new System.Windows.Forms.Padding(4);
            this.includeBox.Name = "includeBox";
            this.includeBox.Padding = new System.Windows.Forms.Padding(4);
            this.includeBox.Size = new System.Drawing.Size(332, 334);
            this.includeBox.TabIndex = 0;
            this.includeBox.TabStop = false;
            this.includeBox.Text = "Include";
            // 
            // convertDummies
            // 
            this.convertDummies.AutoSize = true;
            this.convertDummies.Location = new System.Drawing.Point(19, 205);
            this.convertDummies.Margin = new System.Windows.Forms.Padding(4);
            this.convertDummies.Name = "convertDummies";
            this.convertDummies.Size = new System.Drawing.Size(302, 20);
            this.convertDummies.TabIndex = 5;
            this.convertDummies.Text = "Convert Deforming Dummies to Bones";
            this.convertDummies.UseVisualStyleBackColor = true;
            this.convertDummies.CheckedChanged += new System.EventHandler(this.exportOpnions_CheckedChanged);
            // 
            // embedBox
            // 
            this.embedBox.AutoSize = true;
            this.embedBox.Enabled = false;
            this.embedBox.Location = new System.Drawing.Point(19, 288);
            this.embedBox.Margin = new System.Windows.Forms.Padding(4);
            this.embedBox.Name = "embedBox";
            this.embedBox.Size = new System.Drawing.Size(118, 20);
            this.embedBox.TabIndex = 4;
            this.embedBox.Text = "Embed Media";
            this.embedBox.UseVisualStyleBackColor = true;
            // 
            // lightsBox
            // 
            this.lightsBox.AutoSize = true;
            this.lightsBox.Enabled = false;
            this.lightsBox.Location = new System.Drawing.Point(19, 260);
            this.lightsBox.Margin = new System.Windows.Forms.Padding(4);
            this.lightsBox.Name = "lightsBox";
            this.lightsBox.Size = new System.Drawing.Size(78, 20);
            this.lightsBox.TabIndex = 3;
            this.lightsBox.Text = "Lights";
            this.lightsBox.UseVisualStyleBackColor = true;
            // 
            // camerasBox
            // 
            this.camerasBox.AutoSize = true;
            this.camerasBox.Enabled = false;
            this.camerasBox.Location = new System.Drawing.Point(19, 232);
            this.camerasBox.Margin = new System.Windows.Forms.Padding(4);
            this.camerasBox.Name = "camerasBox";
            this.camerasBox.Size = new System.Drawing.Size(86, 20);
            this.camerasBox.TabIndex = 2;
            this.camerasBox.Text = "Cameras";
            this.camerasBox.UseVisualStyleBackColor = true;
            // 
            // exportDeformers
            // 
            this.exportDeformers.AutoSize = true;
            this.exportDeformers.Location = new System.Drawing.Point(19, 178);
            this.exportDeformers.Margin = new System.Windows.Forms.Padding(4);
            this.exportDeformers.Name = "exportDeformers";
            this.exportDeformers.Size = new System.Drawing.Size(142, 20);
            this.exportDeformers.TabIndex = 1;
            this.exportDeformers.Text = "Skin Deformers";
            this.exportDeformers.UseVisualStyleBackColor = true;
            this.exportDeformers.CheckedChanged += new System.EventHandler(this.exportDeformers_CheckedChanged);
            // 
            // geometryBox
            // 
            this.geometryBox.AutoSize = true;
            this.geometryBox.Controls.Add(this.exportColors);
            this.geometryBox.Controls.Add(this.exportUVs);
            this.geometryBox.Controls.Add(this.exportTangents);
            this.geometryBox.Controls.Add(this.exportNormals);
            this.geometryBox.Location = new System.Drawing.Point(9, 22);
            this.geometryBox.Margin = new System.Windows.Forms.Padding(4);
            this.geometryBox.Name = "geometryBox";
            this.geometryBox.Padding = new System.Windows.Forms.Padding(4);
            this.geometryBox.Size = new System.Drawing.Size(313, 152);
            this.geometryBox.TabIndex = 0;
            this.geometryBox.TabStop = false;
            this.geometryBox.Text = "Geometry";
            // 
            // exportColors
            // 
            this.exportColors.AutoSize = true;
            this.exportColors.Checked = true;
            this.exportColors.CheckState = System.Windows.Forms.CheckState.Checked;
            this.exportColors.Location = new System.Drawing.Point(9, 106);
            this.exportColors.Margin = new System.Windows.Forms.Padding(4);
            this.exportColors.Name = "exportColors";
            this.exportColors.Size = new System.Drawing.Size(134, 20);
            this.exportColors.TabIndex = 3;
            this.exportColors.Text = "Vertex Colors";
            this.exportColors.UseVisualStyleBackColor = true;
            this.exportColors.CheckedChanged += new System.EventHandler(this.exportOpnions_CheckedChanged);
            // 
            // exportUVs
            // 
            this.exportUVs.AutoSize = true;
            this.exportUVs.Checked = true;
            this.exportUVs.CheckState = System.Windows.Forms.CheckState.Checked;
            this.exportUVs.Location = new System.Drawing.Point(9, 79);
            this.exportUVs.Margin = new System.Windows.Forms.Padding(4);
            this.exportUVs.Name = "exportUVs";
            this.exportUVs.Size = new System.Drawing.Size(142, 20);
            this.exportUVs.TabIndex = 2;
            this.exportUVs.Text = "UV Coordinates";
            this.exportUVs.UseVisualStyleBackColor = true;
            this.exportUVs.CheckedChanged += new System.EventHandler(this.exportOpnions_CheckedChanged);
            // 
            // exportTangents
            // 
            this.exportTangents.AutoSize = true;
            this.exportTangents.Location = new System.Drawing.Point(9, 51);
            this.exportTangents.Margin = new System.Windows.Forms.Padding(4);
            this.exportTangents.Name = "exportTangents";
            this.exportTangents.Size = new System.Drawing.Size(94, 20);
            this.exportTangents.TabIndex = 1;
            this.exportTangents.Text = "Tangents";
            this.exportTangents.UseVisualStyleBackColor = true;
            this.exportTangents.CheckedChanged += new System.EventHandler(this.exportOpnions_CheckedChanged);
            // 
            // exportNormals
            // 
            this.exportNormals.AutoSize = true;
            this.exportNormals.Checked = true;
            this.exportNormals.CheckState = System.Windows.Forms.CheckState.Checked;
            this.exportNormals.Location = new System.Drawing.Point(9, 22);
            this.exportNormals.Margin = new System.Windows.Forms.Padding(4);
            this.exportNormals.Name = "exportNormals";
            this.exportNormals.Size = new System.Drawing.Size(86, 20);
            this.exportNormals.TabIndex = 0;
            this.exportNormals.Text = "Normals";
            this.exportNormals.UseVisualStyleBackColor = true;
            this.exportNormals.CheckedChanged += new System.EventHandler(this.exportOpnions_CheckedChanged);
            // 
            // advancedBox
            // 
            this.advancedBox.AutoSize = true;
            this.advancedBox.Controls.Add(this.axisLabel);
            this.advancedBox.Controls.Add(this.upAxis);
            this.advancedBox.Controls.Add(this.scaleFactor);
            this.advancedBox.Controls.Add(this.scaleLabel);
            this.advancedBox.Location = new System.Drawing.Point(16, 355);
            this.advancedBox.Margin = new System.Windows.Forms.Padding(4);
            this.advancedBox.Name = "advancedBox";
            this.advancedBox.Padding = new System.Windows.Forms.Padding(4);
            this.advancedBox.Size = new System.Drawing.Size(332, 96);
            this.advancedBox.TabIndex = 5;
            this.advancedBox.TabStop = false;
            this.advancedBox.Text = "Advanced Options";
            // 
            // axisLabel
            // 
            this.axisLabel.AutoSize = true;
            this.axisLabel.Location = new System.Drawing.Point(8, 50);
            this.axisLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.axisLabel.Name = "axisLabel";
            this.axisLabel.Size = new System.Drawing.Size(72, 16);
            this.axisLabel.TabIndex = 3;
            this.axisLabel.Text = "Up Axis:";
            // 
            // upAxis
            // 
            this.upAxis.FormattingEnabled = true;
            this.upAxis.Items.AddRange(new object[] {
            "Y-up"});
            this.upAxis.Location = new System.Drawing.Point(88, 47);
            this.upAxis.Margin = new System.Windows.Forms.Padding(4);
            this.upAxis.MaxDropDownItems = 2;
            this.upAxis.Name = "upAxis";
            this.upAxis.Size = new System.Drawing.Size(92, 23);
            this.upAxis.TabIndex = 2;
            // 
            // scaleFactor
            // 
            this.scaleFactor.DecimalPlaces = 2;
            this.scaleFactor.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.scaleFactor.Location = new System.Drawing.Point(128, 17);
            this.scaleFactor.Margin = new System.Windows.Forms.Padding(4);
            this.scaleFactor.Name = "scaleFactor";
            this.scaleFactor.Size = new System.Drawing.Size(61, 25);
            this.scaleFactor.TabIndex = 1;
            this.scaleFactor.Value = new decimal(new int[] {
            254,
            0,
            0,
            131072});
            // 
            // scaleLabel
            // 
            this.scaleLabel.AutoSize = true;
            this.scaleLabel.Location = new System.Drawing.Point(8, 19);
            this.scaleLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.scaleLabel.Name = "scaleLabel";
            this.scaleLabel.Size = new System.Drawing.Size(112, 16);
            this.scaleLabel.TabIndex = 0;
            this.scaleLabel.Text = "Scale Factor:";
            // 
            // fbxOKbutton
            // 
            this.fbxOKbutton.Location = new System.Drawing.Point(443, 455);
            this.fbxOKbutton.Margin = new System.Windows.Forms.Padding(4);
            this.fbxOKbutton.Name = "fbxOKbutton";
            this.fbxOKbutton.Size = new System.Drawing.Size(100, 26);
            this.fbxOKbutton.TabIndex = 6;
            this.fbxOKbutton.Text = "OK";
            this.fbxOKbutton.UseVisualStyleBackColor = true;
            this.fbxOKbutton.Click += new System.EventHandler(this.fbxOKbutton_Click);
            // 
            // fbxCancel
            // 
            this.fbxCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.fbxCancel.Location = new System.Drawing.Point(560, 455);
            this.fbxCancel.Margin = new System.Windows.Forms.Padding(4);
            this.fbxCancel.Name = "fbxCancel";
            this.fbxCancel.Size = new System.Drawing.Size(100, 26);
            this.fbxCancel.TabIndex = 7;
            this.fbxCancel.Text = "Cancel";
            this.fbxCancel.UseVisualStyleBackColor = true;
            this.fbxCancel.Click += new System.EventHandler(this.fbxCancel_Click);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Filter = "FBX file|*.fbx|Collada|*.dae";
            this.saveFileDialog1.RestoreDirectory = true;
            // 
            // showExpOpt
            // 
            this.showExpOpt.AutoSize = true;
            this.showExpOpt.Location = new System.Drawing.Point(16, 459);
            this.showExpOpt.Margin = new System.Windows.Forms.Padding(4);
            this.showExpOpt.Name = "showExpOpt";
            this.showExpOpt.Size = new System.Drawing.Size(294, 20);
            this.showExpOpt.TabIndex = 8;
            this.showExpOpt.Text = "Show this dialog for every export";
            this.showExpOpt.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.convertfsb);
            this.groupBox1.Controls.Add(this.panel1);
            this.groupBox1.Controls.Add(this.converttexture);
            this.groupBox1.Location = new System.Drawing.Point(356, 15);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(304, 436);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Convert";
            // 
            // convertfsb
            // 
            this.convertfsb.AutoSize = true;
            this.convertfsb.Checked = true;
            this.convertfsb.CheckState = System.Windows.Forms.CheckState.Checked;
            this.convertfsb.Location = new System.Drawing.Point(11, 101);
            this.convertfsb.Margin = new System.Windows.Forms.Padding(4);
            this.convertfsb.Name = "convertfsb";
            this.convertfsb.Size = new System.Drawing.Size(174, 20);
            this.convertfsb.TabIndex = 6;
            this.convertfsb.Text = "Convert FSB to WAV";
            this.convertfsb.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tojpg);
            this.panel1.Controls.Add(this.topng);
            this.panel1.Controls.Add(this.tobmp);
            this.panel1.Location = new System.Drawing.Point(40, 52);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(195, 38);
            this.panel1.TabIndex = 5;
            // 
            // tojpg
            // 
            this.tojpg.AutoSize = true;
            this.tojpg.Location = new System.Drawing.Point(129, 8);
            this.tojpg.Margin = new System.Windows.Forms.Padding(4);
            this.tojpg.Name = "tojpg";
            this.tojpg.Size = new System.Drawing.Size(61, 20);
            this.tojpg.TabIndex = 4;
            this.tojpg.Text = "JPEG";
            this.tojpg.UseVisualStyleBackColor = true;
            // 
            // topng
            // 
            this.topng.AutoSize = true;
            this.topng.Checked = true;
            this.topng.Location = new System.Drawing.Point(67, 8);
            this.topng.Margin = new System.Windows.Forms.Padding(4);
            this.topng.Name = "topng";
            this.topng.Size = new System.Drawing.Size(53, 20);
            this.topng.TabIndex = 3;
            this.topng.TabStop = true;
            this.topng.Text = "PNG";
            this.topng.UseVisualStyleBackColor = true;
            // 
            // tobmp
            // 
            this.tobmp.AutoSize = true;
            this.tobmp.Location = new System.Drawing.Point(4, 8);
            this.tobmp.Margin = new System.Windows.Forms.Padding(4);
            this.tobmp.Name = "tobmp";
            this.tobmp.Size = new System.Drawing.Size(53, 20);
            this.tobmp.TabIndex = 2;
            this.tobmp.Text = "BMP";
            this.tobmp.UseVisualStyleBackColor = true;
            // 
            // converttexture
            // 
            this.converttexture.AutoSize = true;
            this.converttexture.Checked = true;
            this.converttexture.CheckState = System.Windows.Forms.CheckState.Checked;
            this.converttexture.Location = new System.Drawing.Point(11, 25);
            this.converttexture.Margin = new System.Windows.Forms.Padding(4);
            this.converttexture.Name = "converttexture";
            this.converttexture.Size = new System.Drawing.Size(254, 20);
            this.converttexture.TabIndex = 1;
            this.converttexture.Text = "Convert Texture (If support)";
            this.converttexture.UseVisualStyleBackColor = true;
            // 
            // ExportOptions
            // 
            this.AcceptButton = this.fbxOKbutton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.fbxCancel;
            this.ClientSize = new System.Drawing.Size(684, 490);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.showExpOpt);
            this.Controls.Add(this.fbxCancel);
            this.Controls.Add(this.fbxOKbutton);
            this.Controls.Add(this.advancedBox);
            this.Controls.Add(this.includeBox);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExportOptions";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Export options";
            this.TopMost = true;
            this.includeBox.ResumeLayout(false);
            this.includeBox.PerformLayout();
            this.geometryBox.ResumeLayout(false);
            this.geometryBox.PerformLayout();
            this.advancedBox.ResumeLayout(false);
            this.advancedBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scaleFactor)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox includeBox;
        private System.Windows.Forms.GroupBox advancedBox;
        private System.Windows.Forms.NumericUpDown scaleFactor;
        private System.Windows.Forms.Label scaleLabel;
        private System.Windows.Forms.CheckBox embedBox;
        private System.Windows.Forms.CheckBox lightsBox;
        private System.Windows.Forms.CheckBox camerasBox;
        private System.Windows.Forms.CheckBox exportDeformers;
        private System.Windows.Forms.GroupBox geometryBox;
        private System.Windows.Forms.CheckBox exportColors;
        private System.Windows.Forms.CheckBox exportUVs;
        private System.Windows.Forms.CheckBox exportTangents;
        private System.Windows.Forms.CheckBox exportNormals;
        private System.Windows.Forms.Label axisLabel;
        private System.Windows.Forms.ComboBox upAxis;
        private System.Windows.Forms.Button fbxOKbutton;
        private System.Windows.Forms.Button fbxCancel;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.CheckBox showExpOpt;
        private System.Windows.Forms.CheckBox convertDummies;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox converttexture;
        private System.Windows.Forms.RadioButton tojpg;
        private System.Windows.Forms.RadioButton topng;
        private System.Windows.Forms.RadioButton tobmp;
        private System.Windows.Forms.CheckBox convertfsb;
        private System.Windows.Forms.Panel panel1;
    }
}