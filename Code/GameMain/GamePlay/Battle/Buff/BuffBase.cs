using System;

namespace GameMain
{
    /// <summary>
    /// buff基类
    /// </summary>
    public class BuffBase : IBuff
    {
        public int Id
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Enter()
        {
            throw new NotImplementedException();
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }

        public float GetLeftTime()
        {
            throw new NotImplementedException();
        }

        public void Overlay()
        {
            throw new NotImplementedException();
        }

        public void Refresh()
        {
            throw new NotImplementedException();
        }

        public void SetEffectEnable(bool enable)
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            throw new NotImplementedException();
        }
    }
}
