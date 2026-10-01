using EFT;
using tarkin.ladders.bep;
using tarkin.ladders.shared;
using UnityEngine;

#if SPT_4_0
using DamageInfo = DamageInfoStruct;
#endif

namespace tarkin.ladders.fika
{
    [DefaultExecutionOrder(100)]
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
            this.player = GetComponent<Player>();

            // ref: AUD-01-02 Oculta arma no clone remoto em 3ª pessoa
            player?.HideWeapon();

            body = new ProceduralLadderBody(player, ladder);

            currentVisualAngle = 0f;
            targetAngle = 0f;

            player.OnPlayerDead += Player_OnPlayerDead;
        }

        private void Player_OnPlayerDead(Player player, IPlayer lastAggressor, DamageInfo damageInfo, EBodyPart part)
        {
            Destroy(this);
        }

        public void ReceiveBarAngle(float angle)
        {
            targetAngle = angle;
        }

        void LateUpdate()
        {
            if (body == null) return;

            currentVisualAngle = Mathf.SmoothDampAngle(
                currentVisualAngle,
                targetAngle,
                ref angleVelocity,
                SmoothTime
            );

            body.Update(currentVisualAngle);
        }

        void OnDestroy()
        {
            if (player != null)
            {
                player.OnPlayerDead -= Player_OnPlayerDead;

                // ref: AUD-01-02 Restaura arma ao sair da escada se o jogador estiver vivo
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
