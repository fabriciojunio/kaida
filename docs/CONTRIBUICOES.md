# Divisão de tarefas

**Equipe:** Camila Pereira Raimundo · Fabrício Júnio Almeida Dias ·
Kauã Limão Nunes · Luan Miranda Padilha

O projeto foi dividido em frentes, e cada uma corresponde a pastas e arquivos
identificáveis no repositório. É assim que dá para mostrar o código na
apresentação sem ficar caçando arquivo.

## Quem fez o quê

| Integrante | Frentes | Onde está no repositório |
|---|---|---|
| Camila Pereira Raimundo | Personagem e controles; level design e geração de cenas | `Assets/Scripts/Player/`, `Assets/Editor/` |
| Fabrício Júnio Almeida Dias | Inimigos e chefe; laje solta; testes e qualidade | `Assets/Scripts/Enemies/`, `Assets/Scripts/World/PlataformaSolta.cs`, `Assets/Tests/` |
| Kauã Limão Nunes | Mundo, progressão e salvamento | `Assets/Scripts/World/`, `Assets/Scripts/Systems/` |
| Luan Miranda Padilha | Interface e áudio; documentação e entrega | `Assets/Scripts/UI/`, `TrilhaSonora.cs`, `docs/` |

## Áreas do projeto

### 1. Personagem e controles

**Arquivos:** `Assets/Scripts/Player/`

Máquina de estados do jogador (9 estados, um arquivo cada), física de
movimento com gravidade assimétrica, pulo de altura variável, dash com
invulnerabilidade, coyote time e jump buffer. Configuração de balanceamento
em `PlayerStats`.

**Responsável:** Camila Pereira Raimundo

### 2. Inimigos e chefe

**Arquivos:** `Assets/Scripts/Enemies/` e `Assets/Scripts/Enemies/Boss/`

Inimigo base com patrulha, detecção por linha de visão e dano por contato.
Três comportamentos derivados (javali, abelha, caracol) e o Guardião do
Lúmen, com máquina de estados própria e barra de vida única.

**Responsável:** Fabrício Júnio Almeida Dias

### 3. Mundo, progressão e save

**Arquivos:** `Assets/Scripts/World/` e `Assets/Scripts/Systems/`

Checkpoints, transições entre regiões com ponto de chegada, coletáveis,
perigos, parallax, sistema de save em JSON e controle de dificuldade.

**Responsável:** Kauã Limão Nunes

### 4. Interface e áudio

**Arquivos:** `Assets/Scripts/UI/` e `Assets/Scripts/Systems/TrilhaSonora.cs`

Menu principal com cenário desfocado ao fundo, menu de pausa, telas de
morte e vitória, HUD de vida, barra do chefe, tela de créditos e a trilha
sonora gerada por síntese.

**Responsável:** Luan Miranda Padilha

### 5. Level design e geração de cenas

**Arquivos:** `Assets/Editor/`

Pipeline que monta o jogo por código: fatiamento de sprites com detecção de
frames, geração de animações e prefabs, recorte de tiles e construção das
seis cenas a partir de mapas em texto.

**Responsável:** Camila Pereira Raimundo

### 6. Testes e qualidade

**Arquivos:** `Assets/Tests/`

157 casos automatizados em EditMode e PlayMode, incluindo o validador de
alcance dos mapas e os testes de colisão do chão nas cenas reais.

**Responsável:** Fabrício Júnio Almeida Dias

### 7. Laje solta da Orla da Vila

**Arquivos:** `Assets/Scripts/World/PlataformaSolta.cs`,
`Assets/Tests/PlayMode/PlataformaSoltaTests.cs`

Plataforma que fica parada no ar até ser pisada, desliza cinco segundos
levando junto quem está em cima e então trava e cai pela gravidade. Nasceu
do trabalho individual da aula 8 e ficou no jogo. Fica listada à parte por
isso, e não junto do resto do mundo.

**Responsável:** Fabrício Júnio Almeida Dias

### 8. Documentação e entrega

**Arquivos:** `README.md`, `CREDITOS.md`, `docs/`

Game Design Document, instruções de execução, créditos de arte conferidos
contra a licença de cada pacote e o empacotamento das releases.

**Responsável:** Luan Miranda Padilha

## Sugestão de fala na apresentação

O professor reserva 15 minutos por grupo, cobrindo o jogo, o código-fonte e
as contribuições de cada integrante. Uma divisão que cabe no tempo:

| Momento | Duração | Conteúdo |
|---|---|---|
| Abertura | 1 min | O que é o jogo, gênero e referências |
| Demonstração | 5 min | Jogar do menu até o chefe, mostrando as habilidades |
| Código | 6 min | Cada integrante mostra a sua frente |
| Fechamento | 2 min | Testes, dificuldades encontradas e o que aprenderam |
| Perguntas | 1 min | - |

### Pontos fortes que vale destacar no código

1. **Máquina de estados do jogador** - mostra separação de
   responsabilidades: cada estado num arquivo, sem condicionais aninhadas.

2. **Cenas geradas a partir de mapas em texto** - mostrar
   `SceneBuilder.cs`, editar um mapa ao vivo e regenerar a região é uma
   demonstração forte.

3. **Trilha gerada por síntese** - nenhum pacote trazia áudio, e em vez de
   deixar o jogo mudo a música é construída por código, com escala menor e
   tônica diferente por região.

4. **Testes que nasceram de bug real** - o validador de alcance dos mapas
   reprovou as cinco regiões da primeira versão, e o teste de colisão do
   chão pegou um defeito que deixava a personagem atravessar o cenário sem
   que nada aparecesse na tela.

5. **A laje solta** - um corpo que começa cinemático e vira dinâmico na hora
   certa. Dá para explicar em trinta segundos por que os dois tipos de corpo
   existem, usando um objeto que está ali na tela.

### Dificuldades que valem ser contadas

Vale mencionar os problemas enfrentados, porque mostram processo:

- O tamanho de frame das folhas de sprite varia por animação (48 a 96 px),
  então foi preciso detectá-lo contando pixels em vez de fixar um valor.
- O pivô precisava ser medido no primeiro frame de cada folha: medindo a
  folha inteira, o rastro do golpe puxava o pivô e a personagem subia meia
  unidade ao atacar.
- O chefe, como corpo dinâmico, pousava numa plataforma da arena e ficava
  alto demais para o ataque corpo a corpo. Virou corpo cinemático.
- Virar a personagem de lado só troca o flipX do sprite: os marcadores
  presos a ela continuavam à direita, então virada para a esquerda o golpe
  saía pelas costas. Nenhum teste pegou porque todos punham o inimigo à
  direita.
- A laje não levava a personagem junto. O controlador reescreve a velocidade
  dela a cada passo de física, então o atrito não empurra ninguém: quem
  carrega precisa ser a própria laje.
