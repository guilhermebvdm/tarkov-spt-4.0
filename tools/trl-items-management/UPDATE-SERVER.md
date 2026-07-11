# Como atualizar o mod no servidor (passo a passo rápido)

Isso aqui é o resumo direto. Pra entender o que cada passo faz por baixo dos panos (migração
automática, o que é preservado, troubleshooting), veja [DEPLOY.md](DEPLOY.md).

**Rede de segurança:** nada nesse processo apaga dado de produção. O setup antigo (se ainda
existir) é **movido** (não apagado) pra uma pasta `_removed-mods\`, e os overrides dele viram um
backup em `_backup-migration-items-management-*\` antes de qualquer coisa ser tocada. Se algo der
errado, dá pra voltar atrás.

## 1. No seu PC: gerar o pacote

Abra o Git Bash na raiz do repo e rode:

```bash
bash tools/trl-items-management/scripts/package-release.sh
```

Espere terminar. No final ele mostra onde salvou o zip, algo como:

```
✓ bundle: /c/Repos/spt/tarkov-spt-4.0/dist/trl-release-v1.0.0.zip (960K)
```

Esse é o arquivo que você vai levar pro servidor.

## 2. Levar o zip pro servidor

Copie esse `.zip` pro servidor (AnyDesk — arrasta o arquivo ou usa a transferência de arquivos).
Não precisa extrair ainda.

## 3. No servidor: aplicar a atualização

a. Abra o **PowerShell como Administrador** (botão direito → "Executar como administrador").

b. Extraia o zip e entre na pasta extraída, por exemplo:
   ```powershell
   Expand-Archive "D:\_deploy\trl-release-v1.0.0.zip" "D:\_deploy" -Force
   cd "D:\_deploy\trl-release-v1.0.0"
   ```

c. Rode o updater:
   ```powershell
   powershell -ExecutionPolicy Bypass -File .\update-vm.ps1
   ```
   Isso usa os caminhos padrão (`D:\SPT 4.0\SPT`, etc.). **Se o script reclamar que não achou
   `SPT_Data` ou `BepInEx`**, os caminhos desse servidor são diferentes do padrão — passe
   `-SptPath`/`-GameRoot`/`-ToolDir` apontando pro lugar certo (exemplos no
   [DEPLOY.md](DEPLOY.md)).

d. Deixa rodar até o final. Vai aparecer uma linha verde assim:
   ```
   [OK] Atualizado -> TRL-ItemsManagement 1.0.0.0
   ```

Pronto — o SPT já sobe sozinho no final (o script reinicia o servidor pra você).

**Se for a 1ª vez rodando isso nesse servidor** (setup antigo ainda instalado), sem problema —
o próprio script detecta e migra tudo automaticamente (preços/overrides preservados). Se essa
também for a 1ª vez que o pipeline de catálogo roda **nesse `ToolDir`** (sem nenhum preço em
cache ainda), o passo de regenerar o catálogo pode falhar pedindo pra rodar de novo com
**`-Fetch`** (precisa internet) — é só adicionar essa flag e rodar de novo.

## 4. Confirmar que funcionou

No navegador, **na própria máquina do servidor**, abra:

```
https://127.0.0.1:6969/TRLItemsManagement-Server/index.html
```

(o navegador vai reclamar do certificado — é o do próprio SPT, pode aceitar/prosseguir mesmo
assim). Se a lista de itens carregar, deu certo.

## Deu algo errado?

- **Vermelho = parou de verdade.** O script mostra a mensagem de erro e para ali — leia ela
  primeiro, geralmente já diz o que fazer (ex.: "use `-Fetch`", "ajuste `-SptPath`").
- **Amarelo = aviso, mas continuou.** Coisas como "sessão não elevada" ou "falha ao remover
  serviço antigo" não interrompem a atualização — só avisam de algo que vale conferir depois.
- Rodar `update-vm.ps1` de novo é seguro (não perde nada, não duplica nada).
- Pra mais detalhe de qualquer passo (o que cada coisa preserva, o que fazer se algo específico
  falhar), veja a seção "Troubleshooting" do [DEPLOY.md](DEPLOY.md).
