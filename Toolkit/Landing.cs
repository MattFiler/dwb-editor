using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DWB_Toolkit
{
    public partial class Landing : Form
    {
        public Landing()
        {
            InitializeComponent();
            Directory.CreateDirectory(FilePaths.PathToUnityStreaming + "TILECONFIGS/");
        }

        /* Show prop manager */
        private void openPropManager_Click(object sender, EventArgs e)
        {
            InstanceManager propManager = new InstanceManager(ManagerType.PROPS);
            propManager.Show();
        }

        /* Show tile manager */
        private void openTileManager_Click(object sender, EventArgs e)
        {
            InstanceManager tileManager = new InstanceManager(ManagerType.TILES);
            tileManager.Show();
        }

        /* Show level manager */
        private void openLevelManager_Click(object sender, EventArgs e)
        {
            LevelManager levelManager = new LevelManager();
            levelManager.Show();
        }

        /* Show scripted object manager */
        private void openScriptObjectManager_Click(object sender, EventArgs e)
        {

        }
    }
}
