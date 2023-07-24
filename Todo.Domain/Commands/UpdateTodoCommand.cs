using Flunt.Notifications;
using Flunt.Validations;
using Todo.Domain.Commands.Contracts;

namespace Todo.Domain.Commands;
public class UpdateTodoAsUndoneCommand : Notifiable, ICommand
{
    public UpdateTodoAsUndoneCommand(Guid id, string? title, string? user)
    {
        Id = id;
        Title = title;
        User = user;
    }

    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? User { get; set; }

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