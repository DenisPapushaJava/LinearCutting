namespace LinearCutting
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Text = "Linear Cutting Optimizer";
            this.labelQuantity = new Label
            {
                Text = "Количество:",
                Location = new Point(375, 15),
                AutoSize = true
            };
            this.txtQuantity = new TextBox
            {
                Location = new Point(450, 12),
                Size = new Size(80, 23),
                Text = "1"
            };
            this.btnQuickAdd = new Button
            {
                Text = "Быстрое добавление",
                Location = new Point(540, 40),
                Size = new Size(120, 25)
            };
            // Создание и настройка контролов
            CreateControls();
        }

        private void CreateControls()
        {
            // Основные контролы
            var label1 = new Label { Text = "Длина материала:", Location = new Point(12, 15), AutoSize = true };
            var txtMaterialLength = new TextBox { Location = new Point(116, 12), Size = new Size(100, 20), Text = "6000", Name = "txtMaterialLength" };

            var label2 = new Label { Text = "Зазор:", Location = new Point(222, 15), AutoSize = true };
            var txtGap = new TextBox { Location = new Point(269, 12), Size = new Size(100, 20), Text = "5", Name = "txtGap" };

            var label3 = new Label { Text = "Длина детали:", Location = new Point(12, 45), AutoSize = true };
            var txtPartLength = new TextBox { Location = new Point(116, 42), Size = new Size(100, 20), Name = "txtPartLength" };

            var btnAddPart = new Button { Text = "Добавить", Location = new Point(222, 40), Size = new Size(75, 23), Name = "btnAddPart" };
            var btnRemoveSelected = new Button { Text = "Удалить выбранную", Location = new Point(303, 40), Size = new Size(120, 23), Name = "btnRemoveSelected" };

            var dataGridViewParts = new DataGridView { Location = new Point(12, 80), Size = new Size(760, 150), Name = "dataGridViewParts" };

            var btnCalculate = new Button { Text = "Рассчитать", Location = new Point(12, 240), Size = new Size(120, 30), Name = "btnCalculate" };
            var btnClear = new Button { Text = "Очистить", Location = new Point(140, 240), Size = new Size(120, 30), Name = "btnClear" };

            var label5 = new Label { Text = "Результаты расчета:", Location = new Point(12, 274), AutoSize = true };
            var txtResults = new TextBox { Location = new Point(12, 290), Size = new Size(300, 250), Multiline = true, ScrollBars = ScrollBars.Vertical, Name = "txtResults" };

            var label6 = new Label { Text = "Графическое отображение:", Location = new Point(320, 274), AutoSize = true };
            var panelVisualization = new Panel { Location = new Point(320, 290), Size = new Size(452, 250), AutoScroll = true, BorderStyle = BorderStyle.FixedSingle, Name = "panelVisualization" };

            // Добавление контролов на форму
            this.Controls.AddRange(new Control[] {
            label1, txtMaterialLength, label2, txtGap, label3, txtPartLength,
            btnAddPart, btnRemoveSelected, dataGridViewParts, btnCalculate,
            btnClear, label5, txtResults, label6, panelVisualization
        });
            this.Controls.Add(this.labelQuantity);
            this.Controls.Add(this.txtQuantity);
            this.Controls.Add(this.btnQuickAdd);

            // Привязка событий
            btnAddPart.Click += btnAddPart_Click;
            btnRemoveSelected.Click += btnRemoveSelected_Click;
            btnCalculate.Click += btnCalculate_Click;
            btnClear.Click += btnClear_Click;
            this.btnQuickAdd.Click += new EventHandler(this.btnQuickAdd_Click);

            // Сохранение ссылок для доступа из кода
            this.txtMaterialLength = txtMaterialLength;
            this.txtGap = txtGap;
            this.txtPartLength = txtPartLength;
            this.btnAddPart = btnAddPart;
            this.btnRemoveSelected = btnRemoveSelected;
            this.dataGridViewParts = dataGridViewParts;
            this.btnCalculate = btnCalculate;
            this.btnClear = btnClear;
            this.txtResults = txtResults;
            this.panelVisualization = panelVisualization;
        }

        // Объявления для доступа к контролам из кода
        private TextBox txtMaterialLength;
        private TextBox txtGap;
        private TextBox txtPartLength;
        private Button btnAddPart;
        private Button btnRemoveSelected;
        private DataGridView dataGridViewParts;
        private Button btnCalculate;
        private Button btnClear;
        private TextBox txtResults;
        private Panel panelVisualization;
        private TextBox txtQuantity;
        private Label labelQuantity;
        private Button btnQuickAdd;
    }
}
