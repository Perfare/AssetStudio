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
            this.includeBox.SuspendLayout();
            this.geometryBox.SuspendLayout();
            this.advancedBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scaleFactor)).BeginInit();
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
            this.includeBox.Location = new System.Drawing.Point(12, 12);
            this.includeBox.Name = "includeBox";
            this.includeBox.Size = new System.Drawing.Size(360, 285);
            this.includeBox.TabIndex = 0;
            this.includeBox.TabStop = false;
            this.includeBox.Text = "Include";
            // 
            // convertDummies
            // 
            this.convertDummies.AutoSize = true;
            this.convertDummies.Location = new System.Drawing.Point(14, 178);
            this.convertDummies.Name = "convertDummies";
            this.convertDummies.Size = new System.Drawing.Size(205, 17);
            this.convertDummies.TabIndex = 5;
            this.convertDummies.Text = "Convert Deforming Dummies to Bones";
            this.convertDummies.UseVisualStyleBackColor = true;
            this.convertDummies.CheckedChanged += new System.EventHandler(this.exportOpnions_CheckedChanged);
            // 
            // embedBox
            // 
            this.embedBox.AutoSize = true;
            this.embedBox.Enabled = false;
            this.embedBox.Location = new System.Drawing.Point(14, 249);
            this.embedBox.Name = "embedBox";
            this.embedBox.Size = new System.Drawing.Size(91, 17);
            this.embedBox.TabIndex = 4;
            this.embedBox.Text = "Embed Media";
            this.embedBox.UseVisualStyleBackColor = true;
            // 
            // lightsBox
            // 
            this.lightsBox.AutoSize = true;
            this.lightsBox.Enabled = false;
            this.lightsBox.Location = new System.Drawing.Point(14, 225);
            this.lightsBox.Name = "lightsBox";
            this.lightsBox.Size = new System.Drawing.Size(54, 17);
            this.lightsBox.TabIndex = 3;
            this.lightsBox.Text = "Lights";
            this.lightsBox.UseVisualStyleBackColor = true;
            // 
            // camerasBox
            // 
            this.camerasBox.AutoSize = true;
            this.camerasBox.Enabled = false;
            this.camerasBox.Location = new System.Drawing.Point(14, 201);
            this.camerasBox.Name = "camerasBox";
            this.camerasBox.Size = new System.Drawing.Size(67, 17);
            this.camerasBox.TabIndex = 2;
            this.camerasBox.Text = "Cameras";
            this.camerasBox.UseVisualStyleBackColor = true;
            // 
            // exportDeformers
            // 
            this.exportDeformers.AutoSize = true;
            this.exportDeformers.Location = new System.Drawing.Point(14, 154);
            this.exportDeformers.Name = "exportDeformers";
            this.exportDeformers.Size = new System.Drawing.Size(98, 17);
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
            this.geometryBox.Location = new System.Drawing.Point(7, 20);
            this.geometryBox.Name = "geometryBox";
            this.geometryBox.Size = new System.Drawing.Size(346, 128);
            this.geometryBox.TabIndex = 0;
            this.geometryBox.TabStop = false;
            this.geometryBox.Text = "Geometry";
            // 
            // exportColors
            // 
            this.exportColors.AutoSize = true;
            this.exportColors.Checked = true;
            this.exportColors.CheckState = System.Windows.Forms.CheckState.Checked;
            this.exportColors.Location = new System.Drawing.Point(7, 92);
            this.exportColors.Name = "exportColors";
            this.exportColors.Size = new System.Drawing.Size(88, 17);
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
            this.exportUVs.Location = new System.Drawing.Point(7, 68);
            this.exportUVs.Name = "exportUVs";
            this.exportUVs.Size = new System.Drawing.Size(100, 17);
            this.exportUVs.TabIndex = 2;
            this.exportUVs.Text = "UV Coordinates";
            this.exportUVs.UseVisualStyleBackColor = true;
            this.exportUVs.CheckedChanged += new System.EventHandler(this.exportOpnions_CheckedChanged);
            // 
            // exportTangents
            // 
            this.exportTangents.AutoSize = true;
            this.exportTangents.Location = new System.Drawing.Point(7, 44);
            this.exportTangents.Name = "exportTangents";
            this.exportTangents.Size = new System.Drawing.Size(71, 17);
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
            this.exportNormals.Location = new System.Drawing.Point(7, 20);
            this.exportNormals.Name = "exportNormals";
            this.exportNormals.Size = new System.Drawing.Size(64, 17);
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
            this.advancedBox.Location = new System.Drawing.Point(12, 303);
            this.advancedBox.Name = "advancedBox";
            this.advancedBox.Size = new System.Drawing.Size(360, 80);
            this.advancedBox.TabIndex = 5;
            this.advancedBox.TabStop = false;
            this.advancedBox.Text = "Advanced Options";
            // 
            // axisLabel
            // 
            this.axisLabel.AutoSize = true;
            this.axisLabel.Location = new System.Drawing.Point(6, 43);
            this.axisLabel.Name = "axisLabel";
            this.axisLabel.Size = new System.Drawing.Size(46, 13);
            this.axisLabel.TabIndex = 3;
            this.axisLabel.Text = "Up Axis:";
            // 
            // upAxis
            // 
            this.upAxis.FormattingEnabled = true;
            this.upAxis.Items.AddRange(new object[] {
            "Y-up"});
            this.upAxis.Location = new System.Drawing.Point(58, 40);
            this.upAxis.MaxDropDownItems = 2;
            this.upAxis.Name = "upAxis";
            this.upAxis.Size = new System.Drawing.Size(70, 21);
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
            this.scaleFactor.Location = new System.Drawing.Point(82, 14);
            this.scaleFactor.Name = "scaleFactor";
            this.scaleFactor.Size = new System.Drawing.Size(46, 20);
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
            this.scaleLabel.Location = new System.Drawing.Point(6, 16);
            this.scaleLabel.Name = "scaleLabel";
            this.scaleLabel.Size = new System.Drawing.Size(70, 13);
            this.scaleLabel.TabIndex = 0;
            this.scaleLabel.Text = "Scale Factor:";
            // 
            // fbxOKbutton
            // 
            this.fbxOKbutton.Location = new System.Drawing.Point(216, 389);
            this.fbxOKbutton.Name = "fbxOKbutton";
            this.fbxOKbutton.Size = new System.Drawing.Size(75, 23);
            this.fbxOKbutton.TabIndex = 6;
            this.fbxOKbutton.Text = "OK";
            this.fbxOKbutton.UseVisualStyleBackColor = true;
            this.fbxOKbutton.Click += new System.EventHandler(this.fbxOKbutton_Click);
            // 
            // fbxCancel
            // 
            this.fbxCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.fbxCancel.Location = new System.Drawing.Point(297, 389);
            this.fbxCancel.Name = "fbxCancel";
            this.fbxCancel.Size = new System.Drawing.Size(75, 23);
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
            this.showExpOpt.Location = new System.Drawing.Point(12, 393);
            this.showExpOpt.Name = "showExpOpt";
            this.showExpOpt.Size = new System.Drawing.Size(179, 17);
            this.showExpOpt.TabIndex = 8;
            this.showExpOpt.Text = "Show this dialog for every export";
            this.showExpOpt.UseVisualStyleBackColor = true;
            // 
            // ExportOptions
            // 
            this.AcceptButton = this.fbxOKbutton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.fbxCancel;
            this.ClientSize = new System.Drawing.Size(384, 421);
            this.Controls.Add(this.showExpOpt);
            this.Controls.Add(this.fbxCancel);
            this.Controls.Add(this.fbxOKbutton);
            this.Controls.Add(this.advancedBox);
            this.Controls.Add(this.includeBox);
            this.HelpButton = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExportOptions";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Export options";
            this.TopMost = true;
            this.includeBox.ResumeLayout(false);
            this.includeBox.PerformLayout();
            this.geometryBox.ResumeLayout(false);
            this.geometryBox.PerformLayout();
            this.advancedBox.ResumeLayout(false);
            this.advancedBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scaleFactor)).EndInit();
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
    }
}