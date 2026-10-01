# 010 — Conexão em Raid em Andamento e Sistema de Senha Opcional · Review Técnica 01

**Mod:** FIKA  
**Target:** `mods/FIKA/modded-V2/`  
**Spec técnica:** [010-join-in-progress-senha-lobby-headless-02-spec-tech.md](010-join-in-progress-senha-lobby-headless-02-spec-tech.md)  
**Revisor:** Antigravity (Advanced Agentic Coding / Pair Programming)  
**Data:** 2026-09-13  
**Veredito:** 🟢 APROVADO COM RECOMENDAÇÕES  

---

## 1. Avaliação Arquitetural e Viabilidade

A revisão técnica confirma a viabilidade e excelência do desenho técnico refinado:
1. **Consistência Visual da Interface:** O aproveitamento direto do padrão e layout da janela **"CONFIGURAÇÕES DE SESSÃO"** (`DediSelection`) para abrigar o campo de senha no Host e a replicação dessa mesma identidade visual para o modal de entrada/invasão garante 100% de harmonia com o estilo gráfico do EFT e do FIKA (moldura escura, barra superior, botão `X` vermelho no canto superior direito e botão de ação inferior).
2. **Cobertura Completa de Pontos de Entrada:** O acoplamento da janela de confirmação/senha aos dois fluxos de entrada do cliente — tanto pela tela de incursões (`MatchMakerUIScript`) quanto pela lista de jogadores online do menu principal (`MainMenuUIScript`) — evita redundância de código e impede que um jogador burle a checagem de senha conectando-se diretamente pelo menu.
3. **Propagação Transparente no Headless:** O trajeto do dado de senha através de `StartHeadlessRequest` (WebSocket) até o `FikaHeadlessPlugin` e registro no `FikaServer` via `CreateMatch` mantém a arquitetura limpa, desacoplada e sem hacks em disco.

---

## 2. Recomendações e Diretrizes de Implementação

### 2.1. UX e Comportamento da Janela de Entrada
- **Recomendação 1 (Reutilização de Prefab / Componente Unificado):** Criar a classe `JoinSessionModal` como um prefab ou clonagem de `_fikaMatchMakerUi.DediSelection`. Isso reaproveita o background, bordas, shaders e fontes TextMeshPro nativas do bundle sem risco de desalinhamento de layout.
- **Recomendação 2 (Foco Automático e Tecla Enter):** Ao abrir a janela de entrada com campo de senha, focar automaticamente no `TMP_InputField` (`Select()` / `ActivateInputField()`) e permitir que pressionar a tecla `Enter` dispare o botão de confirmação, agilizando a experiência do usuário.
- **Recomendação 3 (Fechamento Seguro no 'X'):** O botão `X` vermelho deve resetar qualquer estado transitório (`JoinInProgress = false`, `ToggleLoading(false)`), garantindo que a tela anterior volte ao estado interativo normal sem bloqueios.

### 2.2. Validação e Segurança
- **Recomendação 4 (Sanitização e Mascaramento):** O campo de senha deve utilizar `TMP_InputField.ContentType.Password` e aplicar `.Trim()` antes de submeter a requisição, prevenindo senhas com espaços invisíveis.
- **Recomendação 5 (Feedback de Senha Incorreta):** Caso o servidor retorne rejeição por senha incorreta, manter a janela modal aberta e exibir uma mensagem em vermelho (ex: *"Senha incorreta. Tente novamente."*), permitindo que o jogador corrija a digitação sem precisar reiniciar o processo de entrada do zero.

---

## 3. Matriz de Riscos e Mitigações

| Risco Identificado | Severidade | Probabilidade | Mitigação Arquitetural |
|---|---|---|---|
| Entrada pelo Menu Online burlar a senha | Alta | Baixa | `MainMenuUIScript.cs:240` intercepta o clique do botão de join e delega a abertura do mesmo `JoinSessionModal`, enviando a senha no payload `MatchJoinRequest`. |
| Desalinhamento visual do campo injetado em `DediSelection` | Baixa | Baixa | Utilizar âncoras verticais relativas (`VerticalLayoutGroup` ou posicionamento baseado na altura do `HeadlessSelection` e `StartButton`). |
| Invasor entrar enquanto a raid está finalizando / extraindo | Média | Baixa | `FikaServer` rejeita conexões de novos invasores se o status da raid for diferente de `IN_GAME` ou se o Host já tiver acionado o pipeline de extração. |
| Invasor spawnar em local colidente com geometria física | Média | Baixa | Utilizar os pontos de spawn válidos da cena do EFT, checando distância dos demais jogadores. |

---

## 4. Conclusão da Revisão

A especificação atende rigorosamente a todos os requisitos solicitados pelo usuário, estabelecendo uma solução robusta, visualmente polida e compatível com as regras de desenvolvimento do repositório. O plano está apto para execução.
