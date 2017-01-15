using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace renjuTrain
{
    class Program
    {
        static int[,] board = new int[16, 16];
        static int[,] _board = new int[16, 16];

        static List<List<KeyValuePair<int, int>>> allRecord = new List<List<KeyValuePair<int, int>>>();

        /// <summary>
        /// 读取数据并整理橙容易处理的格式
        /// </summary>
        static void ParseData()
        {
            string[] files = Directory.GetFiles("sgf");
            foreach (string file in files)
            {
                //Console.WriteLine(file);
                string sgfString = File.ReadAllText(file);
                int startPos = 0;
                for (int i = 2; i < sgfString.Length; i++)
                {
                    //记录开始位置
                    if (sgfString[i] == 'B' && sgfString[i - 1] == ';' && sgfString[i - 2] == ']')
                    {
                        startPos = i;
                        break;
                    }
                }

                //如果棋谱格式不同，则不解析
                if (startPos == 0)
                    break;
                //存入这一盘棋
                allRecord.Add(new List<KeyValuePair<int, int>>());
                int pos = startPos;
                bool valid = true;
                while (true)
                {
                    if (pos >= sgfString.Length || (sgfString[pos] != 'B' && sgfString[pos] != 'W'))
                        break;
                    allRecord.Last().Add(new KeyValuePair<int, int>(Convert.ToInt32(sgfString[pos + 2] - 'a'), Convert.ToInt32(sgfString[pos + 3] - 'a')));
                    if (!Util.InRange(allRecord.Last().Last().Key, allRecord.Last().Last().Value))
                        valid = false;
                    pos += 6;
                }

                if (!valid)
                    allRecord.Remove(allRecord.Last());
            }
        }

        /// <summary>
        /// 计算AI对于棋谱的预测准确率
        /// </summary>
        /// <param name="AI"></param>
        /// <returns></returns>
        static double Judge(AIInterface AI)
        {
            double right = 0.0, total = 0.0;
            int xxx = 0;
            foreach (List<KeyValuePair<int, int>> game in allRecord)
            {
                //Console.WriteLine(++xxx);
                //重置棋盘
                for (int i = 0; i < 15; i++)
                    for (int j = 0; j < 15; j++)
                        board[i, j] = 0;
                
                for (int i = 0; i < game.Count(); i++)
                {
                    //第一步不进行判断，因为AI的第一步几乎是随机的，影响结果
                    if (i > 0)
                    {
                        int x, y;
                        AI.Running(board, i % 2 + 1);
                        AI.GetNextStep(out x, out y);
                        if (x == game[i].Key && y == game[i].Value)
                            right++;
                        total++;
                    }

                    board[game[i].Key, game[i].Value] = i % 2 + 1;
                }
            }

            return right / total;
        }

        static void TrainFactor()
        {
            Random random = new Random((int)DateTime.Now.Ticks);
            AIInterface bestAI = new ZtxzAI();
            double bestScore = Judge(bestAI);
            int trainTime = 10000;
            for (int i = 0; i < trainTime; i++)
            {
                Console.WriteLine(String.Format("当前最优准确率为{0}%", bestScore * 100.0));
                Console.WriteLine(String.Format("第{0}次训练", i));

                //创建一个AI，复制当前最优的系数
                AIInterface AI = new ZtxzAI();
                for (int j = 0; j < 13; j++)
                {
                    (AI as ZtxzAI).maxValue[j] = (bestAI as ZtxzAI).maxValue[j];
                    (AI as ZtxzAI).minValue[j] = (bestAI as ZtxzAI).minValue[j];
                    (AI as ZtxzAI).otherMaxValue[j] = (bestAI as ZtxzAI).otherMaxValue[j];
                    (AI as ZtxzAI).otherMinValue[j] = (bestAI as ZtxzAI).otherMinValue[j];
                }

                //调整系数
                int pos = random.Next(13);
                if (random.Next() % 2 == 0)
                {
                    int value = (AI as ZtxzAI).minValue[pos] / 2;
                    if (value == 0)
                        continue;
                    (AI as ZtxzAI).AddSelfFactor(pos, random.Next(value + value + 1) - value);
                } else
                {
                    int value = (AI as ZtxzAI).otherMinValue[pos] / 2;
                    if (value == 0)
                        continue;
                    (AI as ZtxzAI).AddOtherFactor(pos, random.Next(value + value + 1) - value);
                }

                //计算准确率，如果当前准确率比较好就替换
                double currentScore = Judge(AI);
                if (currentScore > bestScore)
                {
                    bestScore = currentScore;
                    bestAI = AI;
                }
            }

            File.AppendAllText("result.txt", String.Format("当前最优准确率为{0}%\n", bestScore * 100.0));
            File.AppendAllText("result.txt", JsonConvert.SerializeObject(bestAI as ZtxzAI) + "\n");
        }

        static void Reinforce()
        {
            Random random = new Random((int)DateTime.Now.Ticks);
            AIInterface bestAI = new ZtxzAI();
            int trainTime = 1000;
            for (int i = 0; i < trainTime; i++)
            {
                Console.WriteLine(String.Format("第{0}次训练", i));

                //创建一个AI，复制当前最优的系数
                AIInterface AI = new ZtxzAI();
                for (int j = 0; j < 13; j++)
                {
                    (AI as ZtxzAI).maxValue[j] = (bestAI as ZtxzAI).maxValue[j];
                    (AI as ZtxzAI).minValue[j] = (bestAI as ZtxzAI).minValue[j];
                    (AI as ZtxzAI).otherMaxValue[j] = (bestAI as ZtxzAI).otherMaxValue[j];
                    (AI as ZtxzAI).otherMinValue[j] = (bestAI as ZtxzAI).otherMinValue[j];
                }

                //调整系数
                int pos = random.Next(13);
                if (random.Next() % 2 == 0)
                {
                    int value = (AI as ZtxzAI).minValue[pos] / 2;
                    if (value == 0)
                        continue;
                    (AI as ZtxzAI).AddSelfFactor(pos, random.Next(value + value + 1) - value);
                }
                else
                {
                    int value = (AI as ZtxzAI).otherMinValue[pos] / 2;
                    if (value == 0)
                        continue;
                    (AI as ZtxzAI).AddOtherFactor(pos, random.Next(value + value + 1) - value);
                }

                int cnt = 1000;
                int bestWin = 0, nowWin = 0;
                for (; cnt > 0; cnt--)
                {
                    for (int j = 0; j < 15; j++)
                        for (int k = 0; k < 15; k++)
                            board[j, k] = 0;
                    int now = 1;
                    int step = 0;
                    while (true)
                    {
                        int x, y;
                        for (int j = 0; j < 15; j++)
                            for (int k = 0; k < 15; k++)
                                _board[j, k] = board[j, k];
                        if (now != cnt % 2)
                        {
                            bestAI.Running(_board, now);
                            bestAI.GetNextStep(out x, out y);
                        } else
                        {
                            AI.Running(_board, now);
                            AI.GetNextStep(out x, out y);
                        }

                        board[x, y] = now;
                        int result = Util.CheckBoardResult(board, x, y);
                        if (result > 0)
                        {
                            if (cnt % 2 != now)
                                bestWin++;
                            else
                                nowWin++;
                            break;
                        }
                        else
                            now = 3 - now;

                        step++;
                        if (step == 225)
                            break;
                    }
                }

                if (nowWin > bestWin)
                    bestAI = AI;
            }

            File.AppendAllText("result.txt", JsonConvert.SerializeObject(bestAI as ZtxzAI) + "\n");
        }

        static void Main(string[] args)
        {
            ParseData();

            //TrainFactor();

            Reinforce();
        }
    }
}
