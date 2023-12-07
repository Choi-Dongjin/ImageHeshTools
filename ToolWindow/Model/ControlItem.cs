using System;
using System.Windows;

namespace ImageHeshTools.ToolWindow.Model
{
    internal class ControlItem
    {
        private readonly Type _contentType;
        private readonly object? _dataContext;
        private object? _content;

        public ControlItem(string name, Type contentType, object? dataContext = null)
        {
            Name = name;
            _contentType = contentType;
            _dataContext = dataContext;
        }

        public object? Content => _content ??= CreateContent();

        public string Name { get; }

        private object? CreateContent()
        {
            var content = Activator.CreateInstance(_contentType);
            if (_dataContext != null && content is FrameworkElement element)
            {
                element.DataContext = _dataContext;
            }
            return content;
        }
    }
}
