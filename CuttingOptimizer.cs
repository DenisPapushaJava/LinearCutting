namespace LinearCuttingOptimizer;

public class CuttingRod
{
    public List<double> Parts { get; set; } = new List<double>();
    public double UsedLength { get; set; }
}

public class CuttingRodGroup
{
    public CuttingRod Rod { get; set; }
    public int Count { get; set; }
    public string Pattern { get; set; } // Уникальный идентификатор паттерна
}

public class CuttingResult
{
    public List<CuttingRod> Rods { get; set; } = new List<CuttingRod>();
    public List<CuttingRodGroup> RodGroups { get; set; } = new List<CuttingRodGroup>();
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

        // Группируем одинаковые раскрои
        GroupIdenticalRods(result);

        return result;
    }

    // Альтернативный метод с явной группировкой
    public CuttingResult OptimizeCuttingWithGrouping(List<double> partLengths, double materialLength, double gap)
    {
        var result = OptimizeCutting(partLengths, materialLength, gap);
        return result;
    }

    private void GroupIdenticalRods(CuttingResult result)
    {
        var groups = result.Rods
            .GroupBy(rod => GetRodPattern(rod))
            .Select(g => new CuttingRodGroup
            {
                Rod = g.First(),
                Count = g.Count(),
                Pattern = g.Key
            })
            .OrderByDescending(g => g.Rod.UsedLength) // Сортируем по эффективности
            .ThenBy(g => g.Rod.Parts.Count) // Затем по количеству деталей
            .ToList();

        result.RodGroups = groups;
    }

    private string GetRodPattern(CuttingRod rod)
    {
        // Создаем уникальный идентификатор паттерна на основе длин деталей
        var sortedParts = rod.Parts.OrderBy(p => p).ToList();
        return string.Join("|", sortedParts.Select(p => p.ToString("F1")));
    }

    // Альтернативный алгоритм - First Fit Decreasing
    public CuttingResult OptimizeCuttingFFD(List<double> partLengths, double materialLength, double gap)
    {
        var result = new CuttingResult();
        var sortedParts = partLengths.OrderByDescending(p => p).ToList();

        foreach (var part in sortedParts)
        {
            bool placed = false;

            // Пытаемся разместить в существующих материалах
            foreach (var rod in result.Rods)
            {
                double currentGap = rod.Parts.Count > 0 ? gap : 0;
                if (rod.UsedLength + part + currentGap <= materialLength)
                {
                    rod.Parts.Add(part);
                    rod.UsedLength += part + currentGap;
                    placed = true;
                    break;
                }
            }

            // Если не поместилось - создаем новый материал
            if (!placed)
            {
                var newRod = new CuttingRod();
                newRod.Parts.Add(part);
                newRod.UsedLength = part;
                result.Rods.Add(newRod);
            }
        }

        // Расчет эффективности и отходов
        double totalUsed = result.Rods.Sum(r => r.UsedLength);
        double totalAvailable = result.Rods.Count * materialLength;
        result.Efficiency = totalUsed / totalAvailable;
        result.Waste = totalAvailable - totalUsed;

        // Группируем одинаковые раскрои
        GroupIdenticalRods(result);

        return result;
    }

    // Метод для сравнения двух раскроев
    public bool AreRodsIdentical(CuttingRod rod1, CuttingRod rod2)
    {
        if (rod1.Parts.Count != rod2.Parts.Count)
            return false;

        var sorted1 = rod1.Parts.OrderBy(p => p).ToList();
        var sorted2 = rod2.Parts.OrderBy(p => p).ToList();

        for (int i = 0; i < sorted1.Count; i++)
        {
            if (Math.Abs(sorted1[i] - sorted2[i]) > 0.001)
                return false;
        }

        return true;
    }

    // Метод для получения статистики по группировке
    public (int totalRods, int uniquePatterns, int maxRepeats, int minRepeats) GetGroupingStats(CuttingResult result)
    {
        int totalRods = result.RodGroups.Sum(g => g.Count);
        int uniquePatterns = result.RodGroups.Count;
        int maxRepeats = result.RodGroups.Max(g => g.Count);
        int minRepeats = result.RodGroups.Min(g => g.Count);

        return (totalRods, uniquePatterns, maxRepeats, minRepeats);
    }
}