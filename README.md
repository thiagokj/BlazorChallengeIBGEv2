# Blazor Challenge IBGE

Olá Dev! 😎

Esse projeto é a continuação do desafio do [balta.io][BlazorChallengeV1].

Na documentação, apenas as alterações e melhorias serão pontuadas.

## Notas da versão

- Nova Model para Estados
- Upload de localidades por planilha

### Passos para Desenvolvimento

1. **Modelagem** -> Organização das informações sobre as Localidades e Estados como Entidades.
2. **Mapeamento** -> Mapeamento e migrações das Entidades no banco de dados.
3. **Componentes / Páginas** -> Telas para interação do usuário com a aplicação.
4. **Navegação** -> Configuração das rotas para acesso as páginas.
5. **Filtros e Paginação** -> Utilização de componentes como o Quickgrid para organizar os dados.

## 01 - Modelagem

Atualize as entidades para representar as Localidades e Estados.
Os IDs do Estado são representados por códigos da tabela do [IBGE][CodigosEstadoIBGE]

```csharp
using System.ComponentModel.DataAnnotations;
namespace BlazorChallengeIBGE.Models;

public class Locality
{
  [Key]
  public Guid Id { get; set; } = Guid.NewGuid();

  [Required(ErrorMessage = "Informe o código IBGE")]
  [StringLength(7, MinimumLength = 7, ErrorMessage = "O código deve conter 7 digitos")]
  [RegularExpression(@"^\d+$", ErrorMessage = "Apenas digitos são permitidos")]
  public string IbgeCode { get; set; } = null!;

  // Adicionada propriedades para navegação, sendo uma o Id e a outra do Tipo de objeto Estado
  [Required(ErrorMessage = "Informe o Id do Estado")]
  public int StateId { get; set; }
  public State State { get; set; } = null!;

  [Required(ErrorMessage = "Informe a Cidade")]
  [MinLength(3, ErrorMessage = "A cidade deve ter pelo menos 3 caracteres")]
  [MaxLength(100, ErrorMessage = "A cidade deve ter no máximo 100 caracteres")]
  [RegularExpression(@"^[a-zA-ZÀ-ÿ ]*$", ErrorMessage = "Apenas letras e acentuação são permitidos")]
  public string City { get; set; } = null!;
}


// Criada Model State, definindo todas as informações do Estado
using System.ComponentModel.DataAnnotations;
namespace BlazorChallengeIBGE.Models;

public class State
{
    // O Id será o código fornecido na tabela do IBGE
    [Key]
    [Required(ErrorMessage = "Informe o código IBGE")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "O código deve conter 2 digitos")]
    [RegularExpression(@"^\d+$", ErrorMessage = "Apenas digitos são permitidos")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe a sigla do Estado")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "A sigla deve conter 2 caracteres")]
    [RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "Apenas letras são permitidas")]
    public string Uf { get; set; } = null!;

    [Required(ErrorMessage = "Informe o nome do Estado")]
    [MinLength(3, ErrorMessage = "O Estado deve ter pelo menos 4 caracteres")]
    [MaxLength(100, ErrorMessage = "O Estado deve ter no máximo 100 caracteres")]
    [RegularExpression(@"^[a-zA-ZÀ-ÿ ]*$", ErrorMessage = "Apenas letras e acentuação são permitidos")]
    public string FullName { get; set; } = null!;
}

```

## 02 - Mapeamento para o banco de dados

1. Com a modelagem definida, é hora de mapear as Entidades no Banco.
   Obs: Pode ser necessário criar uma nova base e fazer um DE/PARA, caso já existam dados anteriores.

