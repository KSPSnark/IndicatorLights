## What it does

Adds various helpful LED "indicators" to enhance your ships.  Broadly speaking, these fall into two categories:

 * **Standalone parts:**  radially attachable LED lights with customizable colors, which can be toggled on/off with action groups.
 * **Enhancements to existing parts:**  add LEDs to stock parts to provide a visible, automatic indication of part status (such as whether a fuel cell is on or off).

## How to install

Unzip the contents of "GameData" to your GameData folder, same as with most mods. (Note, includes ModuleManager.)


## IMPORTANT:   This is a *pre-release*

Please note that IndicatorLights is currently in **pre-release**.  In practical terms, this is what it means to you:

* Everything you see here is fully-functional and you can use it to your heart's content.
* However, it's not yet feature-complete (there's a lot of stuff I plan to add that's not there yet).
* It's also not "style-complete":  there are parts in here that have only very rudimentary models, with known cosmetic issues. I'm more of a programmer than a modeler.  I'll be revisiting the visual style later; right now I'm focusing on adding the functionality I want.
* Until I get to the point that I call it "released" rather than "pre-release", I reserve the right to make breaking changes when updating to new versions.  (This is mainly a concern if you are a modder and want to make your own parts that use my modules; see discussion below.)


## Current features

### New parts
At the moment there's just one part, the BL-01 Indicator Light:

