# SDV-AccessibleTiles

Accessibility mod which seeks to make movement and tracking easier.

# Default Controls

## General

* Press HOME to center yourself in the tile you are currently standing in
* Press I to toggle between regular movement and grid movement.
* WASD to walk between tiles (If grid movement is active)
    * Also supports the controller d-pad
* Screen reader will read out which movement mode the player is in when they toggle it (With installed)

## Tracking Utility

> Note: Requires [Stardew Access](https://stardew-access.github.io) to be installed.

* CTRL + PageUp/PageDown - Cycle through object categories
* PageUp/PageDown - Cycle through objects in current object category
* HOME - Read the direction, distance and tile location of the nearest focused object
* END - Read the tile location of the nearest focused object
* CTRL + HOME - Automatically walk near focused object
* CTRL + END - Get detailed directions on how to reach a focused object
* Grave/Tilde to change tracker object sort type

### Tracking Utility Categories

Any given category will only be selectable if there are objects that fit into that category present in your current map location.
Categories are mostly derived from the Stardew Access mod.

#### Sort Types

* By Name
* By Proximity (closest first)

## JAWS Support

If you plan to use JAWS while playing with grid movement keys assigned to the arrow keys, you will need to apply the JAWS Passthrough Settings file found [here](https://stardew.grumpycrouton.com/releases/StardewModdingAPI.jkm)
