using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace renjuTrain
{
    public class Util
    {
        static int[] dx = { 0, 1, 1, 1 };
        static int[] dy = { 1, -1, 0, 1 };
        public static double GetDist(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }

        public static bool InRange(int x, int y)
        {
            return x >= 0 && x < 15 && y >= 0 && y < 15;
        }

        public static int CheckBoardResult(int[,] board, int x, int y)
        {
            int now = board[x, y];
            for (int d = 0; d < 4; d++)
            {
                int s = 1;
                for (int i = 1; true; i++)
                {
                    int nowX = x + dx[d] * i, nowY = y + dy[d] * i;
                    if (!InRange(nowX, nowY) || board[nowX, nowY] != now)
                        break;
                    s++;
                }

                for (int i = 1; true; i++)
                {
                    int nowX = x - dx[d] * i, nowY = y - dy[d] * i;
                    if (!InRange(nowX, nowY) || board[nowX, nowY] != now)
                        break;
                    s++;
                }

                if (s > 4)
                    return now;
            }

            return 0;
        }
    }
}
