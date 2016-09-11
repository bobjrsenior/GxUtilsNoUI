using GxModelViewer_WinFormsExt;

namespace GxModelViewer
{
    partial class ModelViewer
    {

        private System.Windows.Forms.TreeView treeMaterials;
        private GxModelViewer_WinFormsExt.TreeViewAutoPartialCheckBox treeModel;
        private System.Windows.Forms.TreeView treeTextures;
        private OpenTK.GLControl glControlModel;

        #region Windows Form Designer generated code

        private void IntitializeComponent()
        {
            this.treeMaterials = new System.Windows.Forms.TreeView();

            // 
            // treeMaterials
            // 
            this.treeMaterials.Name = "treeMaterials";
            this.treeMaterials.TabIndex = 0;
            this.treeMaterials.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeMaterials_AfterSelect);

            this.treeModel = new GxModelViewer_WinFormsExt.TreeViewAutoPartialCheckBox();

            // 
            // treeModel
            // 
            this.treeModel.Name = "treeModel";
            this.treeModel.TabIndex = 0;
            this.treeModel.AfterCheckState += new System.Windows.Forms.TreeViewEventHandler(this.treeModel_AfterCheckState);
            this.treeModel.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeModel_AfterSelect);

            this.treeTextures = new System.Windows.Forms.TreeView();

            // 
            // treeTextures
            // 
            this.treeTextures.Name = "treeTextures";
            this.treeTextures.TabIndex = 0;
            this.treeTextures.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeTextures_AfterSelect);

            this.glControlModel = new OpenTK.GLControl();

            // 
            // glControlModel
            // 
            this.glControlModel.Name = "glControlModel";
            this.glControlModel.VSync = false;
        }

        #endregion
    }
}