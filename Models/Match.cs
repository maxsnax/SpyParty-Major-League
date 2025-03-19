using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static SML.Models.Replays;

namespace SML.Models {
    public class Match {
        public Player PlayerOne { get; set; }
        public Player PlayerTwo { get; set; }
        public List<ReplayData> Replays { get; set; }
        public int ID { get; set; } = 0;
        public int SeasonID { get; set; } = 0;
        public int DivisionID { get; set; } = 0;
        public int Winner { get; set; } = -1;
        public int Forfeit => PlayerOne.Forfeit + PlayerTwo.Forfeit;


        public Match() {
            Replays = new List<ReplayData>();
        }


        public Match(Player playerOne, Player playerTwo) {
            PlayerOne = playerOne;
            PlayerTwo = playerTwo;
            Replays = new List<ReplayData>();
        }


        public void CalculateWinner() {
            int forfeit = PlayerOne.Forfeit + PlayerTwo.Forfeit;

            if (forfeit == 2) {
                Winner = -1;
            } else if (forfeit == 1 && PlayerTwo.Forfeit == 1) {
                Winner = PlayerOne.PlayerID;
            } else if (forfeit == 1 && PlayerOne.Forfeit == 1) {
                Winner = PlayerTwo.PlayerID;
            } else if (PlayerOne.Results.Points_Won > PlayerTwo.Results.Points_Won) {
                Winner = PlayerOne.PlayerID;
            } else if (PlayerOne.Results.Points_Won < PlayerTwo.Results.Points_Won) {
                Winner = PlayerTwo.PlayerID;
            } else {
                Winner = 0;
            }
        }


        public void ProcessReplay(ReplayData replay) {
            string spy = replay.spy_displayname.Replace("/steam", "");
            string sniper = replay.sniper_displayname.Replace("/steam", "");

            Player spyPlayer = PlayerOne.Name == spy ? PlayerOne : PlayerTwo;
            Player sniperPlayer = PlayerOne.Name == sniper ? PlayerOne : PlayerTwo;

            switch (replay.result) {
                case "Spy Shot":
                    spyPlayer.Results.Spy_SpyShot += 1;
                    sniperPlayer.Results.Sniper_SpyShot += 1;
                    break;
                case "Missions Win":
                    spyPlayer.Results.Spy_MissionsWin += 1;
                    sniperPlayer.Results.Sniper_MissionsWin += 1;
                    break;
                case "Time Out":
                    spyPlayer.Results.Spy_TimeOut += 1;
                    sniperPlayer.Results.Sniper_TimeOut += 1;
                    break;
                case "Civilian Shot":
                    spyPlayer.Results.Spy_CivilianShot += 1;
                    sniperPlayer.Results.Sniper_CivilianShot += 1;
                    break;
            }

            Replays.Add(replay);
        }


    }
}
