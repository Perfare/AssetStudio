using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AssetStudio
{
    public partial class ExportOptions : Form
    {
        public ExportOptions()
        {
            InitializeComponent();
            exportNormals.Checked = (bool)Properties.Settings.Default["exportNormals"];
            exportTangents.Checked = (bool)Properties.Settings.Default["exportTangents"];
            exportUVs.Checked = (bool)Properties.Settings.Default["exportUVs"];
            exportColors.Checked = (bool)Properties.Settings.Default["exportColors"];
            exportDeformers.Checked = (bool)Properties.Settings.Default["exportDeformers"];
            convertDummies.Checked = (bool)Properties.Settings.Default["convertDummies"];
            convertDummies.Enabled = (bool)Properties.Settings.Default["exportDeformers"];
            scaleFactor.Value = (decimal)Properties.Settings.Default["scaleFactor"];
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
            EulerFilter.Checked = (bool)Properties.Settings.Default["EulerFilter"];
            filterPrecision.Value = (decimal)Properties.Settings.Default["filterPrecision"];
            allFrames.Checked = (bool)Properties.Settings.Default["allFrames"];
            allBones.Checked = (bool)Properties.Settings.Default["allBones"];
            skins.Checked = (bool)Properties.Settings.Default["skins"];
            boneSize.Value = (decimal)Properties.Settings.Default["boneSize"];
            flatInbetween.Checked = (bool)Properties.Settings.Default["flatInbetween"];
            fbxVersion.SelectedIndex = (int)Properties.Settings.Default["fbxVersion"];
        }

        private void exportOpnions_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default[((CheckBox)sender).Name] = ((CheckBox)sender).Checked;
            Properties.Settings.Default.Save();
        }

        private void fbxOKbutton_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default["exportNormals"] = exportNormals.Checked;
            Properties.Settings.Default["exportTangents"] = exportTangents.Checked;
            Properties.Settings.Default["exportUVs"] = exportUVs.Checked;
            Properties.Settings.Default["exportColors"] = exportColors.Checked;
            Properties.Settings.Default["exportDeformers"] = exportDeformers.Checked;
            Properties.Settings.Default["scaleFactor"] = scaleFactor.Value;
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
            Properties.Settings.Default["EulerFilter"] = EulerFilter.Checked;
            Properties.Settings.Default["filterPrecision"] = filterPrecision.Value;
            Properties.Settings.Default["allFrames"] = allFrames.Checked;
            Properties.Settings.Default["allBones"] = allBones.Checked;
            Properties.Settings.Default["skins"] = skins.Checked;
            Properties.Settings.Default["boneSize"] = boneSize.Value;
            Properties.Settings.Default["flatInbetween"] = flatInbetween.Checked;
            Properties.Settings.Default["fbxVersion"] = fbxVersion.SelectedIndex;
            Properties.Settings.Default.Save();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void fbxCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void exportDeformers_CheckedChanged(object sender, EventArgs e)
        {
            exportOpnions_CheckedChanged(sender, e);
            convertDummies.Enabled = exportDeformers.Checked;
        }
    }
}
