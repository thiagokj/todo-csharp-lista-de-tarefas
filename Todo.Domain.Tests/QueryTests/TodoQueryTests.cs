using Todo.Domain.Entities;
using Todo.Domain.Queries;

namespace Todo.Domain.Tests.QueryTests;
[TestClass]
public class TodoQueryTests
{
    private List<TodoItem> _items;

    public TodoQueryTests()
    {
        _items = new List<TodoItem>();
        _items.Add(new TodoItem("Tarefa 1", "Thiago", DateTime.Now));
        _items.Add(new TodoItem("Tarefa 2", "Thiago", DateTime.Now));
        _items.Add(new TodoItem("Tarefa 3", "Ronaldo", DateTime.Now));
        _items.Add(new TodoItem("Tarefa 4", "Lisa", DateTime.Now));
        _items.Add(new TodoItem("Tarefa 5", "Ronaldo", DateTime.Now));
    }

    [TestMethod]
    public void Deve_retornar_apenas_as_tarefas_do_usuario_especifico()
    {
        var result = _items.AsQueryable().Where(TodoQueries.GetAll("Thiago"));
        Assert.AreEqual(2, result.Count());
    }
    [TestMethod]
    public void DefaultValidTest()
    {
        Assert.Fail();
    }
}