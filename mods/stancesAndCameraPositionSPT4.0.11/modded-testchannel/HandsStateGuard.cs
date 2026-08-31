using EFT;
using EFT.InventoryLogic;

namespace CameraRotationMod
{
    /// <summary>
    /// Guard de segurança para checar disponibilidade de mãos e inventário
    /// antes de aplicar mudanças de postura que exigem mãos livres e arma de fogo ativa.
    /// Evita a rejeição de transições e acúmulo de pacotes durante recarga, medicina ou looting.
    /// </summary>
    public static class HandsStateGuard
    {
        public static bool CanChangeStance(Player player)
        {
            if (player == null || player.HealthController == null || !player.HealthController.IsAlive)
                return false;

            try
            {
                var hands = player.HandsController;
                if (hands == null)
                    return false;

                // 1. Deve ser um FirearmController válido com arma em mãos
                if (!(hands is Player.FirearmController fc) || fc.Weapon == null)
                    return false;

                // 2. Não alterar se as mãos estiverem ocupadas com medkit, comida ou consumo
                if (hands.Item != null)
                {
                    var item = hands.Item;
                    if (item is MedsItemClass || item is FoodDrinkItemClass)
                    {
                        return false;
                    }
                }

                // 3. Não alterar se estiver deitado (Prone)
                if (player.IsInPronePose)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

