using MySqlConnector;
using Spectre.Console;
using dotenv.net;

DotEnv.Load();
var dotenv = DotEnv.Read();
string username = dotenv["USERNAME"];
string password = dotenv["PASSWORD"];
string server = dotenv["SERVER"];
Console.Clear();
Console.WriteLine("Hello, Welcome to Knowledge.");
string conenctionString = $"Server={server};User ID={username};Password={password};Database=knowledge";
string exitString = "[ EXIT ]";

void selectionScreen()
{
    var option = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
        .Title("Choose a command to run in the library:")
        .HighlightStyle(new Style(foreground: Color.Orange3))
        .PageSize(10)
        .MoreChoicesText("[grey](Move up and down to reveal more fruits)[/]")
        .AddChoices(new[] {
            "List", "Add", "Search", "Delete", "Exit"
        })
    );

    switch (option)
    {
        case "List":
            Console.Clear();
            listBooks();
            break;

        case "Add":
            Console.Clear();
            addBook();
            break;

        case "Search":
            Console.Clear();
            searchBook();
            break;

        case "Delete":
            Console.Clear();
            deleteBook();
            break;

        case "Exit":
            Environment.Exit(1);
            break;
    }
}
selectionScreen();

void listBooks()
{
    // ! Add a method to list books from the database
    using var connection = new MySqlConnection(conenctionString);
    connection.Open();
    using var command = new MySqlCommand("SELECT * FROM Books;", connection);
    using var reader = command.ExecuteReader();
    // Add a table
    var table = new Table();
    table.Border(TableBorder.Horizontal).BorderColor(Color.Orange3);
    table.AddColumn("BookID");
    table.AddColumn("Title");
    table.AddColumn("Author");
    table.AddColumn("Date");
    while (reader.Read())
    {
        int BookID = reader.GetInt32(0);
        string Title = reader.GetString(1);
        string Author = reader.GetString(2);
        string Date = $"{reader.GetDateTime(3).Day}/{reader.GetDateTime(3).Month}/{reader.GetDateTime(3).Year}";
        table.AddRow(new Text(BookID.ToString()).Centered(), new Text(Title), new Text(Author), new Text(Date));
        // Console.WriteLine($"Book {BookID} - {Title} by {Author} - Released on {Date}");
    }
    // Render the table to the console
    AnsiConsole.Write(table);
    connection.Close();
    Console.ForegroundColor = Color.Red;
    Console.Write($"\n{exitString}");
    Console.ResetColor();
    Console.ReadLine();
    Console.Clear();
    selectionScreen();
}

void addBook()
{
    // ! Add a method to add a book to the database
    string title = AnsiConsole.Ask<string>("[orange3]Title[/] > ");
    string author = AnsiConsole.Ask<string>("[orange3]Author[/] > ");
    DateTime dateTime = Convert.ToDateTime(AnsiConsole.Ask<string>("[orange3]Release Date[/] {DD/MM/YYYY} > "));
    string? date = dateTime.Date.ToString("yyyy-MM-dd");

    var connection = new MySqlConnection(conenctionString);
    connection.Open();
    // string commandString = $"INSERT INTO Books(Title, Author, ReleaseDate) VALUES ({title}, {author}, {date})";
    using (var command = new MySqlCommand())
    {
        command.Connection = connection;
        command.CommandText = "INSERT INTO Books(Title, Author, ReleaseDate) VALUES (@title, @author, @date)";
        command.Parameters.AddWithValue("title", title);
        command.Parameters.AddWithValue("author", author);
        command.Parameters.AddWithValue("date", date);
        command.ExecuteNonQuery();
    }
    AnsiConsole.Write(new Markup($"[orange3]Successfully added \'{title}\' to Knowledge[/]"));
    connection.Close();
    Console.ForegroundColor = Color.Red;
    Console.Write($"\n{exitString}");
    Console.ResetColor();
    Console.ReadLine();
    Console.Clear();
    selectionScreen();
}

void searchBook()
{
    // ! Add a method to search for a book in the database
    string character = AnsiConsole.Ask<string>("[orange3]Book Title[/] > ");
    using var connection = new MySqlConnection(conenctionString);
    connection.Open();
    using var command = new MySqlCommand($"SELECT * FROM Books WHERE Title LIKE '%{character}%';", connection);
    using var reader = command.ExecuteReader();
    // Add a table
    var table = new Table();
    table.Border(TableBorder.Horizontal).BorderColor(Color.Orange3);
    table.AddColumn("BookID");
    table.AddColumn("Title");
    table.AddColumn("Author");
    table.AddColumn("Date");
    while (reader.Read())
    {
        int BookID = reader.GetInt32(0);
        string Title = reader.GetString(1);
        string Author = reader.GetString(2);
        string Date = $"{reader.GetDateTime(3).Day}/{reader.GetDateTime(3).Month}/{reader.GetDateTime(3).Year}";
        table.AddRow(new Text(BookID.ToString()).Centered(), new Text(Title), new Text(Author), new Text(Date));
        // Console.WriteLine($"Book {BookID} - {Title} by {Author} - Released on {Date}");
    }
    AnsiConsole.Write(table);
    connection.Close();
    Console.ForegroundColor = Color.Red;
    Console.Write($"\n{exitString}");
    Console.ResetColor();
    Console.ReadLine();
    Console.Clear();
    selectionScreen();
}

void deleteBook()
{
    // ! Add a method to delete a book from the database
    using var connection = new MySqlConnection(conenctionString);
    connection.Open();
    using var command = new MySqlCommand("SELECT * FROM Books;", connection);
    using var reader = command.ExecuteReader();
    List<string> table = new List<string>();
    while (reader.Read())
    {
        int BookID = reader.GetInt32(0);
        string Title = reader.GetString(1);
        string Author = reader.GetString(2);
        string Date = $"{reader.GetDateTime(3).Day}/{reader.GetDateTime(3).Month}/{reader.GetDateTime(3).Year}";
        table.Add(Title);
        // Console.WriteLine($"Book {BookID} - {Title} by {Author} - Released on {Date}");
    }
    connection.Close();
    table.Add("Exit");
    // Render the table to the console
    var option = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
        .Title("Choose a book to delete:")
        .HighlightStyle(new Style(foreground: Color.Orange3))
        .PageSize(10)
        .MoreChoicesText("[grey](Move up and down to reveal more fruits)[/]")
        .AddChoices(table)
    );
    connection.Open();
    // string delCommandString = $"DELETE FROM Books WHERE Title={option}";
    // var delCommand = new MySqlCommand(delCommandString, connection);
    if (option != "Exit")
    {
        using (var delCommand = new MySqlCommand())
        {
            delCommand.Connection = connection;
            delCommand.CommandText = "DELETE FROM Books WHERE Title=@option";
            delCommand.Parameters.AddWithValue("option", option);
            delCommand.ExecuteNonQuery();
        }
        connection.Close();
        AnsiConsole.Write(new Markup($"[orange3]Successfully deleted {option}[/]"));
        Console.ForegroundColor = Color.Red;
        Console.Write($"\n{exitString}");
        Console.ResetColor();
        Console.ReadLine();
        Console.Clear();
    }
    selectionScreen();
}