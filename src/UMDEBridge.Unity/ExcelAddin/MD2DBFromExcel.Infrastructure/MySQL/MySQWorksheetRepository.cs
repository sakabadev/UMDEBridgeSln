using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MD2DBFromExcel.Domain.Settings.Repository;
using MD2DBFromExcel.Domain.Worksheet.Repository;
using Microsoft.Office.Interop.Excel;
using Constants = MD2DBFromExcel.Domain.Constants.Constants;
using Excel = Microsoft.Office.Interop.Excel;

namespace MD2DBFromExcel.Infrastructure.MySQL {
    /// <summary>
    /// Create、Add ColumnはUnityEditorで行う(MemoryTableのModelを使いたい)ため、ここで出来る事はUpdate、Insert
    /// </summary>
    public sealed class MySQLWorksheetRepository : IWorksheetRepository {
        private readonly ISettingsRepository _settingsRepository;
        
        public MySQLWorksheetRepository(ISettingsRepository settingsRepository) {
            _settingsRepository = settingsRepository;
        }

        public void Save(string bookDirPath, Worksheet sheet) {
            int keyRowPos = 1;
            int valueStartRowPos = 1;

            string sql = string.Empty;
            // keyの行を取得
            while (keyRowPos < sheet.Rows.Count) {
                if (sheet.Cells[++keyRowPos, 1]?.Value?.ToString() == "key")
                    break;

                // idのテーブル名が見つからない場合、テーブルの構造がおかしいのでSqlは実行しない
                if (keyRowPos > 10) {
                    throw new InvalidOperationException("keyのセルが見つかるまでが長すぎます。");
                }
            }

            // valueの開始行を取得
            valueStartRowPos = keyRowPos;
            while (valueStartRowPos < sheet.Rows.Count) {
                if (sheet.Cells[++valueStartRowPos, 1]?.Value?.ToString() == "value")
                    break;

                // idのテーブル名が見つからない場合、テーブルの構造がおかしいのでSqlは実行しない
                if (valueStartRowPos > 10) {
                    throw new InvalidOperationException("valueのセルが見つかるまでが長すぎます。");
                }
            }


            // Insert or Update
            sql = GenerateUpdateSql(keyRowPos, valueStartRowPos, sheet);
            System.Diagnostics.Debug.WriteLine(sql);
            System.Windows.Forms.MessageBox.Show(sql);
            var settings = _settingsRepository.Find(bookDirPath);
            MySQLHelper.Execute(sql, settings.mySqlSettings, null);
        }

        string GenerateUpdateSql(int keyRow, int valueStartRow, Worksheet sheet) {
            StringBuilder sb = new StringBuilder();
            // ループカウントの都合上1減らす
            int colResetValue = 2 - 1;
            int col = colResetValue;

            List<string> cells = new List<string>();

            // nameを取得
            while (sheet.Cells[keyRow, ++col]?.Value != null) {
                cells.Add(sheet.Cells[keyRow, col]?.Value.ToString());
            }
            string[] names = cells.ToArray();

            sb.Append($@"INSERT INTO `{sheet.Name}` ({string.Join(",", cells).TrimEnd(',')}) VALUES ");
            int colCount = colResetValue + cells.Count;
            cells.Clear();

            col = colResetValue;
            int row = valueStartRow;
            while (sheet.Cells[row, col + 1]?.Value != null) {
                while (col < colCount) {
                    cells.Add(CheckIsString(sheet.Cells[row, ++col]?.Value));
                }
                sb.Append($@"({string.Join(",", cells).TrimEnd(',')})");

                cells.Clear();
                col = colResetValue;
                if (sheet.Cells[++row, col + 1]?.Value != null) {
                    sb.Append(",");
                }
            }

            if (names.Length <= 1)
                throw new InvalidOperationException("カラムがidのみのシートなので、更新する意味がなさそうです。");

            sb.Append($@" ON DUPLICATE KEY UPDATE ");
            for (int i = 0; i < names.Length; i++) {
                if (names[i] == "idx" || names[i] == "id")
                    continue;

                sb.Append($"{names[i]} = VALUES({names[i]})");
                if (i < names.Length - 1)
                    sb.Append(",");
            }
            sb.Append(";");
            return sb.ToString();
        }

