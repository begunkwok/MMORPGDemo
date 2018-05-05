using System.Collections.Generic;
using GameFramework;
using GameFramework.Event;

namespace GameMain
{
    public class BoardFormManager : Singleton<BoardFormManager>
    {
        private readonly Dictionary<int,RoleBoardForm> m_BoardForms = new Dictionary<int, RoleBoardForm>();

        public override void Init()
        {
            base.Init();

            GameEntry.Event.Subscribe(RefreshBoardEventArgs.EventId, OnRefreshBoard);
        }

        public void Clear()
        {
            foreach (var roleBoardForm in m_BoardForms)
            {
                GameEntry.UI.CloseUIForm(roleBoardForm.Value);
            }
            m_BoardForms.Clear();

            GameEntry.Event.Unsubscribe(RefreshBoardEventArgs.EventId, OnRefreshBoard);
        }

        public void Create(BoardFormData data)
        {
            if(data == null || data.OwnerId == 0)
                return;

            if (m_BoardForms.ContainsKey(data.OwnerId))
            {
                Log.Error("Board is exit.Id:{0}", data.OwnerId);
                return;
            }

            RoleBoardForm board = GameEntry.UI.OpenAndGetForm(UIFormId.RoleBoardForm, data) as RoleBoardForm;

            if (board == null)
            {
                Log.Error("Conver Fail");
                return;
            }

            m_BoardForms.Add(data.OwnerId, board);
        }

        public void Release(int ownerId)
        {
            RoleBoardForm board;
            if (m_BoardForms.TryGetValue(ownerId, out board))
            {
               GameEntry.UI.CloseUIForm(board);
            }
            else
            {
                Log.Error("Can no find board.");
            }
        }

        public void OnRefreshBoard(object sender, GameEventArgs e)
        {
            RefreshBoardEventArgs ne = e as RefreshBoardEventArgs;
            if (ne == null)
                return;

            RoleBoardForm board;
            if(m_BoardForms.TryGetValue(ne.OwnerId,out board))
            {
                board.Refresh(ne.CurHp, ne.MaxHp, ne.Level);
            }
        }

    }
}
