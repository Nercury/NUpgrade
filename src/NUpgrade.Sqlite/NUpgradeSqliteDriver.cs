using System;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Text;

namespace NUpgrade.Sqlite
{
    public class NUpgradeSqliteDriver : IDbInformationDriver
    {
        private SQLiteConnection cn;
        private ConnectionState? initialState = null;
        private SQLiteTransaction transaction = null;

        public NUpgradeSqliteDriver(SQLiteConnection connection)
        {
            this.cn = connection;
        }

        /// <summary>
        /// Check if database has a table with specified name
        /// </summary>
        /// <param name="name">Table name</param>
        /// <returns>Returns true if table exists</returns>
        public bool HasTable(string name)
        {
            var initialState = cn.State;
            if (initialState != ConnectionState.Open)
                cn.Open();

            try
            {
                using (var cmd = cn.CreateCommand())
                {
                    cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name=@table_name;";
                    cmd.Parameters.Add(new SQLiteParameter("@table_name", name));

                    var rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        return rdr.GetString(0) == name;
                    }
                }
                return false;
            }
            finally 
            {
                if (initialState != ConnectionState.Open)
                    cn.Close();
            }
        }

        /// <summary>
        /// Get or set current database version
        /// </summary>
        public int Version
        {
            get
            {
                if (!this.HasTable("version"))
                {
                    return 0;
                }

                var initialState = cn.State;
                if (initialState != ConnectionState.Open)
                    cn.Open();

                try
                {
                    using (var cmd = cn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT version FROM version";
                        var rdr = cmd.ExecuteReader();
                        if (rdr.Read())
                        {
                            return rdr.GetInt32(0);
                        }
                    }
                    return 0;
                }
                finally 
                {
                    if (initialState != ConnectionState.Open)
                        cn.Close();
                }
            }
            set
            {
                var hasVersionTable = this.HasTable("version");

                var initialState = cn.State;
                if (initialState != ConnectionState.Open)
                    cn.Open();

                try
                {
                    using (var cmd = cn.CreateCommand())
                    {
                        if (!hasVersionTable)
                        {
                            cmd.CommandText = @"CREATE TABLE [version] ([version] integer NOT NULL);";
                            if (LogAction != null) LogAction(ToReadableString(cmd));
                            cmd.ExecuteNonQuery();

                            cmd.CommandText = @"INSERT INTO [version] ([version]) VALUES (@value)";
                            cmd.Parameters.Add(new SQLiteParameter("@value", value));
                            if (LogAction != null) LogAction(ToReadableString(cmd));
                            cmd.ExecuteNonQuery();
                        }
                        else
                        {
                            cmd.CommandText = "UPDATE [version] SET [version] = @value";
                            cmd.Parameters.Add(new SQLiteParameter("@value", value));
                            if (LogAction != null) LogAction(ToReadableString(cmd));
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                finally
                {
                    if (initialState != ConnectionState.Open)
                        cn.Close();
                }
            }
        }

        /// <summary>
        /// Run sql query in database
        /// </summary>
        /// <param name="statement"></param>
        public void RunSql(string statement)
        {
            using (var cmd = cn.CreateCommand())
            {
                if (this.transaction != null)
                {
                    cmd.Transaction = transaction;
                }
                cmd.CommandText = statement;
                if (LogAction != null) LogAction(ToReadableString(cmd));
                cmd.ExecuteNonQuery();
            }
        }

        public void BeginTransaction()
        {
            if (initialState != null || this.transaction != null)
            {
                throw new MigrationException("Can not run multiple migration transactions!");
            }
            this.initialState = cn.State;
            if (initialState != ConnectionState.Open)
            {
                cn.Open();
            }
            this.transaction = cn.BeginTransaction();
        }

        public void CommitTransaction()
        {
            this.transaction.Commit();
            this.transaction = null;
            if (initialState != ConnectionState.Open)
                cn.Close(); // only close connection if not initially open
            this.initialState = null;
        }

        public void RollbackTransaction()
        {
            this.transaction.Rollback();
            this.transaction = null;
            if (initialState != ConnectionState.Open)
                cn.Close(); // only close connection if not initially open
            this.initialState = null;
        }

        /// <summary>
        /// Get driver name
        /// </summary>
        public string DriverName
        {
            get { return "sqlite"; }
        }

        /// <summary>
        /// A helper to print command as string with parameters
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private static string ToReadableString(IDbCommand command)
        {
            StringBuilder builder = new StringBuilder();
            if (command.CommandType == CommandType.StoredProcedure)
                builder.Append("MIGRATION PROCEDURE: " + command.CommandText);
            else
                builder.Append("MIGRATION SQL: " + command.CommandText);
            if (command.Parameters.Count > 0)
                builder.AppendLine().AppendLine("With the following parameters.");
            foreach (IDataParameter param in command.Parameters)
            {
                builder.AppendFormat(
                    "     Parameter {0}: {1}",
                    param.ParameterName,
                    (param.Value == null ?
                    "NULL" : param.Value.ToString()));
            }
            return builder.ToString();
        }


        public Action<string> LogAction
        {
            get;
            set;
        }
    }
}
