using UnityEngine;

namespace GameMain
{
    public class RideCommand : ICommand
    {
        public RideCommand(float lastTime)
        {
            CommandType = CommandType.Ride;
        }

        public CommandType CommandType { get; }
    }
}
