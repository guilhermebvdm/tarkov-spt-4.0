using System.Linq;
using System.Reflection;
using EFT.HealthSystem;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SkillsExtended.Skills.FieldMedicine.Patches;

// smethod_0 (patcheado por StimulatorApplyBuffPatch) é estático e não recebe nenhum dado de qual
// entidade está usando o estimulante. method_6 (instância, dentro de Stimulator) TEM esse contexto
// via base.HealthController — este patch captura esse valor antes de smethod_0 rodar.
// ref: AUD-01-03 (relatorio-auditoria-codigo-01.md) / item de backlog 002
// ref: CR-01-01 (002-corrigir-cap-medicamento-instancia) — reflection guardada com FirstOrDefault/
// null-check em vez de First/acesso direto, consistente com o padrão defensivo que este item inteiro
// existe pra introduzir (o mesmo tipo de falha silenciosa do MeleeSpeedPatch original).
internal class StimulatorHealthControllerTrackerPatch : ModulePatch
{
    // "HealthController" é property pública, declarada em ActiveHealthController.GClass3008
    // (Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs:361-369). Type.GetProperty inclui
    // membros públicos herdados por padrão, então buscar a partir de "Stimulator" já basta.
    private static readonly PropertyInfo HealthControllerProperty = ResolveHealthControllerProperty();

    private static PropertyInfo ResolveHealthControllerProperty()
    {
        var stimulatorType = typeof(ActiveHealthController).GetNestedType("Stimulator", BindingFlags.NonPublic);
        return AccessTools.Property(stimulatorType, "HealthController");
    }

    // Válido só durante a execução síncrona de method_6 (Unity/Harmony rodam no mesmo thread aqui,
    // então não há concorrência real entre o Prefix e o Postfix da mesma chamada).
    internal static ActiveHealthController CurrentEffectOwner;

    protected override MethodBase GetTargetMethod()
    {
        // ref: Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs:2666
        // Resolvido por assinatura (não por nome literal "method_6") — nomes gerados (method_N)
        // podem mudar entre gerações do decompile mesmo pra mesma versão do jogo (AUD-01-03/PA-01-03).
        // FirstOrDefault (não First): se a assinatura não bater em nenhum método, o framework de
        // patch trata GetTargetMethod() == null como "patch não aplicável" e loga, em vez de lançar
        // InvalidOperationException no boot do plugin.
        var stimulatorType = typeof(ActiveHealthController).GetNestedType("Stimulator", BindingFlags.NonPublic);
        var method = AccessTools.GetDeclaredMethods(stimulatorType)
            .FirstOrDefault(m => m.Name.StartsWith("method_")
                                  && m.GetParameters().Length == 2
                                  && m.GetParameters()[0].ParameterType.Name == "Class2222"
                                  && m.GetParameters()[1].ParameterType == typeof(bool));

        if (method == null)
        {
            SkillsExtendedPlugin.Log.LogError(
                "[Skills Extended] StimulatorHealthControllerTrackerPatch: não achou method_6 por assinatura — patch não será aplicado.");
        }

        return method;
    }

    [PatchPrefix]
    private static void Prefix(object __instance)
    {
        if (HealthControllerProperty == null) return;
        CurrentEffectOwner = HealthControllerProperty.GetValue(__instance) as ActiveHealthController;
    }

    [PatchPostfix]
    private static void Postfix()
    {
        CurrentEffectOwner = null;
    }
}
