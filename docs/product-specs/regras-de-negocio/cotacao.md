# Jornada: Cotação (solicitar Cotações às Seguradoras)

Cada RN é uma seção com o ID no título e os quatro blocos abaixo. O ID é `RN-NNN` numa **sequência única do catálogo** (não reinicia por jornada), estável e nunca reaproveitado. Linguagem de negócio, termos do [glossário](../glossario.md), sem path de código no corpo.

> Escopo desta entrega (etapa de cotações — Passo 4): solicitar as Cotações às Seguradoras a partir de um Grupo de Cotação, obtê-las e apresentá-las ao corretor num leque, permitir **selecionar** uma para seguir, e preencher/enviar a **minuta** (Tags + Cláusulas particulares) da Cotação selecionada. Ficam **fora desta fase** (demanda própria): a **emissão** (etapa de emissão), o **followup** da análise de subscrição e a Página de Detalhes da Cotação, o **cancelamento** das Cotações (irmãs na emissão, saída da cotação, expiração por tempo), e quem pode solicitar cotação por Perfil ([OPEN-03](../open-decisions.md)). Refinada em 2026-07-27 e **ratificada em 2026-07-28** por Diego Estácho no lugar da PO ([OPEN-07](../open-decisions.md)); segue aberta a re-avaliação do veredito por cláusula particular ([OPEN-21](../open-decisions.md)).

## RN-056 — Solicitação de Cotações a partir do Grupo de Cotação

**Descrição.** Ao concluir a etapa de risco, o corretor solicita as Cotações do Grupo de Cotação — uma Cotação por Seguradora. A solicitação tem dois escopos, escolhidos na tela de entrada da oferta: **todas as Seguradoras habilitadas** da Corretora (opção recomendada e padrão) ou **um subconjunto escolhido** pelo corretor a partir da lista de habilitadas.

**Pré-condições.** Grupo de Cotação em Rascunho (RN-050, RN-051). A Corretora da solicitação é a do Escopo ativo do acesso, resolvida pelo servidor (RN-103), com ao menos uma Habilitação de Seguradora ativa.

**Critério de aceitação.** Ao entrar na etapa de cotações, a plataforma solicita Cotações conforme o escopo: no modo *todas*, a cada Seguradora habilitada ativa da Corretora; no modo *escolhidas*, exatamente às Seguradoras selecionadas, e a nenhuma outra. No modo *todas*, a solicitação inclui Seguradoras que não ofertam a Modalidade do Grupo — que retornam resultado de indisponibilidade com motivo do provedor — para que o corretor enxergue o resultado de cada Seguradora. No modo *escolhidas*, as Seguradoras habilitadas **não selecionadas** aparecem no leque como **indisponíveis com motivo local** ("não incluída na solicitação"), sem serem cotadas — pela mesma transparência, sem custo nem proposta no provedor.

**Casos limite.** Corretora sem Habilitação de Seguradora ativa: solicitação recusada com aviso. Escopo *escolhidas* sem nenhuma Seguradora selecionada: recusado. Quais Perfis podem solicitar cotação e gerenciar a Habilitação de Seguradora seguem abertos ([OPEN-03](../open-decisions.md)); nesta fase, qualquer usuário autenticado.

## RN-057 — Cada Cotação é obtida e persistida por Seguradora, tolerando falha isolada

**Descrição.** A solicitação obtém as Cotações de forma independente por Seguradora e **persiste cada Cotação assim que a Seguradora responde**, vinculada ao Grupo de Cotação. A falha ou a demora de uma Seguradora não impede a obtenção das demais.

**Pré-condições.** Cotações solicitadas para o Grupo (RN-056).

**Critério de aceitação.** Cada retorno de Seguradora vira uma Cotação registrada no instante em que chega; o corretor acompanha o preenchimento progressivo da lista. Ao atualizar ou reabrir a etapa de cotações, o corretor vê as Cotações já obtidas, e as Seguradoras ainda pendentes seguem sendo obtidas — a solicitação é retomável, sem reiniciar do zero. Uma Seguradora que falha, fica indisponível ou excede o tempo-limite não derruba as demais: resulta numa Cotação com resultado de indisponibilidade e o motivo correspondente (RN-058), e a lentidão de uma não bloqueia a exibição das outras.

