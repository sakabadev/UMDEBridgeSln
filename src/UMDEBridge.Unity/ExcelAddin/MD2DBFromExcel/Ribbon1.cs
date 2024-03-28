using MD2DBFromExcel.UseCase;
using Microsoft.Office.Tools.Ribbon;
using System;
using System.IO;

namespace MD2DBFromExcel
{
    public partial class Ribbon1
    {
        readonly ExcelUseCase excelUseCase = new ExcelUseCase();
        
        private void Ribbon1_Load(object sender, RibbonUIEventArgs e)
        {
            //(string connection, string db) = excelUseCase.GetConnectionString();
            //connectionInfoText.Label = connection + db;
        }

        private void Export2DBButton_Click(object sender, RibbonControlEventArgs e) {
            var workbook = Globals.ThisAddIn.Application.ActiveWorkbook;
            string filePath = workbook.FullName;
            string bookDirPath = Path.GetDirectoryName(filePath);

            try
            {
                excelUseCase.UpdateDBFromWorkbook(bookDirPath, workbook);
                System.Windows.Forms.MessageBox.Show("DBのアップデートが完了しました。");
            } catch(Exception ex) {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        private void ImportSelectedSheetButton_Click(object sender, RibbonControlEventArgs e)
        {
            var workbook = Globals.ThisAddIn.Application.ActiveWorkbook;
            string filePath = workbook.FullName;
            string bookDirPath = Path.GetDirectoryName(filePath);
            var sheet = Globals.ThisAddIn.Application.ActiveSheet;
            try {
                excelUseCase.ImportSheetFromDB(bookDirPath, sheet);
                System.Windows.Forms.MessageBox.Show("DBから読み込みました。");
            } catch(Exception ex) {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
}

        private void ExportSelectedSheetButton_Click(object sender, RibbonControlEventArgs e)
        {
            var workbook = Globals.ThisAddIn.Application.ActiveWorkbook;
            string filePath = workbook.FullName;
            string bookDirPath = Path.GetDirectoryName(filePath);
            var worksheet = Globals.ThisAddIn.Application.ActiveSheet;
            try {
                excelUseCase.ExportSheetToDB(bookDirPath, worksheet);
                System.Windows.Forms.MessageBox.Show("DBのアップデートが完了しました。");
            } catch (Exception ex) {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }
    }
}
