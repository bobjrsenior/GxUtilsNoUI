using GxModelViewer_WinFormsExt;
using LibGxTexture;
using LibGxFormat;
using LibGxFormat.Gma;
using LibGxFormat.ModelRenderer;
using LibGxFormat.Tpl;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using LibGxFormat.ModelLoader;
using System.Collections.Generic;

namespace GxModelViewer
{
    public partial class ModelViewer
    {
        struct TextureReference
        {
            /// <summary>The index of the texture in the TPL file.</summary>
            public int TextureIdx;
            /// <summary>Selected mipmap level in the texture, or -1 for the whole texture.</summary>
            public int TextureLevel;

            public TextureReference(int textureIdx, int textureLevel)
            {
                this.TextureIdx = textureIdx;
                this.TextureLevel = textureLevel;
            }
        };

        struct ModelMeshReference
        {
            /// <summary>The index of the model, or the model that the triangle mesh belongs to.</summary>
            public int ModelIdx;
            /// <summary>The index of the mesh, or (size_t)-1 if it is a whole model</summary>
            public int MeshIdx;

            public ModelMeshReference(int modelIdx, int meshIdx)
            {
                this.ModelIdx = modelIdx;
                this.MeshIdx = meshIdx;
            }
        };

        struct ModelMaterialReference
        {
            /// <summary>The index of the model that contains the material</summary>
            public int ModelIdx;
            /// <summary>The index of the material within the model</summary>
            public int MaterialIdx;
            
            public  ModelMaterialReference(int modelIdx, int materialIdx)
            {
                this.ModelIdx = modelIdx;
                this.MaterialIdx = materialIdx;
            }
        };

        /// <summary>Path to the currently loaded .GMA file, or null if there isn't any.</summary>
        string gmaPath;
        /// <summary>Instance of the currently loaded .GMA file, or null if there isn't any.</summary>
        Gma gma;

        /// <summary>Path to the currently loaded .TPL file, or null if there isn't any.</summary>
        string tplPath;
        /// <summary>Instance of the currently loaded .TPL file, or null if there isn't any.</summary>
        Tpl tpl;

        /// <summary>Manager for the textures and display lists associated with the .GMA/.TPL files.</summary>
        OpenGlModelContext ctx = new OpenGlModelContext();

        public ModelViewer(string objPath, string gmaPath)
        {
            //LoadGmaFile(null);
            //LoadTplFile(null);
            //Console.WriteLine(EnumUtils.GetEnumDescription(GxGame.SuperMonkeyBall));
            IntitializeComponent();
            LoadGmaFile(null);
            LoadTplFile(null);
            loadObjMTL(objPath);
            this.gmaPath = gmaPath;
            this.tplPath = gmaPath.Substring(0, gmaPath.Length - 3) + "tpl";

            if (SaveGmaFile())
            {
                SaveTplFile();
            }
            

        }

