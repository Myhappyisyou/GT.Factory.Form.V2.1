using GT_Common.MyEnum;
using RecipeParameter.RecipeParameter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.DriverForm.Recipe.RecipeParameter
{
    public class ParameterWrapper : ICustomTypeDescriptor
    {
        private ParameterBase _param;

        public ParameterWrapper(ParameterBase param)
        {
            _param = param;
        }

        public PropertyDescriptorCollection GetProperties()
        {
            // 如果是 LimitParameter，先更新子属性只读
            if (_param is LimitParameter limit)
            {
                bool editable = (int)Shared.user.LevelEnum >= (int)limit.EditableRole;
                limit.LowerLimit.IsReadOnly = !editable;
                limit.UpperLimit.IsReadOnly = !editable;
            }
            if (_param is IntParameter intParameter)
            {
                bool editable = (int)Shared.user.LevelEnum >= (int)intParameter.EditableRole;
                intParameter.Value.IsReadOnly = !editable;
            }

            var originalProps = TypeDescriptor.GetProperties(_param);
            var newProps = new PropertyDescriptorCollection(null);
            foreach (PropertyDescriptor pd in originalProps)
            {
                if (pd.Name == "Value" || pd.Name == "LowerLimit" || pd.Name == "UpperLimit")
                {
                    bool editable = (int)Shared.user.LevelEnum >= (int)(_param as ParameterBase).EditableRole;
                    var pd2 = TypeDescriptor.CreateProperty(
                        _param.GetType(),
                        pd,
                        new ReadOnlyAttribute(!editable)
                    );
                    newProps.Add(pd2);
                }
                else
                {
                    newProps.Add(pd);
                }
            }
            return newProps;
        }

        // 其他接口直接委托
        public AttributeCollection GetAttributes() => TypeDescriptor.GetAttributes(_param);
        public string GetClassName() => TypeDescriptor.GetClassName(_param);
        public string GetComponentName() => TypeDescriptor.GetComponentName(_param);
        public TypeConverter GetConverter() => TypeDescriptor.GetConverter(_param);
        public EventDescriptor GetDefaultEvent() => TypeDescriptor.GetDefaultEvent(_param);
        public PropertyDescriptor GetDefaultProperty() => TypeDescriptor.GetDefaultProperty(_param);
        public object GetEditor(Type editorBaseType) => TypeDescriptor.GetEditor(_param, editorBaseType);
        public EventDescriptorCollection GetEvents(Attribute[] attributes) => TypeDescriptor.GetEvents(_param, attributes);
        public EventDescriptorCollection GetEvents() => TypeDescriptor.GetEvents(_param);
        public object GetPropertyOwner(PropertyDescriptor pd) => _param;
        public PropertyDescriptorCollection GetProperties(Attribute[] attributes) => GetProperties();
    }
}
