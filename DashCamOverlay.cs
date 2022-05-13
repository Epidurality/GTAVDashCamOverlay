using System.Drawing;
using System.Windows.Forms;
using System;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;

[assembly: Rage.Attributes.Plugin("Dash Cam Overlay", Description = "Overlay similar to the ASP Police Car Recorder", Author = "Epidurality")]

namespace DashCamOverlay
{
    public class EntryPoint
    {  
        public static void Main()
        {
            KeysConverter kc = new KeysConverter();
            Vehicle vehicle;
            ResText indicators, speed, timestamp, coordinates, identification;

            UIMenu menu = new UIMenu("Dash Cam Overlay", "See INI file for more settings");
            MenuPool menuPool = new MenuPool();
            menuPool.Add(menu);

            InitializationFile ini = new InitializationFile("Plugins/DashCamOverlay.ini");
            ini.Create();

            float overlayScale = (float)ini.ReadDouble("DashCamOverlay", "OverlayScale", 0.5);
            Color textColor = ini.ReadColor("DashCamOverlay", "TextColor", Color.White);
            int overlayFont = ini.ReadInt16("DashCamOverlay", "FontType", 0);
            bool dropShadow = ini.ReadBoolean("DashCamOverlay", "DropShadow", true);
            ulong refreshFrequency = ini.ReadUInt64("DashCamOverlay", "RefreshFrequency", 500);
            ulong lastRefresh = Game.TickCount - refreshFrequency - 1;
            Common.EFont fontType = Common.EFont.ChaletLondon;
            Keys menuKey = (Keys)kc.ConvertFromString(ini.ReadString("DashCamOverlay", "MenuKey", "I"));

            bool showBrakesIndicator = ini.ReadBoolean("Indicators", "ShowBrakesIndicator", true);
            bool showLightsIndicator = ini.ReadBoolean("Indicators", "ShowLightsIndicator", true);
            bool showSirenIndicator = ini.ReadBoolean("Indicators", "ShowSirenIndicator", true);
            int indicatorsX = ini.ReadInt16("Indicators", "IndicatorsX", 960);
            int indicatorsY = ini.ReadInt16("Indicators", "IndicatorsY", 1035);

            bool showSpeed = ini.ReadBoolean("Speed", "ShowSpeed", true);
            int speedX = ini.ReadInt16("Speed", "SpeedX", 1800);
            int speedY = ini.ReadInt16("Speed", "SpeedY", 100);
            bool speedMetric = ini.ReadBoolean("Speed", "SpeedMetric", false);

            bool showTimestamp = ini.ReadBoolean("Timestamp", "ShowTimestamp", true);
            string timestampFormat = ini.ReadString("Timestamp", "TimestampFormat", "yyyy/dd/MM hh:mm:ss:ff");
            int timestampX = ini.ReadInt16("Timestamp", "TimestampX", 960);
            int timestampY = ini.ReadInt16("Timestamp", "TimestampY", 25);

            bool showCoordinates = ini.ReadBoolean("Coordinates", "ShowCoordinates", true);
            int coordinatesX = ini.ReadInt16("Coordinates", "CoordinatesX", 960);
            int coordinatesY = ini.ReadInt16("Corodinates", "CoordinatesY", 0);

            bool showIdentification = ini.ReadBoolean("Identification", "ShowIdentification", true);
            int identificationX = ini.ReadInt16("Identification", "IdentificationX", 1800);
            int identificationY = ini.ReadInt16("Identification", "IdentificationY", 965);
            string departmentName = ini.ReadString("Identification", "DepartmentName", "LSPD");
            string officerName = ini.ReadString("Identification", "OfficerName", "Clancy Wiggum");
            string badgeNumber = ini.ReadString("Identification", "BadgeNumber", "3465550126");

            // Generic settings ini and menu

            UIMenuNumericScrollerItem<float> menuOverlayScale = new UIMenuNumericScrollerItem<float>("Overlay Scale", "Size of text for the overlay", 0.01f, 2.0f, 0.01f) { Value = overlayScale };
            menuOverlayScale.IndexChanged += (sender, oldVal, newVal) => { overlayScale = newVal/100.0f + 0.01f; ini.Write("DashCamOverlay", "OverlayScale", overlayScale); RedrawOverlay(); };
            menu.AddItems(menuOverlayScale);

            // Indicators ini and menu
            UIMenuCheckboxItem menuShowBrakesIndicator = new UIMenuCheckboxItem("Show Brakes Indicator", showBrakesIndicator, "Show or hide specifically the BRAKES indicator overlay");
            menuShowBrakesIndicator.CheckboxEvent += (sender, newVal) => { showBrakesIndicator = newVal; ini.Write("Indicators", "ShowBrakesIndicator", newVal); };
            menu.AddItems(menuShowBrakesIndicator);
            
            UIMenuCheckboxItem menuShowLightsIndicator = new UIMenuCheckboxItem("Show Lights Indicator", showLightsIndicator, "Show or hide specifically the LIGHTS indicator overlay");
            menuShowLightsIndicator.CheckboxEvent += (sender, newVal) => { showLightsIndicator = newVal; ini.Write("Indicators", "ShowLightsIndicator", newVal); };
            menu.AddItems(menuShowLightsIndicator);

            UIMenuCheckboxItem menuShowSirenIndicator = new UIMenuCheckboxItem("Show Siren Indicator", showLightsIndicator, "Show or hide specifically the Siren indicator overlay");
            menuShowSirenIndicator.CheckboxEvent += (sender, newVal) => { showSirenIndicator = newVal; ini.Write("Indicators", "ShowSirenIndicator", newVal); };
            menu.AddItems(menuShowSirenIndicator);

            UIMenuNumericScrollerItem<int> menuIndicatorsX = new UIMenuNumericScrollerItem<int>("Indicators X Position", "Horizontal position of the LIGHTS/BRAKES/SIREN indicators overlay", 0, 1920, 1) { Value = indicatorsX };
            menuIndicatorsX.IndexChanged += (sender, oldVal, newVal) => { indicatorsX = newVal; ini.Write("Indicators", "IndicatorsX", indicatorsX); RedrawOverlay(); };
            menu.AddItems(menuIndicatorsX);

            UIMenuNumericScrollerItem<int> menuIndicatorsY = new UIMenuNumericScrollerItem<int>("Indicators Y Position", "Vertical position of the LIGHTS/BRAKES/SIREN indicators overlay", 0, 1080, 1) { Value = indicatorsY };
            menuIndicatorsY.IndexChanged += (sender, oldVal, newVal) => { indicatorsY = newVal; ini.Write("Indicators", "IndicatorsY", indicatorsY); RedrawOverlay(); };
            menu.AddItems(menuIndicatorsY);

            // Speed ini and menu
            UIMenuCheckboxItem menuShowSpeed = new UIMenuCheckboxItem("Show Speed", showSpeed, "Show or hide the GPS Speed overlay");
            menuShowSpeed.CheckboxEvent += (sender, newVal) => { showSpeed = newVal; ini.Write("Speed", "ShowSpeed", showSpeed); };
            menu.AddItems(menuShowSpeed);

            UIMenuCheckboxItem menuSpeedMetric = new UIMenuCheckboxItem("Use KPH", speedMetric, "Check to use KPH, otherwise will use MPH for speed overlay");
            menuSpeedMetric.CheckboxEvent += (sender, newVal) => { speedMetric = newVal; ini.Write("Speed", "SpeedMetric", speedMetric); };
            menu.AddItems(menuSpeedMetric);

            UIMenuNumericScrollerItem<int> menuSpeedX = new UIMenuNumericScrollerItem<int>("Speed X Position", "Horizontal position of the GPS Speed overlay", 0, 1920, 1) { Value = speedX };
            menuSpeedX.IndexChanged += (sender, oldVal, newVal) => { speedX = newVal; ini.Write("Speed", "SpeedX", speedX); RedrawOverlay(); };
            menu.AddItems(menuSpeedX);

            UIMenuNumericScrollerItem<int> menuSpeedY = new UIMenuNumericScrollerItem<int>("Speed Y Position", "Vertical position of the GPS Speed overlay", 0, 1080, 1) { Value = speedY };
            menuSpeedY.IndexChanged += (sender, oldVal, newVal) => { speedY = newVal; ini.Write("Speed", "SpeedY", speedY); RedrawOverlay(); };
            menu.AddItems(menuSpeedY);

            // Timestamp ini and menu
            UIMenuCheckboxItem menuShowTimestamp = new UIMenuCheckboxItem("Show Timestamp", showTimestamp, "Show or hide the Timestamp overlay");
            menuShowTimestamp.CheckboxEvent += (sender, newVal) => { showTimestamp = newVal; ini.Write("Timestamp", "ShowTimestamp", showTimestamp); };
            menu.AddItems(menuShowTimestamp);

            UIMenuNumericScrollerItem<int> menuTimestampX = new UIMenuNumericScrollerItem<int>("Timestamp X Position", "Horizontal position of the Coordinates overlay", 0, 1920, 1) { Value = timestampX };
            menuTimestampX.IndexChanged += (sender, oldVal, newVal) => { timestampX = newVal; ini.Write("Timestamp", "TimestampX", timestampX); RedrawOverlay(); };
            menu.AddItems(menuTimestampX);

            UIMenuNumericScrollerItem<int> menuTimestampY = new UIMenuNumericScrollerItem<int>("Timestamp Y Position", "Vetical position of the Coordinates overlay", 0, 1080, 1) { Value = timestampY };
            menuTimestampY.IndexChanged += (sender, oldVal, newVal) => { timestampY = newVal; ini.Write("Timestamp", "TimestampY", timestampY); RedrawOverlay(); };
            menu.AddItems(menuTimestampY);

            // Coordinates ini and menu
            UIMenuCheckboxItem menuShowCoordinates = new UIMenuCheckboxItem("Show Coordinates", showCoordinates, "Show or hide the Coordinates overlay");
            menuShowCoordinates.CheckboxEvent += (sender, newVal) => { showCoordinates = newVal; ini.Write("Coordinates", "ShowCoordinates", showCoordinates); };
            menu.AddItems(menuShowCoordinates);

            UIMenuNumericScrollerItem<int> menuCoordinatesX = new UIMenuNumericScrollerItem<int>("Coordinates X Position", "Horizontal position of the Coordinates overlay", 0, 1920, 1) { Value = coordinatesX };
            menuCoordinatesX.IndexChanged += (sender, oldVal, newVal) => { coordinatesX = newVal; ini.Write("Coordinates", "CoordinatesX", coordinatesX); RedrawOverlay(); };
            menu.AddItems(menuCoordinatesX);

            UIMenuNumericScrollerItem<int> menuCoordinatesY = new UIMenuNumericScrollerItem<int>("Coordinates Y Position", "Vertical position of the Coordinates overlay", 0, 1080, 1) { Value = coordinatesY };
            menuCoordinatesY.IndexChanged += (sender, oldVal, newVal) => { coordinatesY = newVal; ini.Write("Coordinates", "CoordinatesY", coordinatesY); RedrawOverlay(); };
            menu.AddItems(menuCoordinatesY);

            // Identification ini and menu
            UIMenuCheckboxItem menuShowIdentification = new UIMenuCheckboxItem("Show Identification", showIdentification, "Show or hide the officer identification overlay");
            menuShowIdentification.CheckboxEvent += (sender, newVal) => { showIdentification = newVal; ini.Write("Identification", "ShowIdentification", showIdentification); };
            menu.AddItems(menuShowIdentification);

            UIMenuNumericScrollerItem<int> menuIdentificationX = new UIMenuNumericScrollerItem<int>("Identification X Position", "Horizontal position of the Identification overlay", 0, 1920, 1) { Value = identificationX };
            menuIdentificationX.IndexChanged += (sender, oldVal, newVal) => { identificationX = newVal; ini.Write("Identification", "IdentificationX", identificationX); RedrawOverlay(); };
            menu.AddItems(menuIdentificationX);

            UIMenuNumericScrollerItem<int> menuIdentificationY = new UIMenuNumericScrollerItem<int>("Identification Y Position", "Vertical position of the Identification overlay", 0, 1080, 1) { Value = identificationY };
            menuIdentificationY.IndexChanged += (sender, oldVal, newVal) => { identificationY = newVal; ini.Write("Identification", "IdentificationY", identificationY); RedrawOverlay(); };
            menu.AddItems(menuIdentificationY);

            RedrawOverlay();

            // Main program loop
            while (true)
            {
                GameFiber.Yield();
                menuPool.ProcessMenus();

                // DELETE WHEN DONE DEBUGGING
                //if (Game.IsKeyDown(Keys.O))
                //{
                //    break; // Effectively ends the plugin for now during debugging.
                //}

                // Process menu show/hide
                if (Game.IsKeyDown(menuKey))
                {
                    if (menu.Visible)
                    {
                        menu.Visible = false;
                    }
                    else
                    {
                        if (!UIMenu.IsAnyMenuVisible)
                        {
                            menu.Visible = true;
                        }
                        else
                        {
                            Game.DisplayNotification("commonmenu", "mp_alerttriangle", "Dash Cam Overlay", "Menu could not be opened.", "Another RageNativeUI menu is in use.");
                        }
                    }
                }

                // Perform overlay functions
                if (Game.LocalPlayer.Character.IsInAnyVehicle(false))
                {
                    vehicle = Game.LocalPlayer.Character.CurrentVehicle;
                    // Perform updates that don't need to be done each tick
                    if (Game.TickCount > lastRefresh + refreshFrequency)
                    {
                        if (showSpeed)
                        {
                            if (speedMetric)
                            {
                                speed.Caption = "GPS " + Math.Floor(MathHelper.ConvertMetersPerSecondToKilometersPerHour(vehicle.Speed)).ToString() + " KPH";
                            }
                            else
                            {
                                speed.Caption = "GPS " + Math.Floor(MathHelper.ConvertMetersPerSecondToMilesPerHour(vehicle.Speed)).ToString() + " MPH";
                            }
                        }
                        if (showTimestamp) { timestamp.Caption = DateTime.Now.ToString(timestampFormat); }
                        // Notes for coordinates: they are roughly scaled to represent the real Los Angeles GPS coordinates.
                        // LosSantos: N: 7641, S: -3560, E: 4000, W: -4000
                        // LosAngeles: N: 34.8134, S: 33.69340, E: -118.59220, W: -117.76540 -> Centered roughly at 34.2534, -118.18228.
                        // Just to make it easy, we'll call it "GameUnits/10,000" per degree of lat/lon, offset by 34.2534deg N and -118.18228deg E
                        if (showCoordinates) { coordinates.Caption = "N:" + Math.Round(34.2534 + vehicle.Position.Y / 10000f, 4).ToString() + " E:" + Math.Round(-118.18228 + vehicle.Position.X / 10000f, 4).ToString(); }                            
                        lastRefresh = Game.TickCount;
                    }

                    // Perform updates and draws every tick
                    indicators.Caption = "";
                    if (vehicle.IsPoliceVehicle)
                    {
                        if (showLightsIndicator)
                        {
                            if (vehicle.IsSirenOn)
                            {
                                indicators.Caption += "~g~[LIGHTS] ";
                            }
                            else
                            {
                                indicators.Caption += "~w~[LIGHTS] ";
                            }
                        }
                        if (showSirenIndicator)
                        {
                            if (vehicle.IsSirenSilent)
                            {
                                indicators.Caption += "~w~[SIREN] ";
                            }
                            else
                            {
                                indicators.Caption += "~g~[SIREN] ";
                            }
                        }
                    }
                    if (showBrakesIndicator)
                    {
                        if (Game.IsControlPressed(0, GameControl.VehicleBrake))
                        {
                            indicators.Caption += "~g~[BRAKES]";
                        }
                        else
                        {
                            indicators.Caption += "~w~[BRAKES]";
                        }
                    }
                    
                    indicators.Draw();
                    if (showSpeed) speed.Draw();
                    if (showTimestamp) timestamp.Draw();
                    if (showCoordinates) coordinates.Draw();
                    if (showIdentification) identification.Draw();
                }

            }
            
            void RedrawOverlay()
            {
                indicators = new ResText("[LIGHTS] [BRAKES] [SIREN]", new Point(indicatorsX, indicatorsY), overlayScale, textColor, fontType, ResText.Alignment.Centered) { DropShadow = dropShadow };
                speed = new ResText("GPS 000 MPH", new Point(speedX, speedY), overlayScale, textColor, fontType, ResText.Alignment.Centered) { DropShadow = dropShadow };
                timestamp = new ResText("YYYY/MM/DD 00:00:00:00", new Point(timestampX, timestampY), overlayScale, textColor, fontType, ResText.Alignment.Centered) { DropShadow = dropShadow };
                coordinates = new ResText("N:0000.0000", new Point(coordinatesX, coordinatesY), overlayScale, textColor, fontType, ResText.Alignment.Centered) { DropShadow = dropShadow };
                identification = new ResText(departmentName + "\n" + officerName + "\n" + badgeNumber, new Point(identificationX, identificationY), overlayScale, textColor, fontType, ResText.Alignment.Centered) { DropShadow = dropShadow };
            }
        }
    }
}
