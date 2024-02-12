# About this repository
 
    该仓库是在Unity引擎上，实现的游戏对象群聚算法的实验Demo。在Unity提供的Nav导航基础上，可使周边对象具有传播性地聚集前往目标点。<br>
 具体参考了李晓磊教授所阐述的仿生学优化方案——“人工鱼群算法”。<br>

     该Demo中，通过全局地控制，将对象的行为拆解成以下几种：<br>
- 目标行为：若已存在目标对象，则根据随机步长进行位移，但若没有对象，则随机向一个点移动，寻找视野范围内的可能目标对象，如若仍未发现，则仅进行随机移动。<br>
- 群聚行为：寻找对象可视范围内的所有其他对象，模拟一个中心点，若该中心点周围的对象浓度优于当前浓度，且中心点的对象浓度在拥挤因子允许范围内，则向中心点移动。<br>
- 跟随行为：寻找对象可视范围内的所有其他对象，若这些对象拥有目标点，则比较其前往目标点的开销，找出最小者。通过计算自身前往最小对象的成本或当自身无对象时，向其位移。<br>
- 在上述行为中，预存预测结果，在更新节点的最后，筛选出最优结果并执行。<br>

    同时可以通过一些比较重要的参数，对实际效果进行调整：<br>
- 视野范围：当前对象的可视范围，用于寻找目标。<br>
- 拥挤因子：控制群体的密集度。<br>
- 步长：区间控制移动范围。<br>
- 尝试次数：在目标行为中，控制随机点位寻找目标的尝试次数。<br>

    需要提一下关于前往对象的开销。因为是以Unity提供的Nav导航为基础，所以开销是自行计算的。具体方法为获取路径的拐点，计算点之间的开销和，Demo中为了方便，假设移动成本都是一致的，但在实际中，例如路点寻路等等，需要考虑移动成本的问题。<br>

## Reference material
- tigerqin1980 (知乎)<br>
https://zhuanlan.zhihu.com/p/100920122
- wp_csdn (人工鱼群算法详解)<br>
https://blog.csdn.net/wp_csdn/article/details/54577567
- _ArayA_ (介绍Nav导航中的拐点)<br>
https://www.jianshu.com/p/6b9426d0fee3
