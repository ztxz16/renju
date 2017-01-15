using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace renju
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        enum GameType { People, Computer};
        enum GameTurn { Black, White };

        Thread aigameThread;
        bool isPlaying = false;

        Dictionary<GameTurn, GameType> type = new Dictionary<GameTurn, GameType>();
        Dictionary<GameTurn, AIInterface> ai = new Dictionary<GameTurn, AIInterface>();

        GameTurn gameTurn = GameTurn.Black;
        bool isEnd = false;

        Brush halfBlack = new SolidColorBrush(Color.FromArgb(127, 0, 0, 0));
        Brush halfWhite = new SolidColorBrush(Color.FromArgb(127, 255, 255, 255));
        Brush black = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
        Brush white = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

        int lastX = 15, lastY = 15;
        double step = 490 / 14.0;
        double top = 7, left = 7;
        double R = 15;

        int[,] board = new int[16, 16];
        int[,] _board = new int[16, 16];

        Ellipse[,] whitePieces = new Ellipse[16, 16];
        Ellipse[,] blackPieces = new Ellipse[16, 16];

        public MainWindow()
        {
            InitializeComponent();
            type[GameTurn.Black] = GameType.People;
            type[GameTurn.White] = GameType.People;
            blackText.Text = "黑: 人";
            whiteText.Text = "白: 人";
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    double x = left + step * i;
                    double y = top + step * j;

                    whitePieces[i, j] = MakeVirtualPieces(x, y, halfWhite);
                    blackPieces[i, j] = MakeVirtualPieces(x, y, halfBlack);
                }
            }

            whitePieces[15, 15] = blackPieces[15, 15] = new Ellipse();
        }

        public Ellipse MakeVirtualPieces(double x, double y, Brush brush)
        {
            Ellipse result = new Ellipse();
            result.Fill = brush;
            result.Width = R * 2;
            result.Height = R * 2;
            chessBoard.Children.Add(result);
            Canvas.SetLeft(result, x);
            Canvas.SetTop(result, y);
            result.Visibility = Visibility.Hidden;
            return result;
        }

        private void HideLastVirtualPiece()
        {
            if (board[lastX, lastY] != 0)
                return;
            if (gameTurn == GameTurn.Black)
                blackPieces[lastX, lastY].Visibility = Visibility.Hidden;
            else
                whitePieces[lastX, lastY].Visibility = Visibility.Hidden;
        }

        private void ShowVirtualPiece(int x, int y)
        {
            if (board[x, y] != 0)
                return;
            if (gameTurn == GameTurn.Black)
                blackPieces[x, y].Visibility = Visibility.Visible;
            else
                whitePieces[x, y].Visibility = Visibility.Visible;
        }

        private void PutPiece(int x, int y)
        {
            if (board[x, y] != 0)
                return;
            if (gameTurn == GameTurn.Black)
            {
                board[x, y] = 1;
                HideLastVirtualPiece();
                blackPieces[x, y].Visibility = Visibility.Visible;
                blackPieces[x, y].Fill = black;
                gameTurn = GameTurn.White;
            }
            else
            {
                board[x, y] = 2;
                whitePieces[lastX, lastY].Visibility = Visibility.Hidden;
                whitePieces[x, y].Visibility = Visibility.Visible;
                whitePieces[x, y].Fill = white;
                gameTurn = GameTurn.Black;
            }

            lastX = lastY = 15;
            int result = Util.CheckBoardResult(board, x, y);
            if (result == 1)
            {
                MessageBox.Show("黑棋获胜，游戏结束");
                isEnd = true;
            }

            if (result == 2)
            {
                MessageBox.Show("白棋获胜，游戏结束");
                isEnd = true;
            }

            if (!isEnd && type[gameTurn] == GameType.Computer)
                UseAI(ai[gameTurn] as AIInterface);
        }

        private void CopyBoard()
        {
            for (int i = 0; i < 15; i++)
                for (int j = 0; j < 15; j++)
                    _board[i, j] = board[i, j];
        }
        private void UseAI(AIInterface ai)
        {
            int now = 1;
            if (gameTurn != GameTurn.Black)
                now = 2;
            CopyBoard();
            ai.Running(_board, now);
            int x, y;
            ai.GetNextStep(out x, out y);
            PutPiece(x, y);
        }

        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            ((sender as TextBlock)).Background = Brushes.Gray;
        }

        private void TextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            ((sender as TextBlock)).Background = Brushes.Transparent;
        }

        private void chessBoard_MouseLeave(object sender, MouseEventArgs e)
        {
            HideLastVirtualPiece();
            lastX = lastY = 15;
        }

        private void ExitProgram(object sender, MouseButtonEventArgs e)
        {
            if (aigameThread != null)
            {
                aigameThread.Abort();
                aigameThread.Join();
            }
            App.Current.Shutdown();
        }

        private void Restart(object sender, MouseButtonEventArgs e)
        {
            if (aigameThread != null)
            {
                aigameThread.Abort();
                aigameThread.Join();
                isPlaying = false;
            }

            MessageBoxResult result = MessageBox.Show(this, "黑方是否使用AI?", "黑", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                ai[GameTurn.Black] = new ZtxzAI();
                type[GameTurn.Black] = GameType.Computer;
                blackText.Text = "黑: AI";
            }
            else
            {
                type[GameTurn.Black] = GameType.People;
                blackText.Text = "黑: 人";
            }

            result = MessageBox.Show(this, "白方是否使用AI?", "白", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                ai[GameTurn.White] = new AI();
                type[GameTurn.White] = GameType.Computer;
                whiteText.Text = "白: AI";
            }
            else
            {
                type[GameTurn.White] = GameType.People;
                whiteText.Text = "白: 人";
            }

            isEnd = false;
            for (int i = 0; i < 15; i++)
                for (int j = 0; j < 15; j++)
                {
                    blackPieces[i, j].Fill = halfBlack;
                    whitePieces[i, j].Fill = halfWhite;
                    blackPieces[i, j].Visibility = Visibility.Hidden;
                    whitePieces[i, j].Visibility = Visibility.Hidden;
                    board[i, j] = 0;
                    gameTurn = GameTurn.Black;
                }
            if (type[GameTurn.Black] == GameType.Computer)
                UseAI(ai[GameTurn.Black]);
        }

        private void AIgame(object sender, MouseButtonEventArgs e)
        {
            log.Text = "";
            aigameThread = new Thread(PlayAIGAME);
            aigameThread.Name = "aigame thread";
            aigameThread.Start();
        }

        private void PlayAIGAME()
        {
            isPlaying = true;

            ai[GameTurn.Black] = new ZtxzAI();
            ai[GameTurn.White] = new AI();

            int cnt = 10000;
            int blackWin = 0, whiteWin = 0, noWin = 0;
            for (int i = 0; i < cnt; i++)
            {
                for (int j = 0; j < 15; j++)
                    for (int k = 0; k < 15; k++)
                        board[j, k] = 0;
                GameTurn turn = GameTurn.Black;
                int now = 1;
                int step = 0;
                while (true)
                {
                    int x, y;
                    CopyBoard();
                    ai[turn].Running(_board, now);
                    ai[turn].GetNextStep(out x, out y);
                    if (x < 0 || x > 14 || y < 0 || y > 14 || board[x, y] != 0)
                    {
                        AIwin(i, 3 - now, ref blackWin, ref whiteWin, ref noWin);
                        break;
                    }

                    board[x, y] = now;
                    int result = Util.CheckBoardResult(board, x, y);
                    if (result > 0)
                    {
                        AIwin(i, now, ref blackWin, ref whiteWin, ref noWin);
                        break;
                    }
                    else
                    {
                        if (turn == GameTurn.Black)
                            turn = GameTurn.White;
                        else
                            turn = GameTurn.Black;
                        now = 3 - now;
                    }

                    step++;
                    if (step == 225)
                    {
                        AIwin(i, 3, ref blackWin, ref whiteWin, ref noWin);
                        break;
                    }
                }
            }

            isPlaying = false;
        }

        private void AIwin(int round, int win, ref int blackWin, ref int whiteWin, ref int noWin)
        {
            if (win == 1)
            {
                blackWin++;
                //printLog("第" + round.ToString() + "盘结束，黑棋获胜");
            } else if (win == 2)
            {
                whiteWin++;
                //printLog("第" + round.ToString() + "盘结束，白棋获胜");
            } else
            {
                noWin++;
            }

            if ((round + 1) % 100 == 0)
            {
                PrintLog("当前黑棋获胜" + blackWin.ToString() + "盘，白棋获胜" + whiteWin.ToString() + "盘，和棋" + noWin.ToString() + "盘");
                Thread.Sleep(1);
            }
        }

        void PrintLog(String logText)
        {
            mainWindow.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                log.Text += (logText + "\n");
                scroller.ScrollToVerticalOffset(scroller.ActualHeight + scroller.ViewportHeight + scroller.ExtentHeight);
            }));
        }

        private void chessBoard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (type[gameTurn] == GameType.Computer || isEnd || isPlaying)
                return;
            double xPos = e.GetPosition(chessBoard).X;
            double yPos = e.GetPosition(chessBoard).Y;
            int x = Convert.ToInt32((xPos - left) / step - 0.5);
            int y = Convert.ToInt32((yPos - top) / step - 0.5);
            if (x >= 0 && x < 15 && y >= 0 && y < 15)
                PutPiece(x, y);
        }

        private void chessBoard_MouseMove(object sender, MouseEventArgs e)
        {
            if (type[gameTurn] == GameType.Computer || isEnd || isPlaying)
                return;
            double xPos = e.GetPosition(chessBoard).X;
            double yPos = e.GetPosition(chessBoard).Y;
            int x = Convert.ToInt32((xPos - left) / step - 0.5);
            int y = Convert.ToInt32((yPos - top) / step - 0.5);
            if (x >= 0 && x < 15 && y >= 0 && y < 15)
            {
                HideLastVirtualPiece();
                ShowVirtualPiece(x, y);
                lastX = x;
                lastY = y;
            }
        }
    }
}
