using EFT;
using EFT.InventoryLogic;

namespace CameraRotationMod
{
    /// <summary>
    /// Guard de segurança para checar disponibilidade de mãos e inventário
    /// antes de aplicar mudanças de postura que exigem mãos livres.
    /// Evita a rejeição de transições e acúmulo de pacotes durante recarga ou looting.
    /// </summary>
    public static class HandsStateGuard
    {
        public static bool CanChangeStance(Player player)
        {
            if (player == null || player.HandsController == null)
                return false;

            try
            {
                var hands = player.HandsController;

                // 1. Não alterar se as mãos estiverem ocupadas com medkit, comida ou consumo
                if (hands.Item != null)
                {
                    var item = hands.Item;
                    if (item is MedsItemClass || item is FoodDrinkItemClass)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return true;
            }
        }
    }
}
