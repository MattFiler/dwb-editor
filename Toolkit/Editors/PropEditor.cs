using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DWB_Toolkit
{
    public partial class PropEditor : Form
    {
        private PropData thisProp = new PropData();
        private int propIndex = -1;

        private PropSprites propSprites = new PropSprites();
        private List<SpriteBounds> spriteBounds = new List<SpriteBounds>();

        /* On editor launch */
        public PropEditor(int _propIndex)
        {
            InitializeComponent();

            for (int i = 0; i < PropParams.waypointTypes.Length; i++) waypointFor.Items.Add(PropParams.waypointTypes[i]);
            for (int i = 0; i < PropParams.poiTypes.Length; i++) poiType.Items.Add(PropParams.poiTypes[i]);

            //If in editor, setup pre-existing data
            if (_propIndex == -1) return;
            thisProp = PropFileInterface.GetData()[_propIndex];
            propIndex = _propIndex;

            propName.Text = thisProp.propName;
            propDesc.Text = thisProp.propDesc;
            isWaypoint.Checked = thisProp.isWaypoint;
            WaypointCheckChange();
            isStartPoint.Checked = (thisProp.waypointType == 0);
            isMidPoint.Checked = (thisProp.waypointType == 1);
            isEndPoint.Checked = (thisProp.waypointType == 2);
            waypointFor.SelectedIndex = thisProp.waypointFor;
            isEvent.Checked = thisProp.isEventSpawn;
            eventScriptName.Text = thisProp.eventType;
            isPOI.Checked = thisProp.isPOI;
            poiType.SelectedIndex = thisProp.poiType;
            poiGoonCount.Value = thisProp.poiGoonCount;
            useageInterior.Checked = thisProp.isInside;
            useageExterior.Checked = !thisProp.isInside;
            makeTileUnpathable.Checked = thisProp.makesTileUnpathable;
            hideInEditor.Checked = thisProp.hideInEditor;
            zBias.Value = thisProp.zBias;

            string formattedPropName = propName.Text.Trim().ToUpper().Replace(' ', '_');
            propSprites.Front = Environment.CurrentDirectory + "/" + FilePaths.PathToUnityPropResources + formattedPropName + "/FRONT_FACING.png";
            LoadSavedSprite(tilePreviewFrontFacing, propSprites.Front);
            frontSpriteInUse.Checked = !(tilePreviewFrontFacing.Image == null);
            propSprites.Left = Environment.CurrentDirectory + "/" + FilePaths.PathToUnityPropResources + formattedPropName + "/LEFT_FACING.png";
            LoadSavedSprite(tilePreviewLeftFacing, propSprites.Left);
            leftSpriteInUse.Checked = !(tilePreviewLeftFacing.Image == null);
            propSprites.Right = Environment.CurrentDirectory + "/" + FilePaths.PathToUnityPropResources + formattedPropName + "/RIGHT_FACING.png";
            LoadSavedSprite(tilePreviewRightFacing, propSprites.Right);
            rightSpriteInUse.Checked = !(tilePreviewRightFacing.Image == null);
            propSprites.Back = Environment.CurrentDirectory + "/" + FilePaths.PathToUnityPropResources + formattedPropName + "/BACK_FACING.png";
            LoadSavedSprite(tilePreviewBackFacing, propSprites.Back);
            backSpriteInUse.Checked = !(tilePreviewBackFacing.Image == null);
            propSprites.EditorUI = Environment.CurrentDirectory + "/" + FilePaths.PathToUnityPropResources + formattedPropName + "/EDITOR_UI.png";
            LoadSavedSprite(tilePreviewIconUI, propSprites.EditorUI);

            if (frontSpriteInUse.Checked) NESW_CalculateFromSprite(tilePreviewFrontFacing, frontNorthCoverage, frontEastCoverage, frontSouthCoverage, frontWestCoverage, true, "FRONT_FACING");
            if (leftSpriteInUse.Checked) NESW_CalculateFromSprite(tilePreviewLeftFacing, leftNorthCoverage, leftEastCoverage, leftSouthCoverage, leftWestCoverage, true, "LEFT_FACING");
            if (backSpriteInUse.Checked) NESW_CalculateFromSprite(tilePreviewBackFacing, backNorthCoverage, backEastCoverage, backSouthCoverage, backWestCoverage, true, "BACK_FACING");
            if (rightSpriteInUse.Checked) NESW_CalculateFromSprite(tilePreviewRightFacing, rightNorthCoverage, rightEastCoverage, rightSouthCoverage, rightWestCoverage, true, "RIGHT_FACING");

            propName.ReadOnly = true;
        }

        /* Load a sprite from saved data */
        private void LoadSavedSprite(PictureBox picture, string path)
        {
            if (path == "" || !File.Exists(path)) return;
            using (var tempPreviewImg = new Bitmap(path))
            {
                picture.Image = new Bitmap(tempPreviewImg);
                //spriteBounds.Add(SpriteAnalysis.CalculateSpriteBounds((Bitmap)picture.Image));
            }
        }

        /* Check validity of image */
        private bool ImageIsValid(PictureBox image, bool inUse)
        {
            if (!inUse) return true;
            if (image.Image == null) return false;
            return ((image.Image.Width % 200) == 0 && (image.Image.Height % 200) == 0 && image.Image.Width == image.Image.Height);
        }

        /* Save a sprite from entered data */
        private void SaveSprite(PictureBox picture, string name, bool includeDims = false, /* ALL FOLLOWING NEED TO BE SUPPLIED IF includeDims = true */ decimal _eNorth = 0, decimal _eEast = 0, decimal _eSouth = 0, decimal _eWest = 0)
        {
            string filePath = FilePaths.PathToUnityPropResources + name + ".png";
            if (changedSprites) picture.Image.Save(filePath);
            string metaPath = filePath + ".meta";
            if (!File.Exists(metaPath)) File.WriteAllText(metaPath, Properties.Resources.meta_template.ToString());
            if (includeDims)
            {
                spriteBounds.Add(SpriteAnalysis.CalculateSpriteBounds((Bitmap)picture.Image));
                spriteBounds[spriteBounds.Count - 1].tile_excess_north = _eNorth;
                spriteBounds[spriteBounds.Count - 1].tile_excess_east = _eEast;
                spriteBounds[spriteBounds.Count - 1].tile_excess_south = _eSouth;
                spriteBounds[spriteBounds.Count - 1].tile_excess_west = _eWest;
                SpriteAnalysis.SaveData(FilePaths.PathToUnityStreaming + "PROPCONFIGS/" + propName.Text.Trim().ToUpper().Replace(' ', '_') + "/" + Path.GetFileNameWithoutExtension(name) + ".dwb", spriteBounds[spriteBounds.Count - 1]);
            }
        }

        /* Save the prop */
        private void saveProp_Click(object sender, EventArgs e)
        {
            //Validation
            if (propName.Text == "")
            {
                MessageBox.Show("Prop must have a name!", "No prop name.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (propDesc.Text == "")
            {
                MessageBox.Show("Prop must have a description!", "No prop description.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (isWaypoint.Checked && (!(isStartPoint.Checked || isMidPoint.Checked || isEndPoint.Checked) || waypointFor.SelectedIndex == 0))
            {
                MessageBox.Show("Waypoint data must all be selected if waypoint!", "Missing waypoint data.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (isEvent.Checked && eventScriptName.Text == "")
            {
                MessageBox.Show("Enter a script class name, or uncheck scripted object checkbox!", "Invalid scripted prop setup.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!ImageIsValid(tilePreviewFrontFacing, frontSpriteInUse.Checked) || 
                !ImageIsValid(tilePreviewLeftFacing, leftSpriteInUse.Checked) || 
                !ImageIsValid(tilePreviewRightFacing, rightSpriteInUse.Checked) || 
                !ImageIsValid(tilePreviewBackFacing, backSpriteInUse.Checked))
            {
                MessageBox.Show("Prop sprites MUST be square and a division of 200 pixels.\nPlease follow the guidlines for irregular size props!", "Invalid prop size.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult shouldDo = MessageBox.Show("Would you like to see an example?", "Example prop sizing offer", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (shouldDo == DialogResult.Yes && File.Exists("TileExample.png")) Process.Start("TileExample.png");
                return;
            }
            if (tilePreviewIconUI.Image == null || (tilePreviewIconUI.Image.Width != tilePreviewIconUI.Image.Height))
            {
                MessageBox.Show("UI sprite not assigned, or is not square!", "Invalid UI sprite.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!frontSpriteInUse.Checked && !leftSpriteInUse.Checked && !rightSpriteInUse.Checked && !backSpriteInUse.Checked)
            {
                MessageBox.Show("Prop requires at least one sprite!", "No in-use sprites.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if ((frontSpriteInUse.Checked && tilePreviewFrontFacing.Image == null) || 
                (leftSpriteInUse.Checked && tilePreviewLeftFacing.Image == null) || 
                (rightSpriteInUse.Checked && tilePreviewRightFacing.Image == null) || 
                (backSpriteInUse.Checked && tilePreviewBackFacing.Image == null))
            {
                MessageBox.Show("An in-use sprite has been unassigned!", "Unassigned sprite.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //Create and check prop name
            string formattedPropName = propName.Text.Trim().ToUpper().Replace(' ', '_');
            if (propIndex == -1)
            {
                for (int i = 0; i < PropFileInterface.GetData().Count; i++)
                {
                    if (PropFileInterface.GetData()[i].propName == formattedPropName)
                    {
                        MessageBox.Show("Prop name must be unique!", "Prop already exists.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }
            Directory.CreateDirectory(FilePaths.PathToUnityPropResources + formattedPropName);

            //Save sprites
            spriteBounds.Clear();
            if (frontSpriteInUse.Checked) SaveSprite(tilePreviewFrontFacing, formattedPropName + "/FRONT_FACING", true, frontNorthCoverage.Value, frontEastCoverage.Value, frontSouthCoverage.Value, frontWestCoverage.Value);
            if (leftSpriteInUse.Checked) SaveSprite(tilePreviewLeftFacing, formattedPropName + "/LEFT_FACING", true, leftNorthCoverage.Value, leftEastCoverage.Value, leftSouthCoverage.Value, leftWestCoverage.Value);
            if (rightSpriteInUse.Checked) SaveSprite(tilePreviewRightFacing, formattedPropName + "/RIGHT_FACING", true, rightNorthCoverage.Value, rightEastCoverage.Value, rightSouthCoverage.Value, rightWestCoverage.Value);
            if (backSpriteInUse.Checked) SaveSprite(tilePreviewBackFacing, formattedPropName + "/BACK_FACING", true, backNorthCoverage.Value, backEastCoverage.Value, backSouthCoverage.Value, backWestCoverage.Value);
            SaveSprite(tilePreviewIconUI, formattedPropName + "/EDITOR_UI");

            //Work out complete bounds from all rotations
            SpriteBounds allBounds = new SpriteBounds();
            for (int i = 0; i < spriteBounds.Count; i++)
            {
                if (allBounds.from_origin_north < spriteBounds[i].from_origin_north) allBounds.from_origin_north = spriteBounds[i].from_origin_north;
                if (allBounds.from_origin_east < spriteBounds[i].from_origin_east) allBounds.from_origin_east = spriteBounds[i].from_origin_east;
                if (allBounds.from_origin_south < spriteBounds[i].from_origin_south) allBounds.from_origin_south = spriteBounds[i].from_origin_south;
                if (allBounds.from_origin_west < spriteBounds[i].from_origin_west) allBounds.from_origin_west = spriteBounds[i].from_origin_west;
            }
            SpriteAnalysis.SaveData(FilePaths.PathToUnityStreaming + "PROPCONFIGS/" + formattedPropName + "/" + formattedPropName + ".dwb", allBounds);

            //Save data
            thisProp.propName = formattedPropName;
            thisProp.propDesc = propDesc.Text;
            thisProp.isWaypoint = isWaypoint.Checked;
            if (thisProp.isWaypoint)
            {
                thisProp.waypointType = (Int16)((isStartPoint.Checked) ? 0 : (isMidPoint.Checked) ? 1 : (isEndPoint.Checked) ? 2 : -1);
                thisProp.waypointFor = (Int16)waypointFor.SelectedIndex;
            }
            thisProp.isEventSpawn = isEvent.Checked;
            if (thisProp.isEventSpawn)
            {
                thisProp.eventType = eventScriptName.Text;
            }
            thisProp.isPOI = isPOI.Checked;
            if (thisProp.isPOI)
            {
                thisProp.poiType = (Int16)poiType.SelectedIndex;
                thisProp.poiGoonCount = (int)poiGoonCount.Value;
            }
            thisProp.isInside = useageInterior.Checked;
            thisProp.makesTileUnpathable = makeTileUnpathable.Checked;
            thisProp.hideInEditor = hideInEditor.Checked;
            thisProp.zBias = (int)zBias.Value;
            if (propIndex == -1) PropFileInterface.GetData().Add(thisProp);
            PropFileInterface.SaveData();

            MessageBox.Show("Saved!", "Complete.", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }

        /* Select a new sprite */
        bool changedSprites = false;
        private string SelectSprite(PictureBox preview)
        {
            changedSprites = true;

            OpenFileDialog newSprite = new OpenFileDialog();
            newSprite.Multiselect = false;
            newSprite.Filter = "Images (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";
            if (newSprite.ShowDialog() == DialogResult.OK)
            {
                using (var tempPreviewImg = new Bitmap(newSprite.FileName))
                {
                    preview.Image = new Bitmap(tempPreviewImg);
                }
            }
            return newSprite.FileName;
        }

        /* UI sprite selection */
        private void browseFrontSprite_Click(object sender, EventArgs e)
        {
            propSprites.Front = SelectSprite(tilePreviewFrontFacing);
            NESW_CalculateFromSprite(tilePreviewFrontFacing, frontNorthCoverage, frontEastCoverage, frontSouthCoverage, frontWestCoverage);
        }
        private void browseLeftSprite_Click(object sender, EventArgs e)
        {
            propSprites.Left = SelectSprite(tilePreviewLeftFacing);
            NESW_CalculateFromSprite(tilePreviewLeftFacing, leftNorthCoverage, leftEastCoverage, leftSouthCoverage, leftWestCoverage);
        }
        private void browseBackSprite_Click(object sender, EventArgs e)
        {
            propSprites.Back = SelectSprite(tilePreviewBackFacing);
            NESW_CalculateFromSprite(tilePreviewBackFacing, backNorthCoverage, backEastCoverage, backSouthCoverage, backWestCoverage);
        }
        private void browseRightSprite_Click(object sender, EventArgs e)
        {
            propSprites.Right = SelectSprite(tilePreviewRightFacing);
            NESW_CalculateFromSprite(tilePreviewRightFacing, rightNorthCoverage, rightEastCoverage, rightSouthCoverage, rightWestCoverage);
        }
        private void browseEditorIcon_Click(object sender, EventArgs e)
        {
            propSprites.EditorUI = SelectSprite(tilePreviewIconUI);
        }
        private void NESW_CalculateFromSprite(PictureBox picture, NumericUpDown north, NumericUpDown east, NumericUpDown south, NumericUpDown west, bool loadExisting = false, string rotationName = "" /* MUST BE SUPPLIED IF loadExisting = true */)
        {
            SpriteBounds bounds = (loadExisting) ? SpriteAnalysis.LoadData(FilePaths.PathToUnityStreaming + "PROPCONFIGS/" + propName.Text.Trim().ToUpper().Replace(' ', '_') + "/" + Path.GetFileNameWithoutExtension(rotationName) + ".dwb") :
                                                   SpriteAnalysis.CalculateSpriteBounds((Bitmap)picture.Image);
            north.Value = bounds.tile_excess_north;
            east.Value = bounds.tile_excess_east;
            south.Value = bounds.tile_excess_south;
            west.Value = bounds.tile_excess_west;
        }

        /* UI Updates */
        private void isWaypoint_CheckedChanged(object sender, EventArgs e)
        {
            WaypointCheckChange();
        }
        private void WaypointCheckChange()
        {
            isStartPoint.Checked = false;
            isStartPoint.Enabled = isWaypoint.Checked;
            isMidPoint.Checked = false;
            isMidPoint.Enabled = isWaypoint.Checked;
            isEndPoint.Checked = false;
            isEndPoint.Enabled = isWaypoint.Checked;
            waypointFor.SelectedIndex = 0;
            waypointFor.Enabled = isWaypoint.Checked;
        }
        private void isEvent_CheckedChanged(object sender, EventArgs e)
        {
            eventScriptName.Enabled = isEvent.Checked;
            eventScriptName.Text = "";
        }
        private void isPOI_CheckedChanged(object sender, EventArgs e)
        {
            poiGoonCount.Enabled = isPOI.Checked;
            poiGoonCount.Value = 0;
            poiType.Enabled = isPOI.Checked;
            poiType.SelectedIndex = 0;
        }

        private void frontSpriteInUse_CheckedChanged(object sender, EventArgs e)
        {
            browseFrontSprite.Enabled = frontSpriteInUse.Checked;
            if (!frontSpriteInUse.Checked) tilePreviewFrontFacing.Image = null;
        }
        private void backSpriteInUse_CheckedChanged(object sender, EventArgs e)
        {
            browseBackSprite.Enabled = backSpriteInUse.Checked;
            if (!backSpriteInUse.Checked) tilePreviewBackFacing.Image = null;
        }
        private void leftSpriteInUse_CheckedChanged(object sender, EventArgs e)
        {
            browseLeftSprite.Enabled = leftSpriteInUse.Checked;
            if (!leftSpriteInUse.Checked) tilePreviewLeftFacing.Image = null;
        }
        private void rightSpriteInUse_CheckedChanged(object sender, EventArgs e)
        {
            browseRightSprite.Enabled = rightSpriteInUse.Checked;
            if (!rightSpriteInUse.Checked) tilePreviewRightFacing.Image = null;
        }
    }
}
