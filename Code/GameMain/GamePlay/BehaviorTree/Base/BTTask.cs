using GameMain;

namespace BT
{
    public class BTTask : Action
    {
        public ActorBase Owner { get; protected set; }
    
        protected override bool Enter()
        {
            if (Owner == null)
            {
                Owner = GameEntry.BT.GetOwnerByNode(this);
            }
            return true;
        }
    }
}
