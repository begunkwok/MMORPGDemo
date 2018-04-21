namespace GameMain
{
    /// <summary>
    /// 特效绑定位置
    /// </summary>
    public enum EffectBindType
    {
        None    = 0, //无
        World   = 1, //纯坐标
        Trans   = 2, //在某一个物体下面
        OwnFoot = 3, //出现在自身脚部位置
        OwnBody = 4, //出现在自身身体位置
        OwnHead = 5, //出现在自身头部位置
        OwnHand = 6, //出现在自身手上
        TarBody = 7, //出现在目标身体位置
        TarHead = 8, //出现在目标头部位置
        TarFoot = 9, //出现在目标脚部位置
        OwnVTar = 10, //出现在目标与自身的连线中点
    }           
}
