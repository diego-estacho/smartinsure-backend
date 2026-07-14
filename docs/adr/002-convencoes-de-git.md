# ADR-002: Convenções de git

## Status
Proposto (ratificação na Fase A)

## Contexto
Branches e worktrees precisam de um padrão que funcione no Windows e resolva os ponteiros de workspace entre os repos irmãos. O `#` no nome de branch, com worktrees em caminho longo, causava erros de MAX_PATH no Windows.

## Decisão
- Branch: `ab-NNNNN-slug-curto` (sem `#`); o vínculo com o PBI vai como `AB#NNNNN` no commit/PR.
- Worktrees nativas do git em `C:\wt\ab-NNNNN\<repo>`, com os repos como irmãos para os ponteiros de workspace resolverem.

## Consequências
- Raiz curta e sem espaço evita estouro de MAX_PATH no Windows.
- A integração com Azure Boards continua funcionando pelo `AB#NNNNN` no commit/PR.
