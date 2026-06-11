using EFT;
using EFT.Animations;
using UnityEngine;

namespace CameraRotationMod
{
    public enum EBracingDirection
    {
        Top,
        Left,
        Right,
        None
    }

    // Item 004 (06-fix-01): estado explícito do mount PRÓPRIO, substituindo a dependência do mount
    // nativo do EFT (MovementContext.IsInMountedState). Unifica o critério de superfície entre passivo
    // e ativo (mesmo raycast). Passivo = só benefícios (recoil/sway), SEM "grude", ícone transparente.
    // Ativo = tecla, COM "grude", ícone sólido, e reset completo da Stance 0 ao sair.
    public enum EMountState { None, Passive, Active }

    public class MountingManager : MonoBehaviour
    {
        public static MountingManager Instance { get; private set; }

        public static EMountState MountState { get; private set; } = EMountState.None;

        // Semântica EXCLUSIVA (revisão #2): os patches de benefício leem IsMounting (full) e IsBracing
        // (parcial) em ramos if/else-if; manter exclusivos evita sobreposição.
        public static bool IsMounting => MountState == EMountState.Active;   // ativo: grude + force Stance 0
        public static bool IsBracing  => MountState == EMountState.Passive;  // passivo: só benefício parcial
        public static bool CanMount   => MountState != EMountState.None;
        public static EBracingDirection BracingDirection { get; private set; } = EBracingDirection.None;

        private Player _player;

        // Pontos de partida dos raycasts, em local do WeaponRootAnim (ponta da arma — ponto canônico
        // de colisão de arma do EFT; respeita o comprimento real via `ln`).
        private static readonly Vector3 _startLeftDir  = new Vector3(0.143f, 0f, 0f);
        private static readonly Vector3 _startRightDir = new Vector3(-0.143f, 0f, 0f);
        private static readonly Vector3 _startDownDir  = new Vector3(0f, 0f, -0.19f);

        private static float _lastDetectTime = 0f;
        private const float DetectCooldown = 0.1f; // throttle de detecção (~10x/s) — leve no hot path

        private void Awake() { Instance = this; }

        // ==========================================================================
        // Transição central de estado
        // ==========================================================================
        public static void SetMountState(EMountState newState, EBracingDirection dir)
        {
            EMountState old = MountState;
            MountState = newState;
            BracingDirection = (newState == EMountState.None) ? EBracingDirection.None : dir;

            if (old == newState) return; // só a direção mudou — sem efeitos de transição

            var gw = StanceManager.GetCachedGameWorld();
            var mp = gw?.MainPlayer;
            if (mp != null)
            {
                var fc = mp.HandsController as Player.FirearmController;
                if (fc != null && fc.FirearmsAnimator != null)
                    fc.FirearmsAnimator.SetMounted(newState == EMountState.Active);

                // 006 (Fika): remotos veem o mount ativo via sync de stance.
                FikaSync.FikaNetworkSync.SendStanceUpdate(mp.ProfileId, StanceManager.CurrentStance, newState == EMountState.Active);
            }

            // ENTRAR no ativo: força Stance 0 (vanilla) — base limpa para o grude.
            if (newState == EMountState.Active && StanceManager.CurrentStance != Stance.Default)
                StanceManager.SetStance(Stance.Default);

            // SAIR do ativo: zera os offsets de grude imediatamente (sem resíduo). Mantém Stance 0.
            if (old == EMountState.Active && newState != EMountState.Active)
                Patches.MountingCollisionPatch.ResetCollisionOffsets();

            Plugin.Logger.LogDebug($"[Mount] {old} -> {newState} (dir={BracingDirection})");
        }

        public static void ForceNone()
        {
            if (MountState != EMountState.None) SetMountState(EMountState.None, EBracingDirection.None);
        }

