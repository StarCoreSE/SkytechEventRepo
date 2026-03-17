using System;
using VRageMath;

namespace Skytech.Engines
{
    internal class FuelEngineCarburettor : AssemblyBase
    {
        public float ExternalVentingFactor()
        {
            float num = 0f;
            for (int i = 0; i < this.Exhausts.Count; i++)
            {
                bool flag = !this.Exhausts[i].LinkedUp;
                if (flag)
                {
                    num += (float)(this.HadBlockedExhaust ? 0 : 1);
                }
                else
                {
                    num += ((this.Exhausts[i].ExitInfo == null) ? 0f : this.Exhausts[i].ExitInfo.AirflowModifier);
                }
            }
            for (int j = 0; j < this.Turbos.Count; j++)
            {
                bool flag2 = !this.Turbos[j].LinkedUp;
                if (flag2)
                {
                    num += (float)(this.HadBlockedExhaust ? 0 : 1);
                }
                else
                {
                    num += ((this.Turbos[j].ExitInfo == null) ? 0f : this.Turbos[j].ExitInfo.AirflowModifier);
                }
            }
            float num2 = MathHelper.Clamp(num / (float)(this.Exhausts.Count + this.Turbos.Count));
            this.HadBlockedExhaust = (num2 < 1f);
            return num2;
        }
    }
}
