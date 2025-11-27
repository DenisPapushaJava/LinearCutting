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

            var allPartLengths = GetAllPartLengths();

            var optimizer = new CuttingOptimizer();
            var result = optimizer.OptimizeCuttingWithGrouping(allPartLengths, materialLength, gap);

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
            // Отображение текстовых результатов с учетом группировки
            txtResults.Clear();

            var stats = result.RodGroups.Sum(g => g.Count);
            txtResults.AppendText($"=== ОБЩАЯ СВОДКА ===\r\n");
            txtResults.AppendText($"Всего материалов: {stats}\r\n");
            txtResults.AppendText($"Уникальных раскроев: {result.RodGroups.Count}\r\n");
            txtResults.AppendText($"Общая эффективность: {result.Efficiency:P2}\r\n");
            txtResults.AppendText($"Общие отходы: {result.Waste:F2} мм\r\n\r\n");

            txtResults.AppendText($"=== ДЕТАЛИЗАЦИЯ РАСКРОЕВ ===\r\n\r\n");

            for (int i = 0; i < result.RodGroups.Count; i++)
            {
                var group = result.RodGroups[i];
                var rod = group.Rod;
                double waste = materialLength - rod.UsedLength;
                double efficiency = rod.UsedLength / materialLength;

                txtResults.AppendText($"РАСКРОЙ #{i + 1}");
                if (group.Count > 1)
                {
                    txtResults.AppendText($" [Повторяется ×{group.Count}]");
                }
                txtResults.AppendText("\r\n");
                txtResults.AppendText($"Эффективность: {efficiency:P1}\r\n");
                txtResults.AppendText($"Использовано: {rod.UsedLength:F2} / {materialLength:F2} мм\r\n");
                txtResults.AppendText($"Отходы: {waste:F2} мм\r\n");

                // Группируем детали по длинам для удобства чтения
                var groupedParts = rod.Parts.GroupBy(p => p)
                    .Select(g => new { Length = g.Key, Count = g.Count() })
                    .OrderByDescending(g => g.Length);

                txtResults.AppendText($"Детали: {string.Join(", ", groupedParts.Select(g => $"{g.Length:F2} мм × {g.Count}"))}\r\n");

                // Если есть повторения, показываем суммарную статистику
                if (group.Count > 1)
                {
                    txtResults.AppendText($"--- Суммарно для {group.Count} повторов ---\r\n");
                    txtResults.AppendText($"Общее использование: {rod.UsedLength * group.Count:F2} мм\r\n");
                    txtResults.AppendText($"Общие отходы: {waste * group.Count:F2} мм\r\n");
                }

                txtResults.AppendText("\r\n");
            }

            // Отображение графического представления
            DisplayGraphicalView(result, materialLength);
        }

        private void DisplayGraphicalView(CuttingResult result, double materialLength)
        {
            panelVisualization.Controls.Clear();

            int rodHeight = 70;
            int spacing = 15;
            int currentY = 10;

            foreach (var group in result.RodGroups)
            {
                var rod = group.Rod;

                // Создаем панель для группы раскроев
                var groupPanel = new Panel
                {
                    Width = panelVisualization.Width - 20,
                    Height = rodHeight + (group.Count > 1 ? 25 : 0),
                    Location = new Point(10, currentY),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.LightBlue
                };

                // Заголовок с информацией о группе
                var headerLabel = new Label
                {
                    Text = $"Раскрой: {rod.UsedLength:F1}/{materialLength:F1} мм" +
                           (group.Count > 1 ? $" [×{group.Count}]" : ""),
                    Location = new Point(5, 5),
                    AutoSize = true,
                    BackColor = Color.Transparent,
                    Font = new Font("Arial", 9, FontStyle.Bold),
                    ForeColor = group.Count > 1 ? Color.DarkRed : Color.Black
                };
                groupPanel.Controls.Add(headerLabel);

                // Отображаем детали
                DisplayPartsOnRod(groupPanel, rod, materialLength, group.Count > 1);

                panelVisualization.Controls.Add(groupPanel);
                currentY += groupPanel.Height + spacing;
            }
        }

        private void DisplayPartsOnRod(Panel rodPanel, CuttingRod rod, double materialLength, bool isGrouped)
        {
            int panelWidth = rodPanel.Width - 200;
            int currentX = 200;
            int partHeight = 30;
            int partY = 30; // Смещаем вниз для заголовка

            Random rand = new Random();

            foreach (var part in rod.Parts)
            {
                // Вычисляем ширину части пропорционально длине
                int partWidth = (int)((part / materialLength) * panelWidth);

                if (partWidth < 3) partWidth = 3;

                var partPanel = new Panel
                {
                    Width = partWidth,
                    Height = partHeight,
                    Location = new Point(currentX, partY),
                    BackColor = Color.FromArgb(rand.Next(150, 255), rand.Next(150, 255), rand.Next(150, 255)),
                    BorderStyle = BorderStyle.FixedSingle
                };

                // Подпись на детали
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
                currentX += partWidth + 2;
            }

            // Если это группировка, добавляем визуальный индикатор
            if (isGrouped)
            {
                var groupIndicator = new Label
                {
                    Text = "🔄 Повторяющийся раскрой",
                    Location = new Point(5, rodPanel.Height - 20),
                    AutoSize = true,
                    ForeColor = Color.DarkGreen,
                    Font = new Font("Arial", 8, FontStyle.Italic)
                };
                rodPanel.Controls.Add(groupIndicator);
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
                saveDialog.Filter = "HTML файлы (*.html)|*.html|Все файлы (*.*)|*.*";
                saveDialog.FilterIndex = 1;
                saveDialog.Title = "Сохранить результаты раскроя";
                saveDialog.DefaultExt = "html";
                saveDialog.FileName = $"раскрой_{DateTime.Now:yyyyMMdd_HHmmss}.html";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {

                    try
                    {
                        SaveAsHtml(saveDialog.FileName);

                        // Показываем опцию открытия файла
                        var result = MessageBox.Show($"HTML отчет успешно сохранен!\n\n{saveDialog.FileName}\n\nХотите открыть файл?",
                            "Успех", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                        if (result == DialogResult.Yes)
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = saveDialog.FileName,
                                UseShellExecute = true
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при сохранении HTML файла:\n{ex.Message}", "Ошибка",
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

        private void SaveAsHtml(string filePath)
        {
            double materialLength = double.Parse(txtMaterialLength.Text);
            double gap = double.Parse(txtGap.Text);
            var allPartLengths = GetAllPartLengths();
            var optimizer = new CuttingOptimizer();
            var result = optimizer.OptimizeCuttingWithGrouping(allPartLengths, materialLength, gap);

            var sb = new StringBuilder();

            // Начало HTML документа
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang='ru'>");
            sb.AppendLine("<head>");
            sb.AppendLine("    <meta charset='utf-8'>");
            sb.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1'>");
            sb.AppendLine("    <title>Отчет по раскрою материала</title>");
            sb.AppendLine("    <style>");
            sb.AppendLine("        * { box-sizing: border-box; margin: 0; padding: 0; }");
            sb.AppendLine("        body { ");
            sb.AppendLine("            font-family: 'Segoe UI', Arial, sans-serif; ");
            sb.AppendLine("            line-height: 1.6; ");
            sb.AppendLine("            color: #333; ");
            sb.AppendLine("            background: #f5f5f5;");
            sb.AppendLine("            min-height: 100vh;");
            sb.AppendLine("            padding: 20px;");
            sb.AppendLine("        }");
            sb.AppendLine("        .container { ");
            sb.AppendLine("            max-width: 1200px; ");
            sb.AppendLine("            margin: 0 auto; ");
            sb.AppendLine("            background: white; ");
            sb.AppendLine("            border-radius: 10px; ");
            sb.AppendLine("            box-shadow: 0 5px 15px rgba(0,0,0,0.1); ");
            sb.AppendLine("            overflow: hidden;");
            sb.AppendLine("        }");
            sb.AppendLine("        .header { ");
            sb.AppendLine("            background: #2c3e50; ");
            sb.AppendLine("            color: white; ");
            sb.AppendLine("            padding: 25px; ");
            sb.AppendLine("            text-align: center; ");
            sb.AppendLine("        }");
            sb.AppendLine("        .header h1 { ");
            sb.AppendLine("            font-size: 2em; ");
            sb.AppendLine("            margin-bottom: 10px; ");
            sb.AppendLine("        }");
            sb.AppendLine("        .header .subtitle { ");
            sb.AppendLine("            font-size: 1.1em; ");
            sb.AppendLine("            opacity: 0.9; ");
            sb.AppendLine("        }");
            sb.AppendLine("        .content { padding: 25px; }");
            sb.AppendLine("        .section { ");
            sb.AppendLine("            margin-bottom: 25px; ");
            sb.AppendLine("            background: #f8f9fa; ");
            sb.AppendLine("            border-radius: 8px; ");
            sb.AppendLine("            padding: 20px; ");
            sb.AppendLine("            border-left: 4px solid #3498db;");
            sb.AppendLine("        }");
            sb.AppendLine("        .section h2 { ");
            sb.AppendLine("            color: #2c3e50; ");
            sb.AppendLine("            margin-bottom: 15px; ");
            sb.AppendLine("            font-size: 1.3em;");
            sb.AppendLine("        }");
            sb.AppendLine("        .params-grid { ");
            sb.AppendLine("            display: grid; ");
            sb.AppendLine("            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); ");
            sb.AppendLine("            gap: 12px; ");
            sb.AppendLine("            margin-bottom: 15px; ");
            sb.AppendLine("        }");
            sb.AppendLine("        .param-card { ");
            sb.AppendLine("            background: white; ");
            sb.AppendLine("            padding: 12px; ");
            sb.AppendLine("            border-radius: 6px; ");
            sb.AppendLine("            box-shadow: 0 1px 3px rgba(0,0,0,0.1); ");
            sb.AppendLine("            text-align: center; ");
            sb.AppendLine("        }");
            sb.AppendLine("        .param-value { ");
            sb.AppendLine("            font-size: 1.5em; ");
            sb.AppendLine("            font-weight: bold; ");
            sb.AppendLine("            color: #e74c3c; ");
            sb.AppendLine("            margin: 3px 0; ");
            sb.AppendLine("        }");
            sb.AppendLine("        .param-label { ");
            sb.AppendLine("            color: #7f8c8d; ");
            sb.AppendLine("            font-size: 0.85em; ");
            sb.AppendLine("        }");
            sb.AppendLine("        table { ");
            sb.AppendLine("            width: 100%; ");
            sb.AppendLine("            border-collapse: collapse; ");
            sb.AppendLine("            margin: 12px 0; ");
            sb.AppendLine("            background: white; ");
            sb.AppendLine("            border-radius: 6px; ");
            sb.AppendLine("            overflow: hidden; ");
            sb.AppendLine("            box-shadow: 0 1px 3px rgba(0,0,0,0.1); ");
            sb.AppendLine("        }");
            sb.AppendLine("        th { ");
            sb.AppendLine("            background: #34495e; ");
            sb.AppendLine("            color: white; ");
            sb.AppendLine("            padding: 10px; ");
            sb.AppendLine("            text-align: left; ");
            sb.AppendLine("            font-weight: 600; ");
            sb.AppendLine("        }");
            sb.AppendLine("        td { ");
            sb.AppendLine("            padding: 10px; ");
            sb.AppendLine("            border-bottom: 1px solid #ecf0f1; ");
            sb.AppendLine("        }");
            sb.AppendLine("        tr:hover { background: #f8f9fa; }");
            sb.AppendLine("        .rod { ");
            sb.AppendLine("            background: white; ");
            sb.AppendLine("            border-radius: 8px; ");
            sb.AppendLine("            padding: 15px; ");
            sb.AppendLine("            margin: 15px 0; ");
            sb.AppendLine("            box-shadow: 0 2px 5px rgba(0,0,0,0.1); ");
            sb.AppendLine("            border: 1px solid #ddd;");
            sb.AppendLine("        }");
            sb.AppendLine("        .rod-group { ");
            sb.AppendLine("            border: 2px solid #e74c3c; ");
            sb.AppendLine("            background: #fffaf0; ");
            sb.AppendLine("        }");
            sb.AppendLine("        .rod-header { ");
            sb.AppendLine("            font-size: 1.1em; ");
            sb.AppendLine("            font-weight: bold; ");
            sb.AppendLine("            margin-bottom: 10px; ");
            sb.AppendLine("            color: #2c3e50; ");
            sb.AppendLine("            display: flex; ");
            sb.AppendLine("            justify-content: space-between; ");
            sb.AppendLine("            align-items: center; ");
            sb.AppendLine("        }");
            sb.AppendLine("        .group-badge { ");
            sb.AppendLine("            background: #e74c3c; ");
            sb.AppendLine("            color: white; ");
            sb.AppendLine("            padding: 3px 10px; ");
            sb.AppendLine("            border-radius: 12px; ");
            sb.AppendLine("            font-size: 0.8em; ");
            sb.AppendLine("            font-weight: bold; ");
            sb.AppendLine("        }");
            sb.AppendLine("        .efficiency { ");
            sb.AppendLine("            background: #27ae60; ");
            sb.AppendLine("            color: white; ");
            sb.AppendLine("            padding: 3px 8px; ");
            sb.AppendLine("            border-radius: 12px; ");
            sb.AppendLine("            font-size: 0.8em; ");
            sb.AppendLine("        }");
            sb.AppendLine("        .parts-container { ");
            sb.AppendLine("            display: flex; ");
            sb.AppendLine("            height: 50px; ");
            sb.AppendLine("            background: #ecf0f1; ");
            sb.AppendLine("            border-radius: 6px; ");
            sb.AppendLine("            overflow: hidden; ");
            sb.AppendLine("            border: 2px solid #bdc3c7; ");
            sb.AppendLine("            margin: 10px 0; ");
            sb.AppendLine("        }");
            sb.AppendLine("        .part { ");
            sb.AppendLine("            height: 100%; ");
            sb.AppendLine("            display: flex; ");
            sb.AppendLine("            align-items: center; ");
            sb.AppendLine("            justify-content: center; ");
            sb.AppendLine("            color: black; ");
            sb.AppendLine("            font-weight: bold; ");
            sb.AppendLine("            font-size: 11px; ");
            sb.AppendLine("            border-right: 2px solid white; ");
            sb.AppendLine("            position: relative; ");
            sb.AppendLine("            cursor: help; ");
            sb.AppendLine("            border: 1px solid rgba(0,0,0,0.3); ");
            sb.AppendLine("            margin: -1px; ");
            sb.AppendLine("        }");
            sb.AppendLine("        .part:hover { ");
            sb.AppendLine("            opacity: 0.9; ");
            sb.AppendLine("        }");
            sb.AppendLine("        .part:last-child { border-right: none; }");
            sb.AppendLine("        .waste { ");
            sb.AppendLine("            background: #bdc3c7 !important; ");
            sb.AppendLine("            border-left: 2px dashed #e74c3c !important; ");
            sb.AppendLine("        }");
            sb.AppendLine("        .parts-list { ");
            sb.AppendLine("            margin-top: 8px; ");
            sb.AppendLine("            padding: 8px; ");
            sb.AppendLine("            background: #f8f9fa; ");
            sb.AppendLine("            border-radius: 4px; ");
            sb.AppendLine("            font-size: 0.9em; ");
            sb.AppendLine("        }");
            sb.AppendLine("        .group-summary { ");
            sb.AppendLine("            background: #ffeaa7; ");
            sb.AppendLine("            border: 1px dashed #e17055; ");
            sb.AppendLine("            border-radius: 6px; ");
            sb.AppendLine("            padding: 10px; ");
            sb.AppendLine("            margin-top: 10px; ");
            sb.AppendLine("            font-size: 0.85em; ");
            sb.AppendLine("        }");
            sb.AppendLine("        .summary { ");
            sb.AppendLine("            background: #3498db; ");
            sb.AppendLine("            color: white; ");
            sb.AppendLine("            padding: 20px; ");
            sb.AppendLine("            border-radius: 8px; ");
            sb.AppendLine("            text-align: center; ");
            sb.AppendLine("            margin-top: 20px; ");
            sb.AppendLine("        }");
            sb.AppendLine("        .summary h3 { ");
            sb.AppendLine("            margin-bottom: 12px; ");
            sb.AppendLine("            font-size: 1.3em; ");
            sb.AppendLine("        }");
            sb.AppendLine("        .stats { ");
            sb.AppendLine("            display: grid; ");
            sb.AppendLine("            grid-template-columns: repeat(auto-fit, minmax(150px, 1fr)); ");
            sb.AppendLine("            gap: 12px; ");
            sb.AppendLine("            margin-top: 12px; ");
            sb.AppendLine("        }");
            sb.AppendLine("        .stat-item { ");
            sb.AppendLine("            background: rgba(255,255,255,0.2); ");
            sb.AppendLine("            padding: 12px; ");
            sb.AppendLine("            border-radius: 6px; ");
            sb.AppendLine("        }");
            sb.AppendLine("        .stat-value { ");
            sb.AppendLine("            font-size: 1.5em; ");
            sb.AppendLine("            font-weight: bold; ");
            sb.AppendLine("            margin: 3px 0; ");
            sb.AppendLine("        }");
            sb.AppendLine("        .footer { ");
            sb.AppendLine("            text-align: center; ");
            sb.AppendLine("            padding: 15px; ");
            sb.AppendLine("            color: #7f8c8d; ");
            sb.AppendLine("            font-size: 0.85em; ");
            sb.AppendLine("            border-top: 1px solid #ecf0f1; ");
            sb.AppendLine("        }");
            sb.AppendLine("        .grouping-stats { ");
            sb.AppendLine("            background: #2ecc71; ");
            sb.AppendLine("            color: white; ");
            sb.AppendLine("            padding: 15px; ");
            sb.AppendLine("            border-radius: 8px; ");
            sb.AppendLine("            margin: 15px 0; ");
            sb.AppendLine("            text-align: center; ");
            sb.AppendLine("        }");
            sb.AppendLine("        .repeat-stats { ");
            sb.AppendLine("            display: grid; ");
            sb.AppendLine("            grid-template-columns: repeat(auto-fit, minmax(120px, 1fr)); ");
            sb.AppendLine("            gap: 10px; ");
            sb.AppendLine("            margin-top: 12px; ");
            sb.AppendLine("        }");
            sb.AppendLine("        .repeat-item { ");
            sb.AppendLine("            background: rgba(255,255,255,0.9); ");
            sb.AppendLine("            color: #2c3e50; ");
            sb.AppendLine("            padding: 10px; ");
            sb.AppendLine("            border-radius: 6px; ");
            sb.AppendLine("            text-align: center; ");
            sb.AppendLine("        }");
            sb.AppendLine("        @media (max-width: 768px) { ");
            sb.AppendLine("            .container { margin: 10px; }");
            sb.AppendLine("            .content { padding: 15px; }");
            sb.AppendLine("            .params-grid { grid-template-columns: 1fr; }");
            sb.AppendLine("            .rod-header { flex-direction: column; gap: 8px; }");
            sb.AppendLine("            .stats, .repeat-stats { grid-template-columns: 1fr 1fr; }");
            sb.AppendLine("        }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            // Контейнер
            sb.AppendLine("    <div class='container'>");

            // Шапка
            sb.AppendLine("        <div class='header'>");
            sb.AppendLine("            <h1>📐 Отчет по раскрою материала</h1>");
            sb.AppendLine($"           <div class='subtitle'>Сгенерировано {DateTime.Now:dd.MM.yyyy HH:mm:ss}</div>");
            sb.AppendLine("        </div>");

            // Основное содержимое
            sb.AppendLine("        <div class='content'>");

            // Параметры раскроя
            sb.AppendLine("            <div class='section'>");
            sb.AppendLine("                <h2>⚙️ Параметры раскроя</h2>");
            sb.AppendLine("                <div class='params-grid'>");
            sb.AppendLine("                    <div class='param-card'>");
            sb.AppendLine($"                       <div class='param-value'>{materialLength} мм</div>");
            sb.AppendLine("                        <div class='param-label'>Длина материала</div>");
            sb.AppendLine("                    </div>");
            sb.AppendLine("                    <div class='param-card'>");
            sb.AppendLine($"                       <div class='param-value'>{gap} мм</div>");
            sb.AppendLine("                        <div class='param-label'>Зазор между деталями</div>");
            sb.AppendLine("                    </div>");
            sb.AppendLine("                    <div class='param-card'>");
            sb.AppendLine($"                       <div class='param-value'>{allPartLengths.Count} шт</div>");
            sb.AppendLine("                        <div class='param-label'>Всего деталей</div>");
            sb.AppendLine("                    </div>");
            sb.AppendLine("                    <div class='param-card'>");
            sb.AppendLine($"                       <div class='param-value'>{allPartLengths.Sum():F1} мм</div>");
            sb.AppendLine("                        <div class='param-label'>Общая длина деталей</div>");
            sb.AppendLine("                    </div>");
            sb.AppendLine("                </div>");
            sb.AppendLine("            </div>");

            // Детали для раскроя
            sb.AppendLine("            <div class='section'>");
            sb.AppendLine("                <h2>📋 Детали для раскроя</h2>");
            sb.AppendLine("                <table>");
            sb.AppendLine("                    <tr><th>Длина (мм)</th><th>Количество</th><th>Общая длина (мм)</th></tr>");

            var partGroups = partItems.GroupBy(p => p.Length)
                                     .Select(g => new {
                                         Length = g.Key,
                                         Quantity = g.Sum(x => x.Quantity),
                                         TotalLength = g.Sum(x => x.Length * x.Quantity)
                                     })
                                     .OrderByDescending(g => g.Length);

            foreach (var part in partGroups)
            {
                sb.AppendLine($"                    <tr>");
                sb.AppendLine($"                        <td>{part.Length:F1}</td>");
                sb.AppendLine($"                        <td>{part.Quantity}</td>");
                sb.AppendLine($"                        <td>{part.TotalLength:F1}</td>");
                sb.AppendLine($"                    </tr>");
            }

            sb.AppendLine("                </table>");
            sb.AppendLine("            </div>");

            // Статистика группировки
            int totalRods = result.RodGroups.Sum(g => g.Count);
            int uniquePatterns = result.RodGroups.Count;
            int maxRepeats = result.RodGroups.Max(g => g.Count);

            sb.AppendLine("            <div class='grouping-stats'>");
            sb.AppendLine("                <h2>📊 Статистика группировки</h2>");
            sb.AppendLine("                <div class='stats'>");
            sb.AppendLine("                    <div class='stat-item'>");
            sb.AppendLine($"                       <div class='stat-value'>{totalRods}</div>");
            sb.AppendLine("                        <div>Всего материалов</div>");
            sb.AppendLine("                    </div>");
            sb.AppendLine("                    <div class='stat-item'>");
            sb.AppendLine($"                       <div class='stat-value'>{uniquePatterns}</div>");
            sb.AppendLine("                        <div>Уникальных раскроев</div>");
            sb.AppendLine("                    </div>");
            sb.AppendLine("                    <div class='stat-item'>");
            sb.AppendLine($"                       <div class='stat-value'>{maxRepeats}</div>");
            sb.AppendLine("                        <div>Макс. повторений</div>");
            sb.AppendLine("                    </div>");
            sb.AppendLine("                    <div class='stat-item'>");
            sb.AppendLine($"                       <div class='stat-value'>{result.Efficiency:P1}</div>");
            sb.AppendLine("                        <div>Общая эффективность</div>");
            sb.AppendLine("                    </div>");
            sb.AppendLine("                </div>");
            sb.AppendLine("            </div>");

            // Результаты раскроя с группировкой
            sb.AppendLine("            <div class='section'>");
            sb.AppendLine("                <h2>🎯 Результаты раскроя</h2>");

            var colorMap = GenerateSimpleColorMap(result);

            for (int i = 0; i < result.RodGroups.Count; i++)
            {
                var group = result.RodGroups[i];
                var rod = group.Rod;
                double waste = materialLength - rod.UsedLength;
                double efficiency = rod.UsedLength / materialLength;

                // Определяем класс в зависимости от группировки
                string rodClass = group.Count > 1 ? "rod rod-group" : "rod";

                sb.AppendLine($"            <div class='{rodClass}'>");

                // Заголовок с информацией о группе
                sb.AppendLine($"                <div class='rod-header'>");
                sb.AppendLine($"                    <span>");
                sb.AppendLine($"                        📏 Раскрой #{i + 1}");
                if (group.Count > 1)
                {
                    sb.AppendLine($"                        <span class='group-badge'>×{group.Count}</span>");
                }
                sb.AppendLine($"                    </span>");
                sb.AppendLine($"                    <span class='efficiency'>{efficiency:P1}</span>");
                sb.AppendLine($"                </div>");

                // Информация об использовании
                sb.AppendLine($"                <div style='margin-bottom: 8px; font-size: 0.9em;'>");
                sb.AppendLine($"                    Использовано: <strong>{rod.UsedLength:F1}</strong> / <strong>{materialLength:F1}</strong> мм");
                sb.AppendLine($"                    | Отходы: <strong>{waste:F1}</strong> мм");
                sb.AppendLine($"                </div>");

                // Графическое представление - упрощенное как в программе
                sb.AppendLine($"                <div class='parts-container'>");

                foreach (var part in rod.Parts)
                {
                    double widthPercent = (part / materialLength) * 100;
                    string color = colorMap[part];

                    sb.AppendLine($"                    <div class='part' style='width: {widthPercent}%; background: {color};' title='{part} мм'>");

                    // Просто показываем длину внутри детали, если достаточно места
                    if (widthPercent > 15)
                    {
                        sb.AppendLine($"                        {part:F0}");
                    }

                    sb.AppendLine($"                    </div>");
                }

                // Отходы
                if (waste > 0)
                {
                    double wastePercent = (waste / materialLength) * 100;
                    sb.AppendLine($"                    <div class='part waste' style='width: {wastePercent}%' title='Отходы: {waste:F1} мм'>");

                    if (wastePercent > 10)
                    {
                        sb.AppendLine($"                        отходы");
                    }

                    sb.AppendLine($"                    </div>");
                }

                sb.AppendLine($"                </div>");

                // Детализация деталей
                var groupedParts = rod.Parts.GroupBy(p => p)
                    .Select(g => new { Length = g.Key, Count = g.Count() })
                    .OrderByDescending(g => g.Length);

                sb.AppendLine($"                <div class='parts-list'>");
                sb.AppendLine($"                    Детали: {string.Join(", ", groupedParts.Select(g => $"{g.Length:F1} мм × {g.Count}"))}");
                sb.AppendLine($"                </div>");

                // Суммарная информация для группированных раскроев
                if (group.Count > 1)
                {
                    sb.AppendLine($"                <div class='group-summary'>");
                    sb.AppendLine($"                    Суммарно для {group.Count} повторений:");
                    sb.AppendLine($"                    Использовано: {rod.UsedLength * group.Count:F1} мм, Отходы: {waste * group.Count:F1} мм");
                    sb.AppendLine($"                </div>");
                }

                sb.AppendLine($"            </div>");
            }

            sb.AppendLine("            </div>");

            // Итоговая сводка
            sb.AppendLine("            <div class='summary'>");
            sb.AppendLine("                <h3>🏆 Итоговая сводка</h3>");
            sb.AppendLine("                <div class='stats'>");
            sb.AppendLine("                    <div class='stat-item'>");
            sb.AppendLine($"                       <div class='stat-value'>{totalRods}</div>");
            sb.AppendLine("                        <div>Всего материалов</div>");
            sb.AppendLine("                    </div>");
            sb.AppendLine("                    <div class='stat-item'>");
            sb.AppendLine($"                       <div class='stat-value'>{uniquePatterns}</div>");
            sb.AppendLine("                        <div>Уникальных раскроев</div>");
            sb.AppendLine("                    </div>");
            sb.AppendLine("                    <div class='stat-item'>");
            sb.AppendLine($"                       <div class='stat-value'>{result.Efficiency:P1}</div>");
            sb.AppendLine("                        <div>Общая эффективность</div>");
            sb.AppendLine("                    </div>");
            sb.AppendLine("                    <div class='stat-item'>");
            sb.AppendLine($"                       <div class='stat-value'>{result.Waste:F0}</div>");
            sb.AppendLine("                        <div>Общие отходы (мм)</div>");
            sb.AppendLine("                    </div>");
            sb.AppendLine("                </div>");
            sb.AppendLine("            </div>");

            sb.AppendLine("        </div>");

            // Подвал
            sb.AppendLine("        <div class='footer'>");
            sb.AppendLine("            Сгенерировано программой «Оптимизатор линейного раскроя» | ");
            sb.AppendLine($"           {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            sb.AppendLine("        </div>");

            sb.AppendLine("    </div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private Dictionary<double, string> GenerateSimpleColorMap(CuttingResult result)
        {
            var colorMap = new Dictionary<double, string>();

            // Получаем все уникальные длины из всех групп
            var uniqueLengths = result.RodGroups
                .SelectMany(g => g.Rod.Parts)
                .Distinct()
                .OrderBy(l => l)
                .ToList();

            // Простая палитра цветов как в программе
            string[] colors = {
        "#FFB6C1", "#87CEEB", "#98FB98", "#DDA0DD", "#FFD700",
        "#FFA07A", "#20B2AA", "#DEB887", "#FF69B4", "#00CED1",
        "#7B68EE", "#32CD32", "#FF4500", "#DA70D6", "#1E90FF",
        "#FFDAB9", "#EEE8AA", "#B0C4DE", "#FFA500", "#90EE90"
    };

            for (int i = 0; i < uniqueLengths.Count; i++)
            {
                colorMap[uniqueLengths[i]] = colors[i % colors.Length];
            }

            return colorMap;
        }
    }

    // Класс для хранения информации о детали
    public class PartItem
    {
        public double Length { get; set; }
        public int Quantity { get; set; }
    }
}