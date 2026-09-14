using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionPlanner.Swarm
{
    /// <summary>
    /// Hungarian Algorithm.用于解决最佳分配指派问题。这里用作队形变换，目的是求取变换过程中的总路程之和最小。
    /// </summary>
    public static class HungarianAlgorithm
    {
        /// <summary>
        /// 输入：效益矩阵
        /// </summary>
        /// <param name="costs"></param>
        /// <returns>
        /// 返回：对应分配的每一列
        /// </returns>
        public static int[] FindAssignments(this int[,] costs)
        {
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));
            // 获取行数
            var h = costs.GetLength(0);
            // 获取列数
            var w = costs.GetLength(1);
            // 找出效益矩阵中每行中的最小值，并且每行减去这个最小值。原理：矩阵的初等变换不改变矩阵的性质
            for (var i = 0; i < h; i++)
            {
                var min = int.MaxValue;

                for (var j = 0; j < w; j++)
                {
                    min = Math.Min(min, costs[i, j]);
                }

                for (var j = 0; j < w; j++)
                {
                    costs[i, j] -= min;
                }
            }

            var masks = new byte[h, w];
            var rowsCovered = new bool[h];
            var colsCovered = new bool[w];
            // 找零，并将其对应的位置mask(mask=1),如果效益矩阵中的某个零元素被选中，则改零元素所在的行和列中的其他零元素不能mask。
            for (var i = 0; i < h; i++)
            {
                for (var j = 0; j < w; j++)
                {
                    if (costs[i, j] == 0 && !rowsCovered[i] && !colsCovered[j])
                    {
                        masks[i, j] = 1;
                        rowsCovered[i] = true;
                        colsCovered[j] = true;
                    }
                }
            }
            // 将rowsCovered（行覆盖）和colsCovered（列覆盖）标志位清楚
            HungarianAlgorithm.ClearCovers(rowsCovered, colsCovered, w, h);

            var path = new Location[w * h];
            var pathStart = default(Location);
            var step = 1;
            // 匈牙利算法运行
            while (step != -1)
            {
                switch (step)
                {
                    // 第一步，查找是否存在最佳匹配
                    case 1:
                        step = HungarianAlgorithm.RunStep1(masks, colsCovered, w, h);
                        break;
                    // 第二步，寻找没有被划掉（行覆盖，列覆盖）的零元素的位置，没找到，进行第四步操作使效益矩阵生成，否则重新划线
                    case 2:
                        step = HungarianAlgorithm.RunStep2(costs, masks, rowsCovered, colsCovered, w, h, ref pathStart);
                        break;
                    // 第三步，圈零，重新给mask赋值
                    case 3:
                        step = HungarianAlgorithm.RunStep3(masks, rowsCovered, colsCovered, w, h, path, pathStart);
                        break;
                    // 第四步，计算没有划掉区域的计算值，并将划掉的行都加上最小值，没有划掉的列减去最小值
                    case 4:
                        step = HungarianAlgorithm.RunStep4(costs, rowsCovered, colsCovered, w, h);
                        break;
                }
            }
            // 返回最佳分配对应的列，将列索引存在在数组agentsTasks中
            var agentsTasks = new int[h];

            for (var i = 0; i < h; i++)
            {
                for (var j = 0; j < w; j++)
                {
                    if (masks[i, j] == 1)
                    {
                        agentsTasks[i] = j;
                        break;
                    }
                }
            }

            return agentsTasks;
        }
        // 查找是否存在最佳分配
        private static int RunStep1(byte[,] masks, bool[] colsCovered, int w, int h)
        {
            if (masks == null)
                throw new ArgumentNullException(nameof(masks));

            if (colsCovered == null)
                throw new ArgumentNullException(nameof(colsCovered));
            // 将列覆盖标志位赋值
            for (var i = 0; i < h; i++)
            {
                for (var j = 0; j < w; j++)
                {
                    if (masks[i, j] == 1)
                        colsCovered[j] = true;
                }
            }

            var colsCoveredCount = 0;

            for (var j = 0; j < w; j++)
            {
                if (colsCovered[j])
                    colsCoveredCount++;
            }
            // 判断是否找到最佳分配，找到退出循环，没找到，进入第二步
            if (colsCoveredCount == h)
                return -1;

            return 2;
        }
        // 寻找没有被划掉（行覆盖，列覆盖）的零元素的位置，没找到，进行第四步操作使效益矩阵生成，否则重新划线
        private static int RunStep2(int[,] costs, byte[,] masks, bool[] rowsCovered, bool[] colsCovered, int w, int h, ref Location pathStart)
        {
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));

            if (masks == null)
                throw new ArgumentNullException(nameof(masks));

            if (rowsCovered == null)
                throw new ArgumentNullException(nameof(rowsCovered));

            if (colsCovered == null)
                throw new ArgumentNullException(nameof(colsCovered));

            while (true)
            {
                // 寻找没有被划掉（行覆盖，列覆盖）的零元素的位置
                var loc = HungarianAlgorithm.FindZero(costs, rowsCovered, colsCovered, w, h);
                // 没找到进入第四步
                if (loc.row == -1)
                    return 4;
                // 找到，将相应位置mask =2
                masks[loc.row, loc.column] = 2;
                // 寻找第loc.row列被圈出(mask=1)的零元素，返回该元素所在的列
                var starCol = HungarianAlgorithm.FindStarInRow(masks, w, loc.row);
                if (starCol != -1)
                {
                    // 如果starCol找到，则将效益矩阵的loc.row行划掉，解除效益矩阵中starCol的覆盖
                    rowsCovered[loc.row] = true;
                    colsCovered[starCol] = false;
                }
                else
                {
                    // 没找到，记录其实位置，进行第三步
                    pathStart = loc;
                    return 3;
                }
            }
        }
        private static int RunStep3(byte[,] masks, bool[] rowsCovered, bool[] colsCovered, int w, int h, Location[] path, Location pathStart)
        {
            if (masks == null)
                throw new ArgumentNullException(nameof(masks));

            if (rowsCovered == null)
                throw new ArgumentNullException(nameof(rowsCovered));

            if (colsCovered == null)
                throw new ArgumentNullException(nameof(colsCovered));

            var pathIndex = 0;
            path[0] = pathStart;

            while (true)
            {
                var row = HungarianAlgorithm.FindStarInColumn(masks, h, path[pathIndex].column);
                if (row == -1)
                    break;

                pathIndex++;
                path[pathIndex] = new Location(row, path[pathIndex - 1].column);

                var col = HungarianAlgorithm.FindPrimeInRow(masks, w, path[pathIndex].row);

                pathIndex++;
                path[pathIndex] = new Location(path[pathIndex - 1].row, col);
            }

            HungarianAlgorithm.ConvertPath(masks, path, pathIndex + 1);
            HungarianAlgorithm.ClearCovers(rowsCovered, colsCovered, w, h);
            HungarianAlgorithm.ClearPrimes(masks, w, h);

            return 1;
        }
        // 计算没有划掉区域的计算值，并将划掉的行都加上最小值，没有划掉的列减去最小值
        private static int RunStep4(int[,] costs, bool[] rowsCovered, bool[] colsCovered, int w, int h)
        {
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));

            if (rowsCovered == null)
                throw new ArgumentNullException(nameof(rowsCovered));

            if (colsCovered == null)
                throw new ArgumentNullException(nameof(colsCovered));

            var minValue = HungarianAlgorithm.FindMinimum(costs, rowsCovered, colsCovered, w, h);

            for (var i = 0; i < h; i++)
            {
                for (var j = 0; j < w; j++)
                {
                    // 如果被划掉的是行，则改行加上最小值
                    if (rowsCovered[i])
                        costs[i, j] += minValue;
                    // 将没有划掉的列减去最小值
                    if (!colsCovered[j])
                        costs[i, j] -= minValue;
                }
            }
            return 2;
        }
        // 计算没有被划掉(行覆盖，列覆盖)元素中的最小值
        private static int FindMinimum(int[,] costs, bool[] rowsCovered, bool[] colsCovered, int w, int h)
        {
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));

            if (rowsCovered == null)
                throw new ArgumentNullException(nameof(rowsCovered));

            if (colsCovered == null)
                throw new ArgumentNullException(nameof(colsCovered));

            var minValue = int.MaxValue;

            for (var i = 0; i < h; i++)
            {
                for (var j = 0; j < w; j++)
                {
                    if (!rowsCovered[i] && !colsCovered[j])
                        minValue = Math.Min(minValue, costs[i, j]);
                }
            }

            return minValue;
        }
        // 寻找第row列被圈出(mask=1)的零元素，返回该元素所在的列
        private static int FindStarInRow(byte[,] masks, int w, int row)
        {
            if (masks == null)
                throw new ArgumentNullException(nameof(masks));

            for (var j = 0; j < w; j++)
            {
                if (masks[row, j] == 1)
                    return j;
            }

            return -1;
        }
        private static int FindStarInColumn(byte[,] masks, int h, int col)
        {
            if (masks == null)
                throw new ArgumentNullException(nameof(masks));

            for (var i = 0; i < h; i++)
            {
                if (masks[i, col] == 1)
                    return i;
            }

            return -1;
        }
        private static int FindPrimeInRow(byte[,] masks, int w, int row)
        {
            if (masks == null)
                throw new ArgumentNullException(nameof(masks));

            for (var j = 0; j < w; j++)
            {
                if (masks[row, j] == 2)
                    return j;
            }

            return -1;
        }
        // 寻找没有被划掉的零元素
        private static Location FindZero(int[,] costs, bool[] rowsCovered, bool[] colsCovered, int w, int h)
        {
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));

            if (rowsCovered == null)
                throw new ArgumentNullException(nameof(rowsCovered));

            if (colsCovered == null)
                throw new ArgumentNullException(nameof(colsCovered));

            for (var i = 0; i < h; i++)
            {
                for (var j = 0; j < w; j++)
                {
                    if (costs[i, j] == 0 && !rowsCovered[i] && !colsCovered[j])
                        return new Location(i, j);
                }
            }

            return new Location(-1, -1);
        }
        private static void ConvertPath(byte[,] masks, Location[] path, int pathLength)
        {
            if (masks == null)
                throw new ArgumentNullException(nameof(masks));

            if (path == null)
                throw new ArgumentNullException(nameof(path));

            for (var i = 0; i < pathLength; i++)
            {
                if (masks[path[i].row, path[i].column] == 1)
                {
                    masks[path[i].row, path[i].column] = 0;
                }
                else if (masks[path[i].row, path[i].column] == 2)
                {
                    masks[path[i].row, path[i].column] = 1;
                }
            }
        }
        private static void ClearPrimes(byte[,] masks, int w, int h)
        {
            if (masks == null)
                throw new ArgumentNullException(nameof(masks));

            for (var i = 0; i < h; i++)
            {
                for (var j = 0; j < w; j++)
                {
                    if (masks[i, j] == 2)
                        masks[i, j] = 0;
                }
            }
        }
        private static void ClearCovers(bool[] rowsCovered, bool[] colsCovered, int w, int h)
        {
            if (rowsCovered == null)
                throw new ArgumentNullException(nameof(rowsCovered));

            if (colsCovered == null)
                throw new ArgumentNullException(nameof(colsCovered));

            for (var i = 0; i < h; i++)
            {
                rowsCovered[i] = false;
            }

            for (var j = 0; j < w; j++)
            {
                colsCovered[j] = false;
            }
        }

        private struct Location
        {
            public readonly int row;
            public readonly int column;

            public Location(int row, int col)
            {
                this.row = row;
                this.column = col;
            }
        }
    }
}
