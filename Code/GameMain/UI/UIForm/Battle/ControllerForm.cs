using FairyGUI;
using GameFramework;

namespace GameMain
{
    public class ControllerForm : FairyGuiForm
    {
        private JoystickModule m_Joystick;
        private TouchPadModule m_TouchPad;

        private string m_horizontalAxisName = "Horizontal";
        private string m_verticalAxisName = "Vertical";
        private string m_touchPadXAxisName = "TouchPadX";
        private string m_touchPadYAxisName = "TouchPadY";

        private VirtualAxisBase m_HorizontalVirtualAxis;
        private VirtualAxisBase m_VerticalVirtualAxis;
        private VirtualAxisBase m_touchPadXAxis;
        private VirtualAxisBase m_touchPadYAxis;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            m_Joystick = new JoystickModule(UI);
            m_Joystick.OnMove.Add(this.OnJoystickMove);
            m_Joystick.OnEnd.Add(this.OnJoystickEnd);

            m_TouchPad = new TouchPadModule(UI);
            m_TouchPad.OnMove.Add(this.OnTouchPadMove);
            m_TouchPad.OnEnd.Add(this.OnTouchPadEnd);

            //注册输入
            m_HorizontalVirtualAxis = new VirtualAxisBase(m_horizontalAxisName);
            m_VerticalVirtualAxis = new VirtualAxisBase(m_verticalAxisName);
            m_touchPadXAxis = new VirtualAxisBase(m_touchPadXAxisName);
            m_touchPadYAxis = new VirtualAxisBase(m_touchPadYAxisName);

            GameEntry.Input.RegisterVirtualAxis(m_HorizontalVirtualAxis);
            GameEntry.Input.RegisterVirtualAxis(m_VerticalVirtualAxis);
            GameEntry.Input.RegisterVirtualAxis(m_touchPadXAxis);
            GameEntry.Input.RegisterVirtualAxis(m_touchPadYAxis);
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            m_TouchPad.Update();
        }

        protected override void OnClose(object userData)
        {
            base.OnClose(userData);

            m_Joystick.OnMove.Remove(this.OnJoystickMove);
            m_Joystick.OnEnd.Remove(this.OnJoystickEnd);

            m_TouchPad.OnMove.Remove(this.OnTouchPadMove);
            m_TouchPad.OnEnd.Remove(this.OnJoystickEnd);

            GameEntry.Input.UnRegisterVirtualAxis(m_horizontalAxisName);
            GameEntry.Input.UnRegisterVirtualAxis(m_verticalAxisName);
            GameEntry.Input.UnRegisterVirtualAxis(m_touchPadXAxisName);
            GameEntry.Input.UnRegisterVirtualAxis(m_touchPadYAxisName);
        }


        private void OnJoystickEnd()
        {
            m_HorizontalVirtualAxis.Update(0);
            m_VerticalVirtualAxis.Update(0);
        }

        private void OnJoystickMove(EventContext context)
        {
            JoystickEventData data = (JoystickEventData) context.data;
            //Log.Warning(data.DeltaX + "****" + data.DelatY + "****" + data.Degree);
            m_HorizontalVirtualAxis.Update(data.DeltaX);
            m_VerticalVirtualAxis.Update(data.DeltaY);
        }

        private void OnTouchPadEnd()
        {
            //Log.Warning("Pad end");
            m_touchPadXAxis.Update(0);
            m_touchPadYAxis.Update(0);
        }

        private void OnTouchPadMove(EventContext context)
        {
            TouchPadModuleData data = (TouchPadModuleData) context.data;
           // Log.Warning(data.DeltaX + "****" + data.DeltaY);
            m_touchPadXAxis.Update(data.DeltaX);
            m_touchPadYAxis.Update(data.DeltaY);
        }
    }
}
