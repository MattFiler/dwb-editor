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
using Microsoft.VisualBasic;

namespace DWB_Toolkit
{
    public partial class LevelManager : Form
    {
        string pathToStreamingLevels = "Unity Project/Assets/StreamingAssets/LEVELS/";

        int prevGUID = 0;
        List<LevelMetadata> levelMetas = new List<LevelMetadata>();
        public LevelManager()
        {
            InitializeComponent();

            BinaryReader bankReader = new BinaryReader(File.OpenRead("Unity Project/Assets/StreamingAssets/LEVELS_MANIFEST.dwb"));
            prevGUID = bankReader.ReadInt32();
            levelMetas = new List<LevelMetadata>(bankReader.ReadInt32());
            for (int i = 0; i < levelMetas.Capacity; i++) levelMetas.Add(new LevelMetadata(bankReader.ReadString(), bankReader.ReadInt32()));
            int linkedLevelCount = bankReader.ReadInt32();
            for (int i = 0; i < linkedLevelCount; i++)
            {
                int levelGUID = bankReader.ReadInt32();
                int nextLevelGUID = bankReader.ReadInt32();
                bool isFirstLevel = bankReader.ReadBoolean();
                bool isLastLevel = bankReader.ReadBoolean();
                for (int x = 0; x < levelMetas.Count; x++)
                {
                    if (levelMetas[x].levelGUID == levelGUID)
                    {
                        levelMetas[x].nextLevelGUID = nextLevelGUID;
                        levelMetas[x].isFirstLevel = isFirstLevel;
                        levelMetas[x].isLastLevel = isLastLevel;
                        break;
                    }
                }
            }
            bankReader.Close();

            foreach (LevelMetadata level in levelMetas)
            {
                levelList.Items.Add(level.levelName + " (GUID " + level.levelGUID + ")");
                nextLevel.Items.Add(level.levelName);
            }
        }

