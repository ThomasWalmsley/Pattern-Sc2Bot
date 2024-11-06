using System;
using SC2APIProtocol;

namespace Bot {
    internal class Program {
        // Settings for your bot.
        private static readonly Bot bot = new Pattern();
        private const Race race = Race.Terran;

        // Settings for single player mode.
       //private static string mapName = "AbyssalReefLE.SC2Map";
        //private static string mapName = "AbiogenesisLE.SC2Map";

       private static string mapName = "JagannathaAIE.SC2Map";
       //private static string mapName = "BerlingradAIE.SC2Map";
       //private static string mapName = "DeathAura506.SC2Map";
       //private static string mapName = "EverDream506.SC2Map";
       //private static string mapName = "GlitteringAshesAIE.SC2Map";
       //private static string mapName = "GoldenWall506.SC2Map";
       //private static string mapName = "EternalEmpire506.SC2Map";

        //remove comment for multiplayer
        //private static readonly string mapName = "JagannathaAIE";

        private static readonly Race opponentRace = Race.Random;
        private static readonly Difficulty opponentDifficulty = Difficulty.Easy;

        public static GameConnection gc;

        private static bool realtime = true;

        private static void Main(string[] args) {
            try {
                gc = new GameConnection();
                if (args.Length == 0){
                    gc.readSettings();
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