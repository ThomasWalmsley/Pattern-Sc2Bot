using System;
using System.Collections.Generic;
using SC2APIProtocol;

namespace Bot {
    internal class Program {
        // Settings for your bot.
        private static readonly Bot bot = new Pattern();
        private const Race race = Race.Terran;

        // Settings for single player mode.
        //private static string mapName = "AbyssalReefLE.SC2Map";
        //private static string mapName = "AbiogenesisLE.SC2Map";

        //private static string mapName = "JagannathaAIE.SC2Map";
        //private static string mapName = "BerlingradAIE.SC2Map";
        //private static string mapName = "DeathAura506.SC2Map";
        //private static string mapName = "EverDream506.SC2Map";
        //private static string mapName = "GlitteringAshesAIE.SC2Map";
        //private static string mapName = "GoldenWall506.SC2Map";
        //private static string mapName = "EternalEmpire506.SC2Map";

        private static string mapName = "Equilibrium513AIE.SC2Map";
        //private static string mapName = "GoldenAura513AIE.SC2Map";
        //private static string mapName = "Gresvan513AIE.SC2Map";
        //private static string mapName = "HardLead513AIE.SC2Map";
        //private static string mapName = "GoldenAura513AIE.SC2Map";




        //remove comment for multiplayer
        //private static readonly string mapName = "JagannathaAIE";

        private static readonly Race opponentRace = Race.Random;
        private static readonly Difficulty opponentDifficulty = Difficulty.Easy;

        public static GameConnection gc;

        private static bool realtime = true;
        private static bool randomMap = true;

        private static string GetRandomMap() 
        {
            List<string> maps = new List<string>();
            maps.Add("Equilibrium513AIE.SC2Map");
            maps.Add("GoldenAura513AIE.SC2Map");
            maps.Add("Gresvan513AIE.SC2Map");
            maps.Add("HardLead513AIE.SC2Map");
            maps.Add("Oceanborn513AIE.SC2Map");
            maps.Add("SiteDelta513AIE.SC2Map");
            Random random = new Random();
            int mapNumber = random.Next(0, maps.Count);
            return maps[mapNumber];
        }


        private static void Main(string[] args) {
            try {
                gc = new GameConnection();
                if (args.Length == 0){
                    gc.readSettings();
                    if (randomMap) { mapName = GetRandomMap(); }
                    gc.RunSinglePlayer(bot, mapName, race, opponentRace, opponentDifficulty, realtime).Wait();
                }
                else
                    gc.RunLadder(bot, race, args).Wait();
            }
            catch (Exception ex) {
                Logger.Info(ex.ToString());
            }

            Logger.Info("Terminated.");
        }

    }
}