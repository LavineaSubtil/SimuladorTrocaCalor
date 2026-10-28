# 🔥 Simulador de Troca de Calor

Simulação visual da **condução de calor** entre corpos cúbicos dispostos numa matriz N × N, feita em **C# / Windows Forms (.NET 8)**.

Cada quadrado é um cubo de um material real (cobre, prata, vidro, madeira…). Você aquece, esfria e troca materiais com o mouse e acompanha, em tempo real, o calor se espalhando pela matriz até o equilíbrio térmico.

---

## ✨ Funcionalidades

- **Matriz N × N** de corpos cúbicos (até 60 × 60), com lado configurável
- **11 materiais** com propriedades térmicas reais no SI (condutividade, calor específico, densidade)
- **Interação com o mouse**
  - botão esquerdo aquece, botão direito esfria (segurando, continua)
  - modo *Trocar material*: clique ou arraste para pintar materiais
  - modo *Selecionar e editar*: Ctrl+clique para vários e edição em lote
- **Mapa de cores** de 0 K a 12 000 K (escala *Jet*, do azul ao vermelho)
- **Dois critérios de fluxo**: maior temperatura cede ou maior calor sensível cede
- **Computação paralela** com `Parallel.For`, com opção de comparar com o modo sequencial e ver o tempo de cada passo em ms
- **Gravação opcional no MySQL** de cada iteração e do estado de todos os corpos
- **Conservação de energia** visível na tela: a energia total fica constante durante a simulação

## 🧪 A Física por trás

| Grandeza | Fórmula | Onde está no código |
|---|---|---|
| Massa | m = ρ · L³ | `Corpo.Massa` |
| Capacidade térmica | C = m · c | `Corpo.CapacidadeTermica` |
| Calor sensível | Q = m · c · T | `Corpo.CalorSensivel` |
| Fluxo de calor (Lei de Fourier) | q = k · A · ΔT | `Simulador.CalcularEAcumularTroca` |

Entre dois vizinhos, usa-se o **menor k** dos dois materiais e a área da face de contato. A energia trocada em cada passo é limitada para que nenhum corpo "passe do ponto" de equilíbrio, o que mantém a simulação estável.

## 🗂️ Estrutura

```
SimuladorTrocaCalor/
├── SimuladorTrocaCalor.sln      ← abra este no Visual Studio
├── SimuladorTrocaCalor.csproj
├── Program.cs                   ← ponto de entrada
├── Material.cs                  ← catálogo de materiais (k, c, ρ)
├── Corpo.cs                     ← cubo: massa, capacidade térmica, calor
├── Simulador.cs                 ← matriz e Lei de Fourier (sequencial e paralelo)
├── CorPorTemperatura.cs         ← cor de cada temperatura
├── BancoDados.cs                ← gravação no MySQL
├── FormPrincipal.cs             ← interface gráfica
└── schema.sql                   ← script do banco MySQL (opcional)
```

## ▶️ Como rodar

**Requisitos:** Windows, [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) e, de preferência, Visual Studio 2022 com a carga de trabalho *Desenvolvimento para desktop com .NET*.

1. Clone o repositório ou baixe o ZIP (botão **Code → Download ZIP**) e extraia.
2. Abra `SimuladorTrocaCalor.sln` e aperte **F5**.

Pelo terminal também funciona:

```bash
cd SimuladorTrocaCalor
dotnet run
```

### MySQL (opcional)

O simulador funciona sem banco. Para gravar as iterações:

1. Rode `schema.sql` no seu MySQL.
2. Ajuste usuário e senha na `ConnectionString` em `BancoDados.cs`.
3. Marque **Gravar iterações no MySQL** na tela.

## 🕹️ Como usar

1. Escolha **N**, **material** e **lado** e clique em **Criar corpos**. Todos começam em 0 K.
2. Aqueça alguns quadrados, troque materiais e monte o cenário.
3. Clique em **▶ Iniciar** e observe o calor se espalhar.
4. Passe o mouse sobre um quadrado para ver material, massa e calor sensível.

## 👩‍💻 Autora

Feito por **[Lavinea Subtil](https://github.com/LavineaSubtil)**.
