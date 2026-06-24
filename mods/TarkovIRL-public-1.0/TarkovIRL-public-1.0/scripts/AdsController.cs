using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TarkovIRL
{
    public static class AdsController
    {
        // readonlys

        // vars
        static bool _isAiming = false;

        public static void SetAimThisFrame(bool isAiming)
        {
            _isAiming = isAiming;
        }
    }
}
