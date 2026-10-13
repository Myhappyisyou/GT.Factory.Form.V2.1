using GT.Factory.Data.Core;
using GT.Factory.Product.Service;
using GT.Factory.ProductCode.Generator;
using GT.Factory.ProductCode.Processor;
using GT.Factory.ProductCode.Repository;
using GT.Factory.ProductCode.Service;
using GT.Factory.ProductCode.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT.Factory.ProductCode
{
    /// <summary>
    /// ProductCode 独立调试程序入口。
    ///
    /// UI 和现有 GT.Factory.Data 规则仓储继续复用。
    /// 产品码生成改为 ProductCodeGenerator，
    /// 不再调用 sp_GetProductCode。
    /// </summary>
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            DataRuntime dataRuntime =
                new DataRuntime(
                    new MultiDatabaseOptions
                    {
                        Primary = new DatabaseNodeOptions
                        {
                            Enabled = true,
                            DbType = GT.Factory.Common.Enums.DatabaseType.SqlServer,
                            ConnectionString =
                                    "Server=.;database=GT_Template;uid=sa;pwd=root"
                        },
                        LocalCache = new DatabaseNodeOptions
                        {
                            Enabled = true,
                            DbType = GT.Factory.Common.Enums.DatabaseType.SQLite,
                            ConnectionString = "Data Source=XMOilPumpData.sqlite"
                        },
                        Mes = new DatabaseNodeOptions
                        {
                            Enabled = false,
                            DbType = GT.Factory.Common.Enums.DatabaseType.Access,
                            ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Users_Data.mdb"
                        }
                    });

            ProductCodeRuleRepository dataRepository = new ProductCodeRuleRepository(dataRuntime.Database);

            ProductCodeRepository productCodeRepository = new ProductCodeRepository(dataRuntime.Database);

            Product.Repository.ProductModelRepository productModelRepository = new Product.Repository.ProductModelRepository(dataRuntime.Database);

            Product.Repository.ProductPartRepository productPartRepository = new Product.Repository.ProductPartRepository(dataRuntime.Database);

            ProcessorRegistry registry =
                new ProcessorRegistry(
                    new ICodeSegmentProcessor[]
                    {
                        new FixedTextProcessor(),
                        new DateProcessor()
                    });

            ProductCodeGenerator generator =
                new ProductCodeGenerator(registry);

            ProductCodeGeneratorService generatorService =
                new ProductCodeGeneratorService(
                    dataRepository,
                    productCodeRepository,
                    productModelRepository,
                    productPartRepository);

            ProductCodeRuleService ruleService =
                new ProductCodeRuleService(dataRepository, productCodeRepository);

            ProductModelService modelService =
               new ProductModelService(productModelRepository);

            ProductPartService partService =
               new ProductPartService(productPartRepository);

            //var result = codeRepository.GetCode(1, "Daily", DateTime.Now);
            // generatorService 由 ProductCode 业务层持有；
            // 当前管理界面只负责规则配置，后续 Station 可直接复用该 Service。
            ProductCodeRuntime.GenerationService = generatorService;
            ProductCodeValidator productCodeValidator = new ProductCodeValidator();

            int count = 20;

            Task[] tasks = new Task[count];

            for (int i = 0; i < count; i++)
            {
                int index = i;

                tasks[i] = Task.Run(delegate
                {
                    try
                    {
                        var result =
                            ProductCodeRuntime.GenerationService.Generate(1);
                        Console.WriteLine(
                            "线程 {0}：{1}",
                            index,
                            result.Code);

                       var codeValidator =   productCodeValidator.Validate(result.Code, dataRepository.GetRule(1));
                        Console.WriteLine(
                            "线程 {0}：{1}:{2}",
                            index,
                            codeValidator.ProductCode, codeValidator.IsValid);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            "线程 {0} 生成失败：{1}",
                            index,
                            ex.Message);
                    }
                });
            }

            Task.WaitAll(tasks);

            Console.WriteLine("并发测试完成。");


            var codeValidator2 = productCodeValidator.Validate("SHB922N0187", dataRepository.GetRule(1));
            Console.WriteLine(
                "线程 {0}：{1}：{2}",
                codeValidator2.ProductCode, codeValidator2.IsValid,codeValidator2.ErrorMessage);

            Application.Run();
        }
    }

    /// <summary>
    /// 独立调试程序中的运行时入口。
    /// Station 集成时不使用此静态容器，而是由宿主进行依赖注入。
    /// </summary>
    internal static class ProductCodeRuntime
    {
        public static ProductCodeGeneratorService GenerationService { get; set; }

    }
}
