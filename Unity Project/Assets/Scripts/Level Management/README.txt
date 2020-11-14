All level editor specific scripts are stored within the "Editor Specific" folder. These should only apply to the editor scene.
Generic level scripts that are shared between the gameplay scene and editor should be in the root.

	- LevelFileVersion: Version info for saving/loading levels.
	- LevelGrid: The grid makeup of the level which contains the tiles of the map.
	- LevelManager: The controller for a level, containing level metadata, along with load functionality.

	- TileProperties: Functionality to get property information from a tile's type (e.g. pathable, non-pathable).
	- TileTypes: Enums for tile metadata - this is auto generated, do not edit!
	- LevelTileEntity: The tile entity script for objects that occupy a tile.

	- PropProperties: Functionality to get property information from a prop's type (e.g. waypoint, event spawner).
	- PropTypes: Enums for prop metadata - this is auto generated, do not edit!
	- LevelPropEntity: the prop entity script for props that can be placed on occupied tiles.