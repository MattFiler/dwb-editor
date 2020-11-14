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
    public partial class InstanceManager : Form
    {
        ManagerType loadedType;
        string loadedTypeString = "";
        string folderPath = "";
        public InstanceManager(ManagerType _type)
        {
            loadedType = _type;
            switch (loadedType)
            {
                case ManagerType.PROPS:
                    loadedTypeString = "prop";
                    folderPath = "PROPCONFIGS";
                    break;
                case ManagerType.TILES:
                    loadedTypeString = "tile";
                    folderPath = "TILECONFIGS";
                    break;
            }
            InitializeComponent();
            ReloadList();

            string managerNameCapsFirst = loadedTypeString[0].ToString().ToUpper() + loadedTypeString.Substring(1);
            this.Name = managerNameCapsFirst + " Manager";
            newInstance.Text = "New " + managerNameCapsFirst;
            editInstance.Text = "Edit Selected " + managerNameCapsFirst;
            deleteInstance.Text = "Delete Selected " + managerNameCapsFirst;
        }

        /* Reload GUI List */
        private void ReloadList(object sender, EventArgs e)
        {
            ReloadList();
            this.Focus();
        }
        private void ReloadList()
        {
            instanceList.Items.Clear();
            switch (loadedType)
            {
                case ManagerType.PROPS:
                    PropFileInterface.LoadData();
                    foreach (PropData prop in PropFileInterface.GetData()) instanceList.Items.Add(prop.propName);
                    break;
                case ManagerType.TILES:
                    TileFileInterface.LoadData();
                    foreach (TileData tile in TileFileInterface.GetData()) instanceList.Items.Add(tile.tileName);
                    break;
            }
        }

        /* Create new */
        private void newInstance_Click(object sender, EventArgs e)
        {
            Form newInstanceForm = new Form();
            switch (loadedType)
            {
                case ManagerType.PROPS:
                    newInstanceForm = new PropEditor(-1);
                    break;
                case ManagerType.TILES:
                    newInstanceForm = new TileEditor(-1);
                    break;
            }
            newInstanceForm.FormClosed += new FormClosedEventHandler(ReloadList);
            newInstanceForm.Show();
        }

        /* Edit selected */
        private void editInstance_Click(object sender, EventArgs e)
        {
            if (instanceList.SelectedIndex == -1) return;
            Form editInstanceForm = new Form();
            switch (loadedType)
            {
                case ManagerType.PROPS:
                    editInstanceForm = new PropEditor(instanceList.SelectedIndex);
                    break;
                case ManagerType.TILES:
                    editInstanceForm = new TileEditor(instanceList.SelectedIndex);
                    break;
            }
            editInstanceForm.FormClosed += new FormClosedEventHandler(ReloadList);
            editInstanceForm.Show();
        }

        /* Delete selected */
        private void deleteInstance_Click(object sender, EventArgs e)
        {
            if (instanceList.SelectedIndex == -1) return;
            DialogResult shouldDo = MessageBox.Show("Are you sure you wish to delete this " + loadedTypeString + "?\nDeleting a " + loadedTypeString + " can have SERIOUS implications.\nEdit this " + loadedTypeString + " and tag as deprecated instead!", "Confirmation...", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (shouldDo != DialogResult.Yes) return;
            if (Directory.Exists(FilePaths.PathToUnityPropResources + instanceList.Items[instanceList.SelectedIndex] + "/")) Directory.Delete(FilePaths.PathToUnityPropResources + instanceList.Items[instanceList.SelectedIndex] + "/", true);
            if (Directory.Exists(FilePaths.PathToUnityStreaming + folderPath + "/" + instanceList.Items[instanceList.SelectedIndex] + "/")) Directory.Delete(FilePaths.PathToUnityStreaming + folderPath + "/" + instanceList.Items[instanceList.SelectedIndex] + "/", true);
            switch (loadedType)
            {
                case ManagerType.PROPS:
                    PropFileInterface.RemoveDataAt(instanceList.SelectedIndex);
                    PropFileInterface.SaveData();
                    break;
                case ManagerType.TILES:
                    TileFileInterface.RemoveDataAt(instanceList.SelectedIndex);
                    TileFileInterface.SaveData();
                    break;
            }
            ReloadList();
            MessageBox.Show(loadedTypeString[0].ToString().ToUpper() + loadedTypeString.Substring(1) + " deleted!", "Complete.", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /* Show preview */
        private void instanceList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (instanceList.SelectedIndex == -1) return;

            DataStruct thisInstanceSelected = new DataStruct();
            switch (loadedType)
            {
                case ManagerType.PROPS:
                    thisInstanceSelected = PropFileInterface.GetData()[instanceList.SelectedIndex];
                    break;
                case ManagerType.TILES:
                    thisInstanceSelected = TileFileInterface.GetData()[instanceList.SelectedIndex];
                    break;
            }
            isHidden.Checked = thisInstanceSelected.hideInEditor;
        }
    }

    public enum ManagerType {
        PROPS,
        TILES,
    }
}