![BL-01 Indicator Light](https://raw.githubusercontent.com/KSPSnark/IndicatorLights/master/screenshots/blinkenlight.png)

It's a small, light, radially attachable widget that you can put pretty much anywhere.  You can toggle it on/off via its right-click menu, or via action groups.

The light supports two modes, "continuous" (the default) and "blinking". You can customize the blink period, and toggle blink mode via action groups.

It doesn't emit any actual "light" in the sense of "illuminate objects around it"; it just glows.  So you can't light up a scene with it, but you *can* use lots of them, because they're computationally cheap.

Both the "on" and the "off" colors are fully customizable in the editor.  (The default is "bright green" for on, and "completely dark" for off.)

If you have [DefaultActionGroups](https://spacedock.info/mod/24/DefaultActionGroups) installed, this part's "toggle light" action will automatically get added to the Light action group by default.


### Batteries
All [batteries](http://wiki.kerbalspaceprogram.com/wiki/Parts#Batteries) have indicators that show the battery's status.

![batteries](https://raw.githubusercontent.com/KSPSnark/IndicatorLights/master/screenshots/batteries.png)

* **Full** (over 70% charge):  green
* **Medium** (30-70% charge):  yellow
* **Low** (under 30% charge):  red
* **Critical** (under 3% charge):  pulsating red
* **Disabled**:  blinking


### Fuel cells
Both the simple [fuel cells](http://wiki.kerbalspaceprogram.com/wiki/Fuel_Cell) and the large [fuel cell arrays](http://wiki.kerbalspaceprogram.com/wiki/Fuel_Cell_Array) now have green LED indicators that light up when the fuel cell is active.

![fuel cell](https://raw.githubusercontent.com/KSPSnark/IndicatorLights/master/screenshots/fuel%20cell.png)
![fuel cell array](https://raw.githubusercontent.com/KSPSnark/IndicatorLights/master/screenshots/fuel%20cell%20array.png)


### Reaction wheels
All [reaction wheels](http://wiki.kerbalspaceprogram.com/wiki/Parts#Reaction_wheels) now have LED indicators that show their status.

![reaction wheels](https://raw.githubusercontent.com/KSPSnark/IndicatorLights/master/screenshots/reaction%20wheels.png)

* Color shows mode:  green (normal), yellow (SAS-only), blue (pilot-only).
* Turns off when the reaction wheel is disabled.
* Dimmer when not in active use (i.e. no SAS or player input).
* Blinks brightly if electricity-deprived while turned on.

### Docking ports
The small, medium, and large [docking ports](http://wiki.kerbalspaceprogram.com/wiki/Parts#Docking) now have LED status indicators.

(I've left all the other docking ports alone, for the time being: shielded, in-line, Mk2. Those were trickier due to animations. I may come back to revisit them later.)

![docking ports](https://raw.githubusercontent.com/KSPSnark/IndicatorLights/master/screenshots/docking%20ports.png)

* Color shows fuel crossfeed status: green (enabled), red (disabled)
* Blinks rapidly when the docking field is engaged
* After undocking, blinks slowly until the port is "reset" (has separated far enough to be dockable again).
* Indicator lights on docking ports are toggleable on/off via action groups, like the BL-01 standalone lamp. They're off by default.
* If you have [DefaultActionGroups](https://spacedock.info/mod/24/DefaultActionGroups) installed, then the "toggle light" action will automatically get added to the Light action group by default.


### Crew indicators
Various crew modules now have status indicators to show occupancy.

(Only the parts shown here have the indicators, currently.)

![crew indicators](https://raw.githubusercontent.com/KSPSnark/IndicatorLights/master/screenshots/crew.png)

* Indicators light up when crew slot is occupied; dark when empty.
* Color indicates profession (orange = pilot, green = engineer, blue = scientist, white = tourist).
* I was concerned about these being annoying (the "Christmas tree" effect), so they're toggleable on/off, and are **off by default**. You can activate them via right-click menu or action groups.
* **Note:** They don't light up in the editor, only in flight. Eventually I may add that, but it's a lot of code for a minor feature, so it's low priority for now.
* Eventually, I intend to add indicators to the rest of the crewed parts. (For anyone who can't wait, you can do this yourself via ModuleManager config.)


### ISRU units
The large and small ISRU converters have status indicators to show operational status.

![ISRU](https://raw.githubusercontent.com/KSPSnark/IndicatorLights/master/screenshots/isru.png)

* Each of the four converters on the ISRU unit has its own status indicator.
* Indicators are color-coded by resource:  LFO = cyan, liquid fuel = green, oxidizer = blue, monoprop = yellow.
* Currently just a simple on/off indicator. In the future I may add some visualization of heat status.


## Configuration
I use color a *lot* in this mod, as a way of conveying information. My choice of colors may not be to everyone's liking, though.  Maybe you'd like all of them to be dimmer.  Maybe you have color-blindness issues and my choice of red-versus-green for status doesn't work well for you.

All of these colors are read from a config file.  A default config file will be created for you after the first time you run the mod. It will be in your GameData folder, under the path IndicatorLights/PluginData/IndicatorLights/config.xml.

After the initial default config file is written, you can go in and edit the values, and they'll get used the next time KSP starts up.


## What's next?

As described above, this is a pre-release; I'm not even close to "done."  Here are my thoughts as to what I plan to add next (no promises, I reserve the right to change my mind!):

* **More standalone parts**, like the BL-01 Indicator Lamp, in various sizes and styles.
* **More indicator lights for stock parts** to show their status.  I have a *lot* of ideas here!
* **Style overhaul** to make my parts prettier (I'm a pretty good programmer, but a terrible modeler).
* ...And some more ambitious ideas that I won't talk about yet, 'coz I don't want to jinx them.

## Feedback welcome!

My main motivation for making this available when it's still a pre-release is to give folks the chance to try it out and let me know what they think, including "gosh, I'd sure like it if it could do *{thing it doesn't do now}*."

## If you're a modder

One of my goals in producing this mod is to make it friendly to other modders, so that you can enable indicator lights on your own parts.  It's pretty simple to hook up.  It may be a while before I get around to documenting this aspect in detail, but if you're interested, please contact me in the KSP forums and I'll be happy to give you whatever information you need.


---
#### A note of thanks
Deep gratitude to [NecroBones](http://forum.kerbalspaceprogram.com/index.php?/profile/105424-necrobones/) and [VintageXP](http://forum.kerbalspaceprogram.com/index.php?/profile/76701-vintagexp/content/), both of whom provided patient, expert instruction to a clueless newbie (i.e. me) who didn't know Blender and Unity from a hole in the ground.  This is my first parts pack, and I would have been totally at sea without the help of these fine gentlemen.  Their assistance made this mod possible (though they bear no blame for the crudity of my models, that's entirely my own!) 

