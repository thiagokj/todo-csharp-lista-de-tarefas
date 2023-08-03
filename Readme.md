# Todo Csharp | Lista de Tarefas

Seguindo curso 7196 do [balta.io](https://github.com/balta-io/7196)

1. Crie a estrutura do projeto.

1. Faça a referencia na solução com base no domínio.

## Crie as Entidades (Modelagem dos objetos em classes)

1. Crie uma classe base Entity, para compartilhar com as demais entidades as propriedades e métodos comuns.

```csharp
namespace Todo.Domain.Entities;
// Classe abstrata não permite instancia
// Implementa a interface IEquatable para comparar o Id
public abstract class Entity : IEquatable<Entity>
{
    // Construtor gera um novo Guid
    public Entity()
    {
        Id = Guid.NewGuid();
    }

    // Propriedade do tipo Guid
    public Guid Id { get; private set; }

    // Implementação do método para comparar o Guid
    public bool Equals(Entity? other)
    {
        return Id == other?.Id;
    }
}
```

1. Crie uma classe TodoItem para representar um item na lista de tarefas.

```csharp
// O item da lista herda o ID de Entity
public class TodoItem : Entity
{
    // Construtor padrão
    public TodoItem(string? title, DateTime date, string? user)
    {
        Title = title;
        Done = false;
        Date = date;
        User = user;
    }

    // Propriedades
    public string? Title { get; private set; }
    public bool Done { get; private set; }
    public DateTime Date { get; private set; }
    public string? User { get; private set; }

    // Métodos modificadores de estado
    public void MarkAsDone()
    {
        Done = true;
    }

    public void MarkAsUndone()
    {
        Done = false;
    }

    public void UpdateTitle(string title)
    {
        Title = title;
    }
}
```

## Comandos | Ações executadas na aplicação

Os comandos são ações que serão executadas (Todos os inputs da aplicação). Preferencialmente, crie comandos separados.

Por mais que possam surgir comandos muito parecidos, o ideal é criar comandos distintos, pois ao usar em conjunto com o Handler (manipulador), é obrigatório chamar uma classe exclusiva para essa finalidade.

Toda ação gera um resultado. Pensando nisso, crie um CommandResult, padronizando o retorno desse resultado. Isso facilita muito na implementação do Frontend.

Para melhor organização e validação, implemente um ICommand usando Flunt. Utilizando o pacote, reduzimos drasticamente a necessidade de testes, pois as condicionais inclusas já foram testadas.

```csharp
using Flunt.Validations;
namespace Todo.Domain.Commands.Contracts;
public interface ICommand : IValidatable{ }
```

Trabalhar com notificações também melhora a performance da aplicação. Utilizar exceptions somente para situações inesperadas (Queda do banco de dados, queda da infra, etc). Essas situações no IIS geram log no Servidor, gastando recursos desnecessários.

```csharp
using Flunt.Notifications;
using Flunt.Validations;
using Todo.Domain.Commands.Contracts;

namespace Todo.Domain.Commands;
public class CreateTodoCommand : Notifiable, ICommand
{
    public CreateTodoCommand() { }

    public CreateTodoCommand(string? title, string? user, DateTime date)
    {
        Title = title;
        User = user;
        Date = date;
    }

    public string? Title { get; set; }
    public string? User { get; set; }
    public DateTime Date { get; set; }

    // Evita a criação de IFs repetitivos e necessidade de mais testes.
    public void Validate()
    {
        AddNotifications(
            new Contract()
            .Requires()
            .HasMinLen(Title, 3, "Title", "Descreva melhor a tarefa!")
            .HasMinLen(User, 6, "User", "Usuário inválido!")
        );
    }
}
```

## Manipuladores | Por quê utilizar Handlers?

O Handler gerencia o fluxo de execução do código. Ele ajuda muito na reutilização do código.
Dessa forma não há dependência de uma API.

Manter o fluxo dentro dos Handlers, facilita na criação de jobs para outras aplicações, sem a necessidade de utilizar uma API.

Nesse formato há redução de processamento, otimização de custos e facilidade de testes.

Manter o código no domínio deixa tudo muito mais fácil.

```csharp
using Todo.Domain.Commands.Contracts;

namespace Todo.Domain.Handlers.Contracts;

// Restringe a utilização de comandos, determinando que todo comando passe pelo contrato.
// Dessa forma estamos gerenciando os comandos, evitando corrupção de código.
public interface IHandler<T> where T : ICommand
{
    // Padroniza o retorno do comando, inibindo gambiarras no código
    // Os comandos obrigatoriamente tem que passar pela interface ICommand
    ICommandResult Handle(T command);
}
```

## Repositório | Padrão Repository

Onde os dados serão salvos. Aqui dividimos as responsabilidades. Interface entre os dados e a aplicação.
Facilita e muito a troca da fonte de dados. Ex: Uso o EF e agora vou utilizar Dapper.
Isolamos o acesso a dados da aplicação.

```csharp
using Todo.Domain.Entities;

namespace Todo.Domain.Repositories;
// Abstração do repositório, passando a implementação para Infra.
// Dessa forma, o domínio fica independente do EF.
public interface ITodoRepository
{
    void Create(TodoItem todo);
    void Update(TodoItem todo);
}
```

## Blindando a criação de uma tarefa

Abaixo temos um comando sendo manipulado por um Handler.
Então temos uma estrutura padronizada e obrigatória, assegurando a execução e retorno.

```csharp
...
namespace Todo.Domain.Handlers;
public class TodoHandler :
    Notifiable,
    IHandler<CreateTodoCommand>
{
    // Injetamos o repositório no construtor
    private readonly ITodoRepository _repository;

    public TodoHandler(ITodoRepository repository)
    {
        _repository = repository;
    }

    public ICommandResult Handle(CreateTodoCommand command)
    {
        // Fail fast validation. Verifica tudo primeiro para depois prosseguir.
        command.Validate();
        if (command.Invalid)
            return new GenericCommandResult(
                false, "Ops, há um problema com a tarefa.", command.Notifications);

        // Gera uma tarefa
        var todo = new TodoItem(command.Title, command.User, command.Date);

        // Salva no banco de dados
        _repository.Create(todo);

        // Retorna o resultado, trazendo apenas a tarefa inserida com o ID gerado
        return new GenericCommandResult(true, "Tarefa salva", todo);
    }
}
```

## Testes unitários

Ao trabalhar com Design orientado ao domínio (DDD), podemos ir escrevendo os testes necessários. Posteriormente vamos refatorando e testando o fluxo dos comandos.

```csharp
namespace Todo.Domain.Tests.CommandTests;

[TestClass]
public class CreateCommandTests
{
    // Seguindo a técnica Red, Green, Refactor.
    // Falhe todos os testes. Reescreva cada teste até o sucesso. Refatore conforme necessidade.
    [TestMethod]
    public void Dado_um_comando_invalido()
    {
        Assert.Fail();
    }

    public void Dado_um_comando_valido()
    {
        Assert.Fail();
    }
}
```

Versão de sucesso

```csharp
[TestClass]
public class CreateCommandTests
{
    [TestMethod]
    public void Dado_um_comando_invalido()
    {
        var command = new CreateTodoCommand("", "", DateTime.Now);
        command.Validate();
        Assert.AreEqual(command.Valid, false);
    }

    [TestMethod]
    public void Dado_um_comando_valido()
    {
        var command = new CreateTodoCommand("Titulo da tarefa", "meuusuario@dominio.dom", DateTime.Now);
        command.Validate();
        Assert.AreEqual(command.Valid, true);
    }
}
```

Refatorando deixando mais clean

```csharp
namespace Todo.Domain.Tests.CommandTests;

[TestClass]
public class CreateCommandTests
{
    private readonly CreateTodoCommand _invalidCommand = new CreateTodoCommand("", "", DateTime.Now);
    private readonly CreateTodoCommand _validCommand = new CreateTodoCommand("Titulo da tarefa", "meuusuario@dominio.dom", DateTime.Now);

    public CreateCommandTests()
    {
        _invalidCommand.Validate();
        _validCommand.Validate();
    }

    [TestMethod]
    public void Dado_um_comando_invalido()
    {
        Assert.AreEqual(_invalidCommand.Valid, false);
    }

    [TestMethod]
    public void Dado_um_comando_valido()
    {
        Assert.AreEqual(_validCommand.Valid, true);
    }
}
```

## Infra | Utilizando o EntityFramework

Instale os pacotes no projeto de infra.

```csharp
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.EntityFrameworkCore.Relational
```

O EF faz o mapeamento, um DE/PARA das tabelas do banco de dados para objetos csharp.

Para representar o banco em memoria, usamos um DataContext.

```csharp
using Microsoft.EntityFrameworkCore;
using Todo.Domain.Entities;

namespace Todo.Domain.Infra.Contexts;
public class DataContext : DbContext

{
    private readonly IConfiguration? configuration;

    // Construtor vazio, obrigatório para uso do EF
    public DataContext() { }

    // Construtor padrão recuperando a conexão via AppSettings
    public DataContext(DbContextOptions<DataContext> options, IConfiguration configuration)
        : base(options) => this.configuration = configuration;

    // Ligação da Entidade com a tabela do banco
    public DbSet<TodoItem> Todos { get; set; }

    // Método utilizado nas versões mais recentes do EF para indicar o banco
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseInMemoryDatabase("Database");
    // => optionsBuilder.UseSqlServer(configuration?.GetConnectionString("connectionString"));

    // Método que reflete a aplicação no banco, criando tabelas e colunas
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TodoItem>().ToTable("Todo");

        modelBuilder.Entity<TodoItem>()
            .Property(x => x.Id);

        modelBuilder.Entity<TodoItem>()
            .Property(x => x.User)
            .HasMaxLength(120)
            .HasColumnType("varchar(120)");

        modelBuilder.Entity<TodoItem>()
            .Property(x => x.Title)
            .HasMaxLength(160)
            .HasColumnType("varchar(160)");

        modelBuilder.Entity<TodoItem>()
            .Property(x => x.Done)
            .HasColumnType("bit");

        modelBuilder.Entity<TodoItem>()
            .Property(x => x.Date);

        modelBuilder.Entity<TodoItem>()
            .HasIndex(b => b.User);
    }

}
```

## Queries | Recuperando informações da fonte de dados

Não há necessidade de instanciar a classe de queries, pois só queremos retornar as expressões.

```csharp
public static class TodoQueries
{
    public static Expression<Func<TodoItem, bool>> GetAll(string user)
    {
        return x => x.User == user;
    }

    public static Expression<Func<TodoItem, bool>> GetAllDone(string user)
    {
        return x => x.User == user && x.Done;
    }
}
```

## API | Resolvendo as dependências

Comece as resoluções pela raiz.

Para criar um Handler, preciso de um Repositório e para criar um Repositório, preciso de um DataContext.

DataContext => Repository => Handler

```csharp
// Sempre cria um novo item, ideal para repositórios.
builder.Services.AddTransient();

// Uma conexão por transação, garantido a que a conexão é fechada no final.
builder.Services.AddScoped();

// Similar ao AddScoped, porém otimizado ao trabalhar com EF.
builder.Services.AddDbContext();

// Cria uma instancia única e imutável, ideal para configurações.
builder.Services.AddSingleton();
```

Então, vamos injetando da seguinte forma

```csharp
builder.Services.AddDbContext<DataContext>();
builder.Services.AddTransient<ITodoRepository, TodoRepository>();
builder.Services.AddTransient<TodoHandler, TodoHandler>();
```

E a implementação do repositório de Todo fica assim

```csharp
public class TodoRepository : ITodoRepository
{
    // Construtor padrão
    private readonly DataContext _context;
    public TodoRepository(DataContext context)
    {
        _context = context;
    }

    // Adiciona uma nova tarefa
    public void Create(TodoItem todo)
    {
        _context.Todos.Add(todo);
        _context.SaveChanges();
    }

    // Atualiza uma tarefa, fazendo tracking das mudanças.
    // O EF cria um objeto temporário e compara campo a campo o que foi modificado para salvar.
    public void Update(TodoItem todo)
    {
        _context.Entry(todo).State = EntityState.Modified;
        _context.SaveChanges();
    }

    // Retorna todas as tarefas de um usuário.
    // É indispensável usar o método AsNoTracking para consultas que retornam mais de um item.
    // Assim evitamos processamento desnecessário, melhorando a performance.
    // Dessa forma, o EF não irá criar uma cópia em memória do objeto.
    public IEnumerable<TodoItem> GetAll(string user)
    {
        return _context.Todos
            .AsNoTracking()
            .Where(TodoQueries.GetAll(user))
            .OrderBy(x => x.Date);
    }
```

## Controllers | Definição dos endpoints com a execução das Ações

Nos controllers declaramos as ações que são executadas conforme cada endpoint chamado em nossa API.

```csharp
namespace Todo.Domain.Api.Controllers;
// Anotações informando a rota padrão
[ApiController]
[Route("v1/todos")]
public class TodoController : ControllerBase
{
    // Rota raiz para chamadas Ex: //meuapp/v1/todos
    // Retorna todas as tarefas de um usuário.
    [Route("")]
    [HttpGet]
    public IEnumerable<TodoItem> GetAll(
        [FromServices] ITodoRepository repository
    )
    {
        return repository.GetAll("thiago");
    }

    // Rota meuapp/v1/todos/done
    // Retorna todas as tarefas concluídas de um usuário.
    [Route("done")]
    [HttpGet]
    public IEnumerable<TodoItem> GetAllDone(
        [FromServices] ITodoRepository repository
    )
    {
        // var user = User.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value;
        return repository.GetAllDone("thiago");
    }

    // Rota meuapp/v1/todos. Aqui é usado o método post.
    // Os dados são informados no corpo da requisição [FromBody].
    // O Handler irá executar o fluxo de processo conforme o especificado no command.
    // Cria uma nova tarefa conforme usuário informado.
    [Route("")]
    [HttpPost]
    public GenericCommandResult Create(
        [FromBody] CreateTodoCommand command,
        [FromServices] TodoHandler handler
    )
    {
        command.User = "thiago";
        return (GenericCommandResult)handler.Handle(command);
    }
...
}
```

## EF | Executando as migrações para o SQLServer

Crie o banco inicialmente via Azure Data Studio ou SQL Data Studio.

adicione o pacote ao projeto de infra:

```csharp
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

Para informar os dados de conexão, adicione ao appsettings.json e o appsettings.Development.json a chave abaixo:

```csharp
  "ConnectionStrings": {
    "connectionString": "Server=localhost/SQLEXPRESS;Database=Todos;User ID=sa;Password=Sqlexpress_l0c4l"
  }
```

Atualize o EF, removendo e instalando a versão.

```csharp
dotnet tool uninstall --global dotnet-ef
dotnet tool install --global dotnet-ef
```

Verifique se o EF está rodando com **dotnet ef**

Agora, acesse o projeto de infra e execute o comando para gerar as tabelas.

```csharp
// InitialCreate - documenta o histórico de migrações
// --startup-project ..\Todo.Domain.Api\ - Indica com o projeto possui o executável em nossa aplicação.
dotnet ef migrations add InitialCreate --startup-project ..\Todo.Domain.Api\
```

Será criada pasta Migrations no projeto de infra, com histórico e scripts que serão rodados no banco.

Para efetivar as migrações execute o comando abaixo.

```csharp
dotnet ef database update --startup-project ..\Todo.Domain.Api\
```

## Configurando o Firebase

Habilite a autenticação via Google, sem o analytics.

Agora acesse project overview -> Web

Crie um nome para o projeto e copie o firebaseConfig

```csharp
 // Your web app's Firebase configuration
const firebaseConfig = {
 apiKey: "343241222GhgHFQ8hNmtgJPATk",
 authDomain: "234wfsd.firebaseapp.com",
 projectId: "354gdfgg-f68c6",
 storageBucket: "todosdfDFsdf-f68c6.appspot.com",
 messagingSenderId: "845511487076",
 appId: "1:32324sdfwq76:web:1f670d3aqweqwsd4a90ff1"
};
```