**Casos limite.** Interrupção pelo corretor (fechar a aba, queda de conexão) não descarta as Cotações já obtidas. O **tempo-limite por Seguradora é alinhado ao teto do provedor** (nunca mais curto): a plataforma consome o retorno do provedor — o veredito ou o próprio erro/timeout que ele devolve — e só recorre a um limite de cliente como rede de segurança para conexão inerte; **não** encerra prematuramente uma Cotação que o provedor ainda processa (encerrar cedo perderia um veredito válido e deixaria proposta órfã no provedor). **Não há nova tentativa automática** em caso de falha/timeout: como solicitar a uma Seguradora **cria uma proposta** no provedor, um retry cego duplicaria — a Seguradora que falha vira uma Cotação indisponível e o corretor pode **re-solicitar manualmente**. A recuperação de solicitações interrompidas (reinício da plataforma) re-solicita **apenas** as Seguradoras ainda sem Cotação. Grau de paralelismo e o limite de cliente são parâmetros operacionais configuráveis, com valores padrão.

## RN-058 — Resultado da Cotação: classificação estável, esteira e motivos

**Descrição.** Toda Cotação carrega um **resultado classificado de forma estável**: *Pronta para emissão*, *Análise*, *Indisponível/Recusado* ou *Não-reconhecido*. Quando em *Análise*, a Cotação registra **e exibe ao corretor a esteira específica** — subscrição, crédito, PEP, resseguro ou cadastro —, de modo que ele veja *qual* análise (ex.: "Requer análise de subscrição"), nunca um "requer análise" genérico. A classificação é o eixo interno de lógica; a esteira é o rótulo que o corretor lê. Quando *Indisponível/Recusado*, registra a **lista de motivos** — informada pela Seguradora ou **local** (ex.: "não incluída na solicitação" no modo *escolhidas*). À parte da classificação, a Cotação também registra se a Seguradora **exige Contragarantia (CCG)** para emitir — dado capturado na cotação e exibido ao corretor; a assinatura da CCG é da etapa de emissão (fora desta fase).

**Pré-condições.** Cotação obtida de uma Seguradora (RN-057).

**Critério de aceitação.** O resultado informado pela Seguradora é traduzido, **num único ponto** (a camada anticorrupção do provedor), para uma das quatro classificações e, conforme o caso, para a esteira ou os motivos. Uma Cotação em *Análise* apresenta ao corretor a **esteira específica** (o tipo da análise), não apenas "em análise". Uma Cotação sem prêmio aplicável (Análise, Indisponível/Recusado, Não-reconhecido) não apresenta valor de prêmio. Um resultado que a plataforma **não reconhece** é classificado como *Não-reconhecido*: fica visível ao corretor identificado como não classificado, **sem prêmio, não seguível**, e é registrado para revisão — **nunca** é apresentado como *Pronta para emissão* nem exibe prêmio. Quando a Seguradora informa exigência de Contragarantia (CCG), a Cotação registra esse veredito e a plataforma o exibe ao corretor, independentemente da classificação.

**Casos limite.** Resultado novo ou desconhecido da Seguradora recai sempre em *Não-reconhecido*, jamais convertido em silêncio para outra classificação. Ausência de motivos num *Indisponível/Recusado*: apresenta indisponibilidade sem detalhamento, sem impedir as demais Cotações.

## RN-059 — Seleção da Cotação para seguir

**Descrição.** O corretor **seleciona uma** Cotação do Grupo para seguir. São seguíveis as Cotações *Prontas para emissão* e as em *Análise* de **subscrição**; as demais classificações não são seguíveis nesta fase. A seleção **marca a Cotação escolhida**; ao avançar ("Continuar"), o destino depende do resultado da Cotação escolhida.

