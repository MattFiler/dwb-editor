using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DWB_Toolkit
{
    public partial class TileEditor : Form
    {
        private TileData thisTile = new TileData();
        private int tileIndex = -1;

        WallTileSprites wallSprites = new WallTileSprites();
        FloorTileSprites floorSprites = new FloorTileSprites();

        /* On editor launch */
        public TileEditor(int _tileIndex)
        {
            InitializeComponent();

            typeWall.Checked = true;
            useageInterior.Checked = true;

            //If in editor, setup pre-existing data
            if (_tileIndex == -1) return;
            thisTile = TileFileInterface.GetData()[_tileIndex];
            tileIndex = _tileIndex;

            tileName.Text = thisTile.tileName;
            tileDesc.Text = thisTile.tileDesc;
            isFlammable.Checked = thisTile.isFlammable;
            isPathable.Checked = thisTile.isPathable;
            allowProps.Checked = thisTile.allowProps;
            hideInEditor.Checked = thisTile.hideInEditor;
            typeWall.Checked = (thisTile.tileType == 0);
            typeFloor.Checked = (thisTile.tileType == 1);
            useageInterior.Checked = (thisTile.tileUseage == 0);
            useageExterior.Checked = (thisTile.tileUseage == 1);
            zBias.Value = thisTile.zBias;

            string formattedTileName = tileName.Text.Trim().ToUpper().Replace(' ', '_');
            LoadSavedSprite(tilePreviewIconUI, Environment.CurrentDirectory + "/" + FilePaths.PathToUnityTileResources + formattedTileName + "/" + "EDITOR_UI.png");
            if (typeFloor.Checked)
            {
                floorSprites.Load(formattedTileName);

                LoadSavedSprite(FLOOR_previewCornerNorthEast, floorSprites.CORNER_NorthEast);
                LoadSavedSprite(FLOOR_previewCornerSouthEast, floorSprites.CORNER_SouthEast);
                LoadSavedSprite(FLOOR_previewCornerNorthWest, floorSprites.CORNER_NorthWest);
                LoadSavedSprite(FLOOR_previewCornerSouthWest, floorSprites.CORNER_SouthWest);

                LoadSavedSprite(FLOOR_previewEdgeNorth, floorSprites.EDGING_North);
                LoadSavedSprite(FLOOR_previewEdgeEast, floorSprites.EDGING_East);
                LoadSavedSprite(FLOOR_previewEdgeSouth, floorSprites.EDGING_South);
                LoadSavedSprite(FLOOR_previewEdgeWest, floorSprites.EDGING_West);

                for (int i = 0; i < floorSprites.Fillers.Count; i++)
                {
                    FLOOR_fillSprites.Items.Add(floorSprites.Fillers[i]);
                }
                if (floorSprites.Fillers.Count > 0) LoadSavedSprite(FLOOR_fillPreview, floorSprites.Fillers[floorSprites.Fillers.Count - 1]);
            }
            else if (typeWall.Checked)
            {
                wallSprites.Load(formattedTileName);

                for (int i = 0; i < wallSprites.Horizontals.Count; i++)
                {
                    WALL_horizontalSprites.Items.Add(wallSprites.Horizontals[i]);
                }
                if (wallSprites.Horizontals.Count > 0) LoadSavedSprite(WALL_horizontalPreview, wallSprites.Horizontals[wallSprites.Horizontals.Count - 1]);

                for (int i = 0; i < wallSprites.Verticals.Count; i++)
                {
                    WALL_verticalSprites.Items.Add(wallSprites.Verticals[i]);
                }
                if (wallSprites.Verticals.Count > 0) LoadSavedSprite(WALL_verticalPreview, wallSprites.Verticals[wallSprites.Verticals.Count - 1]);
            }
        }

        /* Load a sprite from saved data */
        private void LoadSavedSprite(PictureBox picture, string path)
        {
            if (path == "" || !File.Exists(path)) return;
            using (var tempPreviewImg = new Bitmap(path))
            {
                picture.Image = new Bitmap(tempPreviewImg);
            }
        }

        /* Save a sprite from entered data */
        private void SaveSprite(PictureBox picture, string name)
        {
            picture.Image.Save(FilePaths.PathToUnityTileResources + name + ".png");
            string metaPath = FilePaths.PathToUnityTileResources + name + ".png.meta";
            if (!File.Exists(metaPath)) File.WriteAllText(metaPath, Properties.Resources.meta_template.ToString());
        }

        /* Check validity of image(s) */
        private bool ImageIsValid(PictureBox image)
        {
            if (image.Image == null) return false;
            return ((image.Image.Width % 200) == 0 && (image.Image.Height % 200) == 0 && image.Image.Width == image.Image.Height);
        }
        private bool ImageIsValid(ListBox imagelist, PictureBox imagepartner)
        {
            bool failCheck = false;
            for (int i = 0; i < imagelist.Items.Count; i++)
            {
                imagelist.SelectedIndex = i;
                failCheck = ImageIsValid(imagepartner);
                if (!failCheck) return false;
            }
            return failCheck;
        }

        /* Save the tile */
        private void saveTile_Click(object sender, EventArgs e)
        {
            //Validation
            if (tileName.Text == "")
            {
                MessageBox.Show("Tile must have a name!", "No tile name.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (tileDesc.Text == "")
            {
                MessageBox.Show("Tile must have a description!", "No tile description.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            bool imageValid = false;
            if (typeFloor.Checked)
            {
                imageValid =   (ImageIsValid(FLOOR_previewCornerNorthEast) &&
                                ImageIsValid(FLOOR_previewCornerSouthEast) &&
                                ImageIsValid(FLOOR_previewCornerNorthWest) &&
                                ImageIsValid(FLOOR_previewCornerSouthWest) &&
                                ImageIsValid(FLOOR_previewEdgeNorth) &&
                                ImageIsValid(FLOOR_previewEdgeEast) &&
                                ImageIsValid(FLOOR_previewEdgeSouth) &&
                                ImageIsValid(FLOOR_previewEdgeWest) &&
                                ImageIsValid(FLOOR_fillSprites, FLOOR_fillPreview));
            }
            else if (typeWall.Checked)
            {
                imageValid =   (ImageIsValid(WALL_horizontalSprites, WALL_horizontalPreview) &&
                                ImageIsValid(WALL_verticalSprites, WALL_verticalPreview));
            }
            if (!imageValid || !ImageIsValid(tilePreviewIconUI))
            {
                MessageBox.Show("Tile images MUST be square and a division of 200 pixels.\nPlease follow the guidlines for irregular size tiles!", "Invalid tile size.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult shouldDo = MessageBox.Show("Would you like to see an example?", "Example tile sizing offer", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (shouldDo == DialogResult.Yes && File.Exists("TileExample.png")) Process.Start("TileExample.png");
                return;
            }

            //Create and check tile name
            string formattedTileName = tileName.Text.Trim().ToUpper().Replace(' ', '_');
            if (tileIndex == -1)
            {
                for (int i = 0; i < TileFileInterface.GetData().Count; i++)
                {
                    if (TileFileInterface.GetData()[i].tileName == formattedTileName)
                    {
                        MessageBox.Show("Tile name must be unique!", "Name already exists.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }
            Directory.CreateDirectory(FilePaths.PathToUnityTileResources + formattedTileName);

            //Copy sprites & save Unity metas
            SaveSprite(tilePreviewIconUI, formattedTileName + "/EDITOR_UI");
            if (typeFloor.Checked)
            {
                //Update/create config
                BinaryWriter writer = new BinaryWriter(File.OpenWrite(FilePaths.PathToUnityStreaming + "TILECONFIGS/" + formattedTileName + ".DWB"));
                writer.Write(0);
                writer.Write(FLOOR_fillSprites.Items.Count);
                writer.Close();

                if (changedSprites)
                {
                    //Write out the corners
                    SaveSprite(FLOOR_previewCornerNorthEast, formattedTileName + "/CORNER_NORTH_EAST");
                    SaveSprite(FLOOR_previewCornerSouthEast, formattedTileName + "/CORNER_SOUTH_EAST");
                    SaveSprite(FLOOR_previewCornerNorthWest, formattedTileName + "/CORNER_NORTH_WEST");
                    SaveSprite(FLOOR_previewCornerSouthWest, formattedTileName + "/CORNER_SOUTH_WEST");

                    //Write out the edges
                    SaveSprite(FLOOR_previewEdgeNorth, formattedTileName + "/EDGE_NORTH");
                    SaveSprite(FLOOR_previewEdgeEast, formattedTileName + "/EDGE_EAST");
                    SaveSprite(FLOOR_previewEdgeSouth, formattedTileName + "/EDGE_SOUTH");
                    SaveSprite(FLOOR_previewEdgeWest, formattedTileName + "/EDGE_WEST");

                    //Write out the fillers
                    for (int i = 0; i < FLOOR_fillSprites.Items.Count; i++)
                    {
                        FLOOR_fillSprites.SelectedIndex = i;
                        SaveSprite(FLOOR_fillPreview, formattedTileName + "/FILL_" + i);
                    }
                }
            }
            else if (typeWall.Checked)
            {
                //Update/create config
                BinaryWriter writer = new BinaryWriter(File.OpenWrite(FilePaths.PathToUnityStreaming + "TILECONFIGS/" + formattedTileName + ".DWB"));
                writer.Write(1);
                writer.Write(WALL_verticalSprites.Items.Count);
                writer.Write(WALL_horizontalSprites.Items.Count);
                writer.Close();

                if (changedSprites)
                {
                    //Write out the actual image files
                    for (int i = 0; i < WALL_verticalSprites.Items.Count; i++)
                    {
                        WALL_verticalSprites.SelectedIndex = i;
                        SaveSprite(WALL_verticalPreview, formattedTileName + "/VERTICAL_" + i);
                    }
                    for (int i = 0; i < WALL_horizontalSprites.Items.Count; i++)
                    {
                        WALL_horizontalSprites.SelectedIndex = i;
                        SaveSprite(WALL_horizontalPreview, formattedTileName + "/HORIZONTAL_" + i);
                    }
                }
            }
            
            //Save data
            thisTile.tileName = formattedTileName;
            thisTile.tileDesc = tileDesc.Text;
            thisTile.isFlammable = isFlammable.Checked;
            thisTile.isPathable = isPathable.Checked;
            thisTile.allowProps = allowProps.Checked;
            thisTile.hideInEditor = hideInEditor.Checked;
            thisTile.tileType = (Int16)(typeWall.Checked ? 0 : 1);
            thisTile.tileUseage = (Int16)(useageInterior.Checked ? 0 : 1);
            thisTile.zBias = (int)zBias.Value;
            if (tileIndex == -1) TileFileInterface.GetData().Add(thisTile);
            TileFileInterface.SaveData();

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

        /* GENERIC */
        private void browseEditorIcon_Click(object sender, EventArgs e)
        {
            SelectSprite(tilePreviewIconUI);
        }

        /* Floor sprites */
        private void FLOOR_browseCornerNorth_Click(object sender, EventArgs e)
        {
            floorSprites.CORNER_NorthEast = SelectSprite(FLOOR_previewCornerNorthEast);
        }
        private void FLOOR_browseCornerSouth_Click(object sender, EventArgs e)
        {
            floorSprites.CORNER_NorthWest = SelectSprite(FLOOR_previewCornerNorthWest);
        }
        private void FLOOR_browseCornerEast_Click(object sender, EventArgs e)
        {
            floorSprites.CORNER_SouthEast = SelectSprite(FLOOR_previewCornerSouthEast);
        }
        private void FLOOR_browseCornerWest_Click(object sender, EventArgs e)
        {
            floorSprites.CORNER_SouthWest = SelectSprite(FLOOR_previewCornerSouthWest);
        }
        private void FLOOR_browseEdgeNorth_Click(object sender, EventArgs e)
        {
            floorSprites.EDGING_North = SelectSprite(FLOOR_previewEdgeNorth);
        }
        private void FLOOR_browseEdgeSouth_Click(object sender, EventArgs e)
        {
            floorSprites.EDGING_South = SelectSprite(FLOOR_previewEdgeSouth);
        }
        private void FLOOR_browseEdgeEast_Click(object sender, EventArgs e)
        {
            floorSprites.EDGING_East = SelectSprite(FLOOR_previewEdgeEast);
        }
        private void FLOOR_browseEdgeWest_Click(object sender, EventArgs e)
        {
            floorSprites.EDGING_West = SelectSprite(FLOOR_previewEdgeWest);
        }
        private void FLOOR_browseFill_Click(object sender, EventArgs e)
        {
            floorSprites.Fillers.Add(SelectSprite(FLOOR_fillPreview));
            FLOOR_fillSprites.Items.Add(floorSprites.Fillers[floorSprites.Fillers.Count - 1]);
        }
        private void FLOOR_fillSprites_SelectedIndexChanged(object sender, EventArgs e)
        {
            using (var tempPreviewImg = new Bitmap(FLOOR_fillSprites.Items[FLOOR_fillSprites.SelectedIndex].ToString()))
            {
                FLOOR_fillPreview.Image = new Bitmap(tempPreviewImg);
            }
        }
        private void FLOOR_clearFill_Click(object sender, EventArgs e)
        {
            floorSprites.Fillers.Clear();
            FLOOR_fillSprites.Items.Clear();
            FLOOR_fillPreview.Image = null;
        }

        /* Wall sprites */
        private void WALL_browseHorizontal_Click(object sender, EventArgs e)
        {
            wallSprites.Horizontals.Add(SelectSprite(WALL_horizontalPreview));
            WALL_horizontalSprites.Items.Add(wallSprites.Horizontals[wallSprites.Horizontals.Count - 1]);
        }
        private void WALL_browseVertical_Click(object sender, EventArgs e)
        {
            wallSprites.Verticals.Add(SelectSprite(WALL_verticalPreview));
            WALL_verticalSprites.Items.Add(wallSprites.Verticals[wallSprites.Verticals.Count - 1]);
        }
        private void WALL_clearHorizontal_Click(object sender, EventArgs e)
        {
            wallSprites.Horizontals.Clear();
            WALL_horizontalSprites.Items.Clear();
            WALL_horizontalPreview.Image = null;
        }
        private void WALL_horizontalSprites_SelectedIndexChanged(object sender, EventArgs e)
        {
            using (var tempPreviewImg = new Bitmap(WALL_horizontalSprites.Items[WALL_horizontalSprites.SelectedIndex].ToString()))
            {
                WALL_horizontalPreview.Image = new Bitmap(tempPreviewImg);
            }
        }
        private void WALL_verticalSprites_SelectedIndexChanged(object sender, EventArgs e)
        {
            using (var tempPreviewImg = new Bitmap(WALL_verticalSprites.Items[WALL_verticalSprites.SelectedIndex].ToString()))
            {
                WALL_verticalPreview.Image = new Bitmap(tempPreviewImg);
            }
        }
        private void WALL_clearVertical_Click(object sender, EventArgs e)
        {
            wallSprites.Verticals.Clear();
            WALL_verticalSprites.Items.Clear();
            WALL_verticalPreview.Image = null;
        }

        /* Clear sprite UIs */
        private void ClearSpriteUIs()
        {
            floorSprites = new FloorTileSprites();

            FLOOR_previewCornerNorthEast.Image = null;
            FLOOR_previewCornerSouthEast.Image = null;
            FLOOR_previewCornerNorthWest.Image = null;
            FLOOR_previewCornerSouthWest.Image = null;

            FLOOR_previewEdgeNorth.Image = null;
            FLOOR_previewEdgeEast.Image = null;
            FLOOR_previewEdgeSouth.Image = null;
            FLOOR_previewEdgeWest.Image = null;

            FLOOR_fillSprites.Items.Clear();
            FLOOR_fillPreview.Image = null;

            wallSprites = new WallTileSprites();

            WALL_horizontalSprites.Items.Clear();
            WALL_horizontalPreview.Image = null;
            WALL_verticalSprites.Items.Clear();
            WALL_verticalPreview.Image = null;
        }

        /* Update selection of wall/floor sprite groups */
        private void typeFloor_CheckedChanged(object sender, EventArgs e)
        {
            WallSpriteGroup.Enabled = false;
            FloorSpriteGroup.Enabled = true;
            ClearSpriteUIs();
        }
        private void typeWall_CheckedChanged(object sender, EventArgs e)
        {
            WallSpriteGroup.Enabled = true;
            FloorSpriteGroup.Enabled = false;
            ClearSpriteUIs();
        }
    }
}
