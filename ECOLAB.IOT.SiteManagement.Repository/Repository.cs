namespace ECOLAB.IOT.SiteManagement.Repository
{
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Data.SqlClient;

    public interface IRepository
    {
        public string ConnectionString { get; }

        public T Execute<T>(Func<SqlConnection, T> func);

        public T Execute<T>(string connectionString, Func<SqlConnection, T> func);

        public T Execute<T>(string connectionString, Func<SqlConnection, SqlTransaction, T> func);
    }

    public class Repository : IRepository
    {
        protected IConfiguration _config;
        private string keyName;

        public Repository(IConfiguration config, string keyName= "ConnectionStrings:SqlConnectionString")
        {
            _config = config;
            this.keyName = keyName;
        }

        public string ConnectionString
        {
            get
            {
                return _config[keyName];
            }
        }

        public T Execute<T>(Func<SqlConnection, T> func)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                return func(connection);
            }
        }

        public T Execute<T>(string connectionString, Func<SqlConnection, T> func)
        {
            using (SqlConnection connection = new SqlConnection(connectionString ?? ConnectionString))
            {
                return func(connection);
            }
        }

        public T Execute<T>(Func<SqlConnection, SqlTransaction, T> func)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    return func(connection, transaction);
                }
            }
        }

        public T Execute<T>(string connectionString, Func<SqlConnection, SqlTransaction, T> func)
        {
            using (SqlConnection connection = new SqlConnection(connectionString ?? ConnectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    return func(connection, transaction);
                }
            }
        }

    }
}