```csharp
using BlazorChallengeIBGE.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace BlazorChallengeIBGE.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    // Crias as tabelas do Identity
    base.OnModelCreating(modelBuilder);

    // Especifica o nome das tabelas com base nas entidades
    modelBuilder.Entity<Locality>().ToTable("IbgeLocality");
    modelBuilder.Entity<State>().ToTable("IbgeState");

    // Define o relacionamento indicando que um estado pode estar associado a várias Localidades,
    // mas cada Localidade está associada a apenas um Estado
    modelBuilder.Entity<Locality>()
         .HasOne(l => l.State)
         .WithMany()
         .HasForeignKey(l => l.StateId)
         .IsRequired();

    // Índice para evitar registro duplicado de Localidade e Estado
    modelBuilder.Entity<Locality>()
      .HasIndex(l => new { l.StateId, l.City })
      .IsUnique();
  }

  public DbSet<Locality> Localities { get; set; } = null!;
  public DbSet<State> States { get; set; } = null!;
}

```

1. Utilizando o Terminal, crie uma migração para refletir a Entidade no Banco. Em seguida, atualize os dados.

```csharp
dotnet ef migrations add "v1 - Definição Localidade e Estado"

dotnet ef database update
```

## 03 - Componentes da Localidade

Agora é necessário atualizar as paginas e componentes.

### Create.razor

Página para criar uma nova localidade.

```csharp
@page "/localities/create"
@inject ApplicationDbContext Context
@inject NavigationManager Navigation
@attribute [Authorize]
@rendermode InteractiveServer

<h1>Nova Localidade</h1>
<EditForm Model="@Model" OnValidSubmit="OnValidSubmitAsync" FormName="localities-create">
  <DataAnnotationsValidator />
  @* <ValidationSummary /> *@

  // Estados são retornados em forma de um campo Select
  <div class="mb-3">
    <label class="form-label">Estado</label>
    <InputSelect @bind-Value="Model.StateId" class="form-control">
      @foreach (var state in States)
      {
        <option value="@state.Id">@state.Uf</option>
      }
    </InputSelect>
    <ValidationMessage For="@(() => Model.StateId)" />
  </div>

  <div class="mb-3">
    <label class="form-label">Cidade</label>
    <InputText @bind-Value="Model.City" class="form-control" />
    <ValidationMessage For="@(() => Model.City)" />
  </div>

  <div class="mb-3">
    <label class="form-label">Código IBGE</label>
    <InputText @bind-Value="Model.IbgeCode" class="form-control" />
    <ValidationMessage For="@(() => Model.IbgeCode)" />
  </div>

  <button type="submit" class="btn btn-primary">
    Criar
  </button>
  <a href="/localities">Cancelar</a>
</EditForm>

@code {
  public Locality Model { get; set; } = new();

  // Criada lista para retornar os Estados
  public IEnumerable<State> States { get; set; } = Enumerable.Empty<State>();

  protected override async void OnInitialized()
  {
    States = await Context
    .States
    .AsNoTracking()
    .OrderBy(x => x.Uf)
    .ToListAsync();
  }

  public async Task OnValidSubmitAsync()
  {
    await Context.Localities.AddAsync(Model);
    await Context.SaveChangesAsync();
    Navigation.NavigateTo("localities");
  }
}
```

### Details.razor

Página para exibir os detalhes de uma localidade. Aqui não é necessária validação dos campos.

```csharp
@page "/localities/{id:guid}"
@inject ApplicationDbContext Context
@attribute [Authorize]
@rendermode InteractiveServer

@if (Model is null)
{
  <p class="text-center">
    <em>Localidade não encontrada</em>
  </p>
}
else
{
  <h1>Detalhes | @Model.City</h1>

  // Recupera a UF do Estado
  <EditForm Model="@Model" FormName="localities-details">
    <div class="mb-3">
      <label class="form-label">Estado</label>
      <InputText @bind-Value="Model.State.FullName" class="form-control" readonly />
    </div>

    <div class="mb-3">
      <label class="form-label">Cidade</label>
      <InputText @bind-Value="Model.City" class="form-control" readonly />
    </div>

    <div class="mb-3">
      <label class="form-label">Código IBGE</label>
      <InputText @bind-Value="Model.IbgeCode" class="form-control" readonly />
    </div>

    <a href="/localities">Voltar</a>
  </EditForm>
}

@code {

  [Parameter]
  public Guid Id { get; set; }

  public Locality? Model { get; set; }

  // Utilizando o Include, permite acessar todas as propriedades da Entidade Estado
  protected override async Task OnInitializedAsync()
  {
    Model = await Context
    .Localities
    .Include(l => l.State)
    .AsNoTracking()
    .FirstOrDefaultAsync(x => x.Id == Id);
  }
}
```

