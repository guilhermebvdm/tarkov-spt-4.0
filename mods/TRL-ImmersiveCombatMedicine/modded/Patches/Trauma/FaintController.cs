using System.Collections;
using System.Reflection;
using EFT;
using HarmonyLib;
using UnityEngine;
using Fika.Core.Main.Players;

namespace TrueTrauma
{
    public class FaintController : MonoBehaviour
    {
        private Player _player;
        private FikaPlayer _fikaPlayer;
        private bool _isFainted = false;

        public void Init(Player player)
        {
            _player = player;
            _fikaPlayer = player as FikaPlayer;
        }

        private void Update()
        {
            if (_player == null || _fikaPlayer == null) return;

            string id = _player.ProfileId;

            // Se o jogador deveria estar desmaiado (timer > now)
            if (TraumaState.BlackoutTimers.TryGetValue(id, out float expireTime))
            {
                if (Time.time < expireTime)
                {
                    if (!_isFainted)
                    {
                        // Trigger faint
                        _isFainted = true;
                        if (TraumaState.Logger != null) TraumaState.Logger.LogInfo("Jogador Desmaiou! (Ragdoll Fika)");
                        
                        _fikaPlayer.ToggleDowned(true);
                        var bleedout = _fikaPlayer.gameObject.GetComponent("Bleedout");
                        if (bleedout != null) UnityEngine.Object.Destroy(bleedout);
                    }
                }
                else
                {
                    if (_isFainted)
                    {
                        // Wake up
                        _isFainted = false;
                        if (TraumaState.Logger != null) TraumaState.Logger.LogInfo("Jogador Acordou do desmaio.");

                        _fikaPlayer.ToggleDowned(false);
                        
                        // Força posição Prone após levantar do ragdoll
                        _player.MovementContext.IsInPronePose = true;
                        
                        // Remove timer to prevent looping
                        TraumaState.BlackoutTimers.Remove(id);
                    }
                }
            }
        }
    }
}

