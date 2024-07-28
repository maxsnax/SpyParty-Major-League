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

    public class XmlUpdater {
        XmlDocument xmlDoc;

        public XmlUpdater(XmlDocument xmlDoc) {
            System.Diagnostics.Debug.WriteLine($"Creating an insance of XmlUpdater({xmlDoc.Name})");

            this.xmlDoc = xmlDoc;
        }

        public XmlDocument XmlDoc { get => xmlDoc; set => xmlDoc = value; }

        public void UpdatePlayerStatistics(XmlNode playerNode, Player player) {
            System.Diagnostics.Debug.WriteLine($"UpdatePlayerStatistics: {player.Name}");
            XmlNode resultNode = null;
            if (player.MatchResult == "Win") { resultNode = playerNode.SelectSingleNode("MatchWins"); }
            else if (player.MatchResult == "Tie") { resultNode = playerNode.SelectSingleNode("MatchTies"); }
            else if (player.MatchResult == "Loss") { resultNode = playerNode.SelectSingleNode("MatchLosses"); }
            else { } // Error occurred since there is no proper result for the match
            resultNode.InnerText = (int.Parse(resultNode.InnerText) + 1).ToString();

            System.Diagnostics.Debug.WriteLine($"Calling UpdateRoleStatistics(sniperNode, {player.Name}.Sniper)");
            UpdateRoleStatistics(playerNode, player.Sniper);

            System.Diagnostics.Debug.WriteLine($"Calling UpdatePlayerStatistics(sniperNode, {player.Name}.Spy)");
            UpdateRoleStatistics(playerNode, player.Spy);

            xmlDoc.Save(xmlFilePath);
        }

        public void UpdateRoleStatistics(XmlNode playerNode, Role playerRole) {
            System.Diagnostics.Debug.WriteLine($"UpdateRoleStatistics: {playerNode.Name} - {playerRole.RoleName}");

            Dictionary<string, int> roleStats = new Dictionary<string, int> {
                { "TotalGames", playerRole.Wins + playerRole.Losses },
                { "Wins", playerRole.Wins },
                { "Losses", playerRole.Losses },
                { "CivilianShot", playerRole.CivilianShot },
                { "SpyShot", playerRole.SpyShot },
                { "Timeout", playerRole.Timeout },
                { "MissionWin", playerRole.MissionWin }
            };

            foreach (var stat in roleStats) {
                System.Diagnostics.Debug.WriteLine($"UpdateRoleStatistics: {playerRole.RoleName} - {stat}");

                XmlNode statNode = playerNode.SelectSingleNode($"{playerRole.RoleName}/{stat.Key}");
                statNode.InnerText = (int.Parse(statNode.InnerText) + stat.Value).ToString();
            }

            xmlDoc.Save(xmlFilePath);
        }

        public void AddPlayerNode(string name) {
            System.Diagnostics.Debug.WriteLine($"AddPlayerNode for {name}");

            XmlNode playersRoot = xmlDoc.SelectSingleNode("/PlayerData/Players");

            XmlElement playerElement = xmlDoc.CreateElement("Name");
            playersRoot.AppendChild(playerElement);

            Append("PlayerName", playerElement, name);
            XmlElement seasonElement = Append("Season", playerElement);

            Append("Name", seasonElement, "Test");
            Append("MatchWins", seasonElement, "0");
            Append("MatchTies", seasonElement, "0");
            Append("MatchLosses", seasonElement, "0");

            XmlElement sniperElement = Append("Sniper", seasonElement);
            XmlElement spyElement = Append("Spy", seasonElement);
            string[] childElementNames = { "TotalGames", "Wins", "Losses", "CivilianShot", "SpyShot", "Timeout", "MissionWin" };

            foreach (string elementName in childElementNames) {
                Append(elementName, sniperElement, "0");
                Append(elementName, spyElement, "0");
            }

            xmlDoc.Save(xmlFilePath);
        }

        private XmlElement Append(string elementName, XmlElement parent, string innerText = "") {
            XmlElement element = xmlDoc.CreateElement(elementName);
            if (!string.IsNullOrEmpty(innerText)) element.InnerText = innerText;
            parent.AppendChild(element);
            return element; // Return the created element
        }
    }

}