### Edit.razor

Página para editar os dados de uma localidade. Essa pagina é similar a Create.razor.

```csharp
@page "/localities/edit/{id:guid}"
@inject ApplicationDbContext Context
@inject NavigationManager Navigation
@attribute [Authorize]
@rendermode InteractiveServer

<h1>Editar Localidade</h1>
<EditForm Model="@Model" OnValidSubmit="OnValidSubmitAsync" FormName="localities-edit">
  <DataAnnotationsValidator />
  @* <ValidationSummary /> *@

  // Retorna o estado salvo em um campo do tipo Select
  <div class="mb-3">
    <label class="form-label">Estado</label>
    <InputSelect @bind-Value="Model.StateId" class="form-control">
      @foreach (var state in States)
      {
        <option value="@state.Id">@state.Uf</option>
      }
    </InputSelect>
    <ValidationMessage For="@(() => Model.StateId)" />
  </div>

  <div class="mb-3">
    <label class="form-label">Cidade</label>
    <InputText @bind-Value="Model.City" class="form-control" />
    <ValidationMessage For="@(() => Model.City)" />
  </div>

  <div class="mb-3">
    <label class="form-label">Código IBGE</label>
    <InputText @bind-Value="Model.IbgeCode" class="form-control" />
    <ValidationMessage For="@(() => Model.IbgeCode)" />
  </div>

  <button type="submit" class="btn btn-primary">
    Salvar
  </button>
  <a href="/localities">Cancelar</a>
</EditForm>

@code {

  [Parameter]
  public Guid Id { get; set; }

  public Locality Model { get; set; } = new();

  // Criada lista para recuperar os Estados
  public IEnumerable<State> States { get; set; } = Enumerable.Empty<State>();

  protected override async void OnInitialized()
  {
    // Removido AsNoTracking(). Útil para informações somente leitura
    Model = await Context
    .Localities
    .FirstOrDefaultAsync(x => x.Id == Id) ?? new();

    // Retorna lista em ordem alfabética com base na sigla
    States = await Context
    .States
    .OrderBy(x => x.Uf)
    .ToListAsync();
  }

  // Removido Context.ChangeTracker.Clear(). Útil para limpar o rastreamento do EF.
  public async Task OnValidSubmitAsync()
  {
    Context.Localities.Update(Model);
    await Context.SaveChangesAsync();
    Navigation.NavigateTo("localities");
  }
}
```

### Delete.razor

Página para deletar uma localidade. A página de exclusão é similar as anteriores.

```csharp
@page "/localities/delete/{id:guid}"
@inject ApplicationDbContext Context
@inject NavigationManager Navigation
@attribute [Authorize]
@rendermode InteractiveServer

@if (Model is null)
{
  <p class="text-center">
    <em>Localidade não encontrada</em>
  </p>
}
else
{
  <h1>Excluir Localidade</h1>
  <EditForm Model="@Model" OnValidSubmit="OnValidSubmit" FormName="localities-delete">
    <div class="mb-3">
      <label class="form-label">Código IBGE</label>
      <InputText @bind-Value="Model.IbgeCode" class="form-control" readonly />
    </div>

    // Recupera a UF do Estado
    <div class="mb-3">
      <label class="form-label">Estado</label>
      <InputText @bind-Value="Model.State.Uf" class="form-control" readonly />
    </div>

    <div class="mb-3">
      <label class="form-label">Cidade</label>
      <InputText @bind-Value="Model.City" class="form-control" readonly />
    </div>

    <button type="submit" class="btn btn-danger">
      Excluir
    </button>
    <a href="/localities">Cancelar</a>
  </EditForm>
}

@code {

  [Parameter]
  public Guid Id { get; set; }

  public Locality? Model { get; set; }

  // Removido AsNoTracking.
  protected override async Task OnInitializedAsync()
  {
    Model = await Context
    .Localities
    .Include(l => l.State)
    .FirstOrDefaultAsync(x => x.Id == Id);
  }

  // Removido Context.ChangeTracker.Clear()
  public async Task OnValidSubmit()
  {
    Context.Localities.Remove(Model!);
    await Context.SaveChangesAsync();
    Navigation.NavigateTo("localities");
  }
}
```

