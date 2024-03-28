
using MD2DBFromExcel.Domain.Settings.Repository;
using MD2DBFromExcel.Domain.Worksheet.Repository;
using MD2DBFromExcel.Infrastructure.Json;
using MD2DBFromExcel.Infrastructure.MySQL;
using Excel = Microsoft.Office.Interop.Excel;

namespace MD2DBFromExcel.UseCase {
    public class ExcelUseCase {
        private readonly IWorksheetRepository _worksheetRepository;
        private readonly ISettingsRepository _settingsRepository;

        public ExcelUseCase() : this(new FileSettingsRepository(), null) {
            _worksheetRepository = new MySQLWorksheetRepository(_settingsRepository);
        }

        public ExcelUseCase(ISettingsRepository settingsRepository, IWorksheetRepository worksheetRepository) {
            _settingsRepository = settingsRepository;
            _worksheetRepository = worksheetRepository;
        }

        internal void UpdateDBFromWorkbook(string bookDirPath, Excel.Workbook workbook) {
            foreach (Excel.Worksheet sheet in workbook.Worksheets) {
                ExportSheetToDB(bookDirPath, sheet);
            }
        }

        internal void ExportSheetToDB(string bookDirPath, Excel.Worksheet sheet) {
            _worksheetRepository.Save(bookDirPath, sheet);
        }

        internal void ImportSheetFromDB(string bookDirPath, Excel.Worksheet sheet) {
            _worksheetRepository.Find(bookDirPath, sheet);
        }

        internal (string connection, string database) GetConnectionString(string bookDirPath) {
            var settings = _settingsRepository.Find(bookDirPath);
            return (settings.mySqlSettings.ConnectionCommand, settings.mySqlSettings.Database);
        }
    }
}
