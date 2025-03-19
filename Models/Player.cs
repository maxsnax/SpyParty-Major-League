using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using static SML.Models.Replays;

namespace SML.Models {
    public class Player {
        public int PlayerID { get; set; } = 0;
        public string Username { get; set; } = null;
        public string Name { get; set; } = null;
        public int Forfeit { get; set; } = 0;
        public int Season { get; set; } = 0;
        public int Division { get; set; } = 0;
        public int Wins { get; set; } = 0;
        public int Losses { get; set; } = 0;
        public int Ties { get; set; } = 0;
        public int Points => (Wins * 2) + Ties;
        public Results Results { get; set; } = new Results();

        public Player() { }

        public Player(string name) {
            Name = name;
        }

        public void UpdatePlayerInfo(Player SQL) {
            PlayerID = SQL.PlayerID != 0 ? SQL.PlayerID : PlayerID;
            Name = SQL.Name ?? Name;
            Forfeit = SQL.Forfeit != 0 ? SQL.Forfeit : Forfeit;
            Season = SQL.Season != 0 ? SQL.Season : Season;
            Division = SQL.Division != 0 ? SQL.Division : Division;
            Username = SQL.Username ?? Username;
        }

        public override string ToString() {
            return $"PlayerID: {PlayerID}, " +
                   $"Username: {Username ?? "N/A"}, " +
                   $"Name: {Name ?? "N/A"}, " +
                   $"Forfeit: {Forfeit}, " +
                   $"Season: {Season}, " +
                   $"Division: {Division}, " +
                   $"Wins: {Wins}, " +
                   $"Losses: {Losses}, " +
                   $"Ties: {Ties}, " +
                   $"Points: {Points}, " +
                   $"Results: {Results}";
        }


    }

}