**Pré-condições.** Grupo com ao menos uma Cotação seguível.

**Critério de aceitação.** A plataforma permite marcar como escolhida uma Cotação *Pronta para emissão* ou em *Análise de subscrição*. Cotações em outras esteiras de análise, *Indisponíveis/Recusadas* e *Não-reconhecidas* não podem ser escolhidas. Há no máximo uma Cotação escolhida por Grupo; escolher outra substitui a anterior. Ao acionar "Continuar": se a Cotação escolhida é *Pronta para emissão*, o corretor segue para a etapa de emissão (fora desta fase); se é *Análise de subscrição*, a plataforma pede **confirmação** ("enviar esta Cotação para análise da Seguradora?") antes de encaminhá-la ao acompanhamento (followup — fora desta fase). Uma Cotação seguível que **também exige Contragarantia (CCG)** permanece seguível — o corretor segue normalmente; a exigência de CCG só é enforçada no emitir (etapa de emissão — confirmado pela PO).

**Casos limite.** Grupo sem nenhuma Cotação seguível: nenhuma escolha é possível. O aceite da Cotação escolhida como Proposta, o followup da análise de subscrição e a emissão são demanda própria, fora desta fase ([OPEN-07](../open-decisions.md)).

## RN-060 — Imutabilidade e novo Grupo por mudança de dado-base

**Descrição.** Um Grupo de Cotação **com Cotações obtidas** é imutável nos seus dados-base. Ao voltar às etapas anteriores e **efetivar mudança de valor** de qualquer dado-base — Tomador, **estabelecimento cotado** (RN-102), Segurado, escopo de Seguradoras, Modalidade, valor segurado, vigência, Coberturas Adicionais — a plataforma **não altera** o Grupo atual: cria um **Grupo novo** em Rascunho com os dados alterados (RN-050), **preservando intactos** o Grupo anterior e suas Cotações (inclusive a escolhida, RN-059). Voltar sem alterar valor (ou alterar e desfazer para o mesmo valor) não cria nada.

**Pré-condições.** Grupo com Cotações já obtidas, e retorno do corretor às etapas anteriores do wizard.

**Critério de aceitação.** Uma mudança **efetiva de valor** de dado-base num Grupo que já tem Cotações dispara **confirmação bloqueante** ("iniciar uma nova cotação com os dados alterados? a cotação atual será preservada"); confirmada, a plataforma cria um Grupo novo (RN-050) com os dados alterados e conduz o corretor a ele, deixando o Grupo anterior — Cotações e eventual escolha (RN-059) — inalterado. Se nenhum valor mudou, permanece no Grupo atual sem reprocessar. O **servidor recusa** (fail-closed) atualizar dado-base de um Grupo que já tem Cotações; a criação de um novo Grupo é o único caminho. Grupo ainda **sem** Cotações continua sendo editado no lugar (RN-051).

**Casos limite.** O corretor recusa a confirmação: mantém o Grupo atual sem mudança. Os Grupos resultantes do fork são **independentes** — não guardam vínculo de origem entre si nesta fase; o registro de cada pedido vive no livro de Cotações (listagem read-only, demanda própria). Obter números novos para o **mesmo** pedido (re-solicitar as mesmas Seguradoras sem mudar dado-base) e o cancelamento das Cotações no provedor seguem como demanda própria; até lá, as Cotações do Grupo anterior expiram pelo cancelamento por inatividade do provedor (RN-061).

## RN-061 — Validade da Cotação

**Descrição.** Uma Cotação obtida permanece válida enquanto pertencer ao seu Grupo — mudança de dado-base **não a invalida**: cria um Grupo novo (RN-060), deixando esta intacta — ou até o Grupo seguir para as etapas posteriores. A expiração **por tempo** existe no lado do provedor (cancelamento por inatividade, indicado em ~15 dias), mas **não é modelada pela plataforma nesta fase**.

**Pré-condições.** Cotação obtida.

