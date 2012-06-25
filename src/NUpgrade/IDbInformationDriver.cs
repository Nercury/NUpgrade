
namespace NUpgrade
{
    /// <summary>
    /// Implement this interface for your database to be compatible with NUpgrade
    /// </summary>
    public interface IDbInformationDriver
    {
        /// <summary>
        /// Get or set current database version
        /// </summary>
        int Version { get; set; }

        /// <summary>
        /// Check if database has a table with specified name
        /// </summary>
        /// <param name="name">Table name</param>
        /// <returns>Returns true if table exists</returns>
        bool HasTable(string name);

        /// <summary>
        /// Run sql query in database
        /// </summary>
        /// <param name="statement"></param>
        void RunSql(string statement);

        /// <summary>
        /// Begin migration transaction
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Commit migration transaction
        /// </summary>
        void CommitTransaction();

        /// <summary>
        /// Rollback migration transaction
        /// </summary>
        void RollbackTransaction();

        /// <summary>
        /// Get driver name
        /// </summary>
        string DriverName { get; }
    }
}
