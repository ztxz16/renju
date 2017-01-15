# renju
五子棋AI

基本模型
我们需要训练一个走子网络，输入是一个五子棋棋盘的局面，输出是这个局面下选择的点。为了训练这个走子网络，首先要选择一个合适的估价函数来评价他的好坏。
在alphago的算法中，可以用类蒙特卡洛的方法来评估一个局面下获胜的概率。但是五子棋和围棋是不同的，五子棋在蒙特卡洛下的概率很不准确（想象随机下五子棋，结局很大可能是平局）。因此用这种方法来计算概率来估值并不是一个好方法。
因此我采用了“预测准确率”作为估价函数。既用人类棋谱作为训练的素材，用走子网络去预测每盘棋所有局面下的走法，统计有多少步和人类的走法吻合。走子网络使用了五子棋中比较常用的棋型（冲四冲三，活四等），给每个棋型一个系数，选取加权和最大的一个店作为输出，训练中需要做的就是不断调整这些系数，以期训练出一个优秀的走子网络。

训练方法
训练方法分为两步，第一步是以最大化预测准确率为目标，每次选择一个系数并固定住其他系数，然后调整这个系数，如果使估值变得更优了，则将这个调整过的网络作为当前使用的走子网络。在经过数万轮迭代调整之后，预测准确率由最初的20%上升到了33%左右。
这时候的走子网络已经初步成型了。第二步是reinforce增强学习：用一个固定的走子网络作为靶子，调整另一个走子网络的系数之后两个走子网络进行对战，如果调整过的走子网络的胜率超过一定值则覆盖当前的走子网络。

程序说明
程序全部使用c#编写，一共有两个工程，renjuTrain是用来训练的程序，renju是一个UI界面用来人机对战检测AI的水平。工程需要使用VS打开，DEBUG目录下也有编译好的文件。
在renju工程中，有一个AIInterface的接口，在主程序中，如果开始对局时选择了某一方使用AI，那么会实例化一个AIInterface作为这一方的AI（在当前程序中是ZtxzAI，也就是我已经训练好的AI），如果要测试别的AI，只需要同样实现一个实现了AIInterface接口的类即可。
在renjuTrain工程中，主要的训练过程都在Program.cs中，其中TrainFactor是利用棋谱来训练走子网络，Reinforce是自己和自己下棋来训练走子网络。