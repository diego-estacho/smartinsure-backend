# Tech debt tracker

Dívida técnica aceita conscientemente é registrada aqui com dono e gatilho de revisão (processo em [PLANS.md](../PLANS.md)).

| ID | Descrição | Motivo de aceitar | Dono | Gatilho de revisão |
|---|---|---|---|---|
| TD-001 | `check-harness.py` e `doctor.py` duplicados nos dois repos, sem módulo compartilhado | repos separados clonados isoladamente no CI (ADR-001) impedem import cruzado; custo de manter em sincronia é baixo hoje | harness (Diego) | 3ª divergência entre os lints (regra das 3 ocorrências) ou surgimento de um 3º repo |
| TD-002 | Roles vindas do token do IdP honradas pela policy (`JwtOptions.RoleClaimType`) — segundo caminho de confiança pro perfil Administrador do Sistema além do Profile da plataforma | RN-009/RN-010 não resolvem ambiguidade de fonte de verdade; comportamento preservado por compatibilidade com deploy existente | backend | antes de qualquer configuração de roles no Casdoor — decidir se deixa de honrar roles de token OU registra em SECURITY/ADR que Casdoor nunca emite essa role |
| TD-003 | Invariante do último admin (RN-010) é check-then-act sem serialização — duas revogações concorrentes podem zerar os admins | recuperável pelo runbook; risco baixo enquanto houver procedimento manual; troca necessária só com múltiplos admins ativos em produção | backend | quando houver mais de um admin ativo em produção |
| TD-004 | Corrida de unicidade de CNPJ devolve 500 (`DbUpdateException` do índice único) em vez de 409 | integridade garantida pelo banco; erro raro em produção pois é duplicação intencional; global de `DbUpdateException` deve cobrir futuros índices únicos | backend | primeiro caso real ou tratamento global de DbUpdateException |
| TD-005 | `ListAsync` de Seguradoras ordena só por `CorporateName` — paginação não-determinística em empates | comportamento correto em volume pequeno de dados; necessário quando temos muitas seguradoras com nomes idênticos | backend | junto da próxima mudança na listagem — adicionar `.ThenBy(Id)` |
