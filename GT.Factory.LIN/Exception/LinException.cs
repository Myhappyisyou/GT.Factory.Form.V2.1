using System;


namespace GT.Factory.LIN.Exception
{


    /// <summary>
    /// LIN模块异常
    ///
    /// </summary>
    public class LinException
        : System.Exception
    {


        public LinException(
            string message)
            : base(message)
        {

        }




        public LinException(
            string message,
            System.Exception inner)
            : base(message, inner)
        {

        }


    }

}