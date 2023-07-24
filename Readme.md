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
