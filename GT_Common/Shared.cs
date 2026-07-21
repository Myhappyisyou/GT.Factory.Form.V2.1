
using GT_Common.MyEnum;
using GT_Common.DriverForm.Batch;
using GT_Common.Helper;
using GT_Common.Helper.LanModelSync;
using GT_Common.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using TaskContracts.Models;
using User = GT_Common.Model.User;

namespace GT_Common
{
    public sealed class Shared
    {
        private static readonly Lazy<Shared> lazy = new Lazy<Shared>(() => new Shared());

        private Shared()
        {
           
        }

        public static Shared Instance
        {
            get
            {
                return lazy.Value;
            }
        }

        //  当前工序号

        public static string processNo = "";


        //  
        public static  User user;


        public static bool isBind = true;
        public static bool isTimeOut = false;
        public static bool blockMesBind = false;

        // 状态指示灯

        public static IndicatorStatus plcSatus = IndicatorStatus.Warning;
        public static IndicatorStatus serverSatus = IndicatorStatus.Warning;
        public static IndicatorStatus sqlSatus = IndicatorStatus.Warning;
        public static IndicatorStatus accessSatus = IndicatorStatus.Warning;

        //  易损件工位
        public static string StationName = "";
        public static string ProcessName = "";


        // 当前工单状态
        public static ShopOrderInfo shopOrder;
        public static string currentUser="";
        public static bool isOffline = true;
        public static string productNo="";
        public static string productName = "LC-320H";
        public static string productCode = "";
        public static string fixtureBind = "";
        public static string currentBarNo = "";

        public static ProductModel productModel ;

        public static string workOrder = "";    //  订单号


        public static BindingList<RowDataModel> dataList = new BindingList<RowDataModel>
        {
             new RowDataModel { Col0 = "", Col1 = null, Col2 = null, Col3 = null },
             new RowDataModel { Col0 = "用户", Col1 = "", Col2 = "模式", Col3 = ""},
             new RowDataModel { Col0 = "产品编码", Col1 = "", Col2 = null, Col3 = null },
             new RowDataModel { Col0 = "产品名称", Col1 = "", Col2 = null, Col3 = null },
             new RowDataModel { Col0 = "产品代码", Col1 = "", Col2 = null, Col3 = null },
             new RowDataModel { Col0 = "工装绑定", Col1 = "", Col2 = null, Col3 = null },
             new RowDataModel { Col0 = "", Col1 = null, Col2 = null, Col3 = null }
        };

        // 当前产品状态
        public static BindingList<TestItemMD> testItemMDs = new BindingList<TestItemMD>();

        public static readonly object _dicCurrentTestItemMDsLock = new object();

        public static Dictionary<string, currentProductDataModel> dicCurrentTestItemMDs = new Dictionary<string, currentProductDataModel>();

        //public static Dictionary<string, BindingList<TestItemMD>> dicCurrentTestItemMDs = new Dictionary<string, BindingList<TestItemMD>>();

        public static BindingList<TestItemMD> currentTestItemMDs = new BindingList<TestItemMD>();

        public static ProductStatus currentProductStatus = ProductStatus.Standby;

        //  设备状态

        public static int currentActionStatus = -1;

        //  成产信息
        public static ProductionMonitor monitor;
        public static string totalNub;
        public static string orderNub;
        public static string finishNub;
        public static string okNub;
        public static string rate;
        public static string doTime;

        public static BindingList<RowDataModel2> productInformationDataList = new BindingList<RowDataModel2>
        {
                new RowDataModel2 { Col0 = "总次数", Col1 = ""},
                new RowDataModel2 { Col0 = "订单号", Col1 = ""},
                new RowDataModel2 { Col0 = "工单数量", Col1 = ""},
                new RowDataModel2 { Col0 = "完成数量", Col1 = ""},
                new RowDataModel2 { Col0 = "合格数量", Col1 = ""},
                new RowDataModel2 { Col0 = "合格率", Col1 = "" },
                new RowDataModel2 { Col0 = "生产时间", Col1 = ""},
        };

        public List<PartRuntime> partRuntimes;

        // 测试相关

        public List<string> lsData;

        // 显示部分
        public ConcurrentQueue<TestDispItem> dispItem = new ConcurrentQueue<TestDispItem>();

        // 保存数据

        public List<SaveItem> SaveItems = new List<SaveItem>();

        //  提示信息
        public static List<ActionTipModel> lsActionTips;

        public static string actionTip = null;

        public static ProductStatus actionTipStatus = ProductStatus.Standby;

    }
}
