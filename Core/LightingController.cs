using Fusee.Engine.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Fusee.Engine.Core.Input;

namespace Fusee.Tutorial.Core
{
    public static class LightingController
    {
        public static float lighting(float lightDir)
        {
            if (Keyboard.GetKey(KeyCodes.Left))
            {
                lightDir -= 0.5f;
            }
            else if (Keyboard.GetKey(KeyCodes.Right))
            {
                lightDir += 0.5f;
            }

            return lightDir;
        }
    }
}
