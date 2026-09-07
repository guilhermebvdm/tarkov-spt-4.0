# SPT-MagCheckInterrupt — Memória de Sessões

## Snapshot Delta
- **Versão:** 1.0.4 (SPT 4.0 / EFT 0.16.9)
- **Estado:** Suporte e interoperabilidade ativa com `UIFixes` moderno (`com.tyfon.uifixes`) e legado (`Tyfon.UIFixes`), sincronização de rede FIKA ativa e sem desyncs de armas.
- **Pendências:** 🟢 Nenhuma pendência registrada.

---

## 2026-09-05 — Sessão 1: Detecção Multi-GUID do UIFixes, Ativação do CanExecuteSwapPatch e Release v1.0.4

**Tema central:** Correção da falha silenciosa de detecção do plugin `UIFixes` devido à migração do GUID para o padrão reverso de domínio no SPT 4.0, reativando a permissão de movimentação de itens e troca de carregadores durante a checagem.

**Decisões-chave:**
1. **Suporte a Múltiplos GUIDs do UIFixes (`MagCheckInterrupt.cs`):** Adicionada verificação de ambos os identificadores no `Awake()`:
   `if (Chainloader.PluginInfos.ContainsKey("com.tyfon.uifixes") || Chainloader.PluginInfos.ContainsKey("Tyfon.UIFixes"))`
2. **Declaração Explícita de Dependência Suave:** Adicionados os atributos `[BepInDependency]` para ambos os GUIDs, garantindo precedência de inicialização ordenada pelo BepInEx.
3. **Reativação do `CanExecuteSwapPatch`:** Com o GUID moderno reconhecido, o patch Harmony de `FirearmController.CanExecute` passa a permitir operações de `SwapOperationClass`, `RemoveOperation` e `AttachOperation` durante o `MagCheckReloadOperation`.
4. **Isolamento de Build e SemVer:** Versão elevada para `1.0.4` em `MagCheckInterrupt.cs` e `MagCheckInterrupt.csproj`. Compilação Release com 0 avisos e 0 erros.
