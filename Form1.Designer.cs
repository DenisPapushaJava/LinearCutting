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
            this.SuspendLayout();

            // Основные настройки формы
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.MinimumSize = new System.Drawing.Size(1000, 600);
            this.Text = "Оптимизатор линейного раскроя";
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));

            CreateAdaptiveLayout();

            this.ResumeLayout(false);
        }

        private void CreateAdaptiveLayout()
        {
            // Главный контейнер с табличной layout
            var mainTableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 4,
                Padding = new Padding(10),
                BackColor = SystemColors.Window
            };

            // Настройка колонок
            mainTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            mainTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 10F));
            mainTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));

            // Настройка строк
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 200F));
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // Создаем и инициализируем контролы ПЕРЕД добавлением на форму
            InitializeControls();

            // === ПАНЕЛЬ ВВОДА ПАРАМЕТРОВ ===
            var inputPanel = CreateInputPanel();
            mainTableLayout.Controls.Add(inputPanel, 0, 0);
            mainTableLayout.SetColumnSpan(inputPanel, 3);

            // === СПИСОК ДЕТАЛЕЙ ===
            var partsPanel = CreatePartsPanel();
            mainTableLayout.Controls.Add(partsPanel, 0, 1);
            mainTableLayout.SetColumnSpan(partsPanel, 3);

            // === КНОПКИ УПРАВЛЕНИЯ ===
            var buttonsPanel = CreateButtonsPanel();
            mainTableLayout.Controls.Add(buttonsPanel, 0, 2);
            mainTableLayout.SetColumnSpan(buttonsPanel, 3);

            // === ПАНЕЛЬ РЕЗУЛЬТАТОВ ===
            var resultsPanel = CreateResultsPanel();
            mainTableLayout.Controls.Add(resultsPanel, 0, 3);
            mainTableLayout.SetColumnSpan(resultsPanel, 3);

            this.Controls.Add(mainTableLayout);
        }

        private void InitializeControls()
        {
            // Инициализация всех контролов как полей класса
            this.txtMaterialLength = new TextBox { Text = "6000" };
            this.txtGap = new TextBox { Text = "5" };
            this.txtPartLength = new TextBox();
            this.txtQuantity = new TextBox { Text = "1" };

            this.btnAddPart = new Button { Text = "➕ Добавить" };
            this.btnQuickAdd = new Button { Text = "📋 Быстрое добавление" };
            this.btnRemoveSelected = new Button { Text = "❌ Удалить выбранную" };
            this.btnCalculate = new Button { Text = "🎯 Рассчитать раскрой" };
            this.btnClear = new Button { Text = "🗑️ Очистить все" };

            this.dataGridViewParts = new DataGridView();
            this.txtResults = new TextBox { Multiline = true, ReadOnly = true };
            this.panelVisualization = new Panel { AutoScroll = true };

            this.labelQuantity = new Label { Text = "Количество:" };

            // Привязка событий
            this.btnAddPart.Click += btnAddPart_Click;
            this.btnQuickAdd.Click += btnQuickAdd_Click;
            this.btnRemoveSelected.Click += btnRemoveSelected_Click;
            this.btnCalculate.Click += btnCalculate_Click;
            this.btnClear.Click += btnClear_Click;
        }

        private Panel CreateInputPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10)
            };

            var inputTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 2
            };

            inputTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            inputTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            inputTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            inputTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            inputTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            inputTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // Первая строка - основные параметры
            var label1 = new Label { Text = "Длина материала:", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            this.txtMaterialLength.Dock = DockStyle.Fill;
            this.txtMaterialLength.Margin = new Padding(0, 2, 0, 2);

            var label2 = new Label { Text = "Зазор:", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            this.txtGap.Dock = DockStyle.Fill;
            this.txtGap.Margin = new Padding(0, 2, 0, 2);

            inputTable.Controls.Add(label1, 0, 0);
            inputTable.Controls.Add(this.txtMaterialLength, 1, 0);
            inputTable.Controls.Add(label2, 3, 0);
            inputTable.Controls.Add(this.txtGap, 4, 0);

            // Вторая строка - ввод деталей
            var label3 = new Label { Text = "Длина детали:", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            this.txtPartLength.Dock = DockStyle.Fill;
            this.txtPartLength.Margin = new Padding(0, 2, 0, 2);

            this.labelQuantity.Dock = DockStyle.Fill;
            this.labelQuantity.TextAlign = ContentAlignment.MiddleLeft;
            this.labelQuantity.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            this.txtQuantity.Dock = DockStyle.Fill;
            this.txtQuantity.Margin = new Padding(0, 2, 0, 2);

            this.btnAddPart.Dock = DockStyle.Fill;
            this.btnAddPart.UseVisualStyleBackColor = true;
            this.btnAddPart.Margin = new Padding(5, 2, 0, 2);

            this.btnQuickAdd.Dock = DockStyle.Fill;
            this.btnQuickAdd.UseVisualStyleBackColor = true;
            this.btnQuickAdd.Margin = new Padding(5, 2, 0, 2);

            inputTable.Controls.Add(label3, 0, 1);
            inputTable.Controls.Add(this.txtPartLength, 1, 1);
            inputTable.Controls.Add(this.labelQuantity, 3, 1);
            inputTable.Controls.Add(this.txtQuantity, 4, 1);
            inputTable.Controls.Add(this.btnAddPart, 2, 1);
            inputTable.Controls.Add(this.btnQuickAdd, 5, 1);

            panel.Controls.Add(inputTable);
            return panel;
        }

        private Panel CreatePartsPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10)
            };

            var label = new Label
            {
                Text = "Список деталей:",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Height = 20,
                Margin = new Padding(0, 0, 0, 5)
            };

            this.dataGridViewParts.Dock = DockStyle.Fill;
            this.dataGridViewParts.BackgroundColor = Color.White;
            this.dataGridViewParts.BorderStyle = BorderStyle.Fixed3D;
            this.dataGridViewParts.AllowUserToAddRows = false;
            this.dataGridViewParts.AllowUserToDeleteRows = false;
            this.dataGridViewParts.ReadOnly = false;
            this.dataGridViewParts.RowHeadersVisible = false;
            this.dataGridViewParts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewParts.Margin = new Padding(0, 25, 0, 0);

            panel.Controls.Add(this.dataGridViewParts);
            panel.Controls.Add(label);

            return panel;
        }

        private Panel CreateButtonsPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            var flowLayout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(10, 5, 10, 5)
            };

            // Настройка стилей кнопок
            this.btnCalculate.Size = new Size(150, 30);
            this.btnCalculate.BackColor = Color.SteelBlue;
            this.btnCalculate.ForeColor = Color.White;
            this.btnCalculate.FlatStyle = FlatStyle.Flat;
            this.btnCalculate.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            this.btnClear.Size = new Size(120, 30);
            this.btnClear.BackColor = Color.IndianRed;
            this.btnClear.ForeColor = Color.White;
            this.btnClear.FlatStyle = FlatStyle.Flat;

            this.btnRemoveSelected.Size = new Size(140, 30);
            this.btnRemoveSelected.BackColor = Color.Orange;
            this.btnRemoveSelected.ForeColor = Color.White;
            this.btnRemoveSelected.FlatStyle = FlatStyle.Flat;

            // Применение стилей
            StyleButton(this.btnCalculate);
            StyleButton(this.btnClear);
            StyleButton(this.btnRemoveSelected);

            flowLayout.Controls.Add(this.btnCalculate);
            flowLayout.Controls.Add(this.btnClear);
            flowLayout.Controls.Add(this.btnRemoveSelected);

            // Информационная метка
            var infoLabel = new Label
            {
                Text = "💡 Добавьте детали и нажмите 'Рассчитать' для оптимизации раскроя",
                AutoSize = true,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8F),
                Anchor = AnchorStyles.Right
            };

            panel.Controls.Add(flowLayout);
            panel.Controls.Add(infoLabel);

            // Позиционирование информационной метки
            infoLabel.Location = new Point(panel.Width - infoLabel.Width - 10, 10);

            return panel;
        }

        private void StyleButton(Button button)
        {
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = ControlPaint.Light(button.BackColor);
            button.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(button.BackColor);
        }

        private Panel CreateResultsPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            // Используем TableLayoutPanel вместо SplitContainer для надежности
            var resultsTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                ColumnStyles =
                {
                    new ColumnStyle(SizeType.Percent, 40F),
                    new ColumnStyle(SizeType.Percent, 60F)
                }
            };

            // Левая панель - текстовые результаты
            var leftPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10),
                Margin = new Padding(0, 0, 5, 0)
            };

            var resultsLabel = new Label
            {
                Text = "📊 Результаты расчета:",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Height = 25,
                Margin = new Padding(0, 0, 0, 5)
            };

            this.txtResults.Dock = DockStyle.Fill;
            this.txtResults.Multiline = true;
            this.txtResults.ScrollBars = ScrollBars.Vertical;
            this.txtResults.Font = new Font("Consolas", 9F);
            this.txtResults.BackColor = Color.AliceBlue;
            this.txtResults.ReadOnly = true;
            this.txtResults.Margin = new Padding(0, 30, 0, 0);

            leftPanel.Controls.Add(this.txtResults);
            leftPanel.Controls.Add(resultsLabel);

            // Правая панель - графическое представление
            var rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10),
                Margin = new Padding(5, 0, 0, 0)
            };

            var visualLabel = new Label
            {
                Text = "📐 Графическое отображение раскроя:",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Height = 25,
                Margin = new Padding(0, 0, 0, 5)
            };

            this.panelVisualization.Dock = DockStyle.Fill;
            this.panelVisualization.AutoScroll = true;
            this.panelVisualization.BackColor = Color.WhiteSmoke;
            this.panelVisualization.BorderStyle = BorderStyle.Fixed3D;
            this.panelVisualization.Margin = new Padding(0, 30, 0, 0);

            rightPanel.Controls.Add(this.panelVisualization);
            rightPanel.Controls.Add(visualLabel);

            resultsTable.Controls.Add(leftPanel, 0, 0);
            resultsTable.Controls.Add(rightPanel, 1, 0);

            panel.Controls.Add(resultsTable);
            return panel;
        }

        // Объявления для доступа к контролам из кода
        private TextBox txtMaterialLength;
        private TextBox txtGap;
        private TextBox txtPartLength;
        private TextBox txtQuantity;
        private Label labelQuantity;
        private Button btnAddPart;
        private Button btnQuickAdd;
        private Button btnRemoveSelected;
        private DataGridView dataGridViewParts;
        private Button btnCalculate;
        private Button btnClear;
        private TextBox txtResults;
        private Panel panelVisualization;
    }
}