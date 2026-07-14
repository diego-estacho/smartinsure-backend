# Segurança e dados

Regras obrigatórias. Violação bloqueia merge.

## Dinheiro e estado

- Prêmio, parcela, condição comercial, status financeiro e transição de status são calculados e validados no servidor. Nenhum desses valores é aceito do browser sem validação no servidor.

## Segredos

- Segredo nunca em arquivo versionado — **nem em teste E2E** (ponto recorrente de credencial versionada por descuido).
- Credenciais de seguradora por ambiente, via secret store / variáveis de pipeline.
- Teste automatizado usa sandbox/mock de seguradora; identidade de produção não aparece em teste.

## Agentes de IA

- Agente não roda com permissões irrestritas (`bypassPermissions` ou equivalente de outra ferramenta) em nada que envolva banco, produção ou push.
- Acesso a bancos de produção: somente leitura, garantida **pela credencial no servidor** (usuário de banco read-only por papel) — vale para qualquer ferramenta de IA ([ADR-003](adr/003-agnostico-de-ia-e-framework.md)), não por instrução em prompt nem por hook de cliente. Hooks de cliente são camada extra opcional.
- Dados pessoais de tomadores e corretores (LGPD): não copiar dados reais de produção para fixtures, prompts ou logs de agente — usar dados sintéticos. Política formal do grupo: [OPEN-02](product-specs/open-decisions.md).