**Critério de aceitação.** Nesta fase, a plataforma **não** apresenta nem controla expiração por tempo da Cotação: a Cotação permanece exibida enquanto o Grupo existir (mudança de dado-base cria um Grupo novo, RN-060, e não altera esta) ou até o Grupo avançar. O cancelamento por inatividade do provedor é a garantia interina de que Cotações abandonadas não perduram indefinidamente do lado da Seguradora.

**Casos limite.** O espelhamento da expiração por tempo (apresentar a Cotação como desatualizada e oferecer re-solicitar quando a janela do provedor vence) fica para a demanda de **cancelamento** (demanda própria), junto com o cancelamento das demais Cotações. O prazo exato e o gatilho do job de inatividade do provedor serão confirmados nessa demanda.

## RN-062 — Minuta da Cotação selecionada: Tags e Cláusulas particulares

**Descrição.** Ao selecionar uma Cotação, a plataforma apresenta a **minuta** da Seguradora: as **Tags da minuta** (campos do objeto do contrato, que variam por Seguradora/Modalidade) e as **Cláusulas particulares** disponíveis. O corretor preenche as Tags (refletidas no texto do objeto) e marca as Cláusulas. As definições vêm do catálogo **já importado** (Tag e Cláusulas por Modalidade Importada — RN-047, RN-048). O preenchimento é **opcional na etapa de cotações** e torna-se obrigatório na etapa de emissão (fora desta fase).

**Pré-condições.** Uma Cotação selecionada (RN-059). Tag e Cláusulas particulares da Modalidade importadas (RN-047, RN-048).

**Critério de aceitação.** A plataforma lista as Tags exigidas pela Seguradora da Cotação selecionada e as Cláusulas particulares ativas daquela Modalidade; o corretor preenche/marca livremente (nada obrigatório nesta fase); o texto da minuta é montado com os valores preenchidos inline. Uma Seguradora/Modalidade sem Tags não exibe o bloco de Tags. Cláusulas com campos próprios exibem esses campos quando marcadas.

**Casos limite.** Se marcar uma Cláusula particular altera o resultado da Cotação (automática → subscrição): **não re-avaliado nesta fase** — a minuta é capturada e o veredito da Cotação é mantido; a decisão de re-avaliar aguarda a PO ([OPEN-21](../open-decisions.md)). Tags e o texto da minuta **não** alteram o veredito.

## RN-063 — Envio dos termos da minuta ao provedor

**Descrição.** O corretor pode **enviar** os termos e cláusulas preenchidos da Cotação selecionada ao provedor (atualizando a proposta correspondente) e **obter a minuta** (documento) para baixar. O envio ocorre ao acionar "Baixar minuta" na etapa de cotações.

**Pré-condições.** Cotação selecionada, com Tags/Cláusulas preenchidas ou não (RN-062). A Corretora do envio é a do Escopo ativo do acesso, resolvida pelo servidor (RN-103).

**Critério de aceitação.** Ao acionar "Baixar minuta", a plataforma envia ao provedor os termos e cláusulas atuais da Cotação selecionada, atualizando a proposta, e disponibiliza a minuta gerada para download refletindo esses termos. O preenchimento parcial é aceito nesta fase. O envio definitivo/obrigatório dos termos e a emissão ocorrem na etapa de emissão (fora desta fase).

**Casos limite.** Falha do provedor no envio não descarta o preenchimento local do corretor; a plataforma informa o erro e mantém os dados para nova tentativa. Uma Cotação não seguível não oferece a ação (não há proposta a atualizar).

## RN-103 — Corretora da cotação é a do Escopo ativo

**Descrição.** A Corretora que solicita as Cotações de um Grupo (RN-056) e que envia os termos da minuta (RN-063) é a **Corretora do Escopo ativo** do acesso (RN-064, ADR-065), resolvida pelo servidor a partir do claim — nunca informada pelo cliente. É dela que saem as Habilitações de Seguradora usadas no fan-out e a quem a proposta pertence.

