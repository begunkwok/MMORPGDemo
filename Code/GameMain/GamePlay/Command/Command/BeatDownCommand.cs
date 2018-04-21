using UnityEngine;

namespace GameMain
{
    public class BeatDownCommand : ICommand
    {
        public float LastTime;

        public BeatDownCommand()
        {
            
        }

        public BeatDownCommand(float lastTime)
        {
            this.LastTime = lastTime;
            CommandType = CommandType.Beatdown;
        }

        public CommandType CommandType { get; }
    }
}
