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
- **Limpeza de worktrees mergeadas:** worktree cuja branch já foi mergeada em `origin/main` é removida por `python scripts/worktree-gc.py`. "Mergeada" cobre os **três modos de merge do GitHub** — merge commit (branch é ancestral de `origin/main`) e **squash/rebase** (a árvore final da branch já tem patch equivalente upstream, detectado via `git commit-tree` + `git cherry`, sem depender de `gh`/rede). Fluxo: `git fetch --prune` → `git worktree remove` sem `--force` (preserva trabalho não commitado) → `git worktree prune`. No **Windows**, o `git worktree remove` deixa `node_modules`/`.output` órfãos; o script apaga esse resíduo e a pasta-pai `<slug>` vazia, liberando o disco de fato. O `doctor.py` usa o mesmo critério para avisar. Roda-se o gc no **início de cada tarefa** (Passo 0) e após o merge — assim nenhuma worktree mergeada fica ocupando disco. **Segurança (nunca apagar trabalho ativo):** o gc jamais remove a worktree **atual** (a de onde ele roda) nem uma branch **recém-criada** cuja ponta ainda é igual à de `origin/main` (sem commits próprios) — no Passo 0 você está exatamente nesse estado, e reapá-la apagaria a worktree onde você trabalha.
- **Sincronizar a branch antes do push:** antes de `git push`/abrir PR, `git pull origin main` na branch — nenhuma branch abre PR atrás da main do GitHub (evita PR desatualizado e conflito no merge). O `worktree-gc` avisa quando a branch atual está atrás de `origin/main`, reaproveitando o `git fetch --prune` que já faz no Passo 0.

## Consequências
- Raiz curta e sem espaço evita estouro de MAX_PATH no Windows.
- A integração com Azure Boards continua funcionando pelo `AB#NNNNN` no commit/PR.
