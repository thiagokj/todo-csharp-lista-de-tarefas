using Flunt.Notifications;
using Todo.Domain.Commands;
using Todo.Domain.Commands.Contracts;
using Todo.Domain.Entities;
using Todo.Domain.Handlers.Contracts;
using Todo.Domain.Repositories;

namespace Todo.Domain.Handlers;
public class TodoHandler :
    Notifiable,
    IHandler<CreateTodoCommand>,
    IHandler<UpdateTodoCommand>,
    IHandler<MarkTodoAsUndoneCommand>,
    IHandler<MarkTodoAsDoneCommand>
{

    private readonly ITodoRepository _repository;

    public TodoHandler(ITodoRepository repository)
    {
        _repository = repository;
    }

    public ICommandResult Handle(CreateTodoCommand command)
    {
        // Fail fast validation
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

    public ICommandResult Handle(UpdateTodoCommand command)
    {
        // Fail fast validation
        command.Validate();
        if (command.Invalid)
            return new GenericCommandResult(
                false, "Ops, há um problema com a tarefa.", command.Notifications);

        // Recupera o TodoItem (Reidratação dos dados)
        var todo = _repository.GetById(command.Id, command.User);

        // Altera o titulo
        todo.UpdateTitle(command.Title);

        // Salva no banco de dados
        _repository.Update(todo);

        // Retorna o resultado, trazendo apenas a tarefa com o ID informado
        return new GenericCommandResult(true, "Tarefa atualizada", todo);
    }

    public ICommandResult Handle(MarkTodoAsUndoneCommand command)
    {
        // Fail fast validation
        command.Validate();
        if (command.Invalid)
            return new GenericCommandResult(
                false, "Ops, há um problema com a tarefa.", command.Notifications);

        // Recupera o TodoItem (Reidratação dos dados)
        var todo = _repository.GetById(command.Id, command.User);

        // Altera o estado
        todo.MarkAsUndone();

        // Salva no banco de dados
        _repository.Update(todo);

        // Retorna o resultado, trazendo apenas a tarefa inserida com o ID gerado
        return new GenericCommandResult(true, "Tarefa alterada para não concluída", todo);
    }

    public ICommandResult Handle(MarkTodoAsDoneCommand command)
    {
        // Fail fast validation
        command.Validate();
        if (command.Invalid)
            return new GenericCommandResult(
                false, "Ops, há um problema com a tarefa.", command.Notifications);

        // Recupera o TodoItem (Reidratação dos dados)
        var todo = _repository.GetById(command.Id, command.User);

        // Altera o estado
        todo.MarkAsDone();

        // Salva no banco de dados
        _repository.Update(todo);

        // Retorna o resultado, trazendo apenas a tarefa inserida com o ID gerado
        return new GenericCommandResult(true, "Tarefa alterada para concluída", todo);
    }
}