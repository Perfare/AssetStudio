namespace AssetStudioGUI
{
    partial class AssetStudioGUIForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AssetStudioGUIForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.extractFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.displayAll = new System.Windows.Forms.ToolStripMenuItem();
            this.displayOriginalName = new System.Windows.Forms.ToolStripMenuItem();
            this.enablePreview = new System.Windows.Forms.ToolStripMenuItem();
            this.displayInfo = new System.Windows.Forms.ToolStripMenuItem();
            this.openAfterExport = new System.Windows.Forms.ToolStripMenuItem();
            this.assetGroupOptions = new System.Windows.Forms.ToolStripComboBox();
            this.showExpOpt = new System.Windows.Forms.ToolStripMenuItem();
            this.modelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAllObjectssplitToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.exportSelectedObjectsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportSelectedObjectsWithAnimationClipToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exportSelectedObjectsmergeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportSelectedObjectsmergeWithAnimationClipToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAllAssetsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportSelectedAssetsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportFilteredAssetsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.exportAnimatorWithSelectedAnimationClipToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem9 = new System.Windows.Forms.ToolStripMenuItem();
            this.filterTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buildClassStructuresMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dontBuildAssetListMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dontBuildHierarchyMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exportClassStructuresMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.sceneTreeView = new AssetStudioGUI.GOHierarchy();
            this.treeSearch = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.assetListView = new System.Windows.Forms.ListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listSearch = new System.Windows.Forms.TextBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.classesListView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.progressbarPanel = new System.Windows.Forms.Panel();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.previewPanel = new System.Windows.Forms.Panel();
            this.assetInfoLabel = new System.Windows.Forms.Label();
            this.FMODpanel = new System.Windows.Forms.Panel();
            this.FMODcopyright = new System.Windows.Forms.Label();
            this.FMODinfoLabel = new System.Windows.Forms.Label();
            this.FMODtimerLabel = new System.Windows.Forms.Label();
            this.FMODstatusLabel = new System.Windows.Forms.Label();
            this.FMODprogressBar = new System.Windows.Forms.TrackBar();
            this.FMODvolumeBar = new System.Windows.Forms.TrackBar();
            this.FMODloopButton = new System.Windows.Forms.CheckBox();
            this.FMODstopButton = new System.Windows.Forms.Button();
            this.FMODpauseButton = new System.Windows.Forms.Button();
            this.FMODplayButton = new System.Windows.Forms.Button();
            this.fontPreviewBox = new System.Windows.Forms.RichTextBox();
            this.textPreviewBox = new System.Windows.Forms.TextBox();
            this.glControl1 = new OpenTK.GLControl();
            this.classPreviewPanel = new System.Windows.Forms.Panel();
            this.classTextBox = new System.Windows.Forms.TextBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exportSelectedAssetsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAnimatorwithselectedAnimationClipMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.jumpToSceneHierarchyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showOriginalFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.progressbarPanel.SuspendLayout();
            this.previewPanel.SuspendLayout();
            this.FMODpanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FMODprogressBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FMODvolumeBar)).BeginInit();
            this.classPreviewPanel.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.modelToolStripMenuItem,
            this.exportToolStripMenuItem,
            this.filterTypeToolStripMenuItem,
            this.debugMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1264, 25);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadFileToolStripMenuItem,
            this.loadFolderToolStripMenuItem,
            this.toolStripMenuItem1,
            this.extractFileToolStripMenuItem,
            this.extractFolderToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(39, 21);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadFileToolStripMenuItem
            // 
            this.loadFileToolStripMenuItem.Name = "loadFileToolStripMenuItem";
            this.loadFileToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.loadFileToolStripMenuItem.Text = "Load file";
            this.loadFileToolStripMenuItem.Click += new System.EventHandler(this.loadFile_Click);
            // 
            // loadFolderToolStripMenuItem
            // 
            this.loadFolderToolStripMenuItem.Name = "loadFolderToolStripMenuItem";
            this.loadFolderToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.loadFolderToolStripMenuItem.Text = "Load folder";
            this.loadFolderToolStripMenuItem.Click += new System.EventHandler(this.loadFolder_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(151, 6);
            // 
            // extractFileToolStripMenuItem
            // 
            this.extractFileToolStripMenuItem.Name = "extractFileToolStripMenuItem";
            this.extractFileToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.extractFileToolStripMenuItem.Text = "Extract file";
            this.extractFileToolStripMenuItem.Click += new System.EventHandler(this.extractFileToolStripMenuItem_Click);
            // 
            // extractFolderToolStripMenuItem
            // 
            this.extractFolderToolStripMenuItem.Name = "extractFolderToolStripMenuItem";
            this.extractFolderToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.extractFolderToolStripMenuItem.Text = "Extract folder";
            this.extractFolderToolStripMenuItem.Click += new System.EventHandler(this.extractFolderToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.displayAll,
            this.displayOriginalName,
            this.enablePreview,
            this.displayInfo,
            this.openAfterExport,
            this.assetGroupOptions,
            this.showExpOpt});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(66, 21);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // displayAll
            // 
            this.displayAll.CheckOnClick = true;
            this.displayAll.Name = "displayAll";
            this.displayAll.Size = new System.Drawing.Size(252, 22);
            this.displayAll.Text = "Display all assets";
            this.displayAll.ToolTipText = "Check this option will display all types assets. Not extractable assets can expor" +
    "t the RAW file.";
            this.displayAll.CheckedChanged += new System.EventHandler(this.MenuItem_CheckedChanged);
            // 
            // displayOriginalName
            // 
            this.displayOriginalName.CheckOnClick = true;
            this.displayOriginalName.Name = "displayOriginalName";
            this.displayOriginalName.Size = new System.Drawing.Size(252, 22);
            this.displayOriginalName.Text = "Display asset original name";
            this.displayOriginalName.ToolTipText = "Check this option will use asset original name when display and export";
            this.displayOriginalName.CheckedChanged += new System.EventHandler(this.MenuItem_CheckedChanged);
            // 
            // enablePreview
            // 
            this.enablePreview.Checked = true;
            this.enablePreview.CheckOnClick = true;
            this.enablePreview.CheckState = System.Windows.Forms.CheckState.Checked;
            this.enablePreview.Name = "enablePreview";
            this.enablePreview.Size = new System.Drawing.Size(252, 22);
            this.enablePreview.Text = "Enable preview";
            this.enablePreview.ToolTipText = "Toggle the loading and preview of readable assets, such as images, sounds, text, " +
    "etc.\r\nDisable preview if you have performance or compatibility issues.";
            this.enablePreview.CheckedChanged += new System.EventHandler(this.enablePreview_Check);
            // 
            // displayInfo
            // 
            this.displayInfo.Checked = true;
            this.displayInfo.CheckOnClick = true;
            this.displayInfo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.displayInfo.Name = "displayInfo";
            this.displayInfo.Size = new System.Drawing.Size(252, 22);
            this.displayInfo.Text = "Display asset infromation";
            this.displayInfo.ToolTipText = "Toggle the overlay that shows information about each asset, eg. image size, forma" +
    "t, audio bitrate, etc.";
            this.displayInfo.CheckedChanged += new System.EventHandler(this.displayAssetInfo_Check);
            // 
            // openAfterExport
            // 
            this.openAfterExport.Checked = true;
            this.openAfterExport.CheckOnClick = true;
            this.openAfterExport.CheckState = System.Windows.Forms.CheckState.Checked;
            this.openAfterExport.Name = "openAfterExport";
            this.openAfterExport.Size = new System.Drawing.Size(252, 22);
            this.openAfterExport.Text = "Open file/folder after export";
            this.openAfterExport.CheckedChanged += new System.EventHandler(this.MenuItem_CheckedChanged);
            // 
            // assetGroupOptions
            // 
            this.assetGroupOptions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.assetGroupOptions.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
            this.assetGroupOptions.Items.AddRange(new object[] {
            "Group by type",
            "Group by source file",
            "Do not group"});
            this.assetGroupOptions.Name = "assetGroupOptions";
            this.assetGroupOptions.Size = new System.Drawing.Size(192, 25);
            this.assetGroupOptions.SelectedIndexChanged += new System.EventHandler(this.assetGroupOptions_SelectedIndexChanged);
            // 
            // showExpOpt
            // 
            this.showExpOpt.Name = "showExpOpt";
            this.showExpOpt.Size = new System.Drawing.Size(252, 22);
            this.showExpOpt.Text = "Export options";
            this.showExpOpt.Click += new System.EventHandler(this.showExpOpt_Click);
            // 
            // modelToolStripMenuItem
            // 
            this.modelToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportAllObjectssplitToolStripMenuItem1,
            this.exportSelectedObjectsToolStripMenuItem,
            this.exportSelectedObjectsWithAnimationClipToolStripMenuItem,
            this.toolStripSeparator1,
            this.exportSelectedObjectsmergeToolStripMenuItem,
            this.exportSelectedObjectsmergeWithAnimationClipToolStripMenuItem});
            this.modelToolStripMenuItem.Name = "modelToolStripMenuItem";
            this.modelToolStripMenuItem.Size = new System.Drawing.Size(58, 21);
            this.modelToolStripMenuItem.Text = "Model";
            // 
            // exportAllObjectssplitToolStripMenuItem1
            // 
            this.exportAllObjectssplitToolStripMenuItem1.Name = "exportAllObjectssplitToolStripMenuItem1";
            this.exportAllObjectssplitToolStripMenuItem1.Size = new System.Drawing.Size(417, 22);
            this.exportAllObjectssplitToolStripMenuItem1.Text = "Export all objects (split)";
            this.exportAllObjectssplitToolStripMenuItem1.Click += new System.EventHandler(this.exportAllObjectssplitToolStripMenuItem1_Click);
            // 
            // exportSelectedObjectsToolStripMenuItem
            // 
            this.exportSelectedObjectsToolStripMenuItem.Name = "exportSelectedObjectsToolStripMenuItem";
            this.exportSelectedObjectsToolStripMenuItem.Size = new System.Drawing.Size(417, 22);
            this.exportSelectedObjectsToolStripMenuItem.Text = "Export selected objects (split)";
            this.exportSelectedObjectsToolStripMenuItem.Click += new System.EventHandler(this.exportSelectedObjectsToolStripMenuItem_Click);
            // 
            // exportSelectedObjectsWithAnimationClipToolStripMenuItem
            // 
            this.exportSelectedObjectsWithAnimationClipToolStripMenuItem.Name = "exportSelectedObjectsWithAnimationClipToolStripMenuItem";
            this.exportSelectedObjectsWithAnimationClipToolStripMenuItem.Size = new System.Drawing.Size(417, 22);
            this.exportSelectedObjectsWithAnimationClipToolStripMenuItem.Text = "Export selected objects (split) + selected AnimationClips";
            this.exportSelectedObjectsWithAnimationClipToolStripMenuItem.Click += new System.EventHandler(this.exportObjectswithAnimationClipMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(414, 6);
            // 
            // exportSelectedObjectsmergeToolStripMenuItem
            // 
            this.exportSelectedObjectsmergeToolStripMenuItem.Name = "exportSelectedObjectsmergeToolStripMenuItem";
            this.exportSelectedObjectsmergeToolStripMenuItem.Size = new System.Drawing.Size(417, 22);
            this.exportSelectedObjectsmergeToolStripMenuItem.Text = "Export selected objects (merge)";
            this.exportSelectedObjectsmergeToolStripMenuItem.Click += new System.EventHandler(this.exportSelectedObjectsmergeToolStripMenuItem_Click);
            // 
            // exportSelectedObjectsmergeWithAnimationClipToolStripMenuItem
            // 
            this.exportSelectedObjectsmergeWithAnimationClipToolStripMenuItem.Name = "exportSelectedObjectsmergeWithAnimationClipToolStripMenuItem";
            this.exportSelectedObjectsmergeWithAnimationClipToolStripMenuItem.Size = new System.Drawing.Size(417, 22);
            this.exportSelectedObjectsmergeWithAnimationClipToolStripMenuItem.Text = "Export selected objects (merge) + selected AnimationClips";
            this.exportSelectedObjectsmergeWithAnimationClipToolStripMenuItem.Click += new System.EventHandler(this.exportSelectedObjectsmergeWithAnimationClipToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportAllAssetsMenuItem,
            this.exportSelectedAssetsMenuItem,
            this.exportFilteredAssetsMenuItem,
            this.toolStripSeparator3,
            this.exportAnimatorWithSelectedAnimationClipToolStripMenuItem,
            this.toolStripSeparator4,
            this.toolStripMenuItem2,
            this.toolStripMenuItem3});
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(58, 21);
            this.exportToolStripMenuItem.Text = "Export";
            // 
            // exportAllAssetsMenuItem
            // 
            this.exportAllAssetsMenuItem.Name = "exportAllAssetsMenuItem";
            this.exportAllAssetsMenuItem.Size = new System.Drawing.Size(284, 22);
            this.exportAllAssetsMenuItem.Text = "All assets";
            this.exportAllAssetsMenuItem.Click += new System.EventHandler(this.exportAllAssetsMenuItem_Click);
            // 
            // exportSelectedAssetsMenuItem
            // 
            this.exportSelectedAssetsMenuItem.Name = "exportSelectedAssetsMenuItem";
            this.exportSelectedAssetsMenuItem.Size = new System.Drawing.Size(284, 22);
            this.exportSelectedAssetsMenuItem.Text = "Selected assets";
            this.exportSelectedAssetsMenuItem.Click += new System.EventHandler(this.exportSelectedAssetsMenuItem_Click);
            // 
            // exportFilteredAssetsMenuItem
            // 
            this.exportFilteredAssetsMenuItem.Name = "exportFilteredAssetsMenuItem";
            this.exportFilteredAssetsMenuItem.Size = new System.Drawing.Size(284, 22);
            this.exportFilteredAssetsMenuItem.Text = "Filtered assets";
            this.exportFilteredAssetsMenuItem.Click += new System.EventHandler(this.exportFilteredAssetsMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(281, 6);
            // 
            // exportAnimatorWithSelectedAnimationClipToolStripMenuItem
            // 
            this.exportAnimatorWithSelectedAnimationClipToolStripMenuItem.Name = "exportAnimatorWithSelectedAnimationClipToolStripMenuItem";
            this.exportAnimatorWithSelectedAnimationClipToolStripMenuItem.Size = new System.Drawing.Size(284, 22);
            this.exportAnimatorWithSelectedAnimationClipToolStripMenuItem.Text = "Animator + selected AnimationClips";
            this.exportAnimatorWithSelectedAnimationClipToolStripMenuItem.Click += new System.EventHandler(this.exportAnimatorwithAnimationClipMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(281, 6);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem4,
            this.toolStripMenuItem5,
            this.toolStripMenuItem6});
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(284, 22);
            this.toolStripMenuItem2.Text = "Raw";
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(165, 22);
            this.toolStripMenuItem4.Text = "All assets";
            this.toolStripMenuItem4.Click += new System.EventHandler(this.toolStripMenuItem4_Click);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(165, 22);
            this.toolStripMenuItem5.Text = "Selected assets";
            this.toolStripMenuItem5.Click += new System.EventHandler(this.toolStripMenuItem5_Click);
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Size = new System.Drawing.Size(165, 22);
            this.toolStripMenuItem6.Text = "Filtered assets";
            this.toolStripMenuItem6.Click += new System.EventHandler(this.toolStripMenuItem6_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem7,
            this.toolStripMenuItem8,
            this.toolStripMenuItem9});
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(284, 22);
            this.toolStripMenuItem3.Text = "Dump";
            // 
            // toolStripMenuItem7
            // 
            this.toolStripMenuItem7.Name = "toolStripMenuItem7";
            this.toolStripMenuItem7.Size = new System.Drawing.Size(165, 22);
            this.toolStripMenuItem7.Text = "All assets";
            this.toolStripMenuItem7.Click += new System.EventHandler(this.toolStripMenuItem7_Click);
            // 
            // toolStripMenuItem8
            // 
            this.toolStripMenuItem8.Name = "toolStripMenuItem8";
            this.toolStripMenuItem8.Size = new System.Drawing.Size(165, 22);
            this.toolStripMenuItem8.Text = "Selected assets";
            this.toolStripMenuItem8.Click += new System.EventHandler(this.toolStripMenuItem8_Click);
            // 
            // toolStripMenuItem9
            // 
            this.toolStripMenuItem9.Name = "toolStripMenuItem9";
            this.toolStripMenuItem9.Size = new System.Drawing.Size(165, 22);
            this.toolStripMenuItem9.Text = "Filtered assets";
            this.toolStripMenuItem9.Click += new System.EventHandler(this.toolStripMenuItem9_Click);
            // 
            // filterTypeToolStripMenuItem
            // 
            this.filterTypeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.allToolStripMenuItem});
            this.filterTypeToolStripMenuItem.Name = "filterTypeToolStripMenuItem";
            this.filterTypeToolStripMenuItem.Size = new System.Drawing.Size(80, 21);
            this.filterTypeToolStripMenuItem.Text = "Filter Type";
            // 
            // allToolStripMenuItem
            // 
            this.allToolStripMenuItem.Checked = true;
            this.allToolStripMenuItem.CheckOnClick = true;
            this.allToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.allToolStripMenuItem.Name = "allToolStripMenuItem";
            this.allToolStripMenuItem.Size = new System.Drawing.Size(90, 22);
            this.allToolStripMenuItem.Text = "All";
            this.allToolStripMenuItem.Click += new System.EventHandler(this.typeToolStripMenuItem_Click);
            // 
            // debugMenuItem
            // 
            this.debugMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buildClassStructuresMenuItem,
            this.dontBuildAssetListMenuItem,
            this.dontBuildHierarchyMenuItem,
            this.toolStripSeparator2,
            this.exportClassStructuresMenuItem});
            this.debugMenuItem.Name = "debugMenuItem";
            this.debugMenuItem.Size = new System.Drawing.Size(59, 21);
            this.debugMenuItem.Text = "Debug";
            // 
            // buildClassStructuresMenuItem
            // 
            this.buildClassStructuresMenuItem.CheckOnClick = true;
            this.buildClassStructuresMenuItem.Name = "buildClassStructuresMenuItem";
            this.buildClassStructuresMenuItem.Size = new System.Drawing.Size(224, 22);
            this.buildClassStructuresMenuItem.Text = "Build class structures";
            // 
            // dontBuildAssetListMenuItem
            // 
            this.dontBuildAssetListMenuItem.CheckOnClick = true;
            this.dontBuildAssetListMenuItem.Name = "dontBuildAssetListMenuItem";
            this.dontBuildAssetListMenuItem.Size = new System.Drawing.Size(224, 22);
            this.dontBuildAssetListMenuItem.Text = "Don\'t build asset list";
            this.dontBuildAssetListMenuItem.CheckedChanged += new System.EventHandler(this.dontBuildAssetListMenuItem_CheckedChanged);
            // 
            // dontBuildHierarchyMenuItem
            // 
            this.dontBuildHierarchyMenuItem.CheckOnClick = true;
            this.dontBuildHierarchyMenuItem.Name = "dontBuildHierarchyMenuItem";
            this.dontBuildHierarchyMenuItem.Size = new System.Drawing.Size(224, 22);
            this.dontBuildHierarchyMenuItem.Text = "Don\'t build hierarchy tree";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(221, 6);
            // 
            // exportClassStructuresMenuItem
            // 
            this.exportClassStructuresMenuItem.Name = "exportClassStructuresMenuItem";
            this.exportClassStructuresMenuItem.Size = new System.Drawing.Size(224, 22);
            this.exportClassStructuresMenuItem.Text = "Export class structures";
            this.exportClassStructuresMenuItem.Click += new System.EventHandler(this.exportClassStructuresMenuItem_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 25);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabControl1);
            this.splitContainer1.Panel1.Controls.Add(this.progressbarPanel);
            this.splitContainer1.Panel1MinSize = 200;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.previewPanel);
            this.splitContainer1.Panel2.Controls.Add(this.classPreviewPanel);
            this.splitContainer1.Panel2.Controls.Add(this.statusStrip1);
            this.splitContainer1.Panel2MinSize = 400;
            this.splitContainer1.Size = new System.Drawing.Size(1264, 656);
            this.splitContainer1.SplitterDistance = 420;
            this.splitContainer1.TabIndex = 2;
            this.splitContainer1.TabStop = false;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.Padding = new System.Drawing.Point(17, 3);
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(418, 634);
            this.tabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControl1.TabIndex = 0;
            this.tabControl1.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabPageSelected);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.sceneTreeView);
            this.tabPage1.Controls.Add(this.treeSearch);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(410, 608);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Scene Hierarchy";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // sceneTreeView
            // 
            this.sceneTreeView.CheckBoxes = true;
            this.sceneTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sceneTreeView.HideSelection = false;
            this.sceneTreeView.Location = new System.Drawing.Point(0, 21);
            this.sceneTreeView.Name = "sceneTreeView";
            this.sceneTreeView.Size = new System.Drawing.Size(410, 587);
            this.sceneTreeView.TabIndex = 1;
            this.sceneTreeView.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.sceneTreeView_AfterCheck);
            // 
            // treeSearch
            // 
            this.treeSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.treeSearch.ForeColor = System.Drawing.SystemColors.GrayText;
            this.treeSearch.Location = new System.Drawing.Point(0, 0);
            this.treeSearch.Name = "treeSearch";
            this.treeSearch.Size = new System.Drawing.Size(410, 21);
            this.treeSearch.TabIndex = 0;
            this.treeSearch.Text = " Search ";
            this.treeSearch.TextChanged += new System.EventHandler(this.treeSearch_TextChanged);
            this.treeSearch.Enter += new System.EventHandler(this.treeSearch_Enter);
            this.treeSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treeSearch_KeyDown);
            this.treeSearch.Leave += new System.EventHandler(this.treeSearch_Leave);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.assetListView);
            this.tabPage2.Controls.Add(this.listSearch);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(410, 608);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Asset List";
            this.tabPage2.UseVisualStyleBackColor = true;
            this.tabPage2.Resize += new System.EventHandler(this.tabPage2_Resize);
            // 
            // assetListView
            // 
            this.assetListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderType,
            this.columnHeaderSize});
            this.assetListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.assetListView.FullRowSelect = true;
            this.assetListView.GridLines = true;
            this.assetListView.HideSelection = false;
            this.assetListView.LabelEdit = true;
            this.assetListView.Location = new System.Drawing.Point(0, 21);
            this.assetListView.Name = "assetListView";
            this.assetListView.Size = new System.Drawing.Size(410, 587);
            this.assetListView.TabIndex = 1;
            this.assetListView.UseCompatibleStateImageBehavior = false;
            this.assetListView.View = System.Windows.Forms.View.Details;
            this.assetListView.VirtualMode = true;
            this.assetListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.assetListView_ColumnClick);
            this.assetListView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.selectAsset);
            this.assetListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.assetListView_RetrieveVirtualItem);
            this.assetListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.assetListView_MouseClick);
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Name";
            this.columnHeaderName.Width = 240;
            // 
            // columnHeaderType
            // 
            this.columnHeaderType.Text = "Type";
            this.columnHeaderType.Width = 88;
            // 
            // columnHeaderSize
            // 
            this.columnHeaderSize.Text = "Size";
            this.columnHeaderSize.Width = 23;
            // 
            // listSearch
            // 
            this.listSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.listSearch.ForeColor = System.Drawing.SystemColors.GrayText;
            this.listSearch.Location = new System.Drawing.Point(0, 0);
            this.listSearch.Name = "listSearch";
            this.listSearch.Size = new System.Drawing.Size(410, 21);
            this.listSearch.TabIndex = 0;
            this.listSearch.Text = " Filter ";
            this.listSearch.TextChanged += new System.EventHandler(this.ListSearchTextChanged);
            this.listSearch.Enter += new System.EventHandler(this.listSearch_Enter);
            this.listSearch.Leave += new System.EventHandler(this.listSearch_Leave);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.classesListView);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(410, 608);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Asset Classes";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // classesListView
            // 
            this.classesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.classesListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.classesListView.FullRowSelect = true;
            this.classesListView.HideSelection = false;
            this.classesListView.Location = new System.Drawing.Point(0, 0);
            this.classesListView.MultiSelect = false;
            this.classesListView.Name = "classesListView";
            this.classesListView.Size = new System.Drawing.Size(410, 608);
            this.classesListView.TabIndex = 0;
            this.classesListView.UseCompatibleStateImageBehavior = false;
            this.classesListView.View = System.Windows.Forms.View.Details;
            this.classesListView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.classesListView_ItemSelectionChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.DisplayIndex = 1;
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 328;
            // 
            // columnHeader2
            // 
            this.columnHeader2.DisplayIndex = 0;
            this.columnHeader2.Text = "ID";
            // 
            // progressbarPanel
            // 
            this.progressbarPanel.Controls.Add(this.progressBar1);
            this.progressbarPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressbarPanel.Location = new System.Drawing.Point(0, 634);
            this.progressbarPanel.Name = "progressbarPanel";
            this.progressbarPanel.Padding = new System.Windows.Forms.Padding(1, 3, 1, 1);
            this.progressbarPanel.Size = new System.Drawing.Size(418, 20);
            this.progressbarPanel.TabIndex = 2;
            // 
            // progressBar1
            // 
            this.progressBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar1.Location = new System.Drawing.Point(1, 2);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(416, 17);
            this.progressBar1.Step = 1;
            this.progressBar1.TabIndex = 1;
            // 
            // previewPanel
            // 
            this.previewPanel.BackColor = System.Drawing.SystemColors.ControlDark;
            this.previewPanel.BackgroundImage = global::AssetStudioGUI.Properties.Resources.preview;
            this.previewPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.previewPanel.Controls.Add(this.assetInfoLabel);
            this.previewPanel.Controls.Add(this.FMODpanel);
            this.previewPanel.Controls.Add(this.fontPreviewBox);
            this.previewPanel.Controls.Add(this.textPreviewBox);
            this.previewPanel.Controls.Add(this.glControl1);
            this.previewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.previewPanel.Location = new System.Drawing.Point(0, 0);
            this.previewPanel.Name = "previewPanel";
            this.previewPanel.Size = new System.Drawing.Size(838, 632);
            this.previewPanel.TabIndex = 1;
            this.previewPanel.Resize += new System.EventHandler(this.preview_Resize);
            // 
            // assetInfoLabel
            // 
            this.assetInfoLabel.AutoSize = true;
            this.assetInfoLabel.BackColor = System.Drawing.Color.Transparent;
            this.assetInfoLabel.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.assetInfoLabel.Location = new System.Drawing.Point(4, 7);
            this.assetInfoLabel.Name = "assetInfoLabel";
            this.assetInfoLabel.Size = new System.Drawing.Size(0, 12);
            this.assetInfoLabel.TabIndex = 0;
            // 
            // FMODpanel
            // 
            this.FMODpanel.BackColor = System.Drawing.SystemColors.ControlDark;
            this.FMODpanel.Controls.Add(this.FMODcopyright);
            this.FMODpanel.Controls.Add(this.FMODinfoLabel);
            this.FMODpanel.Controls.Add(this.FMODtimerLabel);
            this.FMODpanel.Controls.Add(this.FMODstatusLabel);
            this.FMODpanel.Controls.Add(this.FMODprogressBar);
            this.FMODpanel.Controls.Add(this.FMODvolumeBar);
            this.FMODpanel.Controls.Add(this.FMODloopButton);
            this.FMODpanel.Controls.Add(this.FMODstopButton);
            this.FMODpanel.Controls.Add(this.FMODpauseButton);
            this.FMODpanel.Controls.Add(this.FMODplayButton);
            this.FMODpanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FMODpanel.Location = new System.Drawing.Point(0, 0);
            this.FMODpanel.Name = "FMODpanel";
            this.FMODpanel.Size = new System.Drawing.Size(838, 632);
            this.FMODpanel.TabIndex = 2;
            this.FMODpanel.Visible = false;
            // 
            // FMODcopyright
            // 
            this.FMODcopyright.AutoSize = true;
            this.FMODcopyright.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.FMODcopyright.Location = new System.Drawing.Point(250, 352);
            this.FMODcopyright.Name = "FMODcopyright";
            this.FMODcopyright.Size = new System.Drawing.Size(341, 12);
            this.FMODcopyright.TabIndex = 9;
            this.FMODcopyright.Text = "Audio Engine supplied by FMOD by Firelight Technologies.";
            // 
            // FMODinfoLabel
            // 
            this.FMODinfoLabel.AutoSize = true;
            this.FMODinfoLabel.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.FMODinfoLabel.Location = new System.Drawing.Point(305, 250);
            this.FMODinfoLabel.Name = "FMODinfoLabel";
            this.FMODinfoLabel.Size = new System.Drawing.Size(0, 12);
            this.FMODinfoLabel.TabIndex = 8;
            // 
            // FMODtimerLabel
            // 
            this.FMODtimerLabel.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.FMODtimerLabel.Location = new System.Drawing.Point(440, 250);
            this.FMODtimerLabel.Name = "FMODtimerLabel";
            this.FMODtimerLabel.Size = new System.Drawing.Size(155, 12);
            this.FMODtimerLabel.TabIndex = 7;
            this.FMODtimerLabel.Text = "0:00.0 / 0:00.0";
            this.FMODtimerLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // FMODstatusLabel
            // 
            this.FMODstatusLabel.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.FMODstatusLabel.Location = new System.Drawing.Point(249, 250);
            this.FMODstatusLabel.Name = "FMODstatusLabel";
            this.FMODstatusLabel.Size = new System.Drawing.Size(50, 12);
            this.FMODstatusLabel.TabIndex = 6;
            this.FMODstatusLabel.Text = "Stopped";
            // 
            // FMODprogressBar
            // 
            this.FMODprogressBar.AutoSize = false;
            this.FMODprogressBar.Location = new System.Drawing.Point(249, 268);
            this.FMODprogressBar.Maximum = 1000;
            this.FMODprogressBar.Name = "FMODprogressBar";
            this.FMODprogressBar.Size = new System.Drawing.Size(350, 22);
            this.FMODprogressBar.TabIndex = 5;
            this.FMODprogressBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.FMODprogressBar.Scroll += new System.EventHandler(this.FMODprogressBar_Scroll);
            this.FMODprogressBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FMODprogressBar_MouseDown);
            this.FMODprogressBar.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FMODprogressBar_MouseUp);
            // 
            // FMODvolumeBar
            // 
            this.FMODvolumeBar.LargeChange = 2;
            this.FMODvolumeBar.Location = new System.Drawing.Point(496, 295);
            this.FMODvolumeBar.Name = "FMODvolumeBar";
            this.FMODvolumeBar.Size = new System.Drawing.Size(104, 45);
            this.FMODvolumeBar.TabIndex = 4;
            this.FMODvolumeBar.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.FMODvolumeBar.Value = 8;
            this.FMODvolumeBar.ValueChanged += new System.EventHandler(this.FMODvolumeBar_ValueChanged);
            // 
            // FMODloopButton
            // 
            this.FMODloopButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.FMODloopButton.Location = new System.Drawing.Point(435, 295);
            this.FMODloopButton.Name = "FMODloopButton";
            this.FMODloopButton.Size = new System.Drawing.Size(55, 42);
            this.FMODloopButton.TabIndex = 3;
            this.FMODloopButton.Text = "Loop";
            this.FMODloopButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.FMODloopButton.UseVisualStyleBackColor = true;
            this.FMODloopButton.CheckedChanged += new System.EventHandler(this.FMODloopButton_CheckedChanged);
            // 
            // FMODstopButton
            // 
            this.FMODstopButton.Location = new System.Drawing.Point(374, 295);
            this.FMODstopButton.Name = "FMODstopButton";
            this.FMODstopButton.Size = new System.Drawing.Size(55, 42);
            this.FMODstopButton.TabIndex = 2;
            this.FMODstopButton.Text = "Stop";
            this.FMODstopButton.UseVisualStyleBackColor = true;
            this.FMODstopButton.Click += new System.EventHandler(this.FMODstopButton_Click);
            // 
            // FMODpauseButton
            // 
            this.FMODpauseButton.Location = new System.Drawing.Point(313, 295);
            this.FMODpauseButton.Name = "FMODpauseButton";
            this.FMODpauseButton.Size = new System.Drawing.Size(55, 42);
            this.FMODpauseButton.TabIndex = 1;
            this.FMODpauseButton.Text = "Pause";
            this.FMODpauseButton.UseVisualStyleBackColor = true;
            this.FMODpauseButton.Click += new System.EventHandler(this.FMODpauseButton_Click);
            // 
            // FMODplayButton
            // 
            this.FMODplayButton.Location = new System.Drawing.Point(252, 295);
            this.FMODplayButton.Name = "FMODplayButton";
            this.FMODplayButton.Size = new System.Drawing.Size(55, 42);
            this.FMODplayButton.TabIndex = 0;
            this.FMODplayButton.Text = "Play";
            this.FMODplayButton.UseVisualStyleBackColor = true;
            this.FMODplayButton.Click += new System.EventHandler(this.FMODplayButton_Click);
            // 
            // fontPreviewBox
            // 
            this.fontPreviewBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.fontPreviewBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fontPreviewBox.Location = new System.Drawing.Point(0, 0);
            this.fontPreviewBox.Name = "fontPreviewBox";
            this.fontPreviewBox.ReadOnly = true;
            this.fontPreviewBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.fontPreviewBox.Size = new System.Drawing.Size(838, 632);
            this.fontPreviewBox.TabIndex = 0;
            this.fontPreviewBox.Text = resources.GetString("fontPreviewBox.Text");
            this.fontPreviewBox.Visible = false;
            this.fontPreviewBox.WordWrap = false;
            // 
            // textPreviewBox
            // 
            this.textPreviewBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textPreviewBox.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textPreviewBox.Location = new System.Drawing.Point(0, 0);
            this.textPreviewBox.Multiline = true;
            this.textPreviewBox.Name = "textPreviewBox";
            this.textPreviewBox.ReadOnly = true;
            this.textPreviewBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textPreviewBox.Size = new System.Drawing.Size(838, 632);
            this.textPreviewBox.TabIndex = 2;
            this.textPreviewBox.Visible = false;
            this.textPreviewBox.WordWrap = false;
            // 
            // glControl1
            // 
            this.glControl1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.glControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glControl1.Location = new System.Drawing.Point(0, 0);
            this.glControl1.Name = "glControl1";
            this.glControl1.Size = new System.Drawing.Size(838, 632);
            this.glControl1.TabIndex = 4;
            this.glControl1.Visible = false;
            this.glControl1.VSync = false;
            this.glControl1.Load += new System.EventHandler(this.glControl1_Load);
            this.glControl1.Paint += new System.Windows.Forms.PaintEventHandler(this.glControl1_Paint);
            this.glControl1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseDown);
            this.glControl1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseMove);
            this.glControl1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseUp);
            this.glControl1.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseWheel);
            // 
            // classPreviewPanel
            // 
            this.classPreviewPanel.Controls.Add(this.classTextBox);
            this.classPreviewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.classPreviewPanel.Location = new System.Drawing.Point(0, 0);
            this.classPreviewPanel.Name = "classPreviewPanel";
            this.classPreviewPanel.Size = new System.Drawing.Size(838, 632);
            this.classPreviewPanel.TabIndex = 3;
            this.classPreviewPanel.Visible = false;
            // 
            // classTextBox
            // 
            this.classTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.classTextBox.Location = new System.Drawing.Point(0, 0);
            this.classTextBox.Multiline = true;
            this.classTextBox.Name = "classTextBox";
            this.classTextBox.ReadOnly = true;
            this.classTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.classTextBox.Size = new System.Drawing.Size(838, 632);
            this.classTextBox.TabIndex = 3;
            this.classTextBox.WordWrap = false;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 632);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(838, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(823, 17);
            this.toolStripStatusLabel1.Spring = true;
            this.toolStripStatusLabel1.Text = "Ready to go";
            this.toolStripStatusLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // timer
            // 
            this.timer.Interval = 10;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.AddExtension = false;
            this.openFileDialog1.Filter = "All types|*.*";
            this.openFileDialog1.Multiselect = true;
            this.openFileDialog1.RestoreDirectory = true;
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Filter = "FBX file|*.fbx";
            this.saveFileDialog1.RestoreDirectory = true;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportSelectedAssetsToolStripMenuItem,
            this.exportAnimatorwithselectedAnimationClipMenuItem,
            this.jumpToSceneHierarchyToolStripMenuItem,
            this.showOriginalFileToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(327, 92);
            // 
            // exportSelectedAssetsToolStripMenuItem
            // 
            this.exportSelectedAssetsToolStripMenuItem.Name = "exportSelectedAssetsToolStripMenuItem";
            this.exportSelectedAssetsToolStripMenuItem.Size = new System.Drawing.Size(326, 22);
            this.exportSelectedAssetsToolStripMenuItem.Text = "Export selected assets";
            this.exportSelectedAssetsToolStripMenuItem.Click += new System.EventHandler(this.exportSelectedAssetsToolStripMenuItem_Click);
            // 
            // exportAnimatorwithselectedAnimationClipMenuItem
            // 
            this.exportAnimatorwithselectedAnimationClipMenuItem.Name = "exportAnimatorwithselectedAnimationClipMenuItem";
            this.exportAnimatorwithselectedAnimationClipMenuItem.Size = new System.Drawing.Size(326, 22);
            this.exportAnimatorwithselectedAnimationClipMenuItem.Text = "Export Animator + selected AnimationClips";
            this.exportAnimatorwithselectedAnimationClipMenuItem.Visible = false;
            this.exportAnimatorwithselectedAnimationClipMenuItem.Click += new System.EventHandler(this.exportAnimatorwithAnimationClipMenuItem_Click);
            // 
            // jumpToSceneHierarchyToolStripMenuItem
            // 
            this.jumpToSceneHierarchyToolStripMenuItem.Name = "jumpToSceneHierarchyToolStripMenuItem";
            this.jumpToSceneHierarchyToolStripMenuItem.Size = new System.Drawing.Size(326, 22);
            this.jumpToSceneHierarchyToolStripMenuItem.Text = "Jump to scene hierarchy";
            this.jumpToSceneHierarchyToolStripMenuItem.Visible = false;
            this.jumpToSceneHierarchyToolStripMenuItem.Click += new System.EventHandler(this.jumpToSceneHierarchyToolStripMenuItem_Click);
            // 
            // showOriginalFileToolStripMenuItem
            // 
            this.showOriginalFileToolStripMenuItem.Name = "showOriginalFileToolStripMenuItem";
            this.showOriginalFileToolStripMenuItem.Size = new System.Drawing.Size(326, 22);
            this.showOriginalFileToolStripMenuItem.Text = "Show original file";
            this.showOriginalFileToolStripMenuItem.Visible = false;
            this.showOriginalFileToolStripMenuItem.Click += new System.EventHandler(this.showOriginalFileToolStripMenuItem_Click);
            // 
            // AssetStudioGUIForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1264, 681);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = global::AssetStudioGUI.Properties.Resources._as;
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(620, 372);
            this.Name = "AssetStudioGUIForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AssetStudioGUI";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AssetStudioForm_KeyDown);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.progressbarPanel.ResumeLayout(false);
            this.previewPanel.ResumeLayout(false);
            this.previewPanel.PerformLayout();
            this.FMODpanel.ResumeLayout(false);
            this.FMODpanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FMODprogressBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FMODvolumeBar)).EndInit();
            this.classPreviewPanel.ResumeLayout(false);
            this.classPreviewPanel.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TextBox treeSearch;
        private System.Windows.Forms.TextBox listSearch;
        private System.Windows.Forms.ToolStripMenuItem loadFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadFolderToolStripMenuItem;
        private System.Windows.Forms.ListView assetListView;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderSize;
        private System.Windows.Forms.ColumnHeader columnHeaderType;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportAllAssetsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportSelectedAssetsMenuItem;
        private System.Windows.Forms.Panel previewPanel;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.Panel progressbarPanel;
        private System.Windows.Forms.ToolStripMenuItem exportFilteredAssetsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem modelToolStripMenuItem;
        private System.Windows.Forms.Label assetInfoLabel;
        private System.Windows.Forms.TextBox textPreviewBox;
        private System.Windows.Forms.RichTextBox fontPreviewBox;
        private System.Windows.Forms.Panel FMODpanel;
        private System.Windows.Forms.TrackBar FMODvolumeBar;
        private System.Windows.Forms.CheckBox FMODloopButton;
        private System.Windows.Forms.Button FMODstopButton;
        private System.Windows.Forms.Button FMODpauseButton;
        private System.Windows.Forms.Button FMODplayButton;
        private System.Windows.Forms.TrackBar FMODprogressBar;
        private System.Windows.Forms.Label FMODstatusLabel;
        private System.Windows.Forms.Label FMODtimerLabel;
        private System.Windows.Forms.Label FMODinfoLabel;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem displayAll;
        private System.Windows.Forms.ToolStripMenuItem displayOriginalName;
        private System.Windows.Forms.ToolStripMenuItem enablePreview;
        private System.Windows.Forms.ToolStripMenuItem displayInfo;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem extractFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extractFolderToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ToolStripComboBox assetGroupOptions;
        private System.Windows.Forms.ToolStripMenuItem openAfterExport;
        private System.Windows.Forms.ToolStripMenuItem showExpOpt;
        private GOHierarchy sceneTreeView;
        private System.Windows.Forms.ToolStripMenuItem debugMenuItem;
        private System.Windows.Forms.ToolStripMenuItem buildClassStructuresMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dontBuildAssetListMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dontBuildHierarchyMenuItem;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.ListView classesListView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Panel classPreviewPanel;
        private System.Windows.Forms.TextBox classTextBox;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem exportClassStructuresMenuItem;
        private System.Windows.Forms.Label FMODcopyright;
        private OpenTK.GLControl glControl1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem showOriginalFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportAnimatorwithselectedAnimationClipMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportSelectedAssetsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem filterTypeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportSelectedObjectsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportSelectedObjectsWithAnimationClipToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem exportAnimatorWithSelectedAnimationClipToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportAllObjectssplitToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem jumpToSceneHierarchyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportSelectedObjectsmergeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportSelectedObjectsmergeWithAnimationClipToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem6;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem7;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem8;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem9;
    }
}

