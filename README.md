# GTAVDashCamOverlay
Simple overlay for a dash-cam-like overlay with functional speed, location, time, and lights/siren/brakes indicators for police vehicles. Does NOT include camera modification, overlay only.

# Prerequisities
This plugin requires the following:
* Rage Hook Plugin (https://ragepluginhook.net/).
* Rage Native UI (https://github.com/alexguirre/RAGENativeUI).

# Installation
1. Install prerequisites according to the latest release instructions. Ensure these are working correctly before troubleshooting issues with this plugin.
2. Drag and drop the contents of the Zip file from the latest Release into your GTAV directory. Files must be put into the RageHookPlugin 'plugins' folder.
* [GTAV Root Directory]/plugins/DashCamOverlay.ini
* [GTAV Root Directory]/plugins/DashCamOverlay.dll
3. Edit the .ini file. Note that positions and scale of the overlay can be configured using an in-game menu. Some aspects are only configurable in the ini.
4. Ensure the plugin is set to load in the RageHookPlugin (RPH) launch settings. Alternatively, use *LoadPlugin DashCamOverlay.dll* in the RPH Console in-game (F4 by default while RPH is active).

![Lights On while Siren is Silent](https://raw.githubusercontent.com/Epidurality/GTAVDashCamOverlay/master/OverlayFull.png)

# Known Issues
* [BRAKES] indicator isn't perfect. Could not find reliable way of matching this with your tailights. Even checking for if the BRAKE button is being pressed apparently isn't consistent.
* [SIREN] does not work correctly with ELS. ELS overrides the default Siren toggling and I don't know how to hook into it. For now, just toggle the SIREN indicator in the ini/menu to hide it.

# Support
Please use the github Issues system to report issues. (https://github.com/Epidurality/GTAVDashCamOverlay)

# Credits
User CC2020 on the LCPDFR forums who suggested a functional overlay, and to the tens of users and developers of RageHook and RageNativeUI who make these plugins possible and usable.
