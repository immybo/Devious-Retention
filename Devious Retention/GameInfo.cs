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
        public static string RESOURCE_FILENAME_BASE { get; internal set; } = BASE_DIRECTORY + "Images\\Resources\\";

        // In milliseconds
        public const int TICK_TIME = 100;
        // Melee, ranged, bombard
        public const int DAMAGE_TYPES  = 3;
        // Metal, oil, energy, science
        public const int RESOURCE_TYPES = 4;

        public List<UnitType> unitTypes { get; internal set; }
        public List<BuildingType> buildingTypes { get; internal set; }
        public List<ResourceType> resourceTypes { get; internal set; }
        public List<Technology> technologies { get; internal set; }
        public List<Faction> factions { get; internal set; }

        // Whether or not debug messages will be sent
        private bool debug = false;
        // The debug window; only used if debug == true
        private DebugWindow debugWindow = null;

        public GameInfo(string unitfname, string buildingfname, string resourcefname, string technologyfname, string factionfname)
        {
            readUnits(unitfname);
            readBuildings(buildingfname);
            readResources(resourcefname);
            readTechnologies(technologyfname);
            readFactions(factionfname);
        }

        /// <summary>
        /// Writes all currently known definitions to the debug window (if the debug window is active).
        /// </summary>
        public void WriteDefinitionsToDebug()
        {
            if (!debug) return;

            WriteDebug("UNIT TYPES", Color.DarkGreen);
            foreach (UnitType u in unitTypes)
                WriteDebug(u.ToString(), Color.Green);
            WriteDebug("BUILDING TYPES", Color.DarkGreen);
            foreach (BuildingType b in buildingTypes)
                WriteDebug(b.ToString(), Color.Green);
            WriteDebug("RESOURCE TYPES", Color.DarkGreen);
            foreach (ResourceType r in resourceTypes)
                WriteDebug(r.ToString(), Color.Green);
            WriteDebug("TECHNOLOGIES", Color.DarkGreen);
            foreach (Technology t in technologies)
                WriteDebug(t.ToString(), Color.Green);
            WriteDebug("FACTIONS", Color.DarkGreen);
            foreach (Faction f in factions)
                WriteDebug(f.ToString(), Color.Green);
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

        private void readUnits(string fname)
        {
            unitTypes = new List<UnitType>();
            try {
                StreamReader r = new StreamReader(fname);
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    unitTypes.Add(new UnitType(line));
                }
                r.Close();
            }
            catch(IOException e)
            {
                Console.WriteLine("Couldn't properly read from unit file "+fname + ". " + e);
            }
        }

        private void readBuildings(string fname)
        {
            buildingTypes = new List<BuildingType>();
            try
            {
                StreamReader r = new StreamReader(fname);
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    buildingTypes.Add(new BuildingType(line));
                }
                r.Close();
            }
            catch (IOException e)
            {
                Console.WriteLine("Couldn't properly read from building file "+fname + ". " + e);
            }
        }

        private void readResources(string fname)
        {
            resourceTypes = new List<ResourceType>();
            try
            {
                StreamReader r = new StreamReader(fname);
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    resourceTypes.Add(new ResourceType(line));
                }
                r.Close();
            }
            catch (IOException e)
            {
                Console.WriteLine("Couldn't properly read from unit file. " + e);
            }
        }

        private void readTechnologies(string fname)
        {
            technologies = new List<Technology>();
            // Each technology is in a set of lines, ended with a line that has only a ~ in it
            // A technology is formatted:
            // name
            // prereq1...prereqX
            // resourcecost1...resourcecostX
            // effect1
            // effect2
            // effectX
            try
            {
                StreamReader r = new StreamReader(fname);
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
                        technologies.Add(new Technology(name, prerequisites, effects, resourceCosts));
                        currentLine = -1;
                        prerequisites = new HashSet<string>();
                        effects = new HashSet<String>();
                    }
                    else if (currentLine == 0) name = line;
                    else if (currentLine == 1)
                    {
                        string[] split = line.Split(new char[] { ' ' });
                        for (int i = 0; i < split.Length; i++)
                            prerequisites.Add(split[i]);
                    }
                    else if(currentLine == 2)
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
            }
            catch (IOException e)
            {
                Console.WriteLine("Couldn't properly read from technology file. " + e);
            }
        }

        private void readFactions(string fname)
        {
            factions = new List<Faction>();
            // Again, different factions are seperated by a sole "~" on a line
            try
            {
                StreamReader r = new StreamReader(fname);
                string line;
                string name = "";
                HashSet<String> effects = new HashSet<string>();
                int currentLine = 0;
                while ((line = r.ReadLine()) != null)
                {
                    if (line.Equals("~"))
                    {
                        factions.Add(new Faction(name, effects));
                        currentLine = -1;
                        effects = new HashSet<String>();
                    }
                    else if (currentLine == 0) name = line;
                    else effects.Add(line);

                    currentLine++;
                }
                r.Close();
            }
            catch (IOException e)
            {
                Console.WriteLine("Couldn't properly read from faction file. " + e);
            }
        }
    }
}
