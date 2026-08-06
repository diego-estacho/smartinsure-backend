# Documentação gerada

Todo arquivo nesta pasta é **derivado por script** — nunca editado à mão (edição manual se perde na próxima regeneração e é reprovada em review). Cada arquivo tem um gerador correspondente em `scripts/`.

Previstos a partir da Fase B (exec-plan 0001):

- `db-schema.md` — schema do banco, derivado das migrations aplicadas (repositório `DBMigrations`).
- `rastreabilidade-rn.md` — mapa RN ↔ teste, derivado dos Traits/describes dos dois repositórios do workspace.
- Resumo do contrato `openapi.json` publicado no CI.

## Política de merge do `openapi.json`

O `openapi.json` é versionado (o front o consome direto, via `../smartinsure-backend/docs/generated/openapi.json` → `pnpm types:gen`, e o diff do PR torna a mudança de contrato revisável), mas é **100% derivado**: quem o produz é o CI, não a mão.

Por isso o `.gitattributes` marca o arquivo com `-merge`: num conflito de merge/rebase, o git **mantém a versão atual sem inserir marcadores** — não tente reconciliar 8 mil linhas à mão. **Conflitou → regenere** (deixe o CI republicar, ou rode o gerador do contrato) e siga. `linguist-generated=true` colapsa o diff no GitHub (o revisor ainda expande).

Regra prática em mudança de contrato (cross-repo, ADR-001): o **backend sobe primeiro** e o CI republica o `openapi.json`; só então o front regenera os types. Assim ninguém edita este arquivo à mão.

> Zerar o conflito de vez (em vez de só evitar os marcadores) é decisão de arquitetura — publicar o contrato como artefato versionado (pacote/asset com versão fixada) consumido pelo front. Fica para ADR própria quando o dono de arquitetura priorizar.
