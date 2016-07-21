﻿using Fusee.Engine.Common;
using Fusee.Math.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Fusee.Engine.Core.Input;

namespace Fusee.Tutorial.Core
{
    public static class LightingController
    {
        public static float3 lighting(float3 LightPos)
        {
            if (Keyboard.GetKey(KeyCodes.Left))
            {
                LightPos.x -= 1f;
            }
            else if (Keyboard.GetKey(KeyCodes.Right))
            {
                LightPos.x += 1f;
            }

            return LightPos;
        }
    }
}
