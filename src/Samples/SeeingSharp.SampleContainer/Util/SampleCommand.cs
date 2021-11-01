using System;

namespace SeeingSharp.SampleContainer.Util
{
    public class SampleCommand : DelegateCommand
    {
        public string CommandText
        {
            get;
        }

        public bool CanExecuteAsProperty => this.CanExecute(null);

        public string IconFontFamily { get; }

        public char IconFontGlyph { get; }

        public SampleCommand(
            string commandText, Action execute, Func<bool> canExecute,
            string iconFontFamily, char iconFontGlyph)
            : base(execute, canExecute)
        {
            this.CommandText = commandText;
            this.IconFontFamily = iconFontFamily;
            this.IconFontGlyph = iconFontGlyph;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.CommandText;
        }
    }
}