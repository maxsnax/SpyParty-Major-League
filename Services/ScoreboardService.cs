using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using SML.Exceptions;
using System.IO.Compression;
using SML.DAL.Repositories;
using SML.DAL;
using System.Configuration;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Linq;
using SML.Models;
using static SML.Models.Player;

namespace SML {
    public class ScoreboardService {

        public List<Tuple<int, string>> LoadSeasons() {
            using UnitOfWork uow = new UnitOfWork(ConfigurationManager.ConnectionStrings["SML_db-connection"].ToString());

            return uow.SeasonsRepo.LoadSeasons();
        }


        // Settle ties between players with the same number of points
        public List<Player> SettleScoreboardTies(Division division) {
            List<Player> playersList = division.Players
                            .OrderByDescending(p => p.Points) // Primary sorting by points
                            .ThenByDescending(p => p.Wins)    // Secondary sorting (will be adjusted later)
                            .ToList();

            // Resolve ties using head-to-head results
            for (int i = 0; i < playersList.Count - 1; i++) {
                Player playerOne = playersList[i];
                Player playerTwo = playersList[i + 1];

                // If players have the same points
                if (playerOne.Points == playerTwo.Points) {
                    // Check if playerOne forfeits the season
                    if (playerOne.Forfeit != 0) {
                        playersList[i] = playerTwo;
                        playersList[i + 1] = playerOne;
                        continue;
                    }
                    else if (playerTwo.Forfeit != 0) {
                        playersList[i] = playerOne;
                        playersList[i + 1] = playerTwo;
                    }

                    using UnitOfWork uow = new UnitOfWork(ConfigurationManager.ConnectionStrings["SML_db-connection"].ToString());

                    List<int> matchWinners = uow.MatchesRepo.GetMatchWinners(playerOne, playerTwo);
                    int playerOneWins = matchWinners.Count(w => w == playerOne.PlayerID);
                    int playerTwoWins = matchWinners.Count(w => w == playerTwo.PlayerID);

                    // Determine ranking based on head-to-head results
                    if (playerTwoWins > playerOneWins) {
                        // Swap players if playerTwo has won more head-to-head matches
                        playersList[i] = playerTwo;
                        playersList[i + 1] = playerOne;
                    }
                    // If still tied, wins remain as a secondary tiebreaker (already handled by initial sorting)
                }
            }

            return playersList;
        }

        public List<Player> FetchPlayerSortedDivisions(Division division) {
            using UnitOfWork uow = new UnitOfWork(ConfigurationManager.ConnectionStrings["SML_db-connection"].ToString());



            //foreach (Player player in playersList) {
            //    try {
            //        if (player.Username == null) {
            //            uow.PlayersRepo.UpdatePlayerUsername(player);
            //        }
            //    }
            //    catch (Exception ex) {
            //        System.Diagnostics.Debug.WriteLine(ex);
            //    }
            //}
            List<Player> playersList = uow.SeasonsRepo.GetPlayersFromDivision(division);

            //foreach (Player player in playersList) {
            //    try {
            //        uow.PlayersRepo.UpdatePlayerStats(player);
            //    }
            //    catch (Exception ex) {
            //        System.Diagnostics.Debug.WriteLine(ex);
            //    }
            //}
            //playersList = uow.SeasonsRepo.GetPlayersFromDivision(division);

            //playersList = uow.SeasonsRepo.GetPlayersFromDivision(division);
            playersList = playersList.OrderByDescending(p => p.Points) // Primary sorting by points
                            .ThenByDescending(p => p.Wins)    // Secondary sorting (will be adjusted later)
                            .ToList();

            // Resolve ties using head-to-head results
            for (int i = 0; i < playersList.Count - 1; i++) {
                Player playerOne = playersList[i];
                Player playerTwo = playersList[i + 1];

                // If players have the same points
                if (playerOne.Points == playerTwo.Points) {
                    // Check if playerOne forfeits the season
                    if (playerOne.Forfeit != 0) {
                        playersList[i] = playerTwo;
                        playersList[i + 1] = playerOne;
                        continue;
                    }

                    List<int> matchWinners = uow.MatchesRepo.GetMatchWinners(playerOne, playerTwo);
                    int playerOneWins = matchWinners.Count(w => w == playerOne.PlayerID);
                    int playerTwoWins = matchWinners.Count(w => w == playerTwo.PlayerID);

                    // Determine ranking based on head-to-head results
                    if (playerTwoWins > playerOneWins) {
                        // Swap players if playerTwo has won more head-to-head matches
                        playersList[i] = playerTwo;
                        playersList[i + 1] = playerOne;
                    }
                    // If still tied, wins remain as a secondary tiebreaker (already handled by initial sorting)
                }
            }

            return playersList;
        }

        public List<Division> FetchOrderSortedDivisions(int seasonID) {
            using UnitOfWork uow = new UnitOfWork(ConfigurationManager.ConnectionStrings["SML_db-connection"].ToString());

            List<Division> divisionList = uow.SeasonsRepo.GetSeasonDivisions(seasonID);
            divisionList = divisionList.OrderBy(d => d.LoadOrder).ToList(); // Reorder the list
            return divisionList;
        }

    }
}
