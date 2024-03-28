namespace MD2DBFromExcel.Domain.Worksheet.Repository {
    public interface IWorksheetRepository {
        void Save(string bookDirPath, Microsoft.Office.Interop.Excel.Worksheet sheet);
        void Find(string bookDirPath, Microsoft.Office.Interop.Excel.Worksheet sheet);
    }
}
