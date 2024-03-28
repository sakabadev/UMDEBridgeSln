using System;
using System.Collections.Generic;
using MD2DBFromExcel.Domain.Settings.Model;
using MySql.Data.MySqlClient;

namespace MD2DBFromExcel.Infrastructure.MySQL {
	public static class MySQLHelper {
		internal static IReadOnlyList<T> Query<T>(
			string sql
			, MySqlSettings settings
			, MySqlParameter[] parameters
			, Func<MySqlDataReader, T> createEntity) {
			var result = new List<T>();
			using (var connection = new MySqlConnection(settings.ConnectionCommand))
			{
                try {
					connection.Open();
					// DBが無ければ作成する
					string databaseName = settings.Database;
					CreateDatabaseIfNotExists(connection, databaseName);

					using (var transaction = connection.BeginTransaction())
					{
						// DBに接続
						connection.ChangeDatabase(databaseName);

						using (var command = new MySqlCommand(sql, connection, transaction))
						{
							if (parameters != null)
							{
								command.Parameters.AddRange(parameters);
							}

							using (var reader = command.ExecuteReader())
							{
								while (reader.Read())
								{
									result.Add(createEntity(reader));
								}
							}
						}

						transaction.Commit();
					}
				}
				catch (Exception ex)
				{
					// エラー発生時の処理
					Console.WriteLine($"Error: {ex.Message}");
					try
					{
						// トランザクションが開かれていた場合、ロールバック
						if (connection.State == System.Data.ConnectionState.Open)
						{
							connection.Close();
						}
					}
					catch (Exception rollbackEx)
					{
						Console.WriteLine($"Rollback Error: {rollbackEx.Message}");
					}
					throw ex;
				}
			}

            return result.AsReadOnly();
		}

		internal static void Execute(string sql, MySqlSettings settings, MySqlParameter[] parameters) {
			using (var connection = new MySqlConnection(settings.ConnectionCommand)) {
				try {
					connection.Open();
					// DBが無ければ作成する
					string databaseName = settings.Database;
					CreateDatabaseIfNotExists(connection, databaseName);

					using (var transaction = connection.BeginTransaction()) {
						// DBに接続
						connection.ChangeDatabase(databaseName);

						using (var command = new MySqlCommand(sql, connection, transaction)) {
							if (parameters != null) {
								command.Parameters.AddRange(parameters);
							}

							command.ExecuteNonQuery();
						}

						transaction.Commit();
					}
				}
				catch (Exception ex) {
					// エラー発生時の処理
					Console.WriteLine($"Error: {ex.Message}");
					try {
						// トランザクションが開かれていた場合、ロールバック
						if (connection.State == System.Data.ConnectionState.Open) {
							connection.Close();
						}
					}
					catch (Exception rollbackEx) {
						Console.WriteLine($"Rollback Error: {rollbackEx.Message}");
                    }

                    throw ex;
                }
			}
		}

		internal static void Execute(string insert, string update, MySqlSettings settings,
			MySqlParameter[] parameters) {
			using (var connection = new MySqlConnection(settings.ConnectionCommand)) {

				try {
					connection.Open();
					// DBが無ければ作成する
					string databaseName = settings.Database;
					CreateDatabaseIfNotExists(connection, databaseName);

					using (var transaction = connection.BeginTransaction()) {
						// DBに接続
						connection.ChangeDatabase(databaseName);

						using (var command = new MySqlCommand(update, connection, transaction)) {
							if (parameters != null) {
								command.Parameters.AddRange(parameters);
							}

							// 対象があったらupdate
							if (command.ExecuteNonQuery() < 1) {
								// 対象がなかったらinsert
								command.CommandText = insert;
								command.ExecuteNonQuery();
							}
						}
						
						transaction.Commit();
					}
				}
				catch (Exception ex) {
					// エラー発生時の処理
					Console.WriteLine($"Error: {ex.Message}");
					try {
						// トランザクションが開かれていた場合、ロールバック
						if (connection.State == System.Data.ConnectionState.Open) {
							connection.Close();
						}
					}
					catch (Exception rollbackEx) {
						Console.WriteLine($"Rollback Error: {rollbackEx.Message}");
					}

                    throw ex;
                }
			}
		}

		static void CreateDatabaseIfNotExists(MySqlConnection connection, string databaseName) {
			// データベースが存在するか確認
			if (!DatabaseExists(connection, databaseName)) {
				Console.WriteLine("Database does not exist. Creating...");
				// データベース作成
				CreateDatabase(connection, databaseName);
				Console.WriteLine("Database created.");
			}
			else {
				// Console.WriteLine("Database already exists.");
			}
		}

		static bool DatabaseExists(MySqlConnection connection, string databaseName) {
			using (MySqlCommand cmd =
			       new MySqlCommand(
				       $"SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{databaseName}'",
				       connection)) {
				object result = cmd.ExecuteScalar();
				return result != null && result.ToString().Equals(databaseName, StringComparison.OrdinalIgnoreCase);
			}
		}

		static void CreateDatabase(MySqlConnection connection, string databaseName) {
			using (MySqlCommand cmd = new MySqlCommand($"CREATE DATABASE `{databaseName}`", connection)) {
				cmd.ExecuteNonQuery();
			}
		}
	}
}