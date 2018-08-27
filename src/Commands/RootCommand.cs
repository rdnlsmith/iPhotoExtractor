using Microsoft.Extensions.CommandLineUtils;

namespace iPhotoExtractor.Commands
{
    public class RootCommand : ICommand
    {
        public static void Configure(CommandLineApplication app)
        {
            app.Name = "iPhotoExtractor";
            app.HelpOption("-h|--help");

            app.Command("preview", PreviewCommand.Configure);
            app.Command("extract", ExtractCommand.Configure);

            app.OnExecute(() =>
            {
                (new RootCommand(app)).Run();
                return 0;
            });
        }

        private readonly CommandLineApplication _app;

        public RootCommand(CommandLineApplication app)
        {
            _app = app;
        }

        public void Run()
        {
            _app.ShowHelp();
        }
    }
}