### Index.razor

Página com a rota inicial para exibição de todas as localidades. Filtros e demais operações estão concentrados aqui.

```csharp
@page "/localities"
@using Microsoft.AspNetCore.Components.QuickGrid
@inject ApplicationDbContext Context
@inject NavigationManager Navigation
@rendermode InteractiveServer
@attribute [StreamRendering(true)]
@attribute [Authorize]

<h1>Localidades</h1>

@if (isLoading)
{
  <p class="text-center">
    <em>Carregando...</em>
  </p>
}
else
{
  <div class="items-per-page">
    <div>
      <a href="/localities/create" class="btn btn-primary">Novo</a>
      <button class="btn btn-secondary" @onclick="ClearFilterAsync">Limpar</button>
    </div>

    <div class="page-size-chooser">
      <span>Itens:</span>
      <select class="form-select custom-select-sm" @bind="@pagination.ItemsPerPage">
        <option>1</option>
        <option>10</option>
      </select>
    </div>
  </div>

  @if (!Localities.Any())
  {
    <p class="text-center">
      <em>@noItemsMessage</em>
    <div><button class="btn btn-link" @onclick="ClearFilterAsync">Voltar</button></div>
    </p>
  }
  else
  {
    <div class="grid table-responsive">
      <QuickGrid Items="@Localities.AsQueryable()" Pagination="@pagination" class="table table-grid">
        <TemplateColumn Title="#" SortBy="@SortByIbgeCode">
          <a class="grid-link" @onclick="@(() => Details(context))">🔗</a>
        </TemplateColumn>

        <PropertyColumn Property="@(item => item.IbgeCode)" Title="IBGE" Sortable="true" Align="Align.Left">
          <ColumnOptions>
            <div class="search-box">
              <input type="search" @oninput="OnInputIbgeCodeAsync" placeholder="Código Ibge..." />
            </div>
          </ColumnOptions>
        </PropertyColumn>

        <PropertyColumn Property="@(item => item.State.Uf)" Title="UF" Sortable="true" Align="Align.Left">
          <ColumnOptions>
            <div class="search-box">
              <input type="search" @oninput="OnInputStateAsync" placeholder="Sigla UF..." />
            </div>
          </ColumnOptions>
        </PropertyColumn>

        <PropertyColumn Property="@(item => item.City)" Title="Cidade" Sortable="true" Align="Align.Left">
          <ColumnOptions>
            <div class="search-box">
              <input type="search" @oninput="OnInputCityAsync" placeholder="Cidade..." />
            </div>
          </ColumnOptions>
        </PropertyColumn>

        <TemplateColumn Title="# Ações">
          <button class="btn btn-primary button-spacing" @onclick="@(() => Edit(context))">Editar</button>
          <button class="btn btn-danger button-spacing" @onclick="@(() => Delete(context))">Excluir</button>
        </TemplateColumn>
      </QuickGrid>
    </div>

    <div class="page-buttons">
      Página:
      @if (pagination.TotalItemCount.HasValue)
      {
        for (var pageIndex = 0; pageIndex <= pagination.LastPageIndex; pageIndex++)
        {
          var capturedIndex = pageIndex;
          <button @onclick="@(() => GoToPageAsync(capturedIndex))" class="@PageButtonClass(capturedIndex) page-button"
            aria-current="@AriaCurrentValue(capturedIndex)" aria-label="Go to page @(pageIndex + 1)">
            @(pageIndex + 1)
          </button>
        }
      }
    </div>
  }
}

@code {
  public IEnumerable<Locality> Localities { get; set; } = Enumerable.Empty<Locality>();

  GridSort<Locality> SortByIbgeCode = GridSort<Locality>
  .ByAscending(l => l.IbgeCode)
  .ThenAscending(l => l.IbgeCode);

  private bool isLoading = true;
  private string noItemsMessage = "Nenhum item encontrado.";
  private string previousFilter = string.Empty;
  private string ibgeCodeFilter = string.Empty;
  private string stateFilter = string.Empty;
  private string cityFilter = string.Empty;
  PaginationState pagination = new PaginationState { ItemsPerPage = 10 };

  protected override async Task OnInitializedAsync() => await LoadDataAsync();

  private async Task LoadDataAsync()
  {
    isLoading = true;

    // Adicionado Include, permitindo navegar por todas as propriedades da Entidade Estado
    Localities = await Context
    .Localities
    .Include(l => l.State)
    .AsNoTracking()
    .ToListAsync();
    isLoading = false;

    pagination.TotalItemCountChanged += (sender, eventArgs) => StateHasChanged();
    pagination.ItemsPerPage = 10;
    await GoToPageAsync(0);
  }

  private async Task OnInputIbgeCodeAsync(ChangeEventArgs e)
  {
    previousFilter = ibgeCodeFilter;
    ibgeCodeFilter = e.Value?.ToString() ?? string.Empty;
    await ApplyFilterAsync();
  }

  private async Task OnInputStateAsync(ChangeEventArgs e)
  {
    previousFilter = stateFilter;
    stateFilter = e.Value?.ToString() ?? string.Empty;
    await ApplyFilterAsync();
  }

  private async Task OnInputCityAsync(ChangeEventArgs e)
  {
    previousFilter = cityFilter;
    cityFilter = e.Value?.ToString() ?? string.Empty;
    await ApplyFilterAsync();
  }

  private async Task ApplyFilterAsync()
  {
    // Atualizado filtro l.State.Uf
    Localities = await Context
    .Localities
    .Include(l => l.State)
    .Where(l => l.IbgeCode.ToLower().Contains(ibgeCodeFilter.ToLower())
    &&
    l.State.Uf.ToLower().Contains(stateFilter.ToLower())
    &&
    l.City.ToLower().Contains(cityFilter.ToLower()))
    .ToListAsync();
  }

  private async Task ClearFilterAsync()
  {
    previousFilter = string.Empty;
    ibgeCodeFilter = string.Empty;
    stateFilter = string.Empty;
    cityFilter = string.Empty;
    await LoadDataAsync();
  }

  public void Details(Locality l) => Navigation.NavigateTo($"/localities/{l.Id}");
  public void Edit(Locality l) => Navigation.NavigateTo($"/localities/edit/{l.Id}");
  public void Delete(Locality l) => Navigation.NavigateTo($"/localities/delete/{l.Id}");

  private async Task GoToPageAsync(int pageIndex) => await pagination.SetCurrentPageIndexAsync(pageIndex);
  private string? PageButtonClass(int pageIndex) => pagination.CurrentPageIndex == pageIndex ? "current" : null;
  private string? AriaCurrentValue(int pageIndex) => pagination.CurrentPageIndex == pageIndex ? "page" : null;
}

```

### É isso aí, fique à vontade para usar como base. Bons estudos e bons códigos! 👍

<!-- Links -->

[CodigosEstadoIBGE]: https://www.ibge.gov.br/explica/codigos-dos-municipios.php
[BlazorChallengeV1]: https://github.com/thiagokj/BlazorChallengeIBGE
