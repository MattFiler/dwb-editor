/*
 * *************************************************************
 * PLEASE DO NOT EDIT THIS SCRIPT WITHOUT CONTACTING THE AUTHORS
 * *************************************************************
 * 
 * 
 * AUTHORS:
 *  - Matt Filer
*/

/* Deletion mode must always accompany a new mode, one up in the chain */
public enum LevelEditorMode
{
    //Default "null" mode
    VOID_MODE,
    VOID_MODE_DUMMY,

    //Painting mode
    PAINTING_TILES,
    DELETING_TILES,

    //Prop dragging mode
    DRAGGING_PROPS,
    DELETING_PROPS,
}
