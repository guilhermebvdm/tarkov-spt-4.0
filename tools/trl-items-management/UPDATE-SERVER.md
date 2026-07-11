# Como atualizar o mod no servidor (passo a passo rápido)

Isso aqui é o resumo direto. Pra entender o que cada passo faz por baixo dos panos (migração
automática, o que é preservado, troubleshooting), veja [DEPLOY.md](DEPLOY.md).

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

1. Abra o **PowerShell como Administrador** (botão direito → "Executar como administrador").
2. Extraia o zip e entre na pasta extraída, por exemplo:
   ```powershell
   Expand-Archive "C:\caminho\onde\voce\salvou\trl-release-v1.0.0.zip" "C:\caminho\onde\voce\salvou" -Force
   cd "C:\caminho\onde\voce\salvou\trl-release-v1.0.0"
   ```
3. Rode o updater:
   ```powershell
   powershell -ExecutionPolicy Bypass -File .\update-vm.ps1
   ```
4. Deixa rodar até o final. Vai aparecer uma linha verde assim:
   ```
   [OK] Atualizado -> TRL-ItemsManagement 1.0.0.0
   ```

Pronto — o SPT já sobe sozinho no final (o script reinicia o servidor pra você).

**Se for a 1ª vez rodando isso nesse servidor** (setup antigo ainda instalado), sem problema —
o próprio script detecta e migra tudo automaticamente (preços/overrides preservados). Só leia os
avisos amarelos que aparecerem, se aparecerem.

## 4. Confirmar que funcionou

No navegador, **na própria máquina do servidor**, abra:

```
https://127.0.0.1:6969/TRLItemsManagement-Server/index.html
```

(o navegador vai reclamar do certificado — é o do próprio SPT, pode aceitar/prosseguir mesmo
assim). Se a lista de itens carregar, deu certo.

## Deu algo errado?

- O script para com uma mensagem de erro clara em vermelho explicando o que falhou — leia ela
  primeiro, geralmente já diz o que fazer.
- Rodar `update-vm.ps1` de novo é seguro (não perde nada, não duplica nada).
- Pra mais detalhe de qualquer passo (o que cada coisa preserva, o que fazer se algo específico
  falhar), veja a seção "Troubleshooting" do [DEPLOY.md](DEPLOY.md).
