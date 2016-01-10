using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Devious_Retention
{
    /// <summary>
    /// Provides constants about the game, and also provides
    /// lists of entity types, technologies and factions for
    /// each client.
    /// </summary>
    public class GameInfo
    {
        public static string BASE_DIRECTORY = AppDomain.CurrentDomain.BaseDirectory.Remove(AppDomain.CurrentDomain.BaseDirectory.Length - 10, 10);
        public static string UNIT_FNAME { get; internal set; } = BASE_DIRECTORY + "Definitions\\Units.txt"; 
        public static string BUILDING_FNAME { get; internal set; } = BASE_DIRECTORY + "Definitions\\Buildings.txt";
        public static string RESOURCE_FNAME { get; internal set; } = BASE_DIRECTORY + "Definitions\\Resources.txt";
        public static string TECHNOLOGY_FNAME { get; internal set; } = BASE_DIRECTORY + "Definitions\\Technologies.txt";
        public static string FACTION_FNAME { get; internal set; } = BASE_DIRECTORY + "Definitions\\Factions.txt";
        public static string TILE_FNAME { get; internal set; } = BASE_DIRECTORY + "Definitions\\Tiles.txt";

        public static string RESOURCE_IMAGE_BASE { get; internal set; } = BASE_DIRECTORY + "Images\\Resources\\";
        public static string BUILDING_IMAGE_BASE { get; internal set; } = BASE_DIRECTORY + "Images\\Buildings\\";
        public static string UNIT_IMAGE_BASE { get; internal set; } = BASE_DIRECTORY + "Images\\Units\\";
        public static string TILE_IMAGE_BASE { get; internal set; } = BASE_DIRECTORY + "Images\\Tiles\\";

        public static string BUILDING_ICON_BASE { get; internal set; } = BASE_DIRECTORY + "Images\\Buildings\\";
        public static string UNIT_ICON_BASE { get; internal set; } = BASE_DIRECTORY + "Images\\Units\\";

        public static string RESOURCE_ICON_IMAGE_BASE { get; internal set; } = BASE_DIRECTORY + "Images\\ResourceIcons\\";
        public static string[] RESOURCE_ICON_NAMES { get; internal set; } = new string[RESOURCE_TYPES]
        {
            "metal.png", "oil.png", "energy.png", "science.png"
        };

        public static string DAMAGE_TYPE_ICON_IMAGE_BASE { get; internal set; } = BASE_DIRECTORY + "Images\\DamageTypeIcons\\";
        public static string[] DAMAGE_TYPE_ICON_NAMES { get; internal set; } = new string[DAMAGE_TYPES]
        {
            "melee.png","ranged.png","explosion.png"
        };

        // For background images in the GUI
        public static string BACKGROUND_IMAGE_BASE { get; private set; } = BASE_DIRECTORY + "Images\\Backgrounds\\";
        public static string BACKGROUND_IMAGE_RESOURCE_DISPLAY_AREA { get; private set; } = "resourcedisplayarea.png";
        public static string BACKGROUND_IMAGE_SELECTED_ENTITY_AREA { get; private set; } = "selectedentityarea.png";

        // Used in the place of an entity image or technology icon if the right one can't be loaded
        public static string DEFAULT_IMAGE_NAME { get; private set; } = BASE_DIRECTORY + "Images\\defaultimage.png";

        // In milliseconds
        public const int TICK_TIME = 100;
        // Melee, ranged, bombard
        public const int DAMAGE_TYPES  = 3;
        // Metal, oil, energy, science
        public const int RESOURCE_TYPES = 4;
        // Infantry, armored (e.g. tanks), flying, ships
        public const int UNIT_TYPES = 4;
        // How many ticks between each attack of units and buildings
        public const int ATTACK_SPEED = 10;
        // How far apart, in tiles, entities need to be from each other to be considered adjacent
        public const double ADJACENT_DISTANCE = 1;

        public SortedDictionary<string,UnitType> unitTypes { get; internal set; }
        public SortedDictionary<string,BuildingType> buildingTypes { get; internal set; }
        public SortedDictionary<string,ResourceType> resourceTypes { get; internal set; }
        public SortedDictionary<string,Technology> technologies { get; internal set; }
        public SortedDictionary<string,Faction> factions { get; internal set; }
        public SortedDictionary<string,Tile> tiles { get; internal set; }

        // Whether or not debug messages will be sent
        private bool debug = false;
        // The debug window; only used if debug == true
        private DebugWindow debugWindow = null;

        public GameInfo(bool debug, DebugWindow debugWindow)
        {
            Console.WriteLine(BASE_DIRECTORY);
            this.debug = debug;
            this.debugWindow = debugWindow;

            ReadUnits(UNIT_FNAME);
            ReadBuildings(BUILDING_FNAME);
            ReadResources(RESOURCE_FNAME);
            ReadTechnologies(TECHNOLOGY_FNAME);
            ReadFactions(FACTION_FNAME);
            ReadTiles(TILE_FNAME);
        }

        /// <summary>
        /// Writes all currently known definitions to the debug window (if the debug window is active).
        /// </summary>
        public void WriteDefinitionsToDebug()
        {
            if (!debug) return;

            WriteDebug("UNIT TYPES", Color.DarkGreen);
            foreach (UnitType u in unitTypes.Values)
                WriteDebug(u.ToString(), Color.Green);
            WriteDebug("BUILDING TYPES", Color.DarkGreen);
            foreach (BuildingType b in buildingTypes.Values)
                WriteDebug(b.ToString(), Color.Green);
            WriteDebug("RESOURCE TYPES", Color.DarkGreen);
            foreach (ResourceType r in resourceTypes.Values)
                WriteDebug(r.ToString(), Color.Green);
            WriteDebug("TECHNOLOGIES", Color.DarkGreen);
            foreach (Technology t in technologies.Values)
                WriteDebug(t.ToString(), Color.Green);
            WriteDebug("FACTIONS", Color.DarkGreen);
            foreach (Faction f in factions.Values)
                WriteDebug(f.ToString(), Color.Green);
            WriteDebug("TILES", Color.DarkGreen);
            foreach (Tile t in tiles.Values)
                WriteDebug(t.ToString(), Color.Green);
        }

        public void SetDebug(DebugWindow d)
        {
            debug = true;
            debugWindow = d;
        }
        public void TurnOffDebug()
        {
            debug = false;
        }
        /// <summary>
        /// Sends a debug message in the specified color.
        /// Does nothing if debug == false.
        /// </summary>
        public void WriteDebug(String s, Color c)
        {
            if (debug)
                debugWindow.WriteLine(s, c);
        }
        /// <summary>
        /// Sends a debug message in the default color.
        /// Does nothing if debug == false.
        /// </summary>
        public void WriteDebug(String s)
        {
            if (debug)
                debugWindow.WriteLine(s);
        }

        private void ReadUnits(string fname)
        {
            unitTypes = new SortedDictionary<string, UnitType>();

            StreamReader r = new StreamReader(fname);
            int j = 0;
            string line;
            while ((line = r.ReadLine()) != null)
            {
                String[] attributes = line.Split(new char[] { ' ' });

                string name = attributes[0];
                int hitpoints = int.Parse(attributes[1]);
                int damage = int.Parse(attributes[2]);
                int damageType = int.Parse(attributes[3]);
                int lineOfSight = int.Parse(attributes[4]);
                double size = double.Parse(attributes[5]);
                int[] resistances = new int[GameInfo.DAMAGE_TYPES];
                for (int i = 0; i < GameInfo.DAMAGE_TYPES; i++)
                    resistances[i] = int.Parse(attributes[6 + i]);
                int trainingTime = int.Parse(attributes[6 + GameInfo.DAMAGE_TYPES]);
                double speed = double.Parse(attributes[7 + GameInfo.DAMAGE_TYPES]);
                string prerequisite = attributes[8 + GameInfo.DAMAGE_TYPES];
                bool canBuild = bool.Parse(attributes[9 + GameInfo.DAMAGE_TYPES]);
                double buildSpeed = double.Parse(attributes[10 + GameInfo.DAMAGE_TYPES]);
                bool aggressive = bool.Parse(attributes[11 + GameInfo.DAMAGE_TYPES]);
                int type = int.Parse(attributes[12 + GameInfo.DAMAGE_TYPES]);
                string imageName = attributes[13 + GameInfo.DAMAGE_TYPES];
                string iconName = attributes[14 + GameInfo.DAMAGE_TYPES];
                int range = int.Parse(attributes[15 + GameInfo.DAMAGE_TYPES]);
                int attackSpeedMilliseconds = int.Parse(attributes[16 + GameInfo.DAMAGE_TYPES]);
                int[] resourceCosts = new int[GameInfo.RESOURCE_TYPES];
                for (int i = 0; i < GameInfo.RESOURCE_TYPES; i++)
                    resourceCosts[i] = int.Parse(attributes[17 + GameInfo.DAMAGE_TYPES + i]);
                unitTypes.Add(name, new UnitType(name, hitpoints, damage, damageType, size, lineOfSight, resistances, trainingTime, speed, prerequisite, canBuild, buildSpeed,
                    aggressive, type, imageName, iconName, range, attackSpeedMilliseconds, resourceCosts));

                j++;
            }
            r.Close();
            WriteDebug("Read " + j + " unit definitions.", Color.Blue);
        }

        private void ReadBuildings(string fname)
        {
            buildingTypes = new SortedDictionary<string,BuildingType>();

            StreamReader r = new StreamReader(fname);
            int j = 0;
            string line;
            while ((line = r.ReadLine()) != null)
            {
                String[] attributes = line.Split(new char[] { ' ' });

                string name = attributes[0];
                int hitpoints = int.Parse(attributes[1]);
                int damage = int.Parse(attributes[2]);
                int damageType = int.Parse(attributes[3]);
                int lineOfSight = int.Parse(attributes[4]);
                int size = int.Parse(attributes[5]);
                int[] resistances = new int[GameInfo.DAMAGE_TYPES];
                for (int i = 0; i < GameInfo.DAMAGE_TYPES; i++)
                    resistances[i] = int.Parse(attributes[6 + i]);
                int buildTime = int.Parse(attributes[6 + GameInfo.DAMAGE_TYPES]);
                string prerequisite = attributes[7 + GameInfo.DAMAGE_TYPES];
                bool providesResource = bool.Parse(attributes[8 + GameInfo.DAMAGE_TYPES]);
                int resourceType = int.Parse(attributes[9 + GameInfo.DAMAGE_TYPES]);
                double gatherSpeed = double.Parse(attributes[10 + GameInfo.DAMAGE_TYPES]);
                bool builtOnResource = bool.Parse(attributes[11 + GameInfo.DAMAGE_TYPES]);
                int builtOnResourceType = int.Parse(attributes[12 + GameInfo.DAMAGE_TYPES]);
                bool aggressive = bool.Parse(attributes[13 + GameInfo.DAMAGE_TYPES]);
                string imageName = attributes[14 + GameInfo.DAMAGE_TYPES];
                string iconName = attributes[15 + GameInfo.DAMAGE_TYPES];
                int range = int.Parse(attributes[16 + GameInfo.DAMAGE_TYPES]);
                int attackSpeedMilliseconds = int.Parse(attributes[17 + GameInfo.DAMAGE_TYPES]);
                int[] resourceCosts = new int[GameInfo.RESOURCE_TYPES];
                for (int i = 0; i < GameInfo.RESOURCE_TYPES; i++)
                    resourceCosts[i] = int.Parse(attributes[18 + GameInfo.DAMAGE_TYPES + i]);
                string[] trainableUnits = new string[attributes.Length - 18 - DAMAGE_TYPES - RESOURCE_TYPES];
                for (int i = 0; i < trainableUnits.Length; i++)
                    trainableUnits[i] = attributes[18 + DAMAGE_TYPES + RESOURCE_TYPES + i];

                buildingTypes.Add(name, new BuildingType(name, hitpoints, damage, damageType, lineOfSight, size, resistances, buildTime, prerequisite, providesResource, resourceType, gatherSpeed,
                    builtOnResource, builtOnResourceType, aggressive, imageName, iconName, range, attackSpeedMilliseconds, resourceCosts, trainableUnits));
                j++;
            }
            r.Close();
            WriteDebug("Read " + j + " building definitions.", Color.Blue);
        }

        private void ReadResources(string fname)
        {
            resourceTypes = new SortedDictionary<string,ResourceType>();

            StreamReader r = new StreamReader(fname);
            int i = 0;
            string line;
            while ((line = r.ReadLine()) != null)
            {
                String[] attributes = line.Split(new char[] { ' ' });
                string name = attributes[0];
                int resourceType = int.Parse(attributes[1]);
                int resourceAmount = int.Parse(attributes[2]);
                string imageFilename = attributes[3];
                double gatherSpeed = double.Parse(attributes[4]);
                double size = double.Parse(attributes[5]);
                resourceTypes.Add(name, new ResourceType(name, resourceType, resourceAmount, imageFilename, gatherSpeed, size));
                i++;
            }
            r.Close();
            WriteDebug("Read " + i + " resource definitions.", Color.Blue);
        }

        private void ReadTechnologies(string fname)
        {
            technologies = new SortedDictionary<string,Technology>();
            // Each technology is in a set of lines, ended with a line that has only a ~ in it
            // A technology is formatted:
            // name
            // prereq1...prereqX
            // resourcecost1...resourcecostX
            // effect1
            // effect2
            // effectX
            StreamReader r = new StreamReader(fname);
            int j = 0;
            string line;
            string name = "";
            HashSet<string> prerequisites = new HashSet<string>();
            HashSet<String> effects = new HashSet<string>();
            int[] resourceCosts = new int[GameInfo.RESOURCE_TYPES];
            int currentLine = 0;
            while ((line = r.ReadLine()) != null)
            {
                if (line.Equals("~"))
                {
                    technologies.Add(name, new Technology(name, prerequisites, effects, resourceCosts));
                    currentLine = -1;
                    prerequisites = new HashSet<string>();
                    effects = new HashSet<String>();
                }
                else if (currentLine == 0) { name = line; j++; }
                else if (currentLine == 1)
                {
                    string[] split = line.Split(new char[] { ' ' });
                    for (int i = 0; i < split.Length; i++)
                        prerequisites.Add(split[i]);
                }
                else if (currentLine == 2)
                {
                    string[] split = line.Split(new char[] { ' ' });
                    for (int i = 0; i < GameInfo.RESOURCE_TYPES; i++)
                        resourceCosts[i] = int.Parse(split[i]);
                }
                else
                    effects.Add(line);

                currentLine++;
            }
            r.Close();
            WriteDebug("Read " + j + " technology definitions.", Color.Blue);
        }

        private void ReadFactions(string fname)
        {
            factions = new SortedDictionary<string,Faction>();
            // Again, different factions are seperated by a sole "~" on a line
            StreamReader r = new StreamReader(fname);
            int i = 0;
            string line;
            string name = "";
            HashSet<String> effects = new HashSet<string>();
            int currentLine = 0;
            while ((line = r.ReadLine()) != null)
            {
                if (line.Equals("~"))
                {
                    factions.Add(name,new Faction(name, effects));
                    currentLine = -1;
                    effects = new HashSet<String>();
                }
                else if (currentLine == 0) { name = line; i++; }
                else effects.Add(line);

                currentLine++;
            }
            r.Close();
            WriteDebug("Read " + i + " faction definitions.", Color.Blue);
        }

        private void ReadTiles(string fname)
        {
            tiles = new SortedDictionary<string,Tile>();

            StreamReader r = new StreamReader(fname);
            int i = 0;
            string line;
            while ((line = r.ReadLine()) != null)
            {
                string[] split = line.Split(new char[] { ' ' });
                string name = split[0];
                string imageName = split[1];
                bool buildable = bool.Parse(split[2]);
                bool[] unitTypePassable = new bool[GameInfo.UNIT_TYPES];
                for (int j = 0; j < GameInfo.UNIT_TYPES; j++)
                    unitTypePassable[j] = bool.Parse(split[3 + j]);

                tiles.Add(name, new Tile(name, imageName, buildable, unitTypePassable));
                i++;
            }
            r.Close();
            WriteDebug("Read " + i + " tile definitions.", Color.Blue);
        }
    }
}
