using SML.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace SML.DAL {
    public class UnitOfWork : IDisposable {
        private readonly SqlConnection _connection;
        private SqlTransaction _transaction;
        public MatchRepository MatchesRepo { get; private set; }
        public SeasonRepository SeasonsRepo { get; private set; }
        public PlayerRepository PlayersRepo { get; private set; }

        public UnitOfWork(string connectionString) {
            _connection = new SqlConnection(connectionString);
            _connection.Open();
            MatchesRepo = new MatchRepository(_connection);
            SeasonsRepo = new SeasonRepository(_connection);
            PlayersRepo = new PlayerRepository(_connection);
        }

        public void BeginTransaction() {
            _transaction = _connection.BeginTransaction();
            PlayersRepo.SetTransaction(_transaction);
            MatchesRepo.SetTransaction(_transaction);
        }

        public void CommitTransaction() {
            _transaction?.Commit();
            _transaction = null;
        }

        public void Rollback() {
            _transaction?.Rollback();
            _transaction = null;
        }


        public void Dispose() {
            
        }

        public string HashPassword(string password) {
            using SHA256 sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
}
