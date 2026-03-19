using System;
using System.Collections.Generic;
using System.Configuration;
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

        //private static string mapName = "Equilibrium513AIE.SC2Map";
        private static string mapName = "IncorporealAIE.SC2Map";
        //private static string mapName = "GoldenAura513AIE.SC2Map";
        //private static string mapName = "Gresvan513AIE.SC2Map";
        //private static string mapName = "HardLead513AIE.SC2Map";
        //private static string mapName = "GoldenAura513AIE.SC2Map";




        //remove comment for multiplayer
        //private static readonly string mapName = "JagannathaAIE";

        private static readonly Race opponentRace = Race.Random;
        private static readonly Difficulty opponentDifficulty = Difficulty.Easy;

        public static GameConnection gc;

        private static bool realtime = false;
        private static bool randomMap = true;

        private static string GetRandomMap() 
        {
            List<string> maps = new List<string>();
            maps.Add("LeyLinesAIE.SC2Map");
            maps.Add("MagannathaAIE.SC2Map");
            maps.Add("PylonAIE.SC2Map");
            maps.Add("TorchesAIE.SC2Map");
            maps.Add("UltraloveAIE.SC2Map");
            maps.Add("LeyLinesAIE.SC2Map");
            maps.Add("LeyLinesAIE.SC2Map");
            Random random = new Random();
            int mapNumber = random.Next(0, maps.Count);
            return maps[mapNumber];
        }

        private static async Task Main(string[] args) {
            try {
                gc = new GameConnection();
                if (args.Length == 0){
                     gc.readSettings();
                    if (randomMap) { mapName = GetRandomMap(); }
                    //mapName = "UltraloveAIE.SC2Map"; 
                   await gc.RunDocker(bot, mapName, race, opponentRace, opponentDifficulty, realtime); 
                }
                else
                    await gc.RunLadder(bot, race, args);
            }
            catch (Exception ex) {
                Logger.Info(ex.ToString());
            }

            Logger.Info("Terminated.");
        }

    }
}