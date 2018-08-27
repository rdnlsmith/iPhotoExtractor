using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iPhotoExtractor.Commands;
using Microsoft.Extensions.CommandLineUtils;

namespace iPhotoExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new CommandLineApplication();
            RootCommand.Configure(app);
            app.Execute(args);
        }
    }
}
