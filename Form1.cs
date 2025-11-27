using LinearCuttingOptimizer;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using System.Windows.Forms;

namespace LinearCutting
{
    public partial class Form1 : Form
    {
        private List<PartItem> partItems = new List<PartItem>();

        public Form1()
        {
            InitializeComponent();
            InitializeDataGridView();
        }

        private void InitializeDataGridView()
        {
            // Настройка DataGridView для ввода деталей с количеством
            dataGridViewParts.Columns.Add("Length", "Длина детали");
            dataGridViewParts.Columns.Add("Quantity", "Количество");
            dataGridViewParts.Columns.Add("TotalLength", "Общая длина");

            dataGridViewParts.Columns["Length"].ValueType = typeof(double);
            dataGridViewParts.Columns["Quantity"].ValueType = typeof(int);
            dataGridViewParts.Columns["TotalLength"].ValueType = typeof(double);
            dataGridViewParts.Columns["TotalLength"].ReadOnly = true;

            // Стилизация DataGridView
            dataGridViewParts.BackgroundColor = Color.White;
            dataGridViewParts.BorderStyle = BorderStyle.None;
            dataGridViewParts.EnableHeadersVisualStyles = false;
            dataGridViewParts.Font = new Font("Segoe UI", 9F);

            // Стиль заголовков
            dataGridViewParts.ColumnHeadersDefaultCellStyle.BackColor = Color.SteelBlue;
            dataGridViewParts.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridViewParts.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGridViewParts.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            dataGridViewParts.ColumnHeadersHeight = 30;

            // Стиль строк
            dataGridViewParts.RowsDefaultCellStyle.BackColor = Color.White;
            dataGridViewParts.AlternatingRowsDefaultCellStyle.BackColor = Color.AliceBlue;

            dataGridViewParts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewParts.AllowUserToResizeRows = false;

            // Добавляем обработчик для автоматического пересчета общей длины
            dataGridViewParts.CellValueChanged += DataGridViewParts_CellValueChanged;

            // Обработчик для форматирования ячеек
            dataGridViewParts.CellFormatting += DataGridViewParts_CellFormatting;
        }
        private void DataGridViewParts_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // Форматирование числовых значений
                if (dataGridViewParts.Columns[e.ColumnIndex].Name == "Length" && e.Value != null)
                {
                    if (double.TryParse(e.Value.ToString(), out double value))
                    {
                        e.Value = value.ToString("F1");
                        e.FormattingApplied = true;
                    }
                }
                else if (dataGridViewParts.Columns[e.ColumnIndex].Name == "TotalLength" && e.Value != null)
                {
                    if (double.TryParse(e.Value.ToString(), out double value))
                    {
                        e.Value = value.ToString("F1");
                        e.FormattingApplied = true;
                    }
                }
            }
        }
        private void DataGridViewParts_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && (e.ColumnIndex == 0 || e.ColumnIndex == 1))
            {
                RecalculateTotalLength(e.RowIndex);
            }
        }

        private void RecalculateTotalLength(int rowIndex)
        {
            if (double.TryParse(dataGridViewParts.Rows[rowIndex].Cells["Length"].Value?.ToString(), out double length) &&
                int.TryParse(dataGridViewParts.Rows[rowIndex].Cells["Quantity"].Value?.ToString(), out int quantity))
            {
                dataGridViewParts.Rows[rowIndex].Cells["TotalLength"].Value = (length * quantity).ToString("F2");
            }
        }

        private void btnAddPart_Click(object sender, EventArgs e)
        {
            if (double.TryParse(txtPartLength.Text, out double length) && length > 0 &&
                int.TryParse(txtQuantity.Text, out int quantity) && quantity > 0)
            {
                // Проверяем, есть ли уже деталь такой длины
                var existingPart = partItems.FirstOrDefault(p => p.Length == length);
                if (existingPart != null)
                {
                    // Если есть - увеличиваем количество
                    existingPart.Quantity += quantity;
                    // Обновляем строку в DataGridView
                    UpdateDataGridViewRow(existingPart);
                }
                else
                {
                    // Если нет - добавляем новую деталь
                    var newPart = new PartItem { Length = length, Quantity = quantity };
                    partItems.Add(newPart);
                    dataGridViewParts.Rows.Add(
                        length.ToString("F2"),
                        quantity.ToString(),
                        (length * quantity).ToString("F2"));
                }

                txtPartLength.Clear();
                txtQuantity.Clear();
                txtPartLength.Focus();
            }
            else
            {
                MessageBox.Show("Введите корректную длину детали и количество", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateDataGridViewRow(PartItem part)
        {
            for (int i = 0; i < dataGridViewParts.Rows.Count; i++)
            {
                if (double.TryParse(dataGridViewParts.Rows[i].Cells["Length"].Value?.ToString(), out double rowLength) &&
                    rowLength == part.Length)
                {
                    dataGridViewParts.Rows[i].Cells["Quantity"].Value = part.Quantity.ToString();
                    dataGridViewParts.Rows[i].Cells["TotalLength"].Value = (part.Length * part.Quantity).ToString("F2");
                    break;
                }
            }
        }

        private List<double> GetAllPartLengths()
        {
            var allLengths = new List<double>();
            foreach (var part in partItems)
            {
                for (int i = 0; i < part.Quantity; i++)
                {
                    allLengths.Add(part.Length);
                }
            }
            return allLengths;
        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            double materialLength = double.Parse(txtMaterialLength.Text);
            double gap = double.Parse(txtGap.Text);

            // Получаем все детали как плоский список
            var allPartLengths = GetAllPartLengths();

            var optimizer = new CuttingOptimizer();
            var result = optimizer.OptimizeCutting(allPartLengths, materialLength, gap);

            DisplayResults(result, materialLength);
        }

        private bool ValidateInputs()
        {
            if (!double.TryParse(txtMaterialLength.Text, out double materialLength) || materialLength <= 0)
            {
                MessageBox.Show("Введите корректную длину материала", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!double.TryParse(txtGap.Text, out double gap) || gap < 0)
            {
                MessageBox.Show("Введите корректный зазор", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (partItems.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы одну деталь", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Проверяем, что ни одна деталь не длиннее материала
            double maxPartLength = partItems.Max(p => p.Length);
            if (maxPartLength > materialLength)
            {
                MessageBox.Show($"Деталь длиной {maxPartLength} не помещается в материал {materialLength}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void DisplayResults(CuttingResult result, double materialLength)
        {
            // Отображение текстовых результатов
            txtResults.Clear();
            txtResults.AppendText($"Всего материалов: {result.Rods.Count}\r\n");
            txtResults.AppendText($"Общая эффективность: {result.Efficiency:P2}\r\n");
            txtResults.AppendText($"Отходы: {result.Waste:F2} единиц\r\n\r\n");

            for (int i = 0; i < result.Rods.Count; i++)
            {
                var rod = result.Rods[i];
                double waste = materialLength - rod.UsedLength;
                txtResults.AppendText($"Материал {i + 1}:\r\n");
                txtResults.AppendText($"  Использовано: {rod.UsedLength:F2} / {materialLength:F2}\r\n");
                txtResults.AppendText($"  Отходы: {waste:F2}\r\n");

                // Группируем детали по длинам для удобства чтения
                var groupedParts = rod.Parts.GroupBy(p => p)
                    .Select(g => new { Length = g.Key, Count = g.Count() })
                    .OrderByDescending(g => g.Length);

                txtResults.AppendText($"  Детали: {string.Join(", ", groupedParts.Select(g => $"{g.Length:F2}×{g.Count}"))}\r\n\r\n");
            }

            // Отображение графического представления
            DisplayGraphicalView(result, materialLength);
        }

        private void DisplayGraphicalView(CuttingResult result, double materialLength)
        {
            panelVisualization.Controls.Clear();

            int rodHeight = 60;
            int spacing = 20;
            int currentY = 10;

            foreach (var rod in result.Rods)
            {
                // Создаем панель для одного материала
                var rodPanel = new Panel
                {
                    Width = panelVisualization.Width - 20,
                    Height = rodHeight,
                    Location = new Point(10, currentY),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.LightBlue
                };

                // Добавляем заголовок
                var label = new Label
                {
                    Text = $"Материал {result.Rods.IndexOf(rod) + 1}: {rod.UsedLength:F2}/{materialLength:F2} (отходы: {materialLength - rod.UsedLength:F2})",
                    Location = new Point(5, 5),
                    AutoSize = true,
                    BackColor = Color.Transparent,
                    Font = new Font("Arial", 8, FontStyle.Bold)
                };
                rodPanel.Controls.Add(label);

                // Отображаем детали
                DisplayPartsOnRod(rodPanel, rod, materialLength);

                panelVisualization.Controls.Add(rodPanel);
                currentY += rodHeight + spacing;
            }
        }

        private void DisplayPartsOnRod(Panel rodPanel, CuttingRod rod, double materialLength)
        {
            int panelWidth = rodPanel.Width - 200; // Оставляем больше места для текста
            int currentX = 200;
            int partHeight = 30;
            int partY = (rodPanel.Height - partHeight) / 2;

            // Группируем детали по длинам для одинакового цвета
            var groupedParts = rod.Parts.GroupBy(p => p).ToList();
            var colorMap = new Dictionary<double, Color>();
            Random rand = new Random();

            foreach (var group in groupedParts)
            {
                colorMap[group.Key] = Color.FromArgb(rand.Next(150, 255), rand.Next(150, 255), rand.Next(150, 255));
            }

            foreach (var part in rod.Parts)
            {
                // Вычисляем ширину части пропорционально длине
                int partWidth = (int)((part / materialLength) * panelWidth);

                if (partWidth < 3) partWidth = 3; // Минимальная ширина

                var partPanel = new Panel
                {
                    Width = partWidth,
                    Height = partHeight,
                    Location = new Point(currentX, partY),
                    BackColor = colorMap[part],
                    BorderStyle = BorderStyle.FixedSingle
                };

                // Подпись на детали (только если достаточно места)
                if (partWidth > 25)
                {
                    var partLabel = new Label
                    {
                        Text = part.ToString("F0"),
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Font = new Font("Arial", 7),
                        ForeColor = Color.Black
                    };
                    partPanel.Controls.Add(partLabel);
                }

                rodPanel.Controls.Add(partPanel);
                currentX += partWidth + 2; // Небольшой отступ между деталями
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            partItems.Clear();
            dataGridViewParts.Rows.Clear();
            txtResults.Clear();
            panelVisualization.Controls.Clear();
        }

        private void btnRemoveSelected_Click(object sender, EventArgs e)
        {
            if (dataGridViewParts.CurrentRow != null)
            {
                int index = dataGridViewParts.CurrentRow.Index;
                if (index < partItems.Count)
                {
                    partItems.RemoveAt(index);
                    dataGridViewParts.Rows.RemoveAt(index);
                }
            }
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtResults.Text))
            {
                MessageBox.Show("Нет данных для сохранения. Сначала выполните расчет.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Файлы CSV (*.csv)|*.csv|Все файлы (*.*)|*.*";
                saveDialog.FilterIndex = 1;
                saveDialog.Title = "Сохранить результаты раскроя";
                saveDialog.DefaultExt = "txt";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string extension = Path.GetExtension(saveDialog.FileName).ToLower();

                        switch (extension)
                        {
                            case ".txt":
                                SaveAsText(saveDialog.FileName);
                                break;
                            case ".csv":
                                SaveAsCsv(saveDialog.FileName);
                                break;
                            default:
                                SaveAsText(saveDialog.FileName);
                                break;
                        }

                        MessageBox.Show($"Результаты успешно сохранены в файл:\n{saveDialog.FileName}",
                            "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при сохранении файла:\n{ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void SaveAsText(string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ОТЧЕТ ПО РАСКРОЮ МАТЕРИАЛА");
            sb.AppendLine("============================");
            sb.AppendLine($"Дата создания: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            sb.AppendLine($"Длина материала: {txtMaterialLength.Text} мм");
            sb.AppendLine($"Зазор между деталями: {txtGap.Text} мм");
            sb.AppendLine();

            // Добавляем информацию о деталях
            sb.AppendLine("РАСКРАИВАЕМЫЕ ДЕТАЛИ:");
            sb.AppendLine("---------------------");
            foreach (DataGridViewRow row in dataGridViewParts.Rows)
            {
                if (row.IsNewRow) continue;
                string length = row.Cells["Length"].Value?.ToString() ?? "0";
                string quantity = row.Cells["Quantity"].Value?.ToString() ?? "0";
                string total = row.Cells["TotalLength"].Value?.ToString() ?? "0";
                sb.AppendLine($"  Деталь: {length} мм × {quantity} шт. = {total} мм");
            }
            sb.AppendLine();

            // Добавляем результаты расчета
            sb.AppendLine(txtResults.Text);

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private void SaveAsCsv(string filePath)
        {
            var sb = new StringBuilder();

            // Заголовок
            sb.AppendLine("Отчет по раскрою материала");
            sb.AppendLine($"Дата создания;{DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            sb.AppendLine($"Длина материала;{txtMaterialLength.Text} мм");
            sb.AppendLine($"Зазор между деталями;{txtGap.Text} мм");
            sb.AppendLine();

            // Детали
            sb.AppendLine("Детали для раскроя");
            sb.AppendLine("Длина;Количество;Общая длина");
            foreach (DataGridViewRow row in dataGridViewParts.Rows)
            {
                if (row.IsNewRow) continue;
                string length = row.Cells["Length"].Value?.ToString() ?? "0";
                string quantity = row.Cells["Quantity"].Value?.ToString() ?? "0";
                string total = row.Cells["TotalLength"].Value?.ToString() ?? "0";
                sb.AppendLine($"{length};{quantity};{total}");
            }
            sb.AppendLine();

            // Результаты
            sb.AppendLine("Результаты раскроя");
            sb.AppendLine("Номер материала;Использовано;Отходы;Детали");

            // Парсим текстовые результаты для CSV
            string[] resultLines = txtResults.Text.Split('\n');
            bool inResultsSection = false;

            foreach (string line in resultLines)
            {
                if (line.Contains("Материал"))
                {
                    inResultsSection = true;
                    // Извлекаем данные из строки вида "Материал 1:"
                    string materialInfo = line.Replace("Материал", "").Replace(":", "").Trim();
                    sb.Append($"{materialInfo};");
                }
                else if (inResultsSection && line.Contains("Использовано:"))
                {
                    string usedInfo = line.Replace("Использовано:", "").Trim();
                    sb.Append($"{usedInfo};");
                }
                else if (inResultsSection && line.Contains("Отходы:"))
                {
                    string wasteInfo = line.Replace("Отходы:", "").Trim();
                    sb.Append($"{wasteInfo};");
                }
                else if (inResultsSection && line.Contains("Детали:"))
                {
                    string partsInfo = line.Replace("Детали:", "").Trim();
                    sb.AppendLine($"{partsInfo}");
                    inResultsSection = false;
                }
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }
        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtResults.Text))
            {
                MessageBox.Show("Нет данных для печати. Сначала выполните расчет.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var printDialog = new PrintDialog())
            using (var printDocument = new PrintDocument())
            {
                printDocument.DocumentName = "Отчет по раскрою материала";
                printDocument.PrintPage += PrintDocument_PrintPage;

                printDialog.Document = printDocument;
                printDialog.AllowSomePages = true;
                printDialog.ShowHelp = true;

                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        printDocument.Print();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при печати:\n{ex.Message}", "Ошибка печати",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            var graphics = e.Graphics;
            var margin = 50;
            var currentY = margin;
            var pageWidth = e.PageBounds.Width - 2 * margin;

            // Шрифты
            var titleFont = new Font("Arial", 16, FontStyle.Bold);
            var headerFont = new Font("Arial", 12, FontStyle.Bold);
            var normalFont = new Font("Arial", 10);
            var smallFont = new Font("Arial", 9);

            // Заголовок
            graphics.DrawString("ОТЧЕТ ПО РАСКРОЮ МАТЕРИАЛА", titleFont, Brushes.Black, margin, currentY);
            currentY += 40;

            // Информация о параметрах
            graphics.DrawString($"Дата создания: {DateTime.Now:dd.MM.yyyy HH:mm:ss}", normalFont, Brushes.Black, margin, currentY);
            currentY += 20;
            graphics.DrawString($"Длина материала: {txtMaterialLength.Text} мм", normalFont, Brushes.Black, margin, currentY);
            currentY += 20;
            graphics.DrawString($"Зазор между деталями: {txtGap.Text} мм", normalFont, Brushes.Black, margin, currentY);
            currentY += 30;

            // Детали
            graphics.DrawString("ДЕТАЛИ ДЛЯ РАСКРОЯ:", headerFont, Brushes.Black, margin, currentY);
            currentY += 25;

            // Таблица деталей
            graphics.DrawString("Длина", headerFont, Brushes.Black, margin, currentY);
            graphics.DrawString("Кол-во", headerFont, Brushes.Black, margin + 100, currentY);
            graphics.DrawString("Общая длина", headerFont, Brushes.Black, margin + 180, currentY);
            currentY += 20;

            graphics.DrawLine(Pens.Black, margin, currentY, margin + 300, currentY);
            currentY += 10;

            foreach (DataGridViewRow row in dataGridViewParts.Rows)
            {
                if (row.IsNewRow) continue;

                string length = row.Cells["Length"].Value?.ToString() ?? "0";
                string quantity = row.Cells["Quantity"].Value?.ToString() ?? "0";
                string total = row.Cells["TotalLength"].Value?.ToString() ?? "0";

                graphics.DrawString(length, normalFont, Brushes.Black, margin, currentY);
                graphics.DrawString(quantity, normalFont, Brushes.Black, margin + 100, currentY);
                graphics.DrawString(total, normalFont, Brushes.Black, margin + 180, currentY);
                currentY += 18;

                // Проверка на конец страницы
                if (currentY > e.PageBounds.Height - margin - 100)
                {
                    e.HasMorePages = true;
                    return;
                }
            }

            currentY += 20;

            // Результаты
            graphics.DrawString("РЕЗУЛЬТАТЫ РАСКРОЯ:", headerFont, Brushes.Black, margin, currentY);
            currentY += 25;

            // Разбиваем текстовые результаты на строки для печати
            string[] resultLines = txtResults.Text.Split('\n');
            foreach (string line in resultLines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                graphics.DrawString(line, normalFont, Brushes.Black, margin, currentY);
                currentY += 16;

                // Проверка на конец страницы
                if (currentY > e.PageBounds.Height - margin - 50)
                {
                    e.HasMorePages = true;
                    return;
                }
            }

            // Подпись в конце
            currentY += 20;
            graphics.DrawString("--- Конец отчета ---", smallFont, Brushes.Gray, margin, currentY);

            e.HasMorePages = false;
        }

        private void btnQuickAdd_Click(object sender, EventArgs e)
        {
            // Быстрое добавление распространенных деталей
            var quickForm = new QuickAddForm();
            if (quickForm.ShowDialog() == DialogResult.OK)
            {
                foreach (var part in quickForm.SelectedParts)
                {
                    var existingPart = partItems.FirstOrDefault(p => p.Length == part.Length);
                    if (existingPart != null)
                    {
                        existingPart.Quantity += part.Quantity;
                        UpdateDataGridViewRow(existingPart);
                    }
                    else
                    {
                        partItems.Add(part);
                        dataGridViewParts.Rows.Add(
                            part.Length.ToString("F2"),
                            part.Quantity.ToString(),
                            (part.Length * part.Quantity).ToString("F2"));
                    }
                }
            }
        }
    }

    // Класс для хранения информации о детали
    public class PartItem
    {
        public double Length { get; set; }
        public int Quantity { get; set; }
    }
}