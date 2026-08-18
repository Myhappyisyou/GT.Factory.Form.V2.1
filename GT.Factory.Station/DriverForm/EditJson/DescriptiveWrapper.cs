using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditJson
{
    public class DescriptiveWrapper
    {
        private readonly object _realObject;
        private readonly string _displayName;

        public DescriptiveWrapper(object realObject, string displayName)
        {
            _realObject = realObject;
            _displayName = displayName;
        }

        public override string ToString()
        {
            return _displayName;
        }

        // 供 PropertyGrid 展示属性
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Browsable(true)]
        public object RealObject => _realObject;
    }

}
