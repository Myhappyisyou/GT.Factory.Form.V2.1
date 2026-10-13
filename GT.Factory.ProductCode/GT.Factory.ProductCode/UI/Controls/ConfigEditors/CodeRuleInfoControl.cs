using System;
using System.Windows.Forms;
using GT.Factory.Common.Models.ProductCode;


namespace GT.Factory.ProductCode.UI.Controls
{

    /// <summary>
    /// 产品码规则基本信息编辑。
    /// </summary>
    public class CodeRuleInfoControl :
        UserControl
    {


        private TextBox _nameBox;


        private TextBox _descriptionBox;

        private TextBox _productMode;


        private CodeRule _rule;



        public CodeRuleInfoControl()
        {

            InitializeUI();

        }




        private void InitializeUI()
        {

            Height =
                80;


            Dock =
                DockStyle.Top;



            TableLayoutPanel table =
                new TableLayoutPanel();


            table.Dock =
                DockStyle.Fill;


            table.ColumnCount = 2;


            table.RowCount = 3;



            table.Controls.Add(
                new Label()
                {
                    Text = "规则名称"                },
                0,
                0);



            _nameBox =
                new TextBox();
            _nameBox.Width = 300;


            table.Controls.Add(
                _nameBox,
                1,
                0);



            table.Controls.Add(
                new Label()
                {
                    Text = "描述"
                },
                0,
                1);



            _descriptionBox =
                new TextBox();
            _descriptionBox.Width = 300;


            table.Controls.Add(
                _descriptionBox,
                1,
                1);



            table.Controls.Add(
                new Label()
                {
                    Text = "产品型号"
                },
                0,
                2);



            _productMode =
                new TextBox();
            _productMode.Width = 300;


            table.Controls.Add(
                _productMode,
                1,
                2);

            Controls.Add(table);

        }





        public void BindRule(
            CodeRule rule)
        {

            _rule =
                rule;



            if (rule == null)
            {
                return;
            }


            _nameBox.Text =
                rule.RuleName;


            _descriptionBox.Text =
                rule.Description;

            _productMode.Text =
               rule.ProductModel;
        }





        public void UpdateRule()
        {

            if (_rule == null)
            {
                return;
            }


            _rule.RuleName =
                _nameBox.Text;


            _rule.Description =
                _descriptionBox.Text;

            _rule.ProductModel =
               _productMode.Text;
        }


    }

}