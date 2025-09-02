using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Run(async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);

    if (context.Request.Path.StartsWithSegments("/"))
    {
        if (context.Request.Method == "GET")
        {
            var todos = TodoRepository.GetTodo();
            if (todos is null || todos.Count == 0)
            {
                await context.Response.WriteAsync("there is no todo!");
            }
            else
            {
                foreach (var todo in todos)
                {
                    await context.Response.WriteAsync($"Id: {todo.Id}\n");
                    await context.Response.WriteAsync($"Title: {todo.Title}\n");
                    await context.Response.WriteAsync($"Edited: {todo.isEdited}");
                    await context.Response.WriteAsync($"created at: {todo.CurrentDateTime}\n");
                    await context.Response.WriteAsync($"--------------------\n");
                }
            }
        }
        else if (context.Request.Method == "POST")
        {
            var body = await reader.ReadToEndAsync();

            var todo = JsonSerializer.Deserialize<Todo>(body);

            if (todo is not null)
            {
                TodoRepository.CreateTodo(todo);
                await context.Response.WriteAsync("todo created successfuly!");
            }
            else
            {
                await context.Response.WriteAsync("please field the input currectly!");
            }
        }
        else if (context.Request.Method == "PUT")
        {
            var body = await reader.ReadToEndAsync();
            var todo = JsonSerializer.Deserialize<Todo>(body);

            if (todo is not null)
            {
                TodoRepository.EditTodo(todo);
                await context.Response.WriteAsync("todo edited successfuly!");
            }
            else
            {
                await context.Response.WriteAsync("todo is not found!");
            }
        }
        else if (context.Request.Method == "DELETE")
        {
            var body = await reader.ReadToEndAsync();

            if (context.Request.Query.ContainsKey("id"))
            {
                var id = context.Request.Query["id"];

                if (int.TryParse(id, out int todoId))
                {
                    var result = TodoRepository.DeleteTodo(todoId);
                    if (result)
                    {
                        await context.Response.WriteAsync($"you removed ${todoId}");
                    }
                    else
                    {
                        await context.Response.WriteAsync("id is not found");
                    }
                }

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

    // Edit Todo
    public static bool EditTodo(Todo? todo)
    {
        Todo td;
        bool result;

        td = todos.FirstOrDefault(t => t.Id == todo.Id);

        if(td is not null)
        {
            td.Title = todo.Title;
            td.Description = todo.Description;
            td.CurrentDateTime = DateTime.Now;

            result = true;
        }else
        {
            result = false;
        }
        return result;
    }

    // Delete Todo
    public static bool DeleteTodo(int id)
    {
        bool result;
        var todo = todos.FirstOrDefault(x=> x.Id == id);

        if(todo is not null)
        {
            todos.Remove(todo);
            result = true;
        }else
        {
            result= false;
        }
        return result;
    }

}


// Todo List Instance
public class Todo
{

    public int Id { get; set; }

    public bool isEdited { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }

    public DateTime CurrentDateTime = DateTime.Now;
}
