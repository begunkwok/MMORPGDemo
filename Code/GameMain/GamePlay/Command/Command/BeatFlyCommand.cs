using UnityEngine;

namespace GameMain
{
    public class BeatFlyCommand : ICommand
    {
        public float LastTime;

        public BeatFlyCommand()
        {

        }

        public BeatFlyCommand(float lastTime)
        {
            this.LastTime = lastTime;
            CommandType = CommandType.Beatfly;
        }

        public CommandType CommandType { get; }
    }
}
