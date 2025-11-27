using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LinearCutting
{
    public partial class QuickAddForm : Form
    {
        public List<PartItem> SelectedParts { get; private set; } = new List<PartItem>();

        public QuickAddForm()
        {
            InitializeComponent1();
            InitializeCommonParts();
        }

        private void InitializeComponent1()
        {
            this.Text = "Быстрое добавление деталей";
            this.Size = new Size(300, 400);
            this.StartPosition = FormStartPosition.CenterParent;

            var dataGridView = new DataGridView
            {
                Location = new Point(10, 10),
                Size = new Size(265, 300),
                AllowUserToAddRows = false
            };
            dataGridView.Columns.Add("Length", "Длина");
            dataGridView.Columns.Add("Quantity", "Количество");
            dataGridView.Columns["Length"].ReadOnly = true;

            var btnAdd = new Button { Text = "Добавить", Location = new Point(100, 320), Size = new Size(80, 30) };
            btnAdd.Click += (s, e) =>
            {
                SelectedParts.Clear();
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    if (row.IsNewRow) continue;
                    if (double.TryParse(row.Cells["Length"].Value?.ToString(), out double length) &&
                        int.TryParse(row.Cells["Quantity"].Value?.ToString(), out int quantity) && quantity > 0)
                    {
                        SelectedParts.Add(new PartItem { Length = length, Quantity = quantity });
                    }
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            var btnCancel = new Button { Text = "Отмена", Location = new Point(190, 320), Size = new Size(80, 30) };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.AddRange(new Control[] { dataGridView, btnAdd, btnCancel });
        }

        private void InitializeCommonParts()
        {
            // Предопределенные распространенные размеры
            double[] commonLengths = { 500, 600, 1000, 1200, 1500, 1800, 2000, 2400, 2700, 3000, 3500, 4000, 4500, 5000, 5500, 6000 };

            var dataGridView = (DataGridView)this.Controls[0];
            foreach (var length in commonLengths)
            {
                dataGridView.Rows.Add(length.ToString("F0"), "0");
            }
        }
    }
}
