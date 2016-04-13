using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
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
        public static string TECHNOLOGY_IMAGE_BASE { get; internal set; } = BASE_DIRECTORY + "Images\\Technologies\\";

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
        public static string BACKGROUND_IMAGE_MINIMAP { get; private set; } = "minimap.png";

        // For borders of areas in the GUI
        public static string BORDER_IMAGE_BASE { get; private set; } = BASE_DIRECTORY + "Images\\Borders\\";
        public static string BORDER_MINIMAP_LEFT { get; private set; } = BORDER_IMAGE_BASE + "minimapleft.png";
        public static string BORDER_MINIMAP_TOP { get; private set; } = BORDER_IMAGE_BASE + "minimaptop.png";
        public static string BORDER_RESOURCE_AREA_TOP { get; private set; } = BORDER_IMAGE_BASE + "resourceareatop.png";
        public static string BORDER_RIGHT_AREA_LEFT { get; private set; } = BORDER_IMAGE_BASE + "rightarealeft.png";

        // Used in the place of an entity image or technology icon if the right one can't be loaded
        public static string DEFAULT_IMAGE_NAME { get; private set; } = BASE_DIRECTORY + "Images\\defaultimage.png";

        public const string TITLE_FONT_NAME = "Arial";
        public const string FONT_NAME = "Arial";

        // A factor of how red reddened buildings are
        public const double RED_SHIFT = 50;

        // In milliseconds
        public const int TICK_TIME = 30;
        public const int WINDOW_REFRESH_TIME = 20;
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
        // What colours each player has
        public static Color[] PLAYER_COLORS { get; private set; } = new Color[3] { Color.White, Color.Blue, Color.Red };
        public static Pen[] PLAYER_PENS { get; private set; } = new Pen[3] { Pens.White, Pens.Blue, Pens.Red };

        // Arbitrary; the port used by the server-to-client connection
        public const int STC_PORT = 4983;
        // Arbitrary; the port used by the client-to-server connection
        public const int CTS_PORT = 4984;
        // Arbitrary; the port that the lobby host listens on
        public const int LOBBY_PORT = 4985;
        // Arbitrary; the port that the lobby clients listen on
        public const int LOBBY_CLIENT_PORT = 4986;

        // Default values, read directly from files
        private static SortedDictionary<string, UnitType> baseUnitTypes;
        private static SortedDictionary<string, BuildingType> baseBuildingTypes;
        private static SortedDictionary<string, ResourceType> baseResourceTypes;
        private static SortedDictionary<string, Technology> baseTechnologies;

        public static SortedDictionary<string, Faction> factions { get; private set; }
        public static SortedDictionary<string, Tile> tiles { get; private set; }

        // Values specific to this instance
        public SortedDictionary<string, UnitType> unitTypes { get; private set; }
        public SortedDictionary<string, BuildingType> buildingTypes { get; private set; }
        public SortedDictionary<string, ResourceType> resourceTypes { get; private set; }
        public SortedDictionary<string, Technology> technologies { get; private set; }

        public GameInfo()
        {
            // Clone all definitions from the static ones
            unitTypes = new SortedDictionary<string, UnitType>();
            buildingTypes = new SortedDictionary<string, BuildingType>();
            resourceTypes = new SortedDictionary<string, ResourceType>();
            technologies = new SortedDictionary<string, Technology>();

            foreach (KeyValuePair<string, UnitType> u in baseUnitTypes)
                unitTypes.Add(u.Key, (UnitType)u.Value.Clone());
            foreach (KeyValuePair<string, BuildingType> b in baseBuildingTypes)
                buildingTypes.Add(b.Key, (BuildingType)b.Value.Clone());
            foreach (KeyValuePair<string, ResourceType> r in baseResourceTypes)
                resourceTypes.Add(r.Key, (ResourceType) r.Value.Clone());
            foreach (KeyValuePair<string, Technology> t in baseTechnologies)
                technologies.Add(t.Key, (Technology)t.Value.Clone());

        }

        /// <summary>
        /// Reads all definitions from the files and sets them to be used
        /// by all GameInfos created in the future.
        /// </summary>
        public static void ReadDefinitions()
        {
            ReadUnits(UNIT_FNAME);
            ReadBuildings(BUILDING_FNAME);
            ReadResources(RESOURCE_FNAME);
            ReadTechnologies(TECHNOLOGY_FNAME);
            ReadFactions(FACTION_FNAME);
            ReadTiles(TILE_FNAME);
        }

        private static void ReadUnits(string fname)
        {
            baseUnitTypes = new SortedDictionary<string, UnitType>();

            StreamReader r = new StreamReader(fname);
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
                bool aggressive = bool.Parse(attributes[9 + GameInfo.DAMAGE_TYPES]);
                int type = int.Parse(attributes[10 + GameInfo.DAMAGE_TYPES]);
                string imageName = attributes[11 + GameInfo.DAMAGE_TYPES];
                string iconName = attributes[12 + GameInfo.DAMAGE_TYPES];
                int range = int.Parse(attributes[13 + GameInfo.DAMAGE_TYPES]);
                int attackSpeedMilliseconds = int.Parse(attributes[14 + GameInfo.DAMAGE_TYPES]);
                int[] resourceCosts = new int[GameInfo.RESOURCE_TYPES];
                for (int i = 0; i < GameInfo.RESOURCE_TYPES; i++)
                    resourceCosts[i] = int.Parse(attributes[15 + GameInfo.DAMAGE_TYPES + i]);
                StringBuilder descriptionBuilder = new StringBuilder();
                for (int i = 15 + GameInfo.DAMAGE_TYPES + GameInfo.RESOURCE_TYPES; i < attributes.Length; i++)
                    descriptionBuilder.Append(attributes[i] + " ");
                baseUnitTypes.Add(name, new UnitType(name, hitpoints, damage, damageType, size, lineOfSight, resistances, trainingTime, speed, prerequisite, 
                    aggressive, type, imageName, iconName, range, attackSpeedMilliseconds, resourceCosts, descriptionBuilder.ToString()));
            }
            r.Close();
        }

        private static void ReadBuildings(string fname)
        {
            baseBuildingTypes = new SortedDictionary<string,BuildingType>();

            StreamReader r = new StreamReader(fname);
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
                string[] trainableUnits = new string[int.Parse(attributes[18+GameInfo.DAMAGE_TYPES+GameInfo.RESOURCE_TYPES])];
                for (int i = 0; i < trainableUnits.Length; i++)
                    trainableUnits[i] = attributes[19 + DAMAGE_TYPES + RESOURCE_TYPES + i];
                StringBuilder descriptionBuilder = new StringBuilder();
                for (int i = 19 + DAMAGE_TYPES + RESOURCE_TYPES + trainableUnits.Length; i < attributes.Length; i++)
                    descriptionBuilder.Append(attributes[i] + " ");

                baseBuildingTypes.Add(name, new BuildingType(name, hitpoints, damage, damageType, lineOfSight, size, resistances, buildTime, prerequisite, providesResource, resourceType, gatherSpeed,
                    builtOnResource, builtOnResourceType, aggressive, imageName, iconName, range, attackSpeedMilliseconds, resourceCosts, trainableUnits, descriptionBuilder.ToString()));
            }
            r.Close();
        }

        private static void ReadResources(string fname)
        {
            baseResourceTypes = new SortedDictionary<string,ResourceType>();

            StreamReader r = new StreamReader(fname);
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
                StringBuilder descriptionBuilder = new StringBuilder();
                for (int i = 6; i < attributes.Length; i++)
                    descriptionBuilder.Append(attributes[i] + " ");
                baseResourceTypes.Add(name, new ResourceType(name, resourceType, resourceAmount, imageFilename, gatherSpeed, size, descriptionBuilder.ToString()));
            }
            r.Close();
        }

        private static void ReadTechnologies(string fname)
        {
            baseTechnologies = new SortedDictionary<string,Technology>();
            // Each technology is in a set of lines, ended with a line that has only a ~ in it
            // A technology is formatted:
            // [name]
            // [prerequisite 1 name] .. [prerequisite x name]
            // [clashing technology 1 name] .. [clashing technology x name]
            // [resource cost 1] .. [resource cost x]
            // [effect 1]
            // [effect 2]
            // [effect x]
            // ~
            StreamReader r = new StreamReader(fname);

            // Define all the variables we need to create a technology
            string line;
            string name = "";
            string description = "";
            string iconName = "";
            List<string> prerequisites = new List<string>();
            List<string> clashing = new List<string>();
            List<string> effects = new List<string>();

            int[] resourceCosts = new int[GameInfo.RESOURCE_TYPES];
            int currentLine = 0;

            while ((line = r.ReadLine()) != null)
            {
                // If we're on the last line of a technology, create that technology and reset the necessary variables for the next one
                if (line.Equals("~"))
                {
                    baseTechnologies.Add(name, new Technology(name, prerequisites, clashing, effects, resourceCosts, iconName, description));
                    currentLine = -1;
                    prerequisites = new List<string>();
                    effects = new List<string>();
                }
                // Otherwise process the line according to the structure defined above
                else if (currentLine == 0) name = line;
                else if (currentLine == 1) iconName = line;
                else if (currentLine == 2)
                {
                    string[] split = line.Split(new char[] { ' ' });
                    for (int i = 0; i < split.Length; i++)
                        if(split[i].Length > 0) prerequisites.Add(split[i]);
                }
                else if (currentLine == 3)
                {
                    string[] split = line.Split(new char[] { ' ' });
                    for (int i = 0; i < split.Length; i++)
                        if (split[i].Length > 0) clashing.Add(split[i]);
                }
                else if (currentLine == 4)
                {
                    string[] split = line.Split(new char[] { ' ' });
                    for (int i = 0; i < GameInfo.RESOURCE_TYPES; i++)
                        resourceCosts[i] = int.Parse(split[i]);
                }
                else if (currentLine == 5)
                    description = line;
                else
                    effects.Add(line);

                currentLine++;
            }
            r.Close();
        }

        private static void ReadFactions(string fname)
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
        }

        private static void ReadTiles(string fname)
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
                int[] colorRGB = new int[3];
                for (int j = 0; j < 3; j++)
                    colorRGB[j] = int.Parse(split[3 + GameInfo.UNIT_TYPES + j]);

                tiles.Add(name, new Tile(name, imageName, buildable, unitTypePassable, colorRGB));
                i++;
            }
            r.Close();
        }
    }
}
