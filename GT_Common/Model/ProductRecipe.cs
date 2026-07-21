using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Model
{
    public class ProductRecipe
    {

        // 产品名
        public string Product_name { get; set; }
        //// 产品码前缀
        //public string Sn_prefix { get; set; }
        // 零件号
        public string Product_no { get; set; }
        //编码规则
        public int Product_model { get; set; }

        ////产地代码
        //public int Origin_no { get; set; } 
        //产线代码
        public string Line_no { get; set; }

        //生成编码模板
        public string Sn_template { get; set; }

        ////工位一打标工单
        //public int LaserMark1_no { get; set; }
        ////工位二打标工单
        //public int LaserMark2_no { get; set; }

        //OP90 X型圈外装是否启用
        public int OP90_x { get; set; }
    }
}
