using EFT;
using EFT.InventoryLogic;

namespace TrueTrauma.Helpers
{
    /// <summary>
    /// Guard de segurança para verificar a integridade e disponibilidade do controlador de mãos/inventário
    /// do jogador antes de executar ações de medicina ou animações customizadas dos mods.
    /// Evita a rejeição de pacotes no Fika e previne a trava 'hands controller can't perform this operation'.
    /// </summary>
    public static class HandsStateGuard
    {
        public static bool CanPerformInteraction(Player player)
        {
            if (player == null || player.HandsController == null)
                return false;

            try
            {
                var hands = player.HandsController;

                // 1. Checar se as mãos estão ocupadas por itens medicinais ou de consumo em progresso
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
                // Em caso de falha defensiva na checagem, libera execução segura
                return true;
            }
        }
    }
}
