using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DWB_Toolkit
{
    class SpriteBounds
    {
        public int from_origin_north = 0;
        public int from_origin_east = 0;
        public int from_origin_south = 0;
        public int from_origin_west = 0;

        public decimal tile_excess_north = 0;
        public decimal tile_excess_east = 0;
        public decimal tile_excess_south = 0;
        public decimal tile_excess_west = 0;
    }
    class SpriteAnalysis
    {
        /* Calculate bounds */
        static public SpriteBounds CalculateSpriteBounds(Bitmap _image)
        {
            int first_nonalpha_topdown = int.MaxValue;
            int first_nonalpha_leftright = int.MaxValue;
            int last_nonalpha_topdown = -int.MaxValue;
            int last_nonalpha_leftright = -int.MaxValue;

            for (int x = 0; x < _image.Width; x++)
            {
                for (int y = 0; y < _image.Height; y++)
                {
                    if (_image.GetPixel(x, y).A != 0)
                    {
                        if (y < first_nonalpha_topdown) first_nonalpha_topdown = y;
                        if (y > last_nonalpha_topdown) last_nonalpha_topdown = y;
                    }
                }
            }

            for (int y = 0; y < _image.Height; y++)
            {
                for (int x = 0; x < _image.Width; x++)
                {
                    if (_image.GetPixel(x, y).A != 0)
                    {
                        if (x < first_nonalpha_leftright) first_nonalpha_leftright = x;
                        if (x > last_nonalpha_leftright) last_nonalpha_leftright = x;
                    }
                }
            }

            int origin_x = _image.Width / 2;
            int origin_y = _image.Height / 2;

            SpriteBounds sprite_dims = new SpriteBounds();

            sprite_dims.from_origin_north = origin_y - first_nonalpha_topdown;
            sprite_dims.from_origin_east = last_nonalpha_leftright - origin_x;
            sprite_dims.from_origin_south = last_nonalpha_topdown - origin_y;
            sprite_dims.from_origin_west = origin_x - first_nonalpha_leftright;

            //The value here should match the spritePixelsToUnits value in meta_template.txt
            decimal northVal = (decimal)(((float)sprite_dims.from_origin_north - 100.0f) / 200.0f);
            sprite_dims.tile_excess_north = ((northVal < 0) ? 0 : northVal);
            decimal eastVal = (decimal)(((float)sprite_dims.from_origin_east - 100.0f) / 200.0f);
            sprite_dims.tile_excess_east = ((eastVal < 0) ? 0 : eastVal);
            decimal southVal = (decimal)(((float)sprite_dims.from_origin_south - 100.0f) / 200.0f);
            sprite_dims.tile_excess_south = ((southVal < 0) ? 0 : southVal);
            decimal westVal = (decimal)(((float)sprite_dims.from_origin_west - 100.0f) / 200.0f);
            sprite_dims.tile_excess_west = ((westVal < 0) ? 0 : westVal);

            return sprite_dims;
        }

        /* Save bounds data */
        static public void SaveData(string _path, SpriteBounds _data)
        {
            Directory.CreateDirectory(_path.Substring(0, _path.Length - Path.GetFileName(_path).Length));
            BinaryWriter writer = new BinaryWriter(File.OpenWrite(_path));
            writer.Write(_data.from_origin_north);
            writer.Write(_data.from_origin_east);
            writer.Write(_data.from_origin_south);
            writer.Write(_data.from_origin_west);
            writer.Write((float)_data.tile_excess_north);
            writer.Write((float)_data.tile_excess_east);
            writer.Write((float)_data.tile_excess_south);
            writer.Write((float)_data.tile_excess_west);
            writer.Close();
        }

        /* Load bounds data */
        static public SpriteBounds LoadData(string _path)
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(_path));
            /* COMPATIBILITY FOR OLDER START */
            if (reader.BaseStream.Length != 32)
            {
                reader.Close();
                return new SpriteBounds();
            }
            /* COMPATIBILITY FOR OLDER END */
            SpriteBounds bounds = new SpriteBounds();
            bounds.from_origin_north = reader.ReadInt32();
            bounds.from_origin_east = reader.ReadInt32();
            bounds.from_origin_south = reader.ReadInt32();
            bounds.from_origin_west = reader.ReadInt32();
            bounds.tile_excess_north = (decimal)reader.ReadSingle();
            bounds.tile_excess_east = (decimal)reader.ReadSingle();
            bounds.tile_excess_south = (decimal)reader.ReadSingle();
            bounds.tile_excess_west = (decimal)reader.ReadSingle();
            reader.Close();
            return bounds;
        }
    }
}
