using Python.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using WebGrease.Css.Ast;
using static SML.Util;
using static SML.Replays;
using System.Threading;
using static SML.XMLUpdater;
using Microsoft.Ajax.Utilities;

namespace SML {
    public class XMLUpdater {
        public XMLUpdater() {
            System.Diagnostics.Debug.WriteLine($"Creating an instance of Stats()");
        }

        // =====================================================================================
        //  Classes of Role and Player used for gathering class Replay data
        // =====================================================================================
        public class Role {
            public string RoleName { get; set; }
            public int TotalGames { get; set; }
            public int Wins { get; set; }
            public int Losses { get; set; }
            public int CivilianShot { get; set; }
            public int SpyShot { get; set; }
            public int Timeout { get; set; }
            public int MissionWin { get; set; }

            public Role(string RoleName) {
                System.Diagnostics.Debug.WriteLine($"Creating an instance of Role({RoleName})");

                this.RoleName = RoleName;
                // Initialize all properties to 0
                TotalGames = 0;
                Wins = 0;
                Losses = 0;
                CivilianShot = 0;
                SpyShot = 0;
                Timeout = 0;
                MissionWin = 0;
            }
        }

        public class Player {
            public string Name { get; set; }
            public string Role { get; set; }
            public int TotalGames { get; set; }
            public Role Sniper { get; set; }
            public Role Spy { get; set; }

            public Player(string name) {
                System.Diagnostics.Debug.WriteLine($"Creating an instance of Player({name})");

                // Initialize to an empty string
                Name = name;

                // Initialize to new Role objects
                Sniper = new Role("Sniper");
                Spy = new Role("Spy");

            }

            public string MatchResult {
                get {
                    if (GameWins > GameLosses) { return "Win"; }
                    else if (GameWins == GameLosses) { return "Tie"; }
                    else if (GameWins < GameLosses) { return "Loss"; }
                    else { return "No Match Result"; }
                }
            }

            public int GameWins {
                get {
                    return Sniper.Wins + Spy.Wins;
                }
            }

            public int GameLosses {
                get {
                    return Sniper.Losses + Spy.Losses;
                }
            }

        }

        public static string xmlFilePath = "C:\\Users\\Max\\source\\repos\\SML\\App_Data\\PlayerData.xml";
        public static string seasonName = "Test";

        public ReplayData CollectGameData(string filePath) {
            System.Diagnostics.Debug.WriteLine($"CollectGameData for {filePath}");

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFilePath);

            ReplayData replay = Replays.ReadFile(filePath);

            string spy = scrubName(replay.spy_displayname);
            string sniper = scrubName(replay.sniper_displayname);

            // Make sure that the correct nodes exist in the XML file
            try {
                string spyXPath = ($"/PlayerData/Players/Name[PlayerName='{spy}']/Season[Name='Test']");
                string sniperXPath = ($"/PlayerData/Players/Name[PlayerName='{sniper}']/Season[Name='Test']");

                XmlNode spyNode = xmlDoc.SelectSingleNode(spyXPath);
                XmlNode sniperNode = xmlDoc.SelectSingleNode(sniperXPath);

                if (spyNode == null) {
                    XmlUpdater xmlUpdate = new XmlUpdater(xmlDoc);
                    xmlUpdate.AddPlayerNode(spy);
                    spyNode = xmlDoc.SelectSingleNode(spyXPath);
                }
                if (sniperNode == null) {
                    XmlUpdater xmlUpdate = new XmlUpdater(xmlDoc);
                    xmlUpdate.AddPlayerNode(sniper);
                    sniperNode = xmlDoc.SelectSingleNode(sniperXPath);
                }

                // Continue with processing the nodes
            }
            catch (Exception e) {
                // Handle the exception appropriately (e.g., log it, show a message, etc.)
                System.Diagnostics.Debug.WriteLine($"Exception: {e.Message}");
            }

            return replay;
        }


        private static void SetPlayerStatistics(Player player) {
            System.Diagnostics.Debug.WriteLine($"SetPlayerStatistics for {player.Name}");

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFilePath);
            player.Name = scrubName(player.Name);

            if (xmlDoc == null) { System.Diagnostics.Debug.WriteLine($"{xmlFilePath} xmlDoc is null"); return; }

            string xpathPlayer = $"/PlayerData/Players/Name[PlayerName='{player.Name}']/Season[Name='Test']";
            XmlNode playerNode = xmlDoc.SelectSingleNode(xpathPlayer);
            if (playerNode == null) { System.Diagnostics.Debug.WriteLine($"{player.Name} Node is null"); return; }

            System.Diagnostics.Debug.WriteLine($"Creating XmlUpdater");
            XmlUpdater updater = new XmlUpdater(xmlDoc);

            System.Diagnostics.Debug.WriteLine($"Calling UpdatePlayerStatistics(sniperNode, {player.Name}.Sniper)");
            updater.UpdatePlayerStatistics(playerNode, player);

            System.Diagnostics.Debug.WriteLine($"Saving {xmlFilePath}");
            xmlDoc.Save(xmlFilePath);

        }

        // Add in Venue counts for Spy/Sniper wins
        public void CollectMatchData(string directoryPath) {
            System.Diagnostics.Debug.WriteLine($"CollectMatchData for {directoryPath}");

            string[] files = Directory.GetFiles(directoryPath);
            ReplayData replay = CollectGameData(files[0]);

            Player playerOne = new Player(replay.spy_displayname);
            Player playerTwo = new Player(replay.sniper_displayname);

            foreach (string file in files) {
                replay = CollectGameData(file);
                string spyName = replay.spy_displayname;
                string sniperName = replay.sniper_displayname;
                Player spy = null;
                Player sniper = null;

                if (playerOne.Name == spyName && playerTwo.Name == sniperName) {
                    spy = playerOne;
                    sniper = playerTwo;
                } else if (playerOne.Name == sniperName && playerTwo.Name == spyName) {
                    sniper = playerOne;
                    spy = playerTwo;
                } else {
                    // The player names are different from the first initial replay and so this set of matches is invalid.
                }

                // when result is mission win or civ shot then spy wins, else sniper wins
                AddResultsToPlayers(spy, sniper, replay);
            }

            SetPlayerStatistics(playerOne);
            SetPlayerStatistics(playerTwo);
        }

        private void AddResultsToPlayers(Player spy, Player sniper, ReplayData replay) {
            System.Diagnostics.Debug.WriteLine($"AddResultsToPlayers for {spy.Name} and {sniper.Name}");

            switch (replay.result) {
                case "Civilian Shot":
                    spy.Spy.CivilianShot++;
                    spy.Spy.Wins++;
                    sniper.Sniper.CivilianShot++;
                    sniper.Sniper.Losses++;
                    break;
                case "Spy Shot":
                    spy.Spy.SpyShot++;
                    spy.Spy.Losses++;
                    sniper.Sniper.SpyShot++;
                    sniper.Sniper.Wins++;
                    break;
                case "Timeout":
                    spy.Spy.Timeout++;
                    spy.Spy.Losses++;
                    sniper.Sniper.Timeout++;
                    sniper.Sniper.Wins++;
                    break;
                case "Missions Win":
                    spy.Spy.MissionWin++;
                    spy.Spy.Wins++;
                    sniper.Sniper.MissionWin++;
                    sniper.Sniper.Losses++;
                    break;
                default:
                    // Must be some kind of mistake in parsing the replay if we get to default. Do something
                    break;
            }

        }

        public void CollectWeekData(string week) {

        }

        public void CollectSeasonData(string seasonName) {

        }

        public void CollectPlayerData(string playerName) {

        }
    }
}
