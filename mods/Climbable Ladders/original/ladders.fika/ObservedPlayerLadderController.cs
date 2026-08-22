using EFT;
using tarkin.ladders.bep;
using tarkin.ladders.shared;
using UnityEngine;

#if SPT_4_0
using DamageInfo = DamageInfoStruct;
#endif

namespace tarkin.ladders.fika
{
    public class ObservedPlayerLadderController : MonoBehaviour
    {
        private Player player;
        private ProceduralLadderBody body;

        private float currentVisualAngle = 0f;
        private float targetAngle = 0f;
        private float angleVelocity = 0f;

        private readonly float smoothTime = 0.1f;

        public void Init(Ladder ladder)
        {
            this.player = GetComponent<Player>();

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
                smoothTime
            );

            body.Update(currentVisualAngle);
        }

        void OnDestroy()
        {
            if (player != null)
                player.OnPlayerDead -= Player_OnPlayerDead;

            body?.Dispose();
        }
    }
}
