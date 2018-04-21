using UnityEngine;

namespace GameMain
{
    public class ActorEnemy : ActorBase
    {
        public MonsterType MonsterType { get; private set; }

        public ActorEnemy(int entityId, int id, GameObject go, ActorType type, BattleCampType camp, CharacterController cc, Animator anim) : base(entityId, id, go, type, camp, cc, anim)
        {
            MonsterType = (MonsterType) m_ActorData.MonsterType;

        }

        public bool IsBoss()
        {
            return MonsterType == MonsterType.Boss || MonsterType == MonsterType.World;
        }

        public bool IsChest()
        {
            return MonsterType == MonsterType.Chest;
        }

    }
}
