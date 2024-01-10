namespace BisleriumCafe.Shared.Layouts;

//A partial class in C# is a class that can be defined in multiple files within the same namespace. The compiler combines all the partial class files into a single class during compilation. This feature allows you to split a class's definition across multiple files, making it easier to manage large codebases or to separate generated code from manually written code.
public partial class MainLayout
{
    private string _title;

    private bool _drawerOpen = true;

    private void DrawerToggle()
    {
        _drawerOpen = !_drawerOpen;
    }

    private void SetAppBarTitle(string title)
    {
        _title = title;
    }
}
