using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TarkovIRL
{
    public static class TIRLUtils
    {
        public enum E_DEBUG_PRIORITY { LOG, SPAM_LOG, ALWAYS_LOG };

        public static ManualLogSource Logger;
        static float _dt = 0;
        static float _spamTimer = 0;
        static float _spamTimeResolution = 2f;

        public static void LogError(string toPrint)
        {
            Logger.LogError((object)toPrint);
        }

        public static void Log(string toPrint, bool spam)
        {
            if (!PrimeMover.IsLogging.Value)
            {
                return;
            }
            if (spam)
                Logger.LogInfo((object)toPrint);
            else
            {
                _spamTimer += _dt;
                if (_spamTimer > PrimeMover.LoggingUpdateFrequency.Value)
                {
                    _spamTimer = 0;
                    Logger.LogInfo((object)toPrint);
                }
            }
        }

        public static void LogPriority(E_DEBUG_PRIORITY priority, string toPrint)
        {
            if (priority == E_DEBUG_PRIORITY.SPAM_LOG)
            {
                if (PrimeMover.IsLogging.Value && PrimeMover.DebugSpam.Value)
                {
                    Logger.LogError((object)toPrint);
                }
            }
            else if (priority == E_DEBUG_PRIORITY.LOG)
            {
                if (PrimeMover.IsLogging.Value && !PrimeMover.DebugSpam.Value)
                {
                    Logger.LogError((object)toPrint);
                }
            }
            else if (priority == E_DEBUG_PRIORITY.ALWAYS_LOG)
            {
                Logger.LogError((object)toPrint);
            }
        }

        public static void Update(float dt)
        {
            _dt = dt;
        }


        public static float FulfilledLerp(float value, float target, float step)
        {
            value += step;
            if (value > target)
            {
                value = target;
            }
            return value;
        }

        public static Quaternion GetQuatFromV3(Vector3 v)
        {
            Quaternion result = Quaternion.identity;
            result.x = v.x;
            result.y = v.y;
            result.z = v.z;

            //UtilsTIRL.Log($" returned quat {result}");

            return result;
        }
    }

}
