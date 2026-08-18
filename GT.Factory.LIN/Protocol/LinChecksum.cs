namespace GT.Factory.LIN.Core
{

    /// <summary>
    /// LIN校验计算
    ///
    /// LIN 2.x:
    ///
    /// Enhanced Checksum
    ///
    /// 计算:
    ///
    /// PID + DATA
    ///
    /// </summary>
    public static class LinChecksum
    {

        /// <summary>
        /// Enhanced checksum
        ///
        /// </summary>
        public static byte CalculateEnhanced(
            byte id,
            byte[] data)
        {

            int sum = id;

            foreach (byte b in data)
            {

                sum += b;


                //
                // LIN采用:
                //
                // Carry回卷
                //
                if (sum > 255)
                {

                    sum = (sum & 0xff) + 1;

                }

            }

            return
                (byte)(~sum);

        }


        /// <summary>
        /// Classic checksum
        ///
        /// 不包含PID
        ///
        /// </summary>
        public static byte CalculateClassic(
            byte[] data)
        {

            int sum = 0;

            foreach (byte b in data)
            {

                sum += b;


                if (sum > 255)
                {

                    sum = (sum & 0xff) + 1;

                }

            }


            return
                (byte)(~sum);

        }

    }

}

