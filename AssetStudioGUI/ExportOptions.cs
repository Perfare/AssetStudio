using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AssetStudioGUI
{
    public partial class ExportOptions : Form
    {
        public ExportOptions()
        {
            InitializeComponent();
            converttexture.Checked = (bool)Properties.Settings.Default["convertTexture"];
            convertAudio.Checked = (bool)Properties.Settings.Default["convertAudio"];
            var str = (string)Properties.Settings.Default["convertType"];
            foreach (Control c in panel1.Controls)
            {
                if (c.Text == str)
                {
                    ((RadioButton)c).Checked = true;
                    break;
                }
            }
            eulerFilter.Checked = (bool)Properties.Settings.Default["eulerFilter"];
            filterPrecision.Value = (decimal)Properties.Settings.Default["filterPrecision"];
            exportAllNodes.Checked = (bool)Properties.Settings.Default["exportAllNodes"];
            exportSkins.Checked = (bool)Properties.Settings.Default["exportSkins"];
            exportAnimations.Checked = (bool)Properties.Settings.Default["exportAnimations"];
            exportBlendShape.Checked = (bool)Properties.Settings.Default["exportBlendShape"];
            castToBone.Checked = (bool)Properties.Settings.Default["castToBone"];
            boneSize.Value = (decimal)Properties.Settings.Default["boneSize"];
            scaleFactor.Value = (decimal)Properties.Settings.Default["scaleFactor"];
            fbxVersion.SelectedIndex = (int)Properties.Settings.Default["fbxVersion"];
            fbxFormat.SelectedIndex = (int)Properties.Settings.Default["fbxFormat"];
        }

        private void OKbutton_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default["convertTexture"] = converttexture.Checked;
            Properties.Settings.Default["convertAudio"] = convertAudio.Checked;
            foreach (Control c in panel1.Controls)
            {
                if (((RadioButton)c).Checked)
                {
                    Properties.Settings.Default["convertType"] = c.Text;
                    break;
                }
            }
            Properties.Settings.Default["eulerFilter"] = eulerFilter.Checked;
            Properties.Settings.Default["filterPrecision"] = filterPrecision.Value;
            Properties.Settings.Default["exportAllNodes"] = exportAllNodes.Checked;
            Properties.Settings.Default["exportSkins"] = exportSkins.Checked;
            Properties.Settings.Default["exportAnimations"] = exportAnimations.Checked;
            Properties.Settings.Default["exportBlendShape"] = exportBlendShape.Checked;
            Properties.Settings.Default["castToBone"] = castToBone.Checked;
            Properties.Settings.Default["boneSize"] = boneSize.Value;
            Properties.Settings.Default["scaleFactor"] = scaleFactor.Value;
            Properties.Settings.Default["fbxVersion"] = fbxVersion.SelectedIndex;
            Properties.Settings.Default["fbxFormat"] = fbxFormat.SelectedIndex;
            Properties.Settings.Default.Save();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
