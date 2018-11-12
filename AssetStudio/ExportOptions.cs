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
            allFrames.Checked = (bool)Properties.Settings.Default["allFrames"];
            allBones.Checked = (bool)Properties.Settings.Default["allBones"];
            skins.Checked = (bool)Properties.Settings.Default["skins"];
            boneSize.Value = (decimal)Properties.Settings.Default["boneSize"];
            scaleFactor.Value = (decimal)Properties.Settings.Default["scaleFactor"];
            flatInbetween.Checked = (bool)Properties.Settings.Default["flatInbetween"];
            fbxVersion.SelectedIndex = (int)Properties.Settings.Default["fbxVersion"];
            fbxFormat.SelectedIndex = (int)Properties.Settings.Default["fbxFormat"];
        }

        private void exportOpnions_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default[((CheckBox)sender).Name] = ((CheckBox)sender).Checked;
            Properties.Settings.Default.Save();
        }

        private void fbxOKbutton_Click(object sender, EventArgs e)
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
            Properties.Settings.Default["allFrames"] = allFrames.Checked;
            Properties.Settings.Default["allBones"] = allBones.Checked;
            Properties.Settings.Default["skins"] = skins.Checked;
            Properties.Settings.Default["boneSize"] = boneSize.Value;
            Properties.Settings.Default["scaleFactor"] = scaleFactor.Value;
            Properties.Settings.Default["flatInbetween"] = flatInbetween.Checked;
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
