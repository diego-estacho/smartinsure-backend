# ADR-002: Convenções de git

## Status
Proposto (ratificação na Fase A)

## Contexto
Branches e worktrees precisam de um padrão que funcione no Windows e resolva os ponteiros de workspace entre os repos irmãos. O `#` no nome de branch, com worktrees em caminho longo, causava erros de MAX_PATH no Windows.

## Decisão
- Branch: `ab-NNNNN-slug-curto` (sem `#`); o vínculo com o PBI vai como `AB#NNNNN` no commit/PR.
- Worktrees nativas do git em `C:\wt\<id>\<repo>` (raiz curta: o MAX_PATH do Windows já é apertado — o caminho mais fundo do front passa de 260), com os repos como irmãos sob `<id>` para os ponteiros de workspace resolverem.
- **Toda tarefa começa numa worktree** do(s) repo(s) que a triagem determinar (só-front, só-back ou cross-repo) — nunca na checkout principal. Isso permite duas atividades ao mesmo tempo, isoladas, em terminais separados. Cross-repo: uma worktree por repo, **irmãs sob a mesma pasta da atividade** (`C:\wt\<id>\smartinsure-frontend` e `...\smartinsure-backend`), para `../smartinsure-<repo>` resolver.
- **Provisório — até existir o `AB#NNNNN`:** enquanto a tarefa não tem PBI, o identificador da atividade `<id>` é um **slug curto da tarefa**, usado como nome de branch e como pasta-pai das worktrees irmãs. Quando o AB# passar a existir, `<id>` = `ab-NNNNN-slug` e a regra recai integralmente no padrão acima — sem migração. As worktrees irmãs "conversam" pelo mesmo `<id>` hoje e pelo mesmo `AB#NNNNN` depois.
- **Limpeza após o merge:** worktree cuja branch já está contida em `origin/main` (mergeada) é removida por `python scripts/worktree-gc.py` (`git fetch --prune` → `git worktree remove` sem `--force`, preservando trabalho não commitado → `git worktree prune`). O `doctor.py` avisa quando há worktree órfã. Assim a worktree não fica ocupando disco depois do PR mergear.

## Consequências
- Raiz curta e sem espaço evita estouro de MAX_PATH no Windows.
- A integração com Azure Boards continua funcionando pelo `AB#NNNNN` no commit/PR.
