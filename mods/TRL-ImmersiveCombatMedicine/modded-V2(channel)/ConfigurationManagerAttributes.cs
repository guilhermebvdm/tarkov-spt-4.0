// Contrato de atributos do BepInEx.ConfigurationManager (match por NOME do tipo via
// reflection — a DLL instalada em plugins/spt/ConfigurationManager NÃO exporta o tipo,
// então a cópia local embutida é o mecanismo canônico; mesmo padrão de SAIN/ORBIT/
// DynamicMaps neste repo). Só os campos usados pelo mod (spec 002 §3: "Avançado = Sim").
internal sealed class ConfigurationManagerAttributes
{
    /// <summary>Entrada só aparece no F12 com "Advanced settings" habilitado.</summary>
    public bool? IsAdvanced;
}
