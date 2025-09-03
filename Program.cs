using System.Security.Cryptography;
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
            if (context.Request.Query.ContainsKey("id")) 
            {
                var id = context.Request.Query["id"];
                if (int.TryParse(id, out int todoId)) 
                {
                    var todo = TodoRepository.GetTodo().FirstOrDefault(td => td.Id == todoId);
                    await context.Response.WriteAsync($"{todo.Id}: {todo.Title}\n");
                    await context.Response.WriteAsync($"{todo.Description}\n");
                    await context.Response.WriteAsync($"is Edited: ${todo.isEdited}");
                    await context.Response.WriteAsync($"Current DateTime:{todo.CurrentDateTime}");
                }
            } else
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
                        await context.Response.WriteAsync($"Edited: {todo.isEdited}\n");
                        await context.Response.WriteAsync($"created at: {todo.CurrentDateTime}\n");
                        await context.Response.WriteAsync($"--------------------\n");
                    }
                }
            }
        }
        else if (context.Request.Method == "POST")
        {
            var body = await reader.ReadToEndAsync();

            try
            {
                var todo = JsonSerializer.Deserialize<Todo>(body);

                if (todo is not null)
                {
                    TodoRepository.CreateTodo(todo);
                    context.Response.StatusCode = 201;
                    await context.Response.WriteAsync("todo created successfuly!");
                }
                else
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("please field the input currectly!");
                }
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync(ex.Message);
            }

        }
        else if (context.Request.Method == "PUT")
        {
            var body = await reader.ReadToEndAsync();

            try
            {
                var todo = JsonSerializer.Deserialize<Todo>(body);

                if (todo is not null)
                {
                    TodoRepository.EditTodo(todo);
                    context.Response.StatusCode = 202;
                    await context.Response.WriteAsync("todo edited successfuly!");
                }
                else
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("todo is not found!");
                }
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync(ex.Message);
            }

        }
        else if (context.Request.Method == "DELETE")
        {
            var body = await reader.ReadToEndAsync();

            if (context.Request.Query.ContainsKey("id"))
            {
                var id = context.Request.Query["id"];

                if (context.Request.Headers["Authorization"] == "auth")
                {
                    if (TodoRepository.DeleteTodo(int.Parse(id)))
                    {
                        context.Response.StatusCode = 202;
                        await context.Response.WriteAsync("todo deleted successfuly!");
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsync("todo is not found!");
                    }
                }
                else
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("you are not authorized to delete this todo!");
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
        if (todo is not null)
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

        if (td is not null)
        {
            td.Title = todo.Title;
            td.Description = todo.Description;
            td.isEdited = true;
            td.CurrentDateTime = DateTime.Now;

            result = true;
        }
        else
        {
            result = false;
        }
        return result;
    }

    // Delete Todo
    public static bool DeleteTodo(int id)
    {
        bool result;
        var todo = todos.FirstOrDefault(x => x.Id == id);

        if (todo is not null)
        {
            todos.Remove(todo);
            result = true;
        }
        else
        {
            result = false;
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
