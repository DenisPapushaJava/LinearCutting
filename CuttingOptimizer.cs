namespace LinearCuttingOptimizer;

public class CuttingRod
{
    public List<double> Parts { get; set; } = new List<double>();
    public double UsedLength { get; set; }
}

public class CuttingResult
{
    public List<CuttingRod> Rods { get; set; } = new List<CuttingRod>();
    public double Efficiency { get; set; }
    public double Waste { get; set; }
}

public class CuttingOptimizer
{
    public CuttingResult OptimizeCutting(List<double> partLengths, double materialLength, double gap)
    {
        var result = new CuttingResult();

        // Сортируем детали по убыванию для лучшего заполнения
        var sortedParts = partLengths.OrderByDescending(p => p).ToList();
        var remainingParts = new List<double>(sortedParts);

        while (remainingParts.Count > 0)
        {
            var rod = new CuttingRod();
            double currentLength = 0;

            // Пытаемся добавить детали в текущий материал
            for (int i = 0; i < remainingParts.Count; i++)
            {
                double partWithGap = remainingParts[i] + (rod.Parts.Count > 0 ? gap : 0);

                if (currentLength + partWithGap <= materialLength)
                {
                    rod.Parts.Add(remainingParts[i]);
                    currentLength += partWithGap;
                    remainingParts.RemoveAt(i);
                    i--; // Уменьшаем счетчик т.к. удалили элемент
                }
            }

            rod.UsedLength = currentLength;
            result.Rods.Add(rod);
        }

        // Расчет эффективности и отходов
        double totalUsed = result.Rods.Sum(r => r.UsedLength);
        double totalAvailable = result.Rods.Count * materialLength;
        result.Efficiency = totalUsed / totalAvailable;
        result.Waste = totalAvailable - totalUsed;

        return result;
    }
}