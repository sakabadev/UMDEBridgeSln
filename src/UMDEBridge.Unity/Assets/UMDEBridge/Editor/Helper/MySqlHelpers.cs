using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using UMDEBridge.Editor.Settings.Model;
using UnityEngine;

namespace UMDEBridge.Editor.Helper {
	public static class MySqlHelpers {
		internal static IReadOnlyList<T> Query<T>(
			string sql
			, MySqlSettings settings
			, MySqlParameter[] parameters
			, Func<MySqlDataReader, T> createEntity) {
			var result = new List<T>();
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

							using (var reader = command.ExecuteReader()) {
								while (reader.Read()) {
									result.Add(createEntity(reader));
								}
							}
						}

						transaction.Commit();
					}
				}
				catch (Exception ex) {
					// エラー発生時の処理
					Debug.Log($"Error: {ex.Message}");
					try {
						// トランザクションが開かれていた場合、ロールバック
						if (connection.State == System.Data.ConnectionState.Open) {
							connection.Close();
						}
					}
					catch (Exception rollbackEx) {
						Debug.Log($"Rollback Error: {rollbackEx.Message}");
					}
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
					Debug.Log($"Error: {ex.Message}");
					try {
						// トランザクションが開かれていた場合、ロールバック
						if (connection.State == System.Data.ConnectionState.Open) {
							connection.Close();
						}
					}
					catch (Exception rollbackEx) {
						Debug.Log($"Rollback Error: {rollbackEx.Message}");
					}
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
					Debug.Log($"Error: {ex.Message}");
					Debug.Log($"Insert: {insert}");
					Debug.Log($"Update: {update}");

					try {
						// トランザクションが開かれていた場合、ロールバック
						if (connection.State == System.Data.ConnectionState.Open) {
							connection.Close();
						}
					}
					catch (Exception rollbackEx) {
						Debug.Log($"Rollback Error: {rollbackEx.Message}");
					}
				}
			}
		}

		internal static void ExecuteAddColumnIfNotExistQuery<T>(MySqlSettings settings) {
#if UMDEBRIDGE_USE_FIELD
			var dic = FieldHelpers.GetFieldNamesAndTypesFrom<T>();
#else
			var dic = PropertyHelpers.GetPropertyNamesAndTypesFrom<T>();
#endif
			using (var connection = new MySqlConnection(settings.ConnectionCommand)) {
				try {
					connection.Open();
					// DBが無ければ作成する
					string databaseName = settings.Database;
					CreateDatabaseIfNotExists(connection, databaseName);

					using (var transaction = connection.BeginTransaction()) {
						// DBに接続
						connection.ChangeDatabase(databaseName);

				
						MySqlScript script = new MySqlScript(connection);

						StringBuilder sb = new StringBuilder();
						sb.Append("DROP PROCEDURE IF EXISTS alter_table_procedure??");
						sb.Append("CREATE PROCEDURE alter_table_procedure() ");
						sb.Append("BEGIN ");

						foreach (var data in dic) {
							foreach (var v in data.Value) {
								sb.Append(
									$"IF NOT EXISTS (select * from information_schema.COLUMNS where table_name = '{data.Key}' and column_name = '{v.Item1}') THEN ");
								sb.Append($"ALTER TABLE {data.Key} ADD COLUMN {v.Item1} {string.Join(" ", v.Item2)}; ");
								sb.Append($"END IF; ");
							}
						}

						sb.Append("END ?? ");
						sb.Append("CALL alter_table_procedure();");

						script.Query = sb.ToString();
						script.Delimiter = "??";
						script.Execute();
						Debug.Log($"Query: {script.Query}");

						script.Delimiter = ";";
						script.Query = "DROP PROCEDURE alter_table_procedure;";
						script.Execute();
						Debug.Log($"Query: {script.Query}");

						transaction.Commit();
					}
				}
				catch (Exception ex) {
					// エラー発生時の処理
					Debug.Log($"Error: {ex.Message}");
					try {
						// トランザクションが開かれていた場合、ロールバック
						if (connection.State == System.Data.ConnectionState.Open) {
							connection.Close();
						}
					}
					catch (Exception rollbackEx) {
						Debug.Log($"Rollback Error: {rollbackEx.Message}");
					}
				}
			}
		}
		
		static void CreateDatabaseIfNotExists(MySqlConnection connection, string databaseName) {
			// データベースが存在するか確認
			if (!DatabaseExists(connection, databaseName))
			{
				Debug.Log("Database does not exist. Creating...");
				// データベース作成
				CreateDatabase(connection, databaseName);
				Debug.Log("Database created.");
			}
			else
			{
				// Debug.Log("Database already exists.");
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