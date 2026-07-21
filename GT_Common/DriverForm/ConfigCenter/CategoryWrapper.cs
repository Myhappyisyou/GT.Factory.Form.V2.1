using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace GT_Common.DriverForm.ConfigCenter
{
    public class CategoryWrapper : ICustomTypeDescriptor
    {
        private object _target;
        private string _category;
        private string _filter = "";

        public CategoryWrapper(object target, string category)
        {
            _target = target;
            _category = category;
        }

        public void SetFilter(string keyword)
        {
            _filter = keyword?.ToLower() ?? "";
        }

        public PropertyDescriptorCollection GetProperties()
        {
            var props = TypeDescriptor.GetProperties(_target);
            var list = new List<PropertyDescriptor>();

            foreach (PropertyDescriptor prop in props)
            {
                // 分类过滤
                if (prop.Category != _category)
                    continue;

                // 搜索过滤
                if (!string.IsNullOrEmpty(_filter))
                {
                    if (!prop.DisplayName.ToLower().Contains(_filter) &&
                        !prop.Name.ToLower().Contains(_filter))
                        continue;
                }

                list.Add(prop);
            }

            return new PropertyDescriptorCollection(list.ToArray());
        }

        // 🔥 必须补这个（你报错的原因）
        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return GetProperties();
        }

        // 下面默认实现
        public AttributeCollection GetAttributes() => TypeDescriptor.GetAttributes(_target);
        public string GetClassName() => TypeDescriptor.GetClassName(_target);
        public string GetComponentName() => TypeDescriptor.GetComponentName(_target);
        public TypeConverter GetConverter() => TypeDescriptor.GetConverter(_target);
        public EventDescriptor GetDefaultEvent() => TypeDescriptor.GetDefaultEvent(_target);
        public PropertyDescriptor GetDefaultProperty() => TypeDescriptor.GetDefaultProperty(_target);
        public object GetEditor(Type editorBaseType) => TypeDescriptor.GetEditor(_target, editorBaseType);
        public EventDescriptorCollection GetEvents() => TypeDescriptor.GetEvents(_target);
        public EventDescriptorCollection GetEvents(Attribute[] attributes) => TypeDescriptor.GetEvents(_target, attributes);
        public object GetPropertyOwner(PropertyDescriptor pd) => _target;
    }
}
