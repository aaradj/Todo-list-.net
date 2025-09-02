using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Run(async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);

    if (context.Request.Path.StartsWithSegments("/"))
    {
        if(context.Request.Method == "GET")
        {
            var todos = TodoRepository.GetTodo();
            if (todos is null || todos.Count == 0)
            {
                await context.Response.WriteAsync("there is no todo!");
            }else
            {
                foreach (var todo in todos)
                {
                    await context.Response.WriteAsync($"Id: {todo.Id}\n");
                    await context.Response.WriteAsync($"Title: {todo.Title}\n");
                    await context.Response.WriteAsync($"created at: {todo.CurrentDateTime}\n");
                    await context.Response.WriteAsync($"--------------------\n");
                }
            }
        }
        else if(context.Request.Method == "POST")
        {
            var body = await reader.ReadToEndAsync();

            var todo = JsonSerializer.Deserialize<Todo>(body);

            if(todo is not null)
            {
                TodoRepository.CreateTodo(todo);
                await context.Response.WriteAsync("todo created successfuly!");
            }else
            {
                await context.Response.WriteAsync("please field the input currectly!");
            }
        }
    }

});

app.Run();

// Todo Reponsitory
static class TodoRepository
{
    private static List<Todo> todos = new List<Todo>();

    // Get All Todos
    public static List<Todo> GetTodo() => todos;

    // Create Todo
    public static void CreateTodo(Todo? todo)
    {
        if(todo is not null)
        {
            todos.Add(todo);
        }
    }

}


// Todo List Instance
public class Todo
{

    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }

    public DateTime CurrentDateTime = DateTime.Now;
}

