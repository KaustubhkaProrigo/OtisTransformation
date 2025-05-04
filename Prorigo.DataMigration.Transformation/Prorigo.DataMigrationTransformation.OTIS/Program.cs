using System;
using System.IO;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using System.Linq;
using OfficeOpenXml;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prorigo.Plm.DataMigration.Utilities;

using Serilog;

using Prorigo.Plm.DataMigration.Transformer;
using Prorigo.Plm.DataMigration.Transformer.Metrics;
using Prorigo.Plm.DataMigration.OtisDataTransformer;


namespace Prorigo.DataMigrationTransformation.OTIS
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var command = new RootCommand
            {
                new Option("--transformType") { Argument = new Argument<TransformType>() }
            };

            command.Handler = CommandHandler.Create(
                async (TransformType? transformType) =>
                {
                    try
                    {
                        if (transformType != null )
                        {
                            var transformName = transformType.ToString();

                            var services = new ServiceCollection();

                            var configuration = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("DataTransformerConfig.json", true)
                            .Build();

                            //License key
                            var LicenKeyFile = configuration.GetSection("Logging").GetValue<string>("LicenseKey");
                            if (string.IsNullOrEmpty(LicenKeyFile))
                            {
                                LicenKeyFile = Path.Combine(Directory.GetCurrentDirectory(), "DMFLicense.txt");
                            }

                            string LicenseKey = null;
                            using (var stream = new StreamReader(LicenKeyFile))
                            {
                                LicenseKey = stream.ReadToEnd();
                            }
                            bool isLicenValid = LicenseUtils.ValidateLicenKey(LicenseKey, "", "DMF");

                            if (isLicenValid)
                            {
                                ConfigureServices(services, transformName, configuration);
                            }
                            else
                            {
                                Console.Error.WriteLine($"License Key is Missing");
                                Console.Error.Flush();
                                Environment.Exit(-1);
                            }

                            using (ServiceProvider serviceProvider = services.BuildServiceProvider())
                            {
                                var dataTransformers = serviceProvider.GetServices<IDataTransformer>();
                                var migrationDiagnostics = serviceProvider.GetService<IMigrationDiagnostics>();
                                var logger = serviceProvider.GetService<Microsoft.Extensions.Logging.ILogger<Program>>();

                                try
                                {
                                    IDataTransformer dataTransformer = dataTransformers.Where(s => s.GetType().Name.StartsWith(transformName)).FirstOrDefault();

                                    migrationDiagnostics.LogTransformStartTime(transformName);
                                    migrationDiagnostics.LogTransformStatus(transformName, TransformStatus.InProgress);

                                    //License key
                                    isLicenValid = LicenseUtils.ValidateLicenKey(LicenseKey, "", "DMF");

                                    if (isLicenValid)
                                    {
                                        dataTransformer.Transform(LicenseKey);
                                    }
                                    else
                                    {
                                        Console.Error.WriteLine($"License Key is Missing");
                                        Console.Error.Flush();
                                        Environment.Exit(-1);
                                    }

                                    migrationDiagnostics.LogTransformEndTime(transformName);
                                    migrationDiagnostics.LogTransformStatus(transformName, TransformStatus.Completed);
                                }
                                catch (Exception exception)
                                {
                                    var errorMessage = exception.InnerException != null ? exception.InnerException.Message : exception.Message;

                                    migrationDiagnostics.LogTransformStatus(transformName, TransformStatus.Failed, errorMessage);  
                                    logger.LogError(exception, errorMessage);

                                    throw exception;
                                }
                            }

                            Console.WriteLine($"Transformation completed");
                            Environment.Exit(0);
                        }
                        else
                        {
                            Console.Error.WriteLine("Please pass mandatory commandline argument transformType");
                            Console.Error.Flush();
                            Environment.Exit(-1);
                        }
                    }
                    catch (Exception exception)
                    {
                        var errorMessage = exception.InnerException != null ? exception.InnerException.Message : exception.Message;
                        Console.Error.WriteLine($"Failed to Transform. Error: {errorMessage}");
                        Console.Error.Flush();

                        Environment.Exit(-1);
                    }
                });

            await command.InvokeAsync(args);
        }

        private static void ConfigureServices(ServiceCollection services, string transformName, IConfigurationRoot configuration)
        {
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();

            services.AddLogging(builder =>
            {
                builder.AddConfiguration(configuration.GetSection("Logging"));
                builder.AddSerilog();
            });

            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<IMigrationDiagnostics>((container) =>
            {
                var transformSection = configuration.GetSection(transformName);
                var processAreaDataPath = transformSection.GetValue<string>("processAreaDataPath"); 
                
                return new MigrationDiagnostics(processAreaDataPath);
            });

            services.AddTransient<IDataTransformer, DatToTsvTransformer>();
            services.AddTransient<IDataTransformer, ReorderColumnsTransformer>();
            services.AddTransient<IDataTransformer, OtisPartTransformer>();
            services.AddTransient<IDataTransformer, OtisFileTransformer>();
            services.AddTransient<IDataTransformer, OtisProductTransformer>();
            services.AddTransient<IDataTransformer, ExcelEBOMExtractDataTransformer>();
            services.AddTransient<IDataTransformer, Otis_VM_BreakdownItemTransformer>();
            services.AddTransient<IDataTransformer, ExcelEBOMInOutExtractDataTransformer>();
            services.AddTransient<IDataTransformer, OtisParameterFeatureOptionTransformer>();
            services.AddTransient<IDataTransformer, ExcelToTsvCellRangeInputOutputTransformer>();
            services.AddTransient<IDataTransformer, ExcelToTsvStartCellValueInputOutputTransformer>();
            services.AddTransient<IDataTransformer, OtisDrawing_ParameterRelationshipTransformer>();
            services.AddTransient<IDataTransformer, ExcelEBOM2ExtractTransformer>();
            services.AddTransient<IDataTransformer, ExcelToTsvPartTransformer>();
            services.AddTransient<IDataTransformer, ExcelToTsvParameterTransformer>();
            services.AddTransient<IDataTransformer, ExcelToTsvDrawingTransformer>();
            services.AddTransient<IDataTransformer, OtisCADDrawingTransformer>();
            services.AddTransient<IDataTransformer, OtisDrawingFileTransformer>();
        }
    }
}