        private void Update()
        {
            var gameWorld = StanceManager.GetCachedGameWorld();
            _player = gameWorld?.MainPlayer;

            if (_player == null || !_player.IsYourPlayer || _player.HealthController == null || !_player.HealthController.IsAlive)
            {
                ForceNone();
                return;
            }

            if (!Plugin._EnableWeaponMounting.Value)
            {
                ForceNone();
                return;
            }

            // Cancelar mount (ativo ou passivo) ao correr.
            if (MountState != EMountState.None && _player.IsSprintEnabled)
                ForceNone();

            // A detecção de superfície (passivo) roda no Prefix de FirearmController.method_11
            // (DetectBracing) — respeita o pipeline de animação e o comprimento real da arma (ln).
        }

        // ==========================================================================
        // Detecção de superfície — chamada pelo FirearmCollisionDetectPatch (method_11).
        // Mesmo raycast para passivo E ativo (critério unificado).
        // ==========================================================================
        public static void DetectBracing(Player.FirearmController fc, Player player, float ln)
        {
            if (Time.time - _lastDetectTime <= DetectCooldown) return;
            _lastDetectTime = Time.time;

            if (MountState == EMountState.Active) return; // ativo: não re-detecta (mantém)
            if (fc == null || fc.Weapon == null || player == null) return;

            var pwa = player.ProceduralWeaponAnimation;
            if (pwa == null || pwa.HandsContainer == null) return;
            Transform weap = pwa.HandsContainer.WeaponRootAnim;
            if (weap == null) return;

            // Prone = sempre apoiável (Top).
            if (player.IsInPronePose)
            {
                SetPassive(EBracingDirection.Top);
                return;
            }

            Vector3 up = weap.TransformDirection(Vector3.up);
            float detectLength = ln * 1.25f;

            Vector3 startDown  = weap.position + weap.TransformDirection(_startDownDir);
            Vector3 startLeft  = weap.position + weap.TransformDirection(_startLeftDir);
            Vector3 startRight = weap.position + weap.TransformDirection(_startRightDir);

            Vector3 sphereDown  = weap.position + weap.TransformDirection(new Vector3(0f,     -0.45f, -0.1f));
            Vector3 sphereLeft  = weap.position + weap.TransformDirection(new Vector3(0.05f,  -0.5f,  -0.065f));
            Vector3 sphereRight = weap.position + weap.TransformDirection(new Vector3(-0.05f, -0.5f,  -0.065f));

            Vector3 dirDown  = startDown  - up * detectLength;
            Vector3 dirLeft  = startLeft  - up * detectLength;
            Vector3 dirRight = startRight - up * detectLength;

            if (CheckCover(EBracingDirection.Top,   startDown,  dirDown,  sphereDown,  0.045f) ||
                CheckCover(EBracingDirection.Left,  startLeft,  dirLeft,  sphereLeft,  0.09f)  ||
                CheckCover(EBracingDirection.Right, startRight, dirRight, sphereRight, 0.09f))
            {
                return; // SetPassive já chamado dentro de CheckCover
            }

            // Nada detectado: se estava passivo, volta a None.
            if (MountState == EMountState.Passive)
                SetMountState(EMountState.None, EBracingDirection.None);
        }

        private static bool CheckCover(EBracingDirection dir, Vector3 start, Vector3 lineEnd, Vector3 spherePos, float radius)
        {
            int layerMask = LayerMaskClass.HighPolyWithTerrainMask;
            int playerLayer = LayerMask.NameToLayer("Player");

            if (Physics.Linecast(start, lineEnd, out RaycastHit hit, layerMask) &&
                hit.collider.gameObject.layer != playerLayer)
            {
                SetPassive(dir);
                return true;
            }

            foreach (var c in Physics.OverlapSphere(spherePos, radius, layerMask))
            {
                if (c.gameObject.layer != playerLayer)
                {
                    SetPassive(dir);
                    return true;
                }
            }
            return false;
        }

        // Entra/atualiza o estado passivo (nunca rebaixa o ativo).
        private static void SetPassive(EBracingDirection dir)
        {
            if (MountState == EMountState.Active)
            {
                BracingDirection = dir; // ativo: só atualiza a direção do ícone
                return;
            }
            SetMountState(EMountState.Passive, dir);
        }
    }
}
