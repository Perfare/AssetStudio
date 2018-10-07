namespace AssetStudio
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
            this.FbxBox = new System.Windows.Forms.GroupBox();
            this.scaleFactor = new System.Windows.Forms.NumericUpDown();
            this.convertDummies = new System.Windows.Forms.CheckBox();
            this.scaleLabel = new System.Windows.Forms.Label();
            this.exportDeformers = new System.Windows.Forms.CheckBox();
            this.geometryBox = new System.Windows.Forms.GroupBox();
            this.exportColors = new System.Windows.Forms.CheckBox();
            this.exportUVs = new System.Windows.Forms.CheckBox();
            this.exportTangents = new System.Windows.Forms.CheckBox();
            this.exportNormals = new System.Windows.Forms.CheckBox();
            this.fbxOKbutton = new System.Windows.Forms.Button();
            this.fbxCancel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.convertAudio = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tojpg = new System.Windows.Forms.RadioButton();
            this.topng = new System.Windows.Forms.RadioButton();
            this.tobmp = new System.Windows.Forms.RadioButton();
            this.converttexture = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.fbxVersion = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.flatInbetween = new System.Windows.Forms.CheckBox();
            this.boneSize = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.skins = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.filterPrecision = new System.Windows.Forms.NumericUpDown();
            this.allBones = new System.Windows.Forms.CheckBox();
            this.allFrames = new System.Windows.Forms.CheckBox();
            this.EulerFilter = new System.Windows.Forms.CheckBox();
            this.FbxBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scaleFactor)).BeginInit();
            this.geometryBox.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.boneSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.filterPrecision)).BeginInit();
            this.SuspendLayout();
            // 
            // FbxBox
            // 
            this.FbxBox.AutoSize = true;
            this.FbxBox.Controls.Add(this.scaleFactor);
            this.FbxBox.Controls.Add(this.convertDummies);
            this.FbxBox.Controls.Add(this.scaleLabel);
            this.FbxBox.Controls.Add(this.exportDeformers);
            this.FbxBox.Controls.Add(this.geometryBox);
            this.FbxBox.Location = new System.Drawing.Point(12, 12);
            this.FbxBox.Name = "FbxBox";
            this.FbxBox.Size = new System.Drawing.Size(247, 235);
            this.FbxBox.TabIndex = 0;
            this.FbxBox.TabStop = false;
            this.FbxBox.Text = "Fbx Ascii";
            // 
            // scaleFactor
            // 
            this.scaleFactor.DecimalPlaces = 2;
            this.scaleFactor.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.scaleFactor.Location = new System.Drawing.Point(95, 194);
            this.scaleFactor.Name = "scaleFactor";
            this.scaleFactor.Size = new System.Drawing.Size(46, 21);
            this.scaleFactor.TabIndex = 1;
            this.scaleFactor.Value = new decimal(new int[] {
            254,
            0,
            0,
            131072});
            // 
            // convertDummies
            // 
            this.convertDummies.AutoSize = true;
            this.convertDummies.Location = new System.Drawing.Point(6, 170);
            this.convertDummies.Name = "convertDummies";
            this.convertDummies.Size = new System.Drawing.Size(228, 16);
            this.convertDummies.TabIndex = 5;
            this.convertDummies.Text = "Convert Deforming Dummies to Bones";
            this.convertDummies.UseVisualStyleBackColor = true;
            this.convertDummies.CheckedChanged += new System.EventHandler(this.exportOpnions_CheckedChanged);
            // 
            // scaleLabel
            // 
            this.scaleLabel.AutoSize = true;
            this.scaleLabel.Location = new System.Drawing.Point(6, 196);
            this.scaleLabel.Name = "scaleLabel";
            this.scaleLabel.Size = new System.Drawing.Size(83, 12);
            this.scaleLabel.TabIndex = 0;
            this.scaleLabel.Text = "Scale Factor:";
            // 
            // exportDeformers
            // 
            this.exportDeformers.AutoSize = true;
            this.exportDeformers.Location = new System.Drawing.Point(6, 148);
            this.exportDeformers.Name = "exportDeformers";
            this.exportDeformers.Size = new System.Drawing.Size(108, 16);
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
            this.geometryBox.Location = new System.Drawing.Point(6, 20);
            this.geometryBox.Name = "geometryBox";
            this.geometryBox.Size = new System.Drawing.Size(235, 122);
            this.geometryBox.TabIndex = 0;
            this.geometryBox.TabStop = false;
            this.geometryBox.Text = "Geometry";
            // 
            // exportColors
            // 
            this.exportColors.AutoSize = true;
            this.exportColors.Checked = true;
            this.exportColors.CheckState = System.Windows.Forms.CheckState.Checked;
            this.exportColors.Location = new System.Drawing.Point(6, 86);
            this.exportColors.Name = "exportColors";
            this.exportColors.Size = new System.Drawing.Size(102, 16);
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
            this.exportUVs.Location = new System.Drawing.Point(6, 64);
            this.exportUVs.Name = "exportUVs";
            this.exportUVs.Size = new System.Drawing.Size(108, 16);
            this.exportUVs.TabIndex = 2;
            this.exportUVs.Text = "UV Coordinates";
            this.exportUVs.UseVisualStyleBackColor = true;
            this.exportUVs.CheckedChanged += new System.EventHandler(this.exportOpnions_CheckedChanged);
            // 
            // exportTangents
            // 
            this.exportTangents.AutoSize = true;
            this.exportTangents.Location = new System.Drawing.Point(6, 42);
            this.exportTangents.Name = "exportTangents";
            this.exportTangents.Size = new System.Drawing.Size(72, 16);
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
            this.exportNormals.Location = new System.Drawing.Point(6, 20);
            this.exportNormals.Name = "exportNormals";
            this.exportNormals.Size = new System.Drawing.Size(66, 16);
            this.exportNormals.TabIndex = 0;
            this.exportNormals.Text = "Normals";
            this.exportNormals.UseVisualStyleBackColor = true;
            this.exportNormals.CheckedChanged += new System.EventHandler(this.exportOpnions_CheckedChanged);
            // 
            // fbxOKbutton
            // 
            this.fbxOKbutton.Location = new System.Drawing.Point(323, 346);
            this.fbxOKbutton.Name = "fbxOKbutton";
            this.fbxOKbutton.Size = new System.Drawing.Size(75, 21);
            this.fbxOKbutton.TabIndex = 6;
            this.fbxOKbutton.Text = "OK";
            this.fbxOKbutton.UseVisualStyleBackColor = true;
            this.fbxOKbutton.Click += new System.EventHandler(this.fbxOKbutton_Click);
            // 
            // fbxCancel
            // 
            this.fbxCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.fbxCancel.Location = new System.Drawing.Point(404, 346);
            this.fbxCancel.Name = "fbxCancel";
            this.fbxCancel.Size = new System.Drawing.Size(75, 21);
            this.fbxCancel.TabIndex = 7;
            this.fbxCancel.Text = "Cancel";
            this.fbxCancel.UseVisualStyleBackColor = true;
            this.fbxCancel.Click += new System.EventHandler(this.fbxCancel_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSize = true;
            this.groupBox1.Controls.Add(this.convertAudio);
            this.groupBox1.Controls.Add(this.panel1);
            this.groupBox1.Controls.Add(this.converttexture);
            this.groupBox1.Location = new System.Drawing.Point(12, 253);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(247, 114);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Convert";
            // 
            // convertAudio
            // 
            this.convertAudio.AutoSize = true;
            this.convertAudio.Checked = true;
            this.convertAudio.CheckState = System.Windows.Forms.CheckState.Checked;
            this.convertAudio.Location = new System.Drawing.Point(6, 78);
            this.convertAudio.Name = "convertAudio";
            this.convertAudio.Size = new System.Drawing.Size(198, 16);
            this.convertAudio.TabIndex = 6;
            this.convertAudio.Text = "Convert AudioClip to WAV(PCM)";
            this.convertAudio.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tojpg);
            this.panel1.Controls.Add(this.topng);
            this.panel1.Controls.Add(this.tobmp);
            this.panel1.Location = new System.Drawing.Point(30, 42);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(146, 30);
            this.panel1.TabIndex = 5;
            // 
            // tojpg
            // 
            this.tojpg.AutoSize = true;
            this.tojpg.Location = new System.Drawing.Point(97, 6);
            this.tojpg.Name = "tojpg";
            this.tojpg.Size = new System.Drawing.Size(47, 16);
            this.tojpg.TabIndex = 4;
            this.tojpg.Text = "JPEG";
            this.tojpg.UseVisualStyleBackColor = true;
            // 
            // topng
            // 
            this.topng.AutoSize = true;
            this.topng.Checked = true;
            this.topng.Location = new System.Drawing.Point(50, 6);
            this.topng.Name = "topng";
            this.topng.Size = new System.Drawing.Size(41, 16);
            this.topng.TabIndex = 3;
            this.topng.TabStop = true;
            this.topng.Text = "PNG";
            this.topng.UseVisualStyleBackColor = true;
            // 
            // tobmp
            // 
            this.tobmp.AutoSize = true;
            this.tobmp.Location = new System.Drawing.Point(3, 6);
            this.tobmp.Name = "tobmp";
            this.tobmp.Size = new System.Drawing.Size(41, 16);
            this.tobmp.TabIndex = 2;
            this.tobmp.Text = "BMP";
            this.tobmp.UseVisualStyleBackColor = true;
            // 
            // converttexture
            // 
            this.converttexture.AutoSize = true;
            this.converttexture.Checked = true;
            this.converttexture.CheckState = System.Windows.Forms.CheckState.Checked;
            this.converttexture.Location = new System.Drawing.Point(6, 20);
            this.converttexture.Name = "converttexture";
            this.converttexture.Size = new System.Drawing.Size(126, 16);
            this.converttexture.TabIndex = 1;
            this.converttexture.Text = "Convert Texture2D";
            this.converttexture.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.AutoSize = true;
            this.groupBox2.Controls.Add(this.fbxVersion);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.flatInbetween);
            this.groupBox2.Controls.Add(this.boneSize);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.skins);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.filterPrecision);
            this.groupBox2.Controls.Add(this.allBones);
            this.groupBox2.Controls.Add(this.allFrames);
            this.groupBox2.Controls.Add(this.EulerFilter);
            this.groupBox2.Location = new System.Drawing.Point(265, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(214, 235);
            this.groupBox2.TabIndex = 11;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Fbx Binary";
            // 
            // fbxVersion
            // 
            this.fbxVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.fbxVersion.FormattingEnabled = true;
            this.fbxVersion.Items.AddRange(new object[] {
            "6.1",
            "7.1",
            "7.2",
            "7.3",
            "7.4",
            "7.5"});
            this.fbxVersion.Location = new System.Drawing.Point(77, 178);
            this.fbxVersion.Name = "fbxVersion";
            this.fbxVersion.Size = new System.Drawing.Size(47, 20);
            this.fbxVersion.TabIndex = 16;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 181);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 15;
            this.label3.Text = "FBXVersion";
            // 
            // flatInbetween
            // 
            this.flatInbetween.AutoSize = true;
            this.flatInbetween.Location = new System.Drawing.Point(6, 155);
            this.flatInbetween.Name = "flatInbetween";
            this.flatInbetween.Size = new System.Drawing.Size(102, 16);
            this.flatInbetween.TabIndex = 12;
            this.flatInbetween.Text = "FlatInbetween";
            this.flatInbetween.UseVisualStyleBackColor = true;
            // 
            // boneSize
            // 
            this.boneSize.Location = new System.Drawing.Point(65, 128);
            this.boneSize.Name = "boneSize";
            this.boneSize.Size = new System.Drawing.Size(46, 21);
            this.boneSize.TabIndex = 11;
            this.boneSize.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 130);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 10;
            this.label2.Text = "BoneSize";
            // 
            // skins
            // 
            this.skins.AutoSize = true;
            this.skins.Checked = true;
            this.skins.CheckState = System.Windows.Forms.CheckState.Checked;
            this.skins.Location = new System.Drawing.Point(6, 105);
            this.skins.Name = "skins";
            this.skins.Size = new System.Drawing.Size(54, 16);
            this.skins.TabIndex = 8;
            this.skins.Text = "Skins";
            this.skins.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 12);
            this.label1.TabIndex = 7;
            this.label1.Text = "FilterPrecision";
            // 
            // filterPrecision
            // 
            this.filterPrecision.DecimalPlaces = 2;
            this.filterPrecision.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.filterPrecision.Location = new System.Drawing.Point(127, 37);
            this.filterPrecision.Name = "filterPrecision";
            this.filterPrecision.Size = new System.Drawing.Size(51, 21);
            this.filterPrecision.TabIndex = 6;
            this.filterPrecision.Value = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            // 
            // allBones
            // 
            this.allBones.AutoSize = true;
            this.allBones.Checked = true;
            this.allBones.CheckState = System.Windows.Forms.CheckState.Checked;
            this.allBones.Location = new System.Drawing.Point(6, 83);
            this.allBones.Name = "allBones";
            this.allBones.Size = new System.Drawing.Size(72, 16);
            this.allBones.TabIndex = 5;
            this.allBones.Text = "AllBones";
            this.allBones.UseVisualStyleBackColor = true;
            // 
            // allFrames
            // 
            this.allFrames.AutoSize = true;
            this.allFrames.Location = new System.Drawing.Point(6, 61);
            this.allFrames.Name = "allFrames";
            this.allFrames.Size = new System.Drawing.Size(78, 16);
            this.allFrames.TabIndex = 4;
            this.allFrames.Text = "AllFrames";
            this.allFrames.UseVisualStyleBackColor = true;
            // 
            // EulerFilter
            // 
            this.EulerFilter.AutoSize = true;
            this.EulerFilter.Checked = true;
            this.EulerFilter.CheckState = System.Windows.Forms.CheckState.Checked;
            this.EulerFilter.Location = new System.Drawing.Point(6, 20);
            this.EulerFilter.Name = "EulerFilter";
            this.EulerFilter.Size = new System.Drawing.Size(90, 16);
            this.EulerFilter.TabIndex = 3;
            this.EulerFilter.Text = "EulerFilter";
            this.EulerFilter.UseVisualStyleBackColor = true;
            // 
            // ExportOptions
            // 
            this.AcceptButton = this.fbxOKbutton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.fbxCancel;
            this.ClientSize = new System.Drawing.Size(493, 382);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.fbxCancel);
            this.Controls.Add(this.fbxOKbutton);
            this.Controls.Add(this.FbxBox);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExportOptions";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Export options";
            this.TopMost = true;
            this.FbxBox.ResumeLayout(false);
            this.FbxBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scaleFactor)).EndInit();
            this.geometryBox.ResumeLayout(false);
            this.geometryBox.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.boneSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.filterPrecision)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox FbxBox;
        private System.Windows.Forms.NumericUpDown scaleFactor;
        private System.Windows.Forms.Label scaleLabel;
        private System.Windows.Forms.CheckBox exportDeformers;
        private System.Windows.Forms.GroupBox geometryBox;
        private System.Windows.Forms.CheckBox exportColors;
        private System.Windows.Forms.CheckBox exportUVs;
        private System.Windows.Forms.CheckBox exportTangents;
        private System.Windows.Forms.CheckBox exportNormals;
        private System.Windows.Forms.Button fbxOKbutton;
        private System.Windows.Forms.Button fbxCancel;
        private System.Windows.Forms.CheckBox convertDummies;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox converttexture;
        private System.Windows.Forms.RadioButton tojpg;
        private System.Windows.Forms.RadioButton topng;
        private System.Windows.Forms.RadioButton tobmp;
        private System.Windows.Forms.CheckBox convertAudio;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox flatInbetween;
        private System.Windows.Forms.NumericUpDown boneSize;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox skins;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown filterPrecision;
        private System.Windows.Forms.CheckBox allBones;
        private System.Windows.Forms.CheckBox allFrames;
        private System.Windows.Forms.CheckBox EulerFilter;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox fbxVersion;
    }
}