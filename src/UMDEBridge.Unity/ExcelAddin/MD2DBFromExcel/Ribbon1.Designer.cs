using MD2DBFromExcel.UseCase;

namespace MD2DBFromExcel {
    partial class Ribbon1 : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public Ribbon1()
            : base(Globals.Factory.GetRibbonFactory()) {
            InitializeComponent();
        }

        /// <summary> 
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region コンポーネント デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent() {
            this.UMDEBridgeTab = this.Factory.CreateRibbonTab();
            this.syncGroup = this.Factory.CreateRibbonGroup();
            this.Export2DBButton = this.Factory.CreateRibbonButton();
            this.ExportSelectedSheetButton = this.Factory.CreateRibbonButton();
            this.ImportSelectedSheetButton = this.Factory.CreateRibbonButton();
            this.infoGroup = this.Factory.CreateRibbonGroup();
            this.connectionLabel = this.Factory.CreateRibbonLabel();
            this.connectionInfoText = this.Factory.CreateRibbonLabel();
            this.UMDEBridgeTab.SuspendLayout();
            this.syncGroup.SuspendLayout();
            this.infoGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // UMDEBridgeTab
            // 
            this.UMDEBridgeTab.Groups.Add(this.syncGroup);
            this.UMDEBridgeTab.Groups.Add(this.infoGroup);
            this.UMDEBridgeTab.Label = "UMDEBridge";
            this.UMDEBridgeTab.Name = "UMDEBridgeTab";
            // 
            // syncGroup
            // 
            this.syncGroup.Items.Add(this.Export2DBButton);
            this.syncGroup.Items.Add(this.ExportSelectedSheetButton);
            this.syncGroup.Items.Add(this.ImportSelectedSheetButton);
            this.syncGroup.Label = "同期";
            this.syncGroup.Name = "syncGroup";
            // 
            // Export2DBButton
            // 
            this.Export2DBButton.Label = "全シートをDBに保存";
            this.Export2DBButton.Name = "Export2DBButton";
            this.Export2DBButton.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.Export2DBButton_Click);
            // 
            // ExportSelectedSheetButton
            // 
            this.ExportSelectedSheetButton.Label = "選択中のシートをDBに保存";
            this.ExportSelectedSheetButton.Name = "ExportSelectedSheetButton";
            this.ExportSelectedSheetButton.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.ExportSelectedSheetButton_Click);
            // 
            // ImportSelectedSheetButton
            // 
            this.ImportSelectedSheetButton.Label = "選択中のシートをDBから読み込み";
            this.ImportSelectedSheetButton.Name = "ImportSelectedSheetButton";
            this.ImportSelectedSheetButton.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.ImportSelectedSheetButton_Click);
            // 
            // infoGroup
            // 
            this.infoGroup.Items.Add(this.connectionLabel);
            this.infoGroup.Items.Add(this.connectionInfoText);
            this.infoGroup.Label = "情報";
            this.infoGroup.Name = "infoGroup";
            // 
            // connectionLabel
            // 
            this.connectionLabel.Label = "接続情報";
            this.connectionLabel.Name = "connectionLabel";
            // 
            // connectionInfoText
            // 
            this.connectionInfoText.Label = "接続情報が表示されていない場合、シートの設定がおかしいです。";
            this.connectionInfoText.Name = "connectionInfoText";
            // 
            // Ribbon1
            // 
            this.Name = "Ribbon1";
            this.RibbonType = "Microsoft.Excel.Workbook";
            this.Tabs.Add(this.UMDEBridgeTab);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.Ribbon1_Load);
            this.UMDEBridgeTab.ResumeLayout(false);
            this.UMDEBridgeTab.PerformLayout();
            this.syncGroup.ResumeLayout(false);
            this.syncGroup.PerformLayout();
            this.infoGroup.ResumeLayout(false);
            this.infoGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab UMDEBridgeTab;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup syncGroup;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton Export2DBButton;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton ImportSelectedSheetButton;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton ExportSelectedSheetButton;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup infoGroup;
        internal Microsoft.Office.Tools.Ribbon.RibbonLabel connectionLabel;
        internal Microsoft.Office.Tools.Ribbon.RibbonLabel connectionInfoText;
    }

    partial class ThisRibbonCollection {
        internal Ribbon1 Ribbon1 {
            get { return this.GetRibbon<Ribbon1>(); }
        }
    }
}