        private void loadObjMTL(string objPath)
        {
            if (objPath == null)
                return;

            List<string> modelWarningLog;
            ObjMtlModel model;
            try
            {
                model = new ObjMtlModel(objPath, out modelWarningLog);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading the OBJ file. " + ex.Message, "Error loading the OBJ file.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Dictionary<Bitmap, int> textureIndexMapping;
            tpl = new Tpl(model, out textureIndexMapping);
            gma = new Gma(model, textureIndexMapping);

            // Update model list
            UpdateModelTree();

            // Update material tab
            UpdateMaterialList();

            // Update texture list
            UpdateTextureTree();
        }

        private GxGame GetSelectedGame()
        {
            return GxGame.SuperMonkeyBall;
        }

        private void LoadGmaFile(string newGmaPath)
        {
            // Try to load the GMA file
            if (newGmaPath != null)
            {
                try
                {
                    using (Stream gmaStream = File.OpenRead(newGmaPath))
                    {
                        gma = new Gma(gmaStream, GetSelectedGame());
                        gmaPath = newGmaPath;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    gma = null;
                    gmaPath = null;
                }
            }
            else
            {
                gma = null;
                gmaPath = null;
            }

            // Update model list
            UpdateModelTree();

            // Update material tab
            UpdateMaterialList();
        }

        private bool SaveGmaFile()
        {
            // If there isn't currently any path set (e.g. we've just imported a model),
            // we have to request one to the user
            if (gmaPath == "")
            {
                return false;
            }

            using (Stream gmaStream = File.OpenWrite(gmaPath))
            {
                gma.Save(gmaStream, GetSelectedGame());
            }

            return true;
        }

        private void UpdateModelTree()
        {
            treeModel.Nodes.Clear();
            if (gma != null)
            {
                for (int i = 0; i < gma.Count; i++)
                {
                    // Add entry corresponding to the whole model
                    TreeNode modelItem = new TreeNode((gma[i] != null) ? gma[i].Name : "Unnamed");
                    modelItem.Tag = new ModelMeshReference(i, -1);
                    modelItem.ForeColor = (gma[i] != null) ? Color.DarkGreen : Color.Red;
                    treeModel.Nodes.Add(modelItem);

                    // Add display list entries for the meshes within the model
                    if (gma[i] != null)
                    {
                        Gcmf model = gma[i].ModelObject;
                        for (int j = 0; j < model.Meshes.Count; j++)
                        {
                            int layerNo = (model.Meshes[j].Layer == GcmfMesh.MeshLayer.Layer1) ? 1 : 2;
                            TreeNode meshItem = new TreeNode(string.Format("[Layer {0}] Mesh {1}", layerNo, j));
                            meshItem.Tag = new ModelMeshReference(i, j);
                            modelItem.Nodes.Add(meshItem);
                        }
                    }

                    treeModel.SetCheckState(modelItem, CheckState.Checked);
                }
            }
        }


        int GetSelectedModelIdx()
        {
            // If no item is selected in the list, return -1
            if (treeModel.SelectedNode == null)
                return -1;

            // Otherwise, extract the model/mesh reference structure and get the model index from there
            ModelMeshReference itemData = (ModelMeshReference)treeModel.SelectedNode.Tag;
            return ((ModelMeshReference)treeModel.SelectedNode.Tag).ModelIdx;
        }

        private void UpdateMaterialList()
        {
            treeMaterials.Nodes.Clear();

            // Make sure that an item is selected in the model list and it corresponds to a non-null model
            int modelIdx = GetSelectedModelIdx();
            if (modelIdx == -1 || gma[modelIdx] == null)
            {
                return;
            }

            // Populate the material list from the model
            Gcmf model = gma[modelIdx].ModelObject;
            for (int i = 0; i < model.Materials.Count; i++)
            {
                TreeNode materialItem = new TreeNode(string.Format("Material {0}", i));
                materialItem.Tag = new ModelMaterialReference(modelIdx, i);
                treeMaterials.Nodes.Add(materialItem);
            }
        }

        private void LoadTplFile(string newTplPath)
        {
            // Try to load the TPL file
            if (newTplPath != null)
            {
                try
                {
                    using (Stream tplStream = File.OpenRead(newTplPath))
                    {
                        tpl = new Tpl(tplStream, GetSelectedGame());
                        tplPath = newTplPath;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    tpl = null;
                    tplPath = null;
                }
            }
            else
            {
                tpl = null;
                tplPath = null;
            }

            // Update texture list
            UpdateTextureTree();
            
        }

        private bool SaveTplFile()
        {
            // If there isn't currently any path set (e.g. we've just imported a model),
            // we have to request one to the user
            if (tplPath == "")
            {
                return false;
            }

            using (Stream tplStream = File.OpenWrite(tplPath))
            {
                tpl.Save(tplStream, GetSelectedGame());
            }

            
            return true;
        }


        private void UpdateTextureTree()
        {
            if (treeTextures.Nodes != null)
            {
                treeTextures.Nodes.Clear();
            }
            if (tpl != null)
            {
                for (int i = 0; i < tpl.Count; i++)
                {
                    TreeNode textureItem = new TreeNode(string.Format("Texture {0}", i));
                    textureItem.ForeColor = (!tpl[i].IsEmpty) ? Color.DarkGreen : Color.Red;
                    textureItem.Tag = new TextureReference(i, -1);
                    treeTextures.Nodes.Add(textureItem);

                    // Add subitems for the texture levels
                    if (!tpl[i].IsEmpty)
                    {
                        for (int j = 0; j < tpl[i].LevelCount; j++)
                        {
                            TreeNode levelItem = new TreeNode(string.Format("Level {0}", j));
                            levelItem.Tag = new TextureReference(i, j);
                            textureItem.Nodes.Add(levelItem);
                        }
                    }
                }
            }
        }

        private void treeModel_AfterCheckState(object sender, TreeViewEventArgs e)
        {
            glControlModel.Invalidate();
        }

        private void treeModel_AfterSelect(object sender, TreeViewEventArgs e)
        {
            
            UpdateMaterialList();
            
        }

        private void treeTextures_AfterSelect(object sender, TreeViewEventArgs e)
        {
            
        }

        private void treeMaterials_AfterSelect(object sender, TreeViewEventArgs e)
        {
            
        }
    }
}
