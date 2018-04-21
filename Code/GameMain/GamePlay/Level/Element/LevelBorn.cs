using UnityEngine;

namespace GameMain
{
    public class LevelBorn : LevelElement
    {
        public BattleCampType Camp = BattleCampType.Ally;
        private GameObject mBody;

        public override void Build()
        {
            if(mBody==null)
            {
                mBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mBody.transform.parent = transform;
                mBody.transform.localPosition = Vector3.zero;
                mBody.transform.localEulerAngles = Vector3.zero;
                mBody.transform.localScale = Vector3.one;
            }
            MeshRenderer render = mBody.GetComponent<MeshRenderer>();
            if (render == null)
            {
                return;
            }
            if (render.sharedMaterial != null)
            {
                Shader shader = Shader.Find("Custom/TranspUnlit");
                render.sharedMaterial = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            }
            switch (Camp)
            {
                case BattleCampType.Ally:
                    render.sharedMaterial.color = Color.green;
                    break;
                case BattleCampType.Enemy:
                    render.sharedMaterial.color = Color.blue;
                    break;
                case BattleCampType.Neutral:
                    render.sharedMaterial.color = Color.yellow;
                    break;
            }
        }

        public override void SetName()
        {
            gameObject.name = "Born_" + Camp.ToString();
        }

        public override XmlObject Export()
        {
            MapBorn data = new MapBorn
            {
                Camp = Camp,
                TransParam = new MapTransform
                {
                    Position = Position,
                    Scale = Scale,
                    EulerAngles = Euler
                }
            };
            return data;
        }

        public override void Import(XmlObject pData,bool pBuild)
        {
            MapBorn data = pData as MapBorn;
            if (data != null)
            {
                Camp = data.Camp;
                Position = data.TransParam.Position;
                Scale = data.TransParam.Scale;
                Euler = data.TransParam.EulerAngles;
            }
            this.Build();
            this.SetName();
        }
    }
}
