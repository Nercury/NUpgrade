using System;
using System.Collections.Generic;
using System.Text;
using FluentNUpgrade.UpgradeGen.Exceptions;
using NHibernate.Dialect;
using FluentNUpgrade.Exceptions;
using NHibernate.SqlCommand;
using NHibernate.SqlTypes;
using NHibernate;

namespace FluentNUpgrade.UpgradeGen
{
    public class EntityUpgradeActions
    {
        public string Name { get; private set; }

        public EntityUpgradeActions(string entityName, Dialect dialect)
        {
            this.Dialect = dialect;
            this.Name = entityName;
        }

        public Dialect Dialect { get; private set; }

        private Dictionary<string, ColumnUpgradeActions> addedColumns = new Dictionary<string, ColumnUpgradeActions>();
        private Dictionary<string, bool> removedColumns = new Dictionary<string, bool>();

        private ColumnUpgradeActions RegisterColumnAdd(string name, Type type)
        {
            ColumnUpgradeActions actions;
            if (addedColumns.TryGetValue(name, out actions))
            {
                throw new ColumnAlreadyAddedException("Column with name [" + name + "] already added to entity.");
            }
            else
            {
                removedColumns.Remove(name);

                actions = new ColumnUpgradeActions(type);
                addedColumns[name] = actions;
            }
            return actions;
        }

        private void RegisterColumnRemove(string name)
        {
            if (removedColumns.ContainsKey(name))
            {
                throw new ColumnAlreadyRemovedException("Column with name [" + name + "] already removed from entity.");
            }
            else
            {
                addedColumns.Remove(name);

                removedColumns[name] = true; // boolean value does not mean anything here
            }
        }

        /// <summary>
        /// Register column addition
        /// </summary>
        /// <typeparam name="ColumnT">Column type</typeparam>
        /// <param name="name">Column name</param>
        /// <returns></returns>
        public EntityUpgradeActions AddColumn<ColumnT>(string name)
        {
            RegisterColumnAdd(name, typeof(ColumnT));
            return this;
        }

        /// <summary>
        /// Register column addition and column setup script.
        /// </summary>
        /// <typeparam name="ColumnT">Column type</typeparam>
        /// <param name="name">Column name</param>
        /// <param name="columnSetup">Setup delegate</param>
        /// <returns></returns>
        public EntityUpgradeActions AddColumn<ColumnT>(string name, Action<ColumnUpgradeActions> columnSetup)
        {
            columnSetup(RegisterColumnAdd(name, typeof(ColumnT)));
            return this;
        }

        /// <summary>
        /// Register column remove action.
        /// </summary>
        /// <param name="name">Column name</param>
        /// <returns></returns>
        public EntityUpgradeActions RemoveColumn(string name)
        {
            RegisterColumnRemove(name);
            return this;
        }

        /// <summary>
        /// Register column modification.
        /// </summary>
        /// <param name="name">Column name</param>
        /// <param name="columnSetup">Setup delegate</param>
        /// <returns></returns>
        public EntityUpgradeActions ModifyColumn(string name, Action<ColumnUpgradeActions> columnSetup)
        {
            ColumnUpgradeActions actions;
            if (!addedColumns.TryGetValue(name, out actions))
            {
                throw new ColumnNotFoundException("Column with name [" + name + "] was not found in entity.");
            }
            else
            {
                columnSetup(actions);
            }
            return this;
        }

        /// <summary>
        /// Generate upgrade scripts using current dialect.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SqlString> GenerateScripts()
        {
            List<SqlString> sqlStrings = new List<SqlString>();

            if (!Dialect.HasAlterTable)
            {
                throw new NUpgradeException("Can't upgrade databases without alter table.");
            }
            else
            {
                foreach (var item in removedColumns)
                {
                    SqlStringBuilder sb = new SqlStringBuilder();
                    sb.Add("alter table ").Add(Dialect.QuoteForTableName(this.Name)).Add(" ");
                    sb.Add("drop column ").Add(Dialect.QuoteForColumnName(item.Key)).Add(" ");
                    sqlStrings.Add(sb.ToSqlString());
                }
                foreach (var item in addedColumns)
                {
                    SqlStringBuilder sb = new SqlStringBuilder();
                    sb.Add("alter table ").Add(Dialect.QuoteForTableName(this.Name)).Add(" ");
                    sb.Add(Dialect.AddColumnString).Add(" ").Add(Dialect.QuoteForColumnName(item.Key)).Add(" ")
                        .Add(Dialect.GetTypeName(NHibernateUtil.String.SqlType));
                    sqlStrings.Add(sb.ToSqlString());
                }
            }

            return sqlStrings;
        }
    }
}
