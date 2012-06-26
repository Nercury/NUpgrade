NUpgrade
========

NUpgrade is a database upgrade helper. At the core is Migration class, which
is used to initialize upgrade driver (so far only SQLite), check if database needs to be upgraded,
registering multiple upgrade paths, and executing actual upgrade.

Example usage:

	// create a new connection for local database, connection string should probably be stored elsewhere
	var connection = new SQLiteConnection("Data Source=local.db;Version=3;New=True;Compress=True;");
	// will migrate to this version
	var requiredDbVersion = 1;

	// initialize new migration manager
	var migration = new Migration(new NUpgradeSqliteDriver(connection), requiredDbVersion);
	// check if we need to migrate
	if (!migration.IsUpToDate)
	{
		// add all possible migration steps

		migration.Add(0, 1, schema => // to migrate from version 0 to 1, execute this
		{
			schema.RunSql("SOME SQL TO MODIFY DB (in driver dialect)");
		});

		migration.Add(1, 2, schema => // to migrate from version 1 to 2
		{
			//...
		});

		// find steps to execute to correctly migrate db
		var migrationSteps = migration.FindUpgradeSteps();
		if (migrationSteps == null)
			throw new MigrationException("No migration path found.");

		migration.Execute(migrationSteps);
	}

NUpgrade.Sqlite
===============

NUpgrade can be extended with additional drivers. So far only Sqlite is implemented.
Internally, SQLite driver creates a "version" table and stores version number in it. It is done behind the scenes, so no
special changes to database are required.