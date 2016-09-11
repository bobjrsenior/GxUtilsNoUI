using System;
using System.Windows.Forms;
using LibGxFormat.ModelLoader;
using System.IO;
using System.Collections.Generic;

namespace GxModelViewer
{
	class MainClass
	{
        [STAThread]
		public static void Main (string[] args)
		{
            //AllocConsole();

            string objFilepath = null;
            string objFileName = null;
            string gmaFilepath = null;

            Console.WriteLine(args.Length);

            if (args.Length == 0)
            {
                OpenFileDialog objDialogue = new OpenFileDialog();
                objDialogue.InitialDirectory = "./";
                objDialogue.Filter = "Object File (*.obj)|*.obj";
                objDialogue.Title = "Select the Object File";
                objDialogue.RestoreDirectory = true;
                objDialogue.AddExtension = true;

                if (objDialogue.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        if (objDialogue.FileName.Length > 0)
                        {
                            objFilepath = objDialogue.FileName;
                            objFileName = objDialogue.SafeFileName;
                            objFilepath = objFilepath.Replace(@"\", "/");
                        }
                        else
                        {
                            MessageBox.Show("Error: Could not find filename");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                        return;
                    }
                }
                else
                {
                    return;
                }

                SaveFileDialog gmaDialogue = new SaveFileDialog();
                gmaDialogue.InitialDirectory = "./";
                gmaDialogue.Filter = "GMA File (*.gma)|*.gma";
                gmaDialogue.Title = "Select Where TO Save The GMA File";
                gmaDialogue.AddExtension = true;
                gmaDialogue.FileName = objFileName.Substring(0, objFileName.Length - 4);


                if (gmaDialogue.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        if (gmaDialogue.FileName.Length > 0)
                        {
                            Console.WriteLine(gmaDialogue.FileName);
                            gmaFilepath = gmaDialogue.FileName;
                        }
                        else
                        {
                            MessageBox.Show("Error: Could not find filename");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            else
            {
                objFilepath = args[0];
                objFilepath = objFilepath.Replace(@"\", "/");
                if (objFilepath.Contains("/"))
                {
                    string[] pathSplit = objFilepath.Split('/');
                    objFileName = pathSplit[pathSplit.Length - 1];
                }

                gmaFilepath = objFilepath.Substring(0, objFilepath.Length - 3) + "gma";
            }


            ModelViewer modelViewer = new ModelViewer(objFilepath, gmaFilepath);

            if (args.Length == 0)
            {
                MessageBox.Show("Done", "Done", MessageBoxButtons.OK, MessageBoxIcon.None,
                                 MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
            else
            {
                Console.WriteLine("Done");
            }

            

        }

        //[System.Runtime.InteropServices.DllImport("kernel32.dll")]
        //private static extern bool AllocConsole();
        

    }
}