**Pré-condições.** Acesso autenticado com Corretora ativa no Escopo (RN-064). Grupo de Cotação em Rascunho (RN-050).

**Critério de aceitação.** Ao solicitar Cotações ou enviar a minuta, a plataforma usa a Corretora ativa do acesso, ignorando qualquer Corretora informada pelo cliente; o corpo da requisição não carrega Corretora. Sem Corretora ativa no acesso, a operação é recusada com aviso.

**Casos limite.** Acesso sem Corretora ativa (Usuário só com vínculo de Tomador, ou nenhum): recusado. Troca de Escopo ativo no meio da jornada passa a valer para as próximas solicitações (o Grupo em Rascunho não muda de dono retroativamente). Quais Perfis podem solicitar cotação seguem em [OPEN-03](../open-decisions.md).

## RN-105 — Envio das Coberturas Adicionais à Seguradora

**Descrição.** Ao solicitar a Cotação, cada Cobertura Adicional canônica escolhida no Grupo é traduzida para o nome com que aquela Seguradora expõe a cobertura na Modalidade cotada, e é esse nome que vai à Seguradora. O identificador de origem da cobertura não serve para isso — a Seguradora reconhece a cobertura pelo nome.

**Pré-condições.** Cotação sendo solicitada a uma Seguradora (RN-057), a partir de um Grupo de Cotação com Coberturas Adicionais escolhidas (RN-104).

**Critério de aceitação.** Para cada canônica escolhida, a plataforma envia o nome da Cobertura Adicional Importada Ativa vinculada a ela, nas Modalidades Importadas Ativas e não Ignoradas daquela Seguradora vinculadas à Modalidade do Grupo. Cada cobertura contribui com um único nome e a Seguradora recebe a lista sem repetição. Grupo sem coberturas escolhidas envia lista vazia. A resolução é feita por Seguradora, no momento da solicitação, a partir dos vínculos vigentes.

**Casos limite.** A mesma canônica vinculada a Importadas com nomes diferentes na mesma Seguradora (tipicamente uma por ramo): não é enviada e é tratada como não contemplada (RN-106) enquanto [OPEN-22](../open-decisions.md) estiver aberta. Nunca é enviado nome que não esteja vinculado a uma canônica escolhida: cobertura não suportada faz a Seguradora recusar a solicitação inteira, derrubando a Cotação — por isso a plataforma erra para menos, nunca para mais. Canônica Inativa não é enviada (RN-046).

## RN-106 — Cobertura pedida e não contemplada pela Seguradora

**Descrição.** Seguradora que não oferece uma Cobertura Adicional escolhida é cotada mesmo assim, sem aquela cobertura. A Cotação registra o que foi enviado e o que ficou de fora, e a comparação sinaliza a lacuna — para que prêmios de escopos diferentes nunca sejam comparados sem aviso.

**Pré-condições.** Cotação solicitada a uma Seguradora, a partir de um Grupo com Coberturas Adicionais escolhidas (RN-104).

**Critério de aceitação.** Cada Cobertura Adicional escolhida aparece na Cotação com situação estável: **Enviada**, quando o nome foi resolvido e enviado; **Não contemplada**, quando a Seguradora não oferece a cobertura na Modalidade cotada. A situação é registrada em toda Cotação, inclusive nas que resultam Indisponível (RN-058) e nas que falham na integração (RN-057). A comparação de Cotações sinaliza cada cobertura não contemplada, identificando-a pelo nome da canônica.

**Casos limite.** Nenhuma das coberturas escolhidas disponível na Seguradora: a Cotação é solicitada sem cobertura alguma e todas constam como não contempladas. Canônica inativada entre a escolha e a solicitação: consta como não contemplada. Nome divergente entre ramos: não contemplada (OPEN-22). Grupo sem coberturas escolhidas: nenhuma situação é registrada. Cotação recusada por pré-condição de dados antes de resolver as coberturas (Seguradora inativa, sem identificador de origem, Modalidade sem Modalidade Global): não há situação a registrar, porque a falha não é de cobertura.
