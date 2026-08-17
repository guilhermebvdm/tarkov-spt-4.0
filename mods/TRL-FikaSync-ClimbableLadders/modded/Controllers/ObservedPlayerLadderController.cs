using System;
using EFT;
using tarkin.ladders.bep;
using tarkin.ladders.shared;
using UnityEngine;

#if SPT_4_0
using DamageInfo = DamageInfoStruct;
#endif

namespace TRL.FikaSync.ClimbableLadders.Controllers
{
    [DefaultExecutionOrder(100)] // Executa após o Animator e movimentação base do ObservedPlayer
    public class ObservedPlayerLadderController : MonoBehaviour
    {
        private Player player;
        private ProceduralLadderBody body;

        private float currentVisualAngle = 0f;
        private float targetAngle = 0f;
        private float angleVelocity = 0f;

        private const float SmoothTime = 0.08f;

        public void Init(Ladder ladder)
        {
            player = GetComponent<Player>();

            player.HideWeapon();

            body = new ProceduralLadderBody(player, ladder);

            currentVisualAngle = 0f;
            targetAngle = 0f;
            angleVelocity = 0f;

            player.OnPlayerDead += Player_OnPlayerDead;
        }

        private void Player_OnPlayerDead(Player deadPlayer, IPlayer lastAggressor, DamageInfo damageInfo, EBodyPart part)
        {
            Destroy(this);
        }

        public void ReceiveBarAngle(float angle)
        {
            targetAngle = angle;
        }

        private void LateUpdate()
        {
            if (body == null || player == null)
                return;

            currentVisualAngle = Mathf.SmoothDampAngle(
                currentVisualAngle,
                targetAngle,
                ref angleVelocity,
                SmoothTime
            );

            body.Update(currentVisualAngle);
        }

        private void OnDestroy()
        {
            if (player != null)
            {
                player.OnPlayerDead -= Player_OnPlayerDead;

                if (player.HealthController != null && player.HealthController.IsAlive)
                {
                    player.RevealWeapon();
                }
            }

            body?.Dispose();
            body = null;
        }
    }
}
