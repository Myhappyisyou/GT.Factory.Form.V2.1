using GT_Common.Helper;
using GT_Common.Helper.Mssql;
using System;
using System.Data;
using System.Windows.Forms;

namespace GT_Common.WordOrder
{
    public  class ChangeModel
    {
        event EventHandler GetChangeOrder;
        public ModelParameter orderParameter;
        string processNo = null;
        public bool start = true;

        Label newProductName;

        public ChangeModel(string process_no, Label productName)
        {

            GetChangeOrder += RunningChange;
            processNo = process_no;
            newProductName = productName;

            GetChangeOrder?.BeginInvoke(null, null, null, null);
        }

        private void RunningChange(object sender, EventArgs e)
        {
            DataSet orderChangeData = new DataSet();
            string sqlStr = $"select order_id from SHModelRunning where process_no='{processNo}' AND state = 0 ";

            while (start)
            {
                try
                {
                    orderChangeData = MSSqlHelper.GetDataSet(MSSqlHelper.Conn1, CommandType.Text, sqlStr);
                }
                catch
                {
                    return;
                }

                if (orderChangeData.Tables[0].Rows.Count > 0)
                {
                    string productName = orderChangeData.Tables[0].Rows[0][0].ToString();

                    DataSet data = new DataSet();
                    try
                    {
                        string sqlStr_ = $"select * from SHOrderName where order_name='{productName}'";
                        data = MSSqlHelper.GetDataSet(MSSqlHelper.Conn1, CommandType.Text, sqlStr_);

                        DataRow dr = null;

                        if (data.Tables[0].Rows.Count == 1)
                        {
                            dr = data.Tables[0].Rows[0];

                            orderParameter = ToModer(dr);
                        }
                        else
                        {
                            return;
                        }
                        
                    }
                    catch (Exception)
                    {

                        return;
                    }


                    newProductName.Invoke((MethodInvoker)delegate
                    {
                        newProductName.Text = productName;
                    });

                    UpdataOrderChangeData(true);
                }
                
            }
            
        }

        private void UpdataOrderChangeData(bool result)
        {
            DataSet data = new DataSet();
            int state = 0;
            if (result)
            {
                state = 1;
            }
            else
            {
                state = 2;
            }

            try
            {
                string sqlStr = $"update SHOrderRunning set state = '{state}', update_time = GETDATE() where process_no ='{processNo}'";
                MSSqlHelper.ExecuteNonQuery(MSSqlHelper.Conn1, CommandType.Text, sqlStr);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private ModelParameter ToModer(DataRow dr)
        {
            ModelParameter orderParameter = new ModelParameter();
            orderParameter.bar_no = dr["bar_no"].ToString();
            orderParameter.Runner_plate = (int)dr["Runner_plate"];

            return orderParameter;
        }
    }
}