        /* DELETE */
        private void deleteBtn_Click(object sender, EventArgs e)
        {
            int currInd = levelList.SelectedIndex;

            if (currInd == -1) return;
            DialogResult shouldDo = MessageBox.Show("Are you sure you wish to delete this?", "Confirmation...", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (shouldDo != DialogResult.Yes) return;

            string levelPath = pathToStreamingLevels + levelMetas[currInd].levelName;
            File.Delete(levelPath + ".dwb");
            if (File.Exists(levelPath + ".png")) File.Delete(levelPath + ".png");

            levelMetas.RemoveAt(currInd);
            levelList.Items.RemoveAt(currInd);
            nextLevel.Items.RemoveAt(currInd);

            SaveChanges();
            MessageBox.Show("Level deleted!", "Deleted.", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /* MOVE UP */
        private void moveUp_Click(object sender, EventArgs e)
        {
            int currInd = levelList.SelectedIndex;
            int currIndCombo = nextLevel.SelectedIndex;

            if (currInd == -1) return;
            if (currInd == 0) return;

            LevelMetadata editing = levelMetas[currInd];
            levelMetas.RemoveAt(currInd);
            levelMetas.Insert(currInd - 1, editing);
            levelList.Items.RemoveAt(currInd);
            levelList.Items.Insert(currInd - 1, editing.levelName + " (GUID " + editing.levelGUID + ")");
            nextLevel.Items.RemoveAt(currInd);
            nextLevel.Items.Insert(currInd - 1, editing.levelName);

            levelList.SelectedIndex = currInd - 1;
            nextLevel.SelectedIndex = currInd - 1;
            SaveChanges();
        }

        /* MOVE DOWN */
        private void moveDown_Click(object sender, EventArgs e)
        {
            int currInd = levelList.SelectedIndex;
            int currIndCombo = nextLevel.SelectedIndex;

            if (currInd == -1) return;
            if (currInd == levelList.Items.Count - 1) return;

            LevelMetadata editing = levelMetas[currInd];
            levelMetas.RemoveAt(currInd);
            levelMetas.Insert(currInd + 1, editing);
            levelList.Items.RemoveAt(currInd);
            levelList.Items.Insert(currInd + 1, editing.levelName + " (GUID " + editing.levelGUID + ")");
            nextLevel.Items.RemoveAt(currInd);
            nextLevel.Items.Insert(currInd + 1, editing.levelName);

            levelList.SelectedIndex = currInd + 1;
            nextLevel.SelectedIndex = currInd + 1;
            SaveChanges();
        }

        /* SAVE EDIT */
        private void saveBtn_Click(object sender, EventArgs e)
        {
            int currInd = levelList.SelectedIndex;
            int currIndCombo = nextLevel.SelectedIndex;

            if (currInd == -1) return;

            string newName = levelName.Text;
            foreach (LevelMetadata level in levelMetas)
            {
                if (levelMetas[currInd].levelGUID == level.levelGUID) continue;
                if (level.levelName == newName || newName == "")
                {
                    MessageBox.Show("Level name must be unique!", "Not unique.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            if (nextLevelEnabled.Checked)
            {
                if (!isLast.Checked && currInd == currIndCombo)
                {
                    MessageBox.Show("Next level cannot be itself!", "Invalid next level.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                levelMetas[currInd].nextLevelGUID = levelMetas[currIndCombo].levelGUID;
                levelMetas[currInd].isFirstLevel = isFirst.Checked;
                levelMetas[currInd].isLastLevel = isLast.Checked;
            }
            else
            {
                levelMetas[currInd].nextLevelGUID = -1;
            }

            string origLevelPath = pathToStreamingLevels + levelMetas[currInd].levelName;
            File.Move(origLevelPath + ".dwb", pathToStreamingLevels + newName + ".dwb");
            if (File.Exists(origLevelPath + ".png")) File.Move(origLevelPath + ".png", pathToStreamingLevels + newName + ".png");

            //ONLY BELOW V13 (NONE SINCE PW REWORK)
            //BinaryWriter writer = new BinaryWriter(File.OpenWrite(pathToStreamingLevels + newName + ".dwb"));
            //writer.BaseStream.Position = 12;
            //writer.Write(isCampaignMap.Checked);
            //writer.Close();

            levelMetas[currInd].levelName = newName;
            levelList.Items[currInd] = newName + " (GUID " + levelMetas[currInd].levelGUID + ")";
            nextLevel.Items[currInd] = newName;

            levelList.SelectedIndex = currInd;
            nextLevel.SelectedIndex = currIndCombo;

            SaveChanges();
            MessageBox.Show("Changes saved!", "Saved.", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /* NEW SELECTION */
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            levelName.Text = "";
            nextLevel.SelectedIndex = 0;
            nextLevel.Enabled = false;
            isFirst.Enabled = false;
            nextLevelEnabled.Checked = false;

            int currInd = levelList.SelectedIndex;

            if (currInd == -1) return;

            levelName.Text = levelMetas[currInd].levelName;
            nextLevelEnabled.Checked = levelMetas[currInd].nextLevelGUID != -1;
            if (nextLevelEnabled.Checked)
            {
                nextLevel.Enabled = true;
                isFirst.Enabled = true;
                isFirst.Checked = levelMetas[currInd].isFirstLevel;
                isLast.Enabled = true;
                isLast.Checked = levelMetas[currInd].isLastLevel;
                for (int i = 0; i < nextLevel.Items.Count; i++)
                {
                    if (levelMetas[i].levelGUID == levelMetas[currInd].nextLevelGUID)
                    {
                        nextLevel.SelectedIndex = i;
                        break;
                    }
                }
                nextLevel.Enabled = !isLast.Checked;
            }

            //ONLY BELOW V13 (NONE SINCE PW REWORK)
            //BinaryReader reader = new BinaryReader(File.OpenRead(pathToStreamingLevels + levelMetas[currInd].levelName + ".dwb"));
            //reader.BaseStream.Position = 12;
            //isCampaignMap.Checked = reader.ReadBoolean();
            //reader.Close();
            isCampaignMap.Checked = levelMetas[currInd].nextLevelGUID != -1;
        }

        /* SAVE TO DISK */
        private void SaveChanges()
        {
            BinaryWriter bankWriter = new BinaryWriter(File.OpenWrite("Unity Project/Assets/StreamingAssets/LEVELS_MANIFEST.dwb"));
            bankWriter.BaseStream.SetLength(0);
            bankWriter.Write(prevGUID);
            bankWriter.Write(levelMetas.Count);
            List<LinkedLevel> linkedLevels = new List<LinkedLevel>();
            for (int i = 0; i < levelMetas.Count; i++)
            {
                bankWriter.Write(levelMetas[i].levelName);
                bankWriter.Write(levelMetas[i].levelGUID);
                if (levelMetas[i].nextLevelGUID != -1) linkedLevels.Add(new LinkedLevel(levelMetas[i].levelGUID, levelMetas[i].nextLevelGUID, levelMetas[i].isFirstLevel, levelMetas[i].isLastLevel));
            }
            bankWriter.Write(linkedLevels.Count);
            for (int i = 0; i < linkedLevels.Count; i++)
            {
                bankWriter.Write(linkedLevels[i].GUID_1);
                bankWriter.Write(linkedLevels[i].GUID_2);
                bankWriter.Write(linkedLevels[i].IS_FIRST);
                bankWriter.Write(linkedLevels[i].IS_LAST);
            }
            bankWriter.Close();
        }

        /* VALIDATE FIRST/LAST CHECK */
        private void isFirst_CheckedChanged_1(object sender, EventArgs e)
        {
            if (isFirst.Checked)
            {
                foreach (LevelMetadata meta in levelMetas)
                {
                    if (levelList.SelectedIndex != -1 && meta.levelGUID == levelMetas[levelList.SelectedIndex].levelGUID) continue;
                    if (meta.isFirstLevel)
                    {
                        MessageBox.Show("Level \"" + meta.levelName + "\" is already set as first campaign level!", "First level already set.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        isFirst.Checked = false;
                        return;
                    }
                }
            }
        }
        private void isLast_CheckedChanged(object sender, EventArgs e)
        {
            if (isLast.Checked)
            {
                foreach (LevelMetadata meta in levelMetas)
                {
                    if (levelList.SelectedIndex != -1 && meta.levelGUID == levelMetas[levelList.SelectedIndex].levelGUID) continue;
                    if (meta.isLastLevel)
                    {
                        MessageBox.Show("Level \"" + meta.levelName + "\" is already set as last campaign level!", "Last level already set.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        isLast.Checked = false;
                        return;
                    }
                }
                nextLevel.Enabled = false;
                return;
            }
            nextLevel.Enabled = true;
        }

        /* OFFICIAL MAPS AND CAMPAIGN MAPS NOW GO HAND IN HAND */
        private void isCampaignMap_CheckedChanged(object sender, EventArgs e)
        {
            nextLevelEnabled.Checked = isCampaignMap.Checked;
            if (levelList.SelectedIndex != -1) isFirst.Checked = levelMetas[levelList.SelectedIndex].isFirstLevel;
            if (levelList.SelectedIndex != -1) isLast.Checked = levelMetas[levelList.SelectedIndex].isLastLevel;
            nextLevel.Enabled = nextLevelEnabled.Checked;
            isFirst.Enabled = nextLevelEnabled.Checked;
            isLast.Enabled = nextLevelEnabled.Checked;
        }
        private void nextLevelEnabled_CheckedChanged(object sender, EventArgs e)
        {
            isCampaignMap.Checked = nextLevelEnabled.Checked;
            if (levelList.SelectedIndex != -1) isFirst.Checked = levelMetas[levelList.SelectedIndex].isFirstLevel;
            if (levelList.SelectedIndex != -1) isLast.Checked = levelMetas[levelList.SelectedIndex].isLastLevel;
            nextLevel.Enabled = nextLevelEnabled.Checked;
            isFirst.Enabled = nextLevelEnabled.Checked;
            isLast.Enabled = nextLevelEnabled.Checked;
        }
    }
}