        string CheckIsString(dynamic value) {
            if (value == null) return "NULL";
            if (value.GetType() == typeof(double)) return value.ToString();
            string str = value.ToString();
            // tinyintで挿入する場合はboolを01に変換する
            if (str.Equals("true") || str.Equals("True") || str.Equals("TRUE")) return "1";
            if (str.Equals("false") || str.Equals("False") || str.Equals("FALSE")) return "0";
            return $"'{str}'";
        }

        /// <summary>
        /// 暫定で副作用で対応してます
        /// </summary>
        /// <param name="sheet"></param>
        public void Find(string bookDirPath, Worksheet sheet) {
            var settings = _settingsRepository.Find(bookDirPath);
            string[] configLabels = Constants.ConfigLabels;

            int sortLabelRow = 1;
            for (int i = 1; i < configLabels.Length; i++) {
                if(configLabels[i] == "column_name") {
                    sheet.Cells[i + 1, 1].Value = "key";
                }
                if (configLabels[i] == "mp_key")
                {
                    sheet.Cells[i + 1, 1].Value = "mp_key";
                }
                if (configLabels[i] == "sort_label") {
                    sheet.Cells[i + 2, 1].Value = "value";
                    sortLabelRow = i + 1;
                }
            }

            int row = 1;
            int col = 2;
            List<string> columnNames = new List<string>();
            List<string> sortLabels = new List<string>();
            List<int> ignoreColumns = new List<int>();
            string sql = $@"SELECT * FROM {sheet.Name}_config";
            // configを取得
            MySQLHelper.Query(sql, settings.mySqlSettings, null,
                reader => {
                    row = 1;
                    foreach (string label in configLabels) {
                        if (reader.GetOrdinal(label) == -1)
                            continue;

                        sheet.Cells[row++, col].Value = reader[label];
                        if (label == "column_name")
                            columnNames.Add((string)reader[label]);
                        if (label == "column_width")
                            sheet.Columns[col].ColumnWidth = 18 * ((int)reader[label] / 100.0f);
                        if (label == "prefer_excel")
                        {
                            if ((bool)reader[label])
                            {
                                ignoreColumns.Add(col);
                            }
                            else
                            {
                                // 列の背景色を変更。使用しない意思表示の色を加える
                                sheet.Columns[col].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Azure);
                            }
                        }
                    }
                    col++;
                    return configLabels;
                });

            sql = $@"SELECT * FROM {sheet.Name}";
            
            // cellに取得した値を書き込み
            MySQLHelper.Query(sql, settings.mySqlSettings,null,
                reader => {
                    col = 2;
                    foreach (string name in columnNames) {
                        if (sheet.Cells[row, col].Value == null || !ignoreColumns.Contains(col))
                            sheet.Cells[row, col].Value = reader[name];
                        col++;
                    }
                    row++;
                    return configLabels;
                });

            // データを書き込んだりソートしたりするためのテーブルレイアウトを作成
            if (sheet.ListObjects.Count <= 0 || sheet.ListObjects["TestTable"] == null) {
                sheet.ListObjects.Add(Excel.XlListObjectSourceType.xlSrcRange, sheet.Range[sheet.Cells[sortLabelRow, 2], sheet.Cells[row - 1, col - 1]],
                Type.Missing, Excel.XlYesNoGuess.xlYes, Type.Missing).Name = "TestTable";
                var table = sheet.ListObjects["TestTable"];
                table.TableStyle = "TableStyleMedium3";
                var headerRange = table.HeaderRowRange;
                headerRange.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Black);
            }
        }
    }
}
