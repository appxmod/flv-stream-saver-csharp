using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using BililiveRecorder.Flv;
using BililiveRecorder.Flv.Grouping;
using BililiveRecorder.Flv.Parser;
using BililiveRecorder.Flv.Pipeline;
using BililiveRecorder.Flv.Pipeline.Actions;
using BililiveRecorder.Flv.Pipeline.Rules;
using BililiveRecorder.Flv.Writer;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace FastStreamFix
{
    class Program
    {
        
        public class Example
        {
            public async Task<int> GetDataAsync()
            {
                // Simulate an asynchronous operation
                await Task.Delay(2000); // Wait for 2 seconds

                return 10;
            }

            public async Task Main()
            {
                int result = await GetDataAsync();
                Console.WriteLine(result);
            }
        }
 
        static string OutputBase = "R:\\cache\\OutputBase.flv";
        
        static async Task Main(string[] args)
        {
            // Example example = new Example();
            // await example.Main();

            for (int i = 0; i < args.Length; i++)
            {
                // VU.log(i, line);
                var line = args[i];
                if (line.StartsWith("-"))
                {
                    if (i+1 < args.Length)
                    {
                        var v = args[i+1];
                    }
                    i += 1;
                    continue;
                }
                if (line.Length > 5 && line.Substring(1,1)==":")
                {
                    OutputBase = line;
                }
            }
            VU.log("OutputBase::", OutputBase);
            
            await doit();
            await dummy();
        }

        async static Task dummy()
        {
        }

        async static Task doit()
        {

            try
            {
                CancellationToken cancellationToken = new CancellationToken();
                var logger = new XXYYZZ();
                // using (StreamReader reader = new StreamReader(Console.OpenStandardInput()))
                //     while ((input = reader.ReadLine()) != null)
            
                // Output
                var outputPaths = new List<string>();
                IFlvTagWriter tagWriter;
            
                var memoryStreamProvider = new RecyclableMemoryStreamProvider();
                var comments = new List<ProcessingComment>();
                var context = new FlvProcessingContext();
                var session = new Dictionary<object, object?>();

                {
                    var targetProvider = new AutoFixFlvWriterTargetProvider(OutputBase);
                    targetProvider.BeforeFileOpen += (sender, path) => outputPaths.Add(path);
                    tagWriter = new FlvTagFileWriter(targetProvider, memoryStreamProvider, logger);
                }


                Stream str;
                // str = new FileStream("....flv", FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
                str = Console.OpenStandardInput();
            
                Console.WriteLine("Hello World!");
            
                FlvTagPipeReader tagReader = new FlvTagPipeReader(PipeReader.Create(str)
                    , memoryStreamProvider, skipData: false, logger: logger);

                // Pipeline
                using var grouping = new TagGroupReader(tagReader);
                using var writer = new FlvProcessingContextWriter(tagWriter: tagWriter, allowMissingHeader: true, noKeyframes: false, logger: logger);
                var statsRule = new StatsRule();
                var ffmpegDetectionRule = new FfmpegDetectionRule();
                var pipeline = new ProcessingPipelineBuilder()
                    .ConfigureServices(services => services.AddSingleton(new ProcessingPipelineSettings()))
                    .AddRule(statsRule)
                    .AddRule(ffmpegDetectionRule)
                    .AddDefaultRules()
                    .AddRemoveFillerDataRule()
                    .Build();

                VU.log("Run ");
                // Run
                await Task.Run(async () =>
                {
                    var count = 0;
                    while (true)
                    {
                        // VU.log("重组FLV");
                        try
                        {
                            var group = await grouping.ReadGroupAsync(cancellationToken).ConfigureAwait(false);
                            // VU.log("输出 ", group);
                            if (group is null)
                                break;

                            context.Reset(group, session);
                            pipeline(context);

                            if (context.Comments.Count > 0)
                            {
                                comments.AddRange(context.Comments);
                                // VU.log("修复逻辑输出 ", context.Comments);
                            }

                            await writer.WriteAsync(context).ConfigureAwait(false);

                            foreach (var action in context.Actions)
                                if (action is PipelineDataAction dataAction)
                                    foreach (var tag in dataAction.Tags)
                                        tag.BinaryData?.Dispose();

                            // if (count++ % 10 == 0 && progress is not null && flvFileStream is not null)
                            //     await progress((double)flvFileStream.Position / flvFileStream.Length);
                        }
                        catch (Exception e)
                        {
                            VU.log(e);
                            throw;
                        }
                    }
                }).ConfigureAwait(false);


                VU.log("done ");
            }
            catch (Exception e)
            {
                VU.log(e);
                throw;
            }
        }



        private class XXYYZZ : ILogger
        {
            public void Write(LogEvent logEvent)
            {
                VU.log(logEvent.Level, logEvent.ToString(), logEvent.Exception);
            }
        }

        private class AutoFixFlvWriterTargetProvider : IFlvWriterTargetProvider
        {
            private readonly string pathTemplate;
            private int fileIndex = 1;
            public event EventHandler<string>? BeforeFileOpen;
            public AutoFixFlvWriterTargetProvider(string pathTemplate)
            {
                this.pathTemplate = pathTemplate;
            }
            public Stream CreateAccompanyingTextLogStream()
            {
                var path = Path.ChangeExtension(this.pathTemplate, "txt");
                return new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
            }
            public (Stream stream, object? state) CreateOutputStream()
            {
                var i = this.fileIndex++;
                var path = i==1?this.pathTemplate:Path.ChangeExtension(this.pathTemplate, $"fix_p{i:D3}.flv");
                var fileStream = File.Open(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
                BeforeFileOpen?.Invoke(this, path);
                return (fileStream, null);
            }
        }


        
        
    }
    
    
